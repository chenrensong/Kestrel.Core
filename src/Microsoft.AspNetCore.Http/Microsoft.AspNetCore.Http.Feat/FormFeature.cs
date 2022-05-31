using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http.Features;

public class FormFeature : IFormFeature
{
	private static readonly FormOptions DefaultFormOptions = new FormOptions();

	private readonly HttpRequest _request;

	private readonly FormOptions _options;

	private Task<IFormCollection> _parsedFormTask;

	private IFormCollection _form;

	private MediaTypeHeaderValue ContentType
	{
		get
		{
			MediaTypeHeaderValue.TryParse(_request.ContentType, out var parsedValue);
			return parsedValue;
		}
	}

	public bool HasFormContentType
	{
		get
		{
			if (Form != null)
			{
				return true;
			}
			MediaTypeHeaderValue contentType = ContentType;
			if (!HasApplicationFormContentType(contentType))
			{
				return HasMultipartFormContentType(contentType);
			}
			return true;
		}
	}

	public IFormCollection Form
	{
		get
		{
			return _form;
		}
		set
		{
			_parsedFormTask = null;
			_form = value;
		}
	}

	public FormFeature(IFormCollection form)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		Form = form;
	}

	public FormFeature(HttpRequest request)
		: this(request, DefaultFormOptions)
	{
	}

	public FormFeature(HttpRequest request, FormOptions options)
	{
		if (request == null)
		{
			throw new ArgumentNullException("request");
		}
		if (options == null)
		{
			throw new ArgumentNullException("options");
		}
		_request = request;
		_options = options;
	}

	public IFormCollection ReadForm()
	{
		if (Form != null)
		{
			return Form;
		}
		if (!HasFormContentType)
		{
			throw new InvalidOperationException("Incorrect Content-Type: " + _request.ContentType);
		}
		return ReadFormAsync().GetAwaiter().GetResult();
	}

	public Task<IFormCollection> ReadFormAsync()
	{
		return ReadFormAsync(CancellationToken.None);
	}

	public Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken)
	{
		if (_parsedFormTask == null)
		{
			if (Form != null)
			{
				_parsedFormTask = Task.FromResult(Form);
			}
			else
			{
				_parsedFormTask = InnerReadFormAsync(cancellationToken);
			}
		}
		return _parsedFormTask;
	}

	private async Task<IFormCollection> InnerReadFormAsync(CancellationToken cancellationToken)
	{
		if (!HasFormContentType)
		{
			throw new InvalidOperationException("Incorrect Content-Type: " + _request.ContentType);
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (_request.ContentLength == 0)
		{
			return FormCollection.Empty;
		}
		if (_options.BufferBody)
		{
			_request.EnableRewind(_options.MemoryBufferThreshold, _options.BufferBodyLengthLimit);
		}
		FormCollection formFields = null;
		FormFileCollection files = null;
		using (cancellationToken.Register(delegate(object state)
		{
			((HttpContext)state).Abort();
		}, _request.HttpContext))
		{
			MediaTypeHeaderValue contentType = ContentType;
			if (HasApplicationFormContentType(contentType))
			{
				Encoding encoding = FilterEncoding(contentType.Encoding);
				using FormReader formReader = new FormReader(_request.Body, encoding)
				{
					ValueCountLimit = _options.ValueCountLimit,
					KeyLengthLimit = _options.KeyLengthLimit,
					ValueLengthLimit = _options.ValueLengthLimit
				};
				formFields = new FormCollection(await formReader.ReadFormAsync(cancellationToken));
			}
			else if (HasMultipartFormContentType(contentType))
			{
				KeyValueAccumulator formAccumulator = default(KeyValueAccumulator);
				string boundary = GetBoundary(contentType, _options.MultipartBoundaryLengthLimit);
				MultipartReader multipartReader = new MultipartReader(boundary, _request.Body)
				{
					HeadersCountLimit = _options.MultipartHeadersCountLimit,
					HeadersLengthLimit = _options.MultipartHeadersLengthLimit,
					BodyLengthLimit = _options.MultipartBodyLengthLimit
				};
				for (MultipartSection section = await multipartReader.ReadNextSectionAsync(cancellationToken); section != null; section = await multipartReader.ReadNextSectionAsync(cancellationToken))
				{
					ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var parsedValue);
					if (parsedValue.IsFileDisposition())
					{
						FileMultipartSection fileSection = new FileMultipartSection(section, parsedValue);
						section.EnableRewind(_request.HttpContext.Response.RegisterForDispose, _options.MemoryBufferThreshold, _options.MultipartBodyLengthLimit);
						await section.Body.DrainAsync(cancellationToken);
						string name = fileSection.Name;
						string fileName = fileSection.FileName;
						FormFile formFile = ((!section.BaseStreamOffset.HasValue) ? new FormFile(section.Body, 0L, section.Body.Length, name, fileName) : new FormFile(_request.Body, section.BaseStreamOffset.Value, section.Body.Length, name, fileName));
						formFile.Headers = new HeaderDictionary(section.Headers);
						if (files == null)
						{
							files = new FormFileCollection();
						}
						if (files.Count >= _options.ValueCountLimit)
						{
							throw new InvalidDataException($"Form value count limit {_options.ValueCountLimit} exceeded.");
						}
						files.Add(formFile);
					}
					else if (parsedValue.IsFormDisposition())
					{
						FormMultipartSection formMultipartSection = new FormMultipartSection(section, parsedValue);
						string key = formMultipartSection.Name;
						formAccumulator.Append(key, await formMultipartSection.GetValueAsync());
						if (formAccumulator.ValueCount > _options.ValueCountLimit)
						{
							throw new InvalidDataException($"Form value count limit {_options.ValueCountLimit} exceeded.");
						}
					}
				}
				if (formAccumulator.HasValues)
				{
					formFields = new FormCollection(formAccumulator.GetResults(), files);
				}
			}
		}
		if (_request.Body.CanSeek)
		{
			_request.Body.Seek(0L, SeekOrigin.Begin);
		}
		if (formFields != null)
		{
			Form = formFields;
		}
		else if (files != null)
		{
			Form = new FormCollection(null, files);
		}
		else
		{
			Form = FormCollection.Empty;
		}
		return Form;
	}

	private Encoding FilterEncoding(Encoding encoding)
	{
		if (encoding == null || Encoding.UTF7.Equals(encoding))
		{
			return Encoding.UTF8;
		}
		return encoding;
	}

	private bool HasApplicationFormContentType(MediaTypeHeaderValue contentType)
	{
		return contentType?.MediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ?? false;
	}

	private bool HasMultipartFormContentType(MediaTypeHeaderValue contentType)
	{
		return contentType?.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase) ?? false;
	}

	private bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
	{
		if (contentDisposition != null && contentDisposition.DispositionType.Equals("form-data") && StringSegment.IsNullOrEmpty(contentDisposition.FileName))
		{
			return StringSegment.IsNullOrEmpty(contentDisposition.FileNameStar);
		}
		return false;
	}

	private bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
	{
		if (contentDisposition != null && contentDisposition.DispositionType.Equals("form-data"))
		{
			if (StringSegment.IsNullOrEmpty(contentDisposition.FileName))
			{
				return !StringSegment.IsNullOrEmpty(contentDisposition.FileNameStar);
			}
			return true;
		}
		return false;
	}

	private static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
	{
		StringSegment value = HeaderUtilities.RemoveQuotes(contentType.Boundary);
		if (StringSegment.IsNullOrEmpty(value))
		{
			throw new InvalidDataException("Missing content-type boundary.");
		}
		if (value.Length > lengthLimit)
		{
			throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
		}
		return value.ToString();
	}
}
