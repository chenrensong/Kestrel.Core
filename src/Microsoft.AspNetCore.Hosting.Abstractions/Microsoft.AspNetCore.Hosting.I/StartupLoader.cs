using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting.Internal;

public class StartupLoader
{
	private abstract class ConfigureServicesDelegateBuilder
	{
		public abstract Func<IServiceCollection, IServiceProvider> Build();
	}

	private class ConfigureServicesDelegateBuilder<TContainerBuilder> : ConfigureServicesDelegateBuilder
	{
		public IServiceProvider HostingServiceProvider { get; }

		public ConfigureServicesBuilder ConfigureServicesBuilder { get; }

		public ConfigureContainerBuilder ConfigureContainerBuilder { get; }

		public object Instance { get; }

		public ConfigureServicesDelegateBuilder(IServiceProvider hostingServiceProvider, ConfigureServicesBuilder configureServicesBuilder, ConfigureContainerBuilder configureContainerBuilder, object instance)
		{
			HostingServiceProvider = hostingServiceProvider;
			ConfigureServicesBuilder = configureServicesBuilder;
			ConfigureContainerBuilder = configureContainerBuilder;
			Instance = instance;
		}

		public override Func<IServiceCollection, IServiceProvider> Build()
		{
			ConfigureServicesBuilder.StartupServiceFilters = BuildStartupServicesFilterPipeline;
			Func<IServiceCollection, IServiceProvider> configureServicesCallback = ConfigureServicesBuilder.Build(Instance);
			ConfigureContainerBuilder.ConfigureContainerFilters = ConfigureContainerPipeline;
			Action<object> configureContainerCallback = ConfigureContainerBuilder.Build(Instance);
			return ConfigureServices(configureServicesCallback, configureContainerCallback);
			Action<object> ConfigureContainerPipeline(Action<object> action)
			{
				return Target;
				void Source(TContainerBuilder containerBuilder)
				{
					action(containerBuilder);
				}
				void Target(object containerBuilder)
				{
					BuildStartupConfigureContainerFiltersPipeline(Source)((TContainerBuilder)containerBuilder);
				}
			}
		}

		private Func<IServiceCollection, IServiceProvider> ConfigureServices(Func<IServiceCollection, IServiceProvider> configureServicesCallback, Action<object> configureContainerCallback)
		{
			return ConfigureServicesWithContainerConfiguration;
			IServiceProvider ConfigureServicesWithContainerConfiguration(IServiceCollection services)
			{
				IServiceProvider serviceProvider = configureServicesCallback(services);
				if (serviceProvider != null)
				{
					return serviceProvider;
				}
				if (ConfigureContainerBuilder.MethodInfo != null)
				{
					IServiceProviderFactory<TContainerBuilder> requiredService = ServiceProviderServiceExtensions.GetRequiredService<IServiceProviderFactory<TContainerBuilder>>(HostingServiceProvider);
					TContainerBuilder val = requiredService.CreateBuilder(services);
					configureContainerCallback(val);
					serviceProvider = requiredService.CreateServiceProvider(val);
				}
				else
				{
					IServiceProviderFactory<IServiceCollection> requiredService2 = ServiceProviderServiceExtensions.GetRequiredService<IServiceProviderFactory<IServiceCollection>>(HostingServiceProvider);
					IServiceCollection containerBuilder = requiredService2.CreateBuilder(services);
					serviceProvider = requiredService2.CreateServiceProvider(containerBuilder);
				}
				return serviceProvider ?? services.BuildServiceProvider();
			}
		}

		private Func<IServiceCollection, IServiceProvider> BuildStartupServicesFilterPipeline(Func<IServiceCollection, IServiceProvider> startup)
		{
			return RunPipeline;
			IServiceProvider RunPipeline(IServiceCollection services)
			{
				IStartupConfigureServicesFilter[] filters = ServiceProviderServiceExtensions.GetRequiredService<IEnumerable<IStartupConfigureServicesFilter>>(HostingServiceProvider).Reverse().ToArray();
				if (filters.Length == 0)
				{
					return startup(services);
				}
				Action<IServiceCollection> action = InvokeStartup;
				for (int i = 0; i < filters.Length; i++)
				{
					action = filters[i].ConfigureServices(action);
				}
				action(services);
				return null;
				void InvokeStartup(IServiceCollection serviceCollection)
				{
					IServiceProvider serviceProvider = startup(serviceCollection);
					if (filters.Length != 0 && serviceProvider != null)
					{
						throw new InvalidOperationException("A ConfigureServices method that returns an IServiceProvider is not compatible with the use of one or more IStartupConfigureServicesFilter. Use a void returning ConfigureServices method instead or a ConfigureContainer method.");
					}
				}
			}
		}

		private Action<TContainerBuilder> BuildStartupConfigureContainerFiltersPipeline(Action<TContainerBuilder> configureContainer)
		{
			return RunPipeline;
			void InvokeConfigureContainer(TContainerBuilder builder)
			{
				configureContainer(builder);
			}
			void RunPipeline(TContainerBuilder containerBuilder)
			{
				IStartupConfigureContainerFilter<TContainerBuilder>[] array = ServiceProviderServiceExtensions.GetRequiredService<IEnumerable<IStartupConfigureContainerFilter<TContainerBuilder>>>(HostingServiceProvider).Reverse().ToArray();
				Action<TContainerBuilder> action = InvokeConfigureContainer;
				for (int i = 0; i < array.Length; i++)
				{
					action = array[i].ConfigureContainer(action);
				}
				action(containerBuilder);
			}
		}
	}

