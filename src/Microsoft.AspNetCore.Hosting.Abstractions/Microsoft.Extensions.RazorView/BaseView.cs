using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.RazorViews;

internal abstract class BaseView
{
	private static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private readonly Stack<TextWriter> _textWriterStack = new Stack<TextWriter>();

	protected HttpContext Context { get; private set; }

	protected HttpRequest Request { get; private set; }

	protected HttpResponse Response { get; private set; }

	protected TextWriter Output { get; private set; }

	protected HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;


	protected UrlEncoder UrlEncoder { get; set; } = UrlEncoder.Default;


	protected JavaScriptEncoder JavaScriptEncoder { get; set; } = JavaScriptEncoder.Default;


	private List<string> AttributeValues { get; set; }

	private string AttributeEnding { get; set; }

	public async Task ExecuteAsync(HttpContext context)
	{
		Context = context;
		Request = Context.Request;
		Response = Context.Response;
		Output = new StreamWriter(Response.Body, UTF8NoBOM, 4096, leaveOpen: true);
		await ExecuteAsync();
		Output.Dispose();
	}

	public abstract Task ExecuteAsync();

	protected virtual void PushWriter(TextWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		_textWriterStack.Push(Output);
		Output = writer;
	}

	protected virtual TextWriter PopWriter()
	{
		Output = _textWriterStack.Pop();
		return Output;
	}

	protected void WriteLiteral(object value)
	{
		WriteLiteral(Convert.ToString(value, CultureInfo.InvariantCulture));
	}

	protected void WriteLiteral(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			Output.Write(value);
		}
	}

	protected void WriteAttributeValue(string thingy, int startPostion, object value, int endValue, int dealyo, bool yesno)
	{
		if (AttributeValues == null)
		{
			AttributeValues = new List<string>();
		}
		AttributeValues.Add(value.ToString());
	}

	protected void BeginWriteAttribute(string name, string begining, int startPosition, string ending, int endPosition, int thingy)
	{
		Output.Write(begining);
		AttributeEnding = ending;
	}

	protected void EndWriteAttribute()
	{
		string value = string.Join(" ", AttributeValues);
		Output.Write(value);
		AttributeValues = null;
		Output.Write(AttributeEnding);
		AttributeEnding = null;
	}

	protected void WriteAttribute(string name, string leader, string trailer, params AttributeValue[] values)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (leader == null)
		{
			throw new ArgumentNullException("leader");
		}
		if (trailer == null)
		{
			throw new ArgumentNullException("trailer");
		}
		WriteLiteral(leader);
		foreach (AttributeValue attributeValue in values)
		{
			WriteLiteral(attributeValue.Prefix);
			string text;
			if (attributeValue.Value is bool)
			{
				if (!(bool)attributeValue.Value)
				{
					continue;
				}
				text = name;
			}
			else
			{
				text = attributeValue.Value as string;
			}
			if (attributeValue.Literal && text != null)
			{
				WriteLiteral(text);
			}
			else if (attributeValue.Literal)
			{
				WriteLiteral(attributeValue.Value);
			}
			else if (text != null)
			{
				Write(text);
			}
			else
			{
				Write(attributeValue.Value);
			}
		}
		WriteLiteral(trailer);
	}

	protected void Write(HelperResult result)
	{
		Write(result);
	}

	protected void Write(object value)
	{
		if (value is HelperResult helperResult)
		{
			helperResult.WriteTo(Output);
		}
		else
		{
			Write(Convert.ToString(value, CultureInfo.InvariantCulture));
		}
	}

	protected void Write(string value)
	{
		WriteLiteral(HtmlEncoder.Encode(value));
	}

	protected string HtmlEncodeAndReplaceLineBreaks(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}
		return string.Join("<br />" + Environment.NewLine, input.Split(new string[1] { "\r\n" }, StringSplitOptions.None).SelectMany((string s) => s.Split(new char[2] { '\r', '\n' }, StringSplitOptions.None)).Select(HtmlEncoder.Encode));
	}
}
