using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class ConfigureBuilder
{
	public MethodInfo MethodInfo { get; }

	public ConfigureBuilder(MethodInfo configure)
	{
		MethodInfo = configure;
	}

	public Action<IApplicationBuilder> Build(object instance)
	{
		return delegate(IApplicationBuilder builder)
		{
			Invoke(instance, builder);
		};
	}

	private void Invoke(object instance, IApplicationBuilder builder)
	{
		using IServiceScope serviceScope = ServiceProviderServiceExtensions.CreateScope(builder.ApplicationServices);
		IServiceProvider serviceProvider = serviceScope.ServiceProvider;
		ParameterInfo[] parameters = MethodInfo.GetParameters();
		object[] array = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterInfo parameterInfo = parameters[i];
			if (parameterInfo.ParameterType == typeof(IApplicationBuilder))
			{
				array[i] = builder;
				continue;
			}
			try
			{
				array[i] = ServiceProviderServiceExtensions.GetRequiredService(serviceProvider, parameterInfo.ParameterType);
			}
			catch (Exception innerException)
			{
				throw new Exception($"Could not resolve a service of type '{parameterInfo.ParameterType.FullName}' for the parameter '{parameterInfo.Name}' of method '{MethodInfo.Name}' on type '{MethodInfo.DeclaringType.FullName}'.", innerException);
			}
		}
		MethodInfo.Invoke(instance, array);
	}
}