	public static StartupMethods LoadMethods(IServiceProvider hostingServiceProvider, Type startupType, string environmentName)
	{
		ConfigureBuilder configureBuilder = FindConfigureDelegate(startupType, environmentName);
		ConfigureServicesBuilder configureServicesBuilder = FindConfigureServicesDelegate(startupType, environmentName);
		ConfigureContainerBuilder configureContainerBuilder = FindConfigureContainerDelegate(startupType, environmentName);
		object obj = null;
		if (!configureBuilder.MethodInfo.IsStatic || (configureServicesBuilder != null && !configureServicesBuilder.MethodInfo.IsStatic))
		{
			obj = ActivatorUtilities.GetServiceOrCreateInstance(hostingServiceProvider, startupType);
		}
		Type type = ((configureContainerBuilder.MethodInfo != null) ? configureContainerBuilder.GetContainerType() : typeof(object));
		ConfigureServicesDelegateBuilder configureServicesDelegateBuilder = (ConfigureServicesDelegateBuilder)Activator.CreateInstance(typeof(ConfigureServicesDelegateBuilder<>).MakeGenericType(type), hostingServiceProvider, configureServicesBuilder, configureContainerBuilder, obj);
		return new StartupMethods(obj, configureBuilder.Build(obj), configureServicesDelegateBuilder.Build());
	}

	public static Type FindStartupType(string startupAssemblyName, string environmentName)
	{
		if (string.IsNullOrEmpty(startupAssemblyName))
		{
			throw new ArgumentException(string.Format("A startup method, startup type or startup assembly is required. If specifying an assembly, '{0}' cannot be null or empty.", "startupAssemblyName"), "startupAssemblyName");
		}
		Assembly assembly = Assembly.Load(new AssemblyName(startupAssemblyName));
		if (assembly == null)
		{
			throw new InvalidOperationException($"The assembly '{startupAssemblyName}' failed to load.");
		}
		string startupNameWithEnv = "Startup" + environmentName;
		string startupNameWithoutEnv = "Startup";
		Type type = assembly.GetType(startupNameWithEnv) ?? assembly.GetType(startupAssemblyName + "." + startupNameWithEnv) ?? assembly.GetType(startupNameWithoutEnv) ?? assembly.GetType(startupAssemblyName + "." + startupNameWithoutEnv);
		if (type == null)
		{
			List<TypeInfo> source = assembly.DefinedTypes.ToList();
			IEnumerable<TypeInfo> first = source.Where((TypeInfo info) => info.Name.Equals(startupNameWithEnv, StringComparison.OrdinalIgnoreCase));
			IEnumerable<TypeInfo> second = source.Where((TypeInfo info) => info.Name.Equals(startupNameWithoutEnv, StringComparison.OrdinalIgnoreCase));
			TypeInfo typeInfo = first.Concat(second).FirstOrDefault();
			if (typeInfo != null)
			{
				type = typeInfo.AsType();
			}
		}
		if (type == null)
		{
			throw new InvalidOperationException($"A type named '{startupNameWithEnv}' or '{startupNameWithoutEnv}' could not be found in assembly '{startupAssemblyName}'.");
		}
		return type;
	}

	private static ConfigureBuilder FindConfigureDelegate(Type startupType, string environmentName)
	{
		return new ConfigureBuilder(FindMethod(startupType, "Configure{0}", environmentName, typeof(void)));
	}

	private static ConfigureContainerBuilder FindConfigureContainerDelegate(Type startupType, string environmentName)
	{
		return new ConfigureContainerBuilder(FindMethod(startupType, "Configure{0}Container", environmentName, typeof(void), required: false));
	}

	private static ConfigureServicesBuilder FindConfigureServicesDelegate(Type startupType, string environmentName)
	{
		return new ConfigureServicesBuilder(FindMethod(startupType, "Configure{0}Services", environmentName, typeof(IServiceProvider), required: false) ?? FindMethod(startupType, "Configure{0}Services", environmentName, typeof(void), required: false));
	}

	private static MethodInfo FindMethod(Type startupType, string methodName, string environmentName, Type returnType = null, bool required = true)
	{
		string methodNameWithEnv = string.Format(CultureInfo.InvariantCulture, methodName, environmentName);
		string methodNameWithNoEnv = string.Format(CultureInfo.InvariantCulture, methodName, "");
		MethodInfo[] methods = startupType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		List<MethodInfo> list = methods.Where((MethodInfo method) => method.Name.Equals(methodNameWithEnv, StringComparison.OrdinalIgnoreCase)).ToList();
		if (list.Count > 1)
		{
			throw new InvalidOperationException($"Having multiple overloads of method '{methodNameWithEnv}' is not supported.");
		}
		if (list.Count == 0)
		{
			list = methods.Where((MethodInfo method) => method.Name.Equals(methodNameWithNoEnv, StringComparison.OrdinalIgnoreCase)).ToList();
			if (list.Count > 1)
			{
				throw new InvalidOperationException($"Having multiple overloads of method '{methodNameWithNoEnv}' is not supported.");
			}
		}
		MethodInfo methodInfo = list.FirstOrDefault();
		if (methodInfo == null)
		{
			if (required)
			{
				throw new InvalidOperationException($"A public method named '{methodNameWithEnv}' or '{methodNameWithNoEnv}' could not be found in the '{startupType.FullName}' type.");
			}
			return null;
		}
		if (returnType != null && methodInfo.ReturnType != returnType)
		{
			if (required)
			{
				throw new InvalidOperationException($"The '{methodInfo.Name}' method in the type '{startupType.FullName}' must have a return type of '{returnType.Name}'.");
			}
			return null;
		}
		return methodInfo;
	}
}
