using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Microsoft.Extensions.Internal;

internal static class ActivatorUtilities
{
	private class ConstructorMatcher
	{
		private readonly ConstructorInfo _constructor;

		private readonly ParameterInfo[] _parameters;

		private readonly object[] _parameterValues;

		private readonly bool[] _parameterValuesSet;

		public ConstructorMatcher(ConstructorInfo constructor)
		{
			_constructor = constructor;
			_parameters = _constructor.GetParameters();
			_parameterValuesSet = new bool[_parameters.Length];
			_parameterValues = new object[_parameters.Length];
		}

		public int Match(object[] givenParameters)
		{
			int num = 0;
			int result = 0;
			for (int i = 0; i != givenParameters.Length; i++)
			{
				TypeInfo typeInfo = givenParameters[i]?.GetType().GetTypeInfo();
				bool flag = false;
				int num2 = num;
				while (!flag && num2 != _parameters.Length)
				{
					if (!_parameterValuesSet[num2] && _parameters[num2].ParameterType.GetTypeInfo().IsAssignableFrom(typeInfo))
					{
						flag = true;
						_parameterValuesSet[num2] = true;
						_parameterValues[num2] = givenParameters[i];
						if (num == num2)
						{
							num++;
							if (num2 == i)
							{
								result = num2;
							}
						}
					}
					num2++;
				}
				if (!flag)
				{
					return -1;
				}
			}
			return result;
		}

		public object CreateInstance(IServiceProvider provider)
		{
			for (int i = 0; i != _parameters.Length; i++)
			{
				if (_parameterValuesSet[i])
				{
					continue;
				}
				object service = provider.GetService(_parameters[i].ParameterType);
				if (service == null)
				{
					if (!ParameterDefaultValue.TryGetDefaultValue(_parameters[i], out var defaultValue))
					{
						throw new InvalidOperationException($"Unable to resolve service for type '{_parameters[i].ParameterType}' while attempting to activate '{_constructor.DeclaringType}'.");
					}
					_parameterValues[i] = defaultValue;
				}
				else
				{
					_parameterValues[i] = service;
				}
			}
			try
			{
				return _constructor.Invoke(_parameterValues);
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;
			}
		}
	}

	private static readonly MethodInfo GetServiceInfo = GetMethodInfo<Func<IServiceProvider, Type, Type, bool, object>>((IServiceProvider sp, Type t, Type r, bool c) => GetService(sp, t, r, c));

