using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Abstractions;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Builder;

public static class UseMiddlewareExtensions
{
	internal const string InvokeMethodName = "Invoke";

	internal const string InvokeAsyncMethodName = "InvokeAsync";

	private static readonly MethodInfo GetServiceInfo = typeof(UseMiddlewareExtensions).GetMethod("GetService", BindingFlags.Static | BindingFlags.NonPublic);

	public static IApplicationBuilder UseMiddleware<TMiddleware>(this IApplicationBuilder app, params object[] args)
	{
		return app.UseMiddleware(typeof(TMiddleware), args);
	}

	public static IApplicationBuilder UseMiddleware(this IApplicationBuilder app, Type middleware, params object[] args)
	{
		if (typeof(IMiddleware).GetTypeInfo().IsAssignableFrom(middleware.GetTypeInfo()))
		{
			if (args.Length != 0)
			{
				throw new NotSupportedException(Resources.FormatException_UseMiddlewareExplicitArgumentsNotSupported(typeof(IMiddleware)));
			}
			return UseMiddlewareInterface(app, middleware);
		}
		IServiceProvider applicationServices = app.ApplicationServices;
		return app.Use(delegate(RequestDelegate next)
		{
			MethodInfo[] array = (from m in middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				where string.Equals(m.Name, "Invoke", StringComparison.Ordinal) || string.Equals(m.Name, "InvokeAsync", StringComparison.Ordinal)
				select m).ToArray();
			if (array.Length > 1)
			{
				throw new InvalidOperationException(Resources.FormatException_UseMiddleMutlipleInvokes("Invoke", "InvokeAsync"));
			}
			if (array.Length == 0)
			{
				throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoInvokeMethod("Invoke", "InvokeAsync", middleware));
			}
			MethodInfo methodInfo = array[0];
			if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
			{
				throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNonTaskReturnType("Invoke", "InvokeAsync", "Task"));
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 0 || parameters[0].ParameterType != typeof(HttpContext))
			{
				throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoParameters("Invoke", "InvokeAsync", "HttpContext"));
			}
			object[] array2 = new object[args.Length + 1];
			array2[0] = next;
			Array.Copy(args, 0, array2, 1, args.Length);
			object instance = ActivatorUtilities.CreateInstance(app.ApplicationServices, middleware, array2);
			if (parameters.Length == 1)
			{
				return (RequestDelegate)methodInfo.CreateDelegate(typeof(RequestDelegate), instance);
			}
			Func<object, HttpContext, IServiceProvider, Task> factory = Compile<object>(methodInfo, parameters);
			return delegate(HttpContext context)
			{
				IServiceProvider serviceProvider = context.RequestServices ?? applicationServices;
				if (serviceProvider == null)
				{
					throw new InvalidOperationException(Resources.FormatException_UseMiddlewareIServiceProviderNotAvailable("IServiceProvider"));
				}
				return factory(instance, context, serviceProvider);
			};
		});
	}

	private static IApplicationBuilder UseMiddlewareInterface(IApplicationBuilder app, Type middlewareType)
	{
		return app.Use((RequestDelegate next) => async delegate(HttpContext context)
		{
			IMiddlewareFactory middlewareFactory = (IMiddlewareFactory)context.RequestServices.GetService(typeof(IMiddlewareFactory));
			if (middlewareFactory == null)
			{
				throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoMiddlewareFactory(typeof(IMiddlewareFactory)));
			}
			IMiddleware middleware = middlewareFactory.Create(middlewareType);
			if (middleware == null)
			{
				throw new InvalidOperationException(Resources.FormatException_UseMiddlewareUnableToCreateMiddleware(middlewareFactory.GetType(), middlewareType));
			}
			try
			{
				await middleware.InvokeAsync(context, next);
			}
			finally
			{
				middlewareFactory.Release(middleware);
			}
		});
	}

	private static Func<T, HttpContext, IServiceProvider, Task> Compile<T>(MethodInfo methodInfo, ParameterInfo[] parameters)
	{
		Type typeFromHandle = typeof(T);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(HttpContext), "httpContext");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
		ParameterExpression parameterExpression3 = Expression.Parameter(typeFromHandle, "middleware");
		Expression[] array = new Expression[parameters.Length];
		array[0] = parameterExpression;
		for (int i = 1; i < parameters.Length; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			if (parameterType.IsByRef)
			{
				throw new NotSupportedException(Resources.FormatException_InvokeDoesNotSupportRefOrOutParams("Invoke"));
			}
			Expression[] arguments = new Expression[3]
			{
				parameterExpression2,
				Expression.Constant(parameterType, typeof(Type)),
				Expression.Constant(methodInfo.DeclaringType, typeof(Type))
			};
			MethodCallExpression expression = Expression.Call(GetServiceInfo, arguments);
			array[i] = Expression.Convert(expression, parameterType);
		}
		Expression expression2 = parameterExpression3;
		if (methodInfo.DeclaringType != typeof(T))
		{
			expression2 = Expression.Convert(expression2, methodInfo.DeclaringType);
		}
		return Expression.Lambda<Func<T, HttpContext, IServiceProvider, Task>>(Expression.Call(expression2, methodInfo, array), new ParameterExpression[3] { parameterExpression3, parameterExpression, parameterExpression2 }).Compile();
	}

	private static object GetService(IServiceProvider sp, Type type, Type middleware)
	{
		return sp.GetService(type) ?? throw new InvalidOperationException(Resources.FormatException_InvokeMiddlewareNoService(type, middleware));
	}
}
