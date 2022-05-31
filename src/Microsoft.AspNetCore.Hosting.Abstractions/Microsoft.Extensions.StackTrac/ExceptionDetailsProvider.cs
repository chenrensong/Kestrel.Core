using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.Extensions.StackTrace.Sources;

internal class ExceptionDetailsProvider
{
	private readonly IFileProvider _fileProvider;

	private readonly int _sourceCodeLineCount;

	public ExceptionDetailsProvider(IFileProvider fileProvider, int sourceCodeLineCount)
	{
		_fileProvider = fileProvider;
		_sourceCodeLineCount = sourceCodeLineCount;
	}

	public IEnumerable<ExceptionDetails> GetDetails(Exception exception)
	{
		IEnumerable<Exception> enumerable = FlattenAndReverseExceptionTree(exception);
		foreach (Exception item in enumerable)
		{
			yield return new ExceptionDetails
			{
				Error = item,
				StackFrames = from frame in StackTraceHelper.GetFrames(item)
					select GetStackFrameSourceCodeInfo(frame.MethodDisplayInfo.ToString(), frame.FilePath, frame.LineNumber)
			};
		}
	}

	private static IEnumerable<Exception> FlattenAndReverseExceptionTree(Exception ex)
	{
		if (ex is ReflectionTypeLoadException ex2)
		{
			List<Exception> list = new List<Exception>();
			Exception[] loaderExceptions = ex2.LoaderExceptions;
			foreach (Exception ex3 in loaderExceptions)
			{
				list.AddRange(FlattenAndReverseExceptionTree(ex3));
			}
			list.Add(ex);
			return list;
		}
		List<Exception> list2 = new List<Exception>();
		if (ex is AggregateException ex4)
		{
			list2.Add(ex);
			{
				foreach (Exception innerException in ex4.Flatten().InnerExceptions)
				{
					list2.Add(innerException);
				}
				return list2;
			}
		}
		while (ex != null)
		{
			list2.Add(ex);
			ex = ex.InnerException;
		}
		list2.Reverse();
		return list2;
	}

	internal StackFrameSourceCodeInfo GetStackFrameSourceCodeInfo(string method, string filePath, int lineNumber)
	{
		StackFrameSourceCodeInfo stackFrameSourceCodeInfo = new StackFrameSourceCodeInfo
		{
			Function = method,
			File = filePath,
			Line = lineNumber
		};
		if (string.IsNullOrEmpty(stackFrameSourceCodeInfo.File))
		{
			return stackFrameSourceCodeInfo;
		}
		IEnumerable<string> enumerable = null;
		if (File.Exists(stackFrameSourceCodeInfo.File))
		{
			enumerable = File.ReadLines(stackFrameSourceCodeInfo.File);
		}
		else
		{
			IFileInfo fileInfo = _fileProvider.GetFileInfo(stackFrameSourceCodeInfo.File);
			if (fileInfo.Exists)
			{
				enumerable = (string.IsNullOrEmpty(fileInfo.PhysicalPath) ? ReadLines(fileInfo) : File.ReadLines(fileInfo.PhysicalPath));
			}
		}
		if (enumerable != null)
		{
			ReadFrameContent(stackFrameSourceCodeInfo, enumerable, stackFrameSourceCodeInfo.Line, stackFrameSourceCodeInfo.Line);
		}
		return stackFrameSourceCodeInfo;
	}

	internal void ReadFrameContent(StackFrameSourceCodeInfo frame, IEnumerable<string> allLines, int errorStartLineNumberInFile, int errorEndLineNumberInFile)
	{
		int num = Math.Max(errorStartLineNumberInFile - _sourceCodeLineCount, 1);
		int num2 = errorEndLineNumberInFile + _sourceCodeLineCount;
		string[] source = allLines.Skip(num - 1).Take(num2 - num + 1).ToArray();
		int num3 = errorEndLineNumberInFile - errorStartLineNumberInFile + 1;
		int num4 = errorStartLineNumberInFile - num;
		frame.PreContextLine = num;
		frame.PreContextCode = source.Take(num4).ToArray();
		frame.ContextCode = source.Skip(num4).Take(num3).ToArray();
		frame.PostContextCode = source.Skip(num4 + num3).ToArray();
	}

	private static IEnumerable<string> ReadLines(IFileInfo fileInfo)
	{
		using StreamReader reader = new StreamReader(fileInfo.CreateReadStream());
		string text;
		while ((text = reader.ReadLine()) != null)
		{
			yield return text;
		}
	}
}