	public static object CreateInstance(IServiceProvider provider, Type instanceType, params object[] parameters)
	{
		int num = -1;
		bool flag = false;
		ConstructorMatcher constructorMatcher = null;
		if (!instanceType.GetTypeInfo().IsAbstract)
		{
			foreach (ConstructorInfo item in instanceType.GetTypeInfo().DeclaredConstructors.Where((ConstructorInfo c) => !c.IsStatic && c.IsPublic))
			{
				ConstructorMatcher constructorMatcher2 = new ConstructorMatcher(item);
				bool flag2 = item.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), inherit: false);
				int num2 = constructorMatcher2.Match(parameters);
				if (flag2)
				{
					if (flag)
					{
						ThrowMultipleCtorsMarkedWithAttributeException();
					}
					if (num2 == -1)
					{
						ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
					}
				}
				if (flag2 || num < num2)
				{
					num = num2;
					constructorMatcher = constructorMatcher2;
				}
				flag = flag || flag2;
			}
		}
		if (constructorMatcher == null)
		{
			throw new InvalidOperationException($"A suitable constructor for type '{instanceType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
		}
		return constructorMatcher.CreateInstance(provider);
	}

	public static ObjectFactory CreateFactory(Type instanceType, Type[] argumentTypes)
	{
		FindApplicableConstructor(instanceType, argumentTypes, out var matchingConstructor, out var parameterMap);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(IServiceProvider), "provider");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "argumentArray");
		return Expression.Lambda<Func<IServiceProvider, object[], object>>(BuildFactoryExpression(matchingConstructor, parameterMap, parameterExpression, parameterExpression2), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile().Invoke;
	}

	public static T CreateInstance<T>(IServiceProvider provider, params object[] parameters)
	{
		return (T)CreateInstance(provider, typeof(T), parameters);
	}

	public static T GetServiceOrCreateInstance<T>(IServiceProvider provider)
	{
		return (T)GetServiceOrCreateInstance(provider, typeof(T));
	}

	public static object GetServiceOrCreateInstance(IServiceProvider provider, Type type)
	{
		return provider.GetService(type) ?? CreateInstance(provider, type);
	}

	private static MethodInfo GetMethodInfo<T>(Expression<T> expr)
	{
		return ((MethodCallExpression)expr.Body).Method;
	}

	private static object GetService(IServiceProvider sp, Type type, Type requiredBy, bool isDefaultParameterRequired)
	{
		object service = sp.GetService(type);
		if (service == null && !isDefaultParameterRequired)
		{
			throw new InvalidOperationException($"Unable to resolve service for type '{type}' while attempting to activate '{requiredBy}'.");
		}
		return service;
	}

	private static Expression BuildFactoryExpression(ConstructorInfo constructor, int?[] parameterMap, Expression serviceProvider, Expression factoryArgumentArray)
	{
		ParameterInfo[] parameters = constructor.GetParameters();
		Expression[] array = new Expression[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo obj = parameters[i];
			Type parameterType = obj.ParameterType;
			object defaultValue;
			bool flag = ParameterDefaultValue.TryGetDefaultValue(obj, out defaultValue);
			if (parameterMap[i].HasValue)
			{
				array[i] = Expression.ArrayAccess(factoryArgumentArray, Expression.Constant(parameterMap[i]));
			}
			else
			{
				Expression[] arguments = new Expression[4]
				{
					serviceProvider,
					Expression.Constant(parameterType, typeof(Type)),
					Expression.Constant(constructor.DeclaringType, typeof(Type)),
					Expression.Constant(flag)
				};
				array[i] = Expression.Call(GetServiceInfo, arguments);
			}
			if (flag)
			{
				ConstantExpression right = Expression.Constant(defaultValue);
				array[i] = Expression.Coalesce(array[i], right);
			}
			array[i] = Expression.Convert(array[i], parameterType);
		}
		return Expression.New(constructor, array);
	}

	private static void FindApplicableConstructor(Type instanceType, Type[] argumentTypes, out ConstructorInfo matchingConstructor, out int?[] parameterMap)
	{
		matchingConstructor = null;
		parameterMap = null;
		if (!TryFindPreferredConstructor(instanceType, argumentTypes, ref matchingConstructor, ref parameterMap) && !TryFindMatchingConstructor(instanceType, argumentTypes, ref matchingConstructor, ref parameterMap))
		{
			throw new InvalidOperationException($"A suitable constructor for type '{instanceType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
		}
	}

	private static bool TryFindMatchingConstructor(Type instanceType, Type[] argumentTypes, ref ConstructorInfo matchingConstructor, ref int?[] parameterMap)
	{
		foreach (ConstructorInfo declaredConstructor in instanceType.GetTypeInfo().DeclaredConstructors)
		{
			if (!declaredConstructor.IsStatic && declaredConstructor.IsPublic && TryCreateParameterMap(declaredConstructor.GetParameters(), argumentTypes, out var parameterMap2))
			{
				if (matchingConstructor != null)
				{
					throw new InvalidOperationException($"Multiple constructors accepting all given argument types have been found in type '{instanceType}'. There should only be one applicable constructor.");
				}
				matchingConstructor = declaredConstructor;
				parameterMap = parameterMap2;
			}
		}
		return matchingConstructor != null;
	}

	private static bool TryFindPreferredConstructor(Type instanceType, Type[] argumentTypes, ref ConstructorInfo matchingConstructor, ref int?[] parameterMap)
	{
		bool flag = false;
		foreach (ConstructorInfo declaredConstructor in instanceType.GetTypeInfo().DeclaredConstructors)
		{
			if (!declaredConstructor.IsStatic && declaredConstructor.IsPublic && declaredConstructor.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), inherit: false))
			{
				if (flag)
				{
					ThrowMultipleCtorsMarkedWithAttributeException();
				}
				if (!TryCreateParameterMap(declaredConstructor.GetParameters(), argumentTypes, out var parameterMap2))
				{
					ThrowMarkedCtorDoesNotTakeAllProvidedArguments();
				}
				matchingConstructor = declaredConstructor;
				parameterMap = parameterMap2;
				flag = true;
			}
		}
		return matchingConstructor != null;
	}

	private static bool TryCreateParameterMap(ParameterInfo[] constructorParameters, Type[] argumentTypes, out int?[] parameterMap)
	{
		parameterMap = new int?[constructorParameters.Length];
		for (int i = 0; i < argumentTypes.Length; i++)
		{
			bool flag = false;
			TypeInfo typeInfo = argumentTypes[i].GetTypeInfo();
			for (int j = 0; j < constructorParameters.Length; j++)
			{
				if (!parameterMap[j].HasValue && constructorParameters[j].ParameterType.GetTypeInfo().IsAssignableFrom(typeInfo))
				{
					flag = true;
					parameterMap[j] = i;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private static void ThrowMultipleCtorsMarkedWithAttributeException()
	{
		throw new InvalidOperationException("Multiple constructors were marked with ActivatorUtilitiesConstructorAttribute.");
	}

	private static void ThrowMarkedCtorDoesNotTakeAllProvidedArguments()
	{
		throw new InvalidOperationException("Constructor marked with ActivatorUtilitiesConstructorAttribute does not accept all given argument types.");
	}
}
