using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class ConfigureServicesBuilder
{
	public MethodInfo MethodInfo { get; }

	public Func<Func<IServiceCollection, IServiceProvider>, Func<IServiceCollection, IServiceProvider>> StartupServiceFilters { get; set; }

	public ConfigureServicesBuilder(MethodInfo configureServices)
	{
		MethodInfo = configureServices;
	}

	public Func<IServiceCollection, IServiceProvider> Build(object instance)
	{
		return (IServiceCollection services) => Invoke(instance, services);
	}

	private IServiceProvider Invoke(object instance, IServiceCollection services)
	{
		return StartupServiceFilters(Startup)(services);
		IServiceProvider Startup(IServiceCollection serviceCollection)
		{
			return InvokeCore(instance, serviceCollection);
		}
	}

	private IServiceProvider InvokeCore(object instance, IServiceCollection services)
	{
		if (MethodInfo == null)
		{
			return null;
		}
		ParameterInfo[] parameters = MethodInfo.GetParameters();
		if (parameters.Length > 1 || parameters.Any((ParameterInfo p) => p.ParameterType != typeof(IServiceCollection)))
		{
			throw new InvalidOperationException("The ConfigureServices method must either be parameterless or take only one parameter of type IServiceCollection.");
		}
		object[] array = new object[MethodInfo.GetParameters().Length];
		if (parameters.Length != 0)
		{
			array[0] = services;
		}
		return MethodInfo.Invoke(instance, array) as IServiceProvider;
	}
}
