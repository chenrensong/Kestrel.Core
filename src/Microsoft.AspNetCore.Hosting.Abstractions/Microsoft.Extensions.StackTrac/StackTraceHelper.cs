using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Internal;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class StackTraceHelper
{
	public static IList<StackFrameInfo> GetFrames(Exception exception)
	{
		List<StackFrameInfo> list = new List<StackFrameInfo>();
		if (exception == null)
		{
			return list;
		}
		using PortablePdbReader portablePdbReader = new PortablePdbReader();
		bool fNeedFileInfo = true;
		StackFrame[] frames = new System.Diagnostics.StackTrace(exception, fNeedFileInfo).GetFrames();
		if (frames == null)
		{
			return list;
		}
		for (int i = 0; i < frames.Length; i++)
		{
			StackFrame stackFrame = frames[i];
			MethodBase method = stackFrame.GetMethod();
			if (ShowInStackTrace(method) || i >= frames.Length - 1)
			{
				StackFrameInfo stackFrameInfo = new StackFrameInfo
				{
					StackFrame = stackFrame,
					FilePath = stackFrame.GetFileName(),
					LineNumber = stackFrame.GetFileLineNumber(),
					MethodDisplayInfo = GetMethodDisplayString(stackFrame.GetMethod())
				};
				if (string.IsNullOrEmpty(stackFrameInfo.FilePath))
				{
					portablePdbReader.PopulateStackFrame(stackFrameInfo, method, stackFrame.GetILOffset());
				}
				list.Add(stackFrameInfo);
			}
		}
		return list;
	}

	internal static MethodDisplayInfo GetMethodDisplayString(MethodBase method)
	{
		if (method == null)
		{
			return null;
		}
		MethodDisplayInfo methodDisplayInfo = new MethodDisplayInfo();
		Type declaringType = method.DeclaringType;
		string name = method.Name;
		if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute)) && (typeof(IAsyncStateMachine).IsAssignableFrom(declaringType) || typeof(IEnumerator).IsAssignableFrom(declaringType)) && TryResolveStateMachineMethod(ref method, out declaringType))
		{
			methodDisplayInfo.SubMethod = name;
		}
		if (declaringType != null)
		{
			methodDisplayInfo.DeclaringTypeName = Microsoft.Extensions.Internal.TypeNameHelper.GetTypeDisplayName(declaringType, fullName: true, includeGenericParameterNames: true);
		}
		methodDisplayInfo.Name = method.Name;
		if (method.IsGenericMethod)
		{
			string text = string.Join(", ", from arg in method.GetGenericArguments()
				select Microsoft.Extensions.Internal.TypeNameHelper.GetTypeDisplayName(arg, fullName: false, includeGenericParameterNames: true));
			methodDisplayInfo.GenericArguments = methodDisplayInfo.GenericArguments + "<" + text + ">";
		}
		methodDisplayInfo.Parameters = method.GetParameters().Select(delegate(ParameterInfo parameter)
		{
			Type type = parameter.ParameterType;
			string prefix = string.Empty;
			if (parameter.IsOut)
			{
				prefix = "out";
			}
			else if (type != null && type.IsByRef)
			{
				prefix = "ref";
			}
			string type2 = "?";
			if (type != null)
			{
				if (type.IsByRef)
				{
					type = type.GetElementType();
				}
				type2 = Microsoft.Extensions.Internal.TypeNameHelper.GetTypeDisplayName(type, fullName: false, includeGenericParameterNames: true);
			}
			return new ParameterDisplayInfo
			{
				Prefix = prefix,
				Name = parameter.Name,
				Type = type2
			};
		});
		return methodDisplayInfo;
	}

	private static bool ShowInStackTrace(MethodBase method)
	{
		if (HasStackTraceHiddenAttribute(method))
		{
			return false;
		}
		Type declaringType = method.DeclaringType;
		if (declaringType == null)
		{
			return true;
		}
		if (HasStackTraceHiddenAttribute(declaringType))
		{
			return false;
		}
		if (declaringType == typeof(ExceptionDispatchInfo) && method.Name == "Throw")
		{
			return false;
		}
		if (declaringType == typeof(TaskAwaiter) || declaringType == typeof(TaskAwaiter<>) || declaringType == typeof(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter) || declaringType == typeof(ConfiguredTaskAwaitable<>.ConfiguredTaskAwaiter))
		{
			switch (method.Name)
			{
			case "HandleNonSuccessAndDebuggerNotification":
			case "ThrowForNonSuccess":
			case "ValidateEnd":
			case "GetResult":
				return false;
			}
		}
		return true;
	}

	private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
	{
		declaringType = method.DeclaringType;
		Type declaringType2 = declaringType.DeclaringType;
		if (declaringType2 == null)
		{
			return false;
		}
		MethodInfo[] methods = declaringType2.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (methods == null)
		{
			return false;
		}
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			IEnumerable<StateMachineAttribute> customAttributes = methodInfo.GetCustomAttributes<StateMachineAttribute>();
			if (customAttributes == null)
			{
				continue;
			}
			foreach (StateMachineAttribute item in customAttributes)
			{
				if (item.StateMachineType == declaringType)
				{
					method = methodInfo;
					declaringType = methodInfo.DeclaringType;
					return item is IteratorStateMachineAttribute;
				}
			}
		}
		return false;
	}

	private static bool HasStackTraceHiddenAttribute(MemberInfo memberInfo)
	{
		IList<CustomAttributeData> customAttributesData;
		try
		{
			customAttributesData = memberInfo.GetCustomAttributesData();
		}
		catch
		{
			return false;
		}
		for (int i = 0; i < customAttributesData.Count; i++)
		{
			if (customAttributesData[i].AttributeType.Name == "StackTraceHiddenAttribute")
			{
				return true;
			}
		}
		return false;
	}
}
