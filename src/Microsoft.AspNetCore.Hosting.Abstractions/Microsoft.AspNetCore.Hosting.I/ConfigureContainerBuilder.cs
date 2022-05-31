using System;
using System.Reflection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class ConfigureContainerBuilder
{
	public MethodInfo MethodInfo { get; }

	public Func<Action<object>, Action<object>> ConfigureContainerFilters { get; set; }

	public ConfigureContainerBuilder(MethodInfo configureContainerMethod)
	{
		MethodInfo = configureContainerMethod;
	}

	public Action<object> Build(object instance)
	{
		return delegate(object container)
		{
			Invoke(instance, container);
		};
	}

	public Type GetContainerType()
	{
		ParameterInfo[] parameters = MethodInfo.GetParameters();
		if (parameters.Length != 1)
		{
			throw new InvalidOperationException("The " + MethodInfo.Name + " method must take only one parameter.");
		}
		return parameters[0].ParameterType;
	}

	private void Invoke(object instance, object container)
	{
		ConfigureContainerFilters(StartupConfigureContainer)(container);
		void StartupConfigureContainer(object containerBuilder)
		{
			InvokeCore(instance, containerBuilder);
		}
	}

	private void InvokeCore(object instance, object container)
	{
		if (!(MethodInfo == null))
		{
			object[] parameters = new object[1] { container };
			MethodInfo.Invoke(instance, parameters);
		}
	}
}
