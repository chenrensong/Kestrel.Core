// Microsoft.AspNetCore.Server.Kestrel.Core.CoreStrings
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.AspNetCore.Server.Kestrel.Core;


internal static class CoreStrings
{
    private static readonly ResourceManager _resourceManager =
        new ResourceManager("CoreStrings",
        typeof(CoreStrings).GetTypeInfo().Assembly);

    internal static string BadRequest => GetString("BadRequest");

    internal static string BadRequest_BadChunkSizeData => GetString("BadRequest_BadChunkSizeData");

    internal static string BadRequest_BadChunkSuffix => GetString("BadRequest_BadChunkSuffix");

    internal static string BadRequest_ChunkedRequestIncomplete => GetString("BadRequest_ChunkedRequestIncomplete");

    internal static string BadRequest_FinalTransferCodingNotChunked => GetString("BadRequest_FinalTransferCodingNotChunked");

    internal static string BadRequest_HeadersExceedMaxTotalSize => GetString("BadRequest_HeadersExceedMaxTotalSize");

    internal static string BadRequest_InvalidCharactersInHeaderName => GetString("BadRequest_InvalidCharactersInHeaderName");

    internal static string BadRequest_InvalidContentLength_Detail => GetString("BadRequest_InvalidContentLength_Detail");

    internal static string BadRequest_InvalidHostHeader => GetString("BadRequest_InvalidHostHeader");

    internal static string BadRequest_InvalidHostHeader_Detail => GetString("BadRequest_InvalidHostHeader_Detail");

    internal static string BadRequest_InvalidRequestHeadersNoCRLF => GetString("BadRequest_InvalidRequestHeadersNoCRLF");

    internal static string BadRequest_InvalidRequestHeader_Detail => GetString("BadRequest_InvalidRequestHeader_Detail");

    internal static string BadRequest_InvalidRequestLine => GetString("BadRequest_InvalidRequestLine");

    internal static string BadRequest_InvalidRequestLine_Detail => GetString("BadRequest_InvalidRequestLine_Detail");

    internal static string BadRequest_InvalidRequestTarget_Detail => GetString("BadRequest_InvalidRequestTarget_Detail");

    internal static string BadRequest_LengthRequired => GetString("BadRequest_LengthRequired");

    internal static string BadRequest_LengthRequiredHttp10 => GetString("BadRequest_LengthRequiredHttp10");

    internal static string BadRequest_MalformedRequestInvalidHeaders => GetString("BadRequest_MalformedRequestInvalidHeaders");

    internal static string BadRequest_MethodNotAllowed => GetString("BadRequest_MethodNotAllowed");

    internal static string BadRequest_MissingHostHeader => GetString("BadRequest_MissingHostHeader");

    internal static string BadRequest_MultipleContentLengths => GetString("BadRequest_MultipleContentLengths");

    internal static string BadRequest_MultipleHostHeaders => GetString("BadRequest_MultipleHostHeaders");

    internal static string BadRequest_RequestLineTooLong => GetString("BadRequest_RequestLineTooLong");

    internal static string BadRequest_RequestHeadersTimeout => GetString("BadRequest_RequestHeadersTimeout");

    internal static string BadRequest_TooManyHeaders => GetString("BadRequest_TooManyHeaders");

    internal static string BadRequest_UnexpectedEndOfRequestContent => GetString("BadRequest_UnexpectedEndOfRequestContent");

    internal static string BadRequest_UnrecognizedHTTPVersion => GetString("BadRequest_UnrecognizedHTTPVersion");

    internal static string BadRequest_UpgradeRequestCannotHavePayload => GetString("BadRequest_UpgradeRequestCannotHavePayload");

    internal static string FallbackToIPv4Any => GetString("FallbackToIPv4Any");

    internal static string ResponseStreamWasUpgraded => GetString("ResponseStreamWasUpgraded");

    internal static string BigEndianNotSupported => GetString("BigEndianNotSupported");

    internal static string MaxRequestBufferSmallerThanRequestHeaderBuffer => GetString("MaxRequestBufferSmallerThanRequestHeaderBuffer");

    internal static string MaxRequestBufferSmallerThanRequestLineBuffer => GetString("MaxRequestBufferSmallerThanRequestLineBuffer");

    internal static string ServerAlreadyStarted => GetString("ServerAlreadyStarted");

    internal static string UnknownTransportMode => GetString("UnknownTransportMode");

    internal static string InvalidAsciiOrControlChar => GetString("InvalidAsciiOrControlChar");

    internal static string InvalidContentLength_InvalidNumber => GetString("InvalidContentLength_InvalidNumber");

    internal static string NonNegativeNumberOrNullRequired => GetString("NonNegativeNumberOrNullRequired");

    internal static string NonNegativeNumberRequired => GetString("NonNegativeNumberRequired");

    internal static string PositiveNumberRequired => GetString("PositiveNumberRequired");

    internal static string PositiveNumberOrNullRequired => GetString("PositiveNumberOrNullRequired");

    internal static string UnixSocketPathMustBeAbsolute => GetString("UnixSocketPathMustBeAbsolute");

    internal static string AddressBindingFailed => GetString("AddressBindingFailed");

    internal static string BindingToDefaultAddress => GetString("BindingToDefaultAddress");

    internal static string ConfigureHttpsFromMethodCall => GetString("ConfigureHttpsFromMethodCall");

    internal static string ConfigurePathBaseFromMethodCall => GetString("ConfigurePathBaseFromMethodCall");

    internal static string DynamicPortOnLocalhostNotSupported => GetString("DynamicPortOnLocalhostNotSupported");

    internal static string EndpointAlreadyInUse => GetString("EndpointAlreadyInUse");

    internal static string InvalidUrl => GetString("InvalidUrl");

    internal static string NetworkInterfaceBindingFailed => GetString("NetworkInterfaceBindingFailed");

    internal static string OverridingWithKestrelOptions => GetString("OverridingWithKestrelOptions");

    internal static string OverridingWithPreferHostingUrls => GetString("OverridingWithPreferHostingUrls");

    internal static string UnsupportedAddressScheme => GetString("UnsupportedAddressScheme");

    internal static string HeadersAreReadOnly => GetString("HeadersAreReadOnly");

    internal static string KeyAlreadyExists => GetString("KeyAlreadyExists");

    internal static string HeaderNotAllowedOnResponse => GetString("HeaderNotAllowedOnResponse");

    internal static string ParameterReadOnlyAfterResponseStarted => GetString("ParameterReadOnlyAfterResponseStarted");

    internal static string RequestProcessingAborted => GetString("RequestProcessingAborted");

    internal static string TooFewBytesWritten => GetString("TooFewBytesWritten");

    internal static string TooManyBytesWritten => GetString("TooManyBytesWritten");

    internal static string UnhandledApplicationException => GetString("UnhandledApplicationException");

    internal static string WritingToResponseBodyNotSupported => GetString("WritingToResponseBodyNotSupported");

    internal static string ConnectionShutdownError => GetString("ConnectionShutdownError");

    internal static string RequestProcessingEndError => GetString("RequestProcessingEndError");

    internal static string CannotUpgradeNonUpgradableRequest => GetString("CannotUpgradeNonUpgradableRequest");

    internal static string UpgradedConnectionLimitReached => GetString("UpgradedConnectionLimitReached");

    internal static string UpgradeCannotBeCalledMultipleTimes => GetString("UpgradeCannotBeCalledMultipleTimes");

    internal static string BadRequest_RequestBodyTooLarge => GetString("BadRequest_RequestBodyTooLarge");

    internal static string MaxRequestBodySizeCannotBeModifiedAfterRead => GetString("MaxRequestBodySizeCannotBeModifiedAfterRead");

    internal static string MaxRequestBodySizeCannotBeModifiedForUpgradedRequests => GetString("MaxRequestBodySizeCannotBeModifiedForUpgradedRequests");

    internal static string PositiveTimeSpanRequired => GetString("PositiveTimeSpanRequired");

    internal static string NonNegativeTimeSpanRequired => GetString("NonNegativeTimeSpanRequired");

    internal static string MinimumGracePeriodRequired => GetString("MinimumGracePeriodRequired");

    internal static string SynchronousReadsDisallowed => GetString("SynchronousReadsDisallowed");

    internal static string SynchronousWritesDisallowed => GetString("SynchronousWritesDisallowed");

    internal static string PositiveNumberOrNullMinDataRateRequired => GetString("PositiveNumberOrNullMinDataRateRequired");

    internal static string ConcurrentTimeoutsNotSupported => GetString("ConcurrentTimeoutsNotSupported");

    internal static string PositiveFiniteTimeSpanRequired => GetString("PositiveFiniteTimeSpanRequired");

    internal static string EndPointRequiresAtLeastOneProtocol => GetString("EndPointRequiresAtLeastOneProtocol");

    internal static string EndPointHttp2NotNegotiated => GetString("EndPointHttp2NotNegotiated");

    internal static string HPackErrorDynamicTableSizeUpdateTooLarge => GetString("HPackErrorDynamicTableSizeUpdateTooLarge");

    internal static string HPackErrorIndexOutOfRange => GetString("HPackErrorIndexOutOfRange");

    internal static string HPackHuffmanErrorIncomplete => GetString("HPackHuffmanErrorIncomplete");

    internal static string HPackHuffmanErrorEOS => GetString("HPackHuffmanErrorEOS");

    internal static string HPackHuffmanErrorDestinationTooSmall => GetString("HPackHuffmanErrorDestinationTooSmall");

    internal static string HPackHuffmanError => GetString("HPackHuffmanError");

    internal static string HPackStringLengthTooLarge => GetString("HPackStringLengthTooLarge");

    internal static string HPackErrorIncompleteHeaderBlock => GetString("HPackErrorIncompleteHeaderBlock");

    internal static string Http2ErrorStreamIdEven => GetString("Http2ErrorStreamIdEven");

    internal static string Http2ErrorPushPromiseReceived => GetString("Http2ErrorPushPromiseReceived");

    internal static string Http2ErrorHeadersInterleaved => GetString("Http2ErrorHeadersInterleaved");

    internal static string Http2ErrorStreamIdZero => GetString("Http2ErrorStreamIdZero");

    internal static string Http2ErrorStreamIdNotZero => GetString("Http2ErrorStreamIdNotZero");

    internal static string Http2ErrorPaddingTooLong => GetString("Http2ErrorPaddingTooLong");

    internal static string Http2ErrorStreamClosed => GetString("Http2ErrorStreamClosed");

    internal static string Http2ErrorStreamHalfClosedRemote => GetString("Http2ErrorStreamHalfClosedRemote");

    internal static string Http2ErrorStreamSelfDependency => GetString("Http2ErrorStreamSelfDependency");

    internal static string Http2ErrorUnexpectedFrameLength => GetString("Http2ErrorUnexpectedFrameLength");

    internal static string Http2ErrorSettingsLengthNotMultipleOfSix => GetString("Http2ErrorSettingsLengthNotMultipleOfSix");

    internal static string Http2ErrorSettingsAckLengthNotZero => GetString("Http2ErrorSettingsAckLengthNotZero");

    internal static string Http2ErrorSettingsParameterOutOfRange => GetString("Http2ErrorSettingsParameterOutOfRange");

    internal static string Http2ErrorWindowUpdateIncrementZero => GetString("Http2ErrorWindowUpdateIncrementZero");

    internal static string Http2ErrorContinuationWithNoHeaders => GetString("Http2ErrorContinuationWithNoHeaders");

    internal static string Http2ErrorStreamIdle => GetString("Http2ErrorStreamIdle");

    internal static string Http2ErrorTrailersContainPseudoHeaderField => GetString("Http2ErrorTrailersContainPseudoHeaderField");

    internal static string Http2ErrorHeaderNameUppercase => GetString("Http2ErrorHeaderNameUppercase");

    internal static string Http2ErrorTrailerNameUppercase => GetString("Http2ErrorTrailerNameUppercase");

    internal static string Http2ErrorHeadersWithTrailersNoEndStream => GetString("Http2ErrorHeadersWithTrailersNoEndStream");

    internal static string Http2ErrorMissingMandatoryPseudoHeaderFields => GetString("Http2ErrorMissingMandatoryPseudoHeaderFields");

    internal static string Http2ErrorPseudoHeaderFieldAfterRegularHeaders => GetString("Http2ErrorPseudoHeaderFieldAfterRegularHeaders");

    internal static string Http2ErrorUnknownPseudoHeaderField => GetString("Http2ErrorUnknownPseudoHeaderField");

    internal static string Http2ErrorResponsePseudoHeaderField => GetString("Http2ErrorResponsePseudoHeaderField");

    internal static string Http2ErrorDuplicatePseudoHeaderField => GetString("Http2ErrorDuplicatePseudoHeaderField");

    internal static string Http2ErrorConnectionSpecificHeaderField => GetString("Http2ErrorConnectionSpecificHeaderField");

    internal static string UnableToConfigureHttpsBindings => GetString("UnableToConfigureHttpsBindings");

    internal static string AuthenticationFailed => GetString("AuthenticationFailed");

    internal static string AuthenticationTimedOut => GetString("AuthenticationTimedOut");

    internal static string InvalidServerCertificateEku => GetString("InvalidServerCertificateEku");

    internal static string PositiveTimeSpanRequired1 => GetString("PositiveTimeSpanRequired1");

    internal static string ServerCertificateRequired => GetString("ServerCertificateRequired");

    internal static string BindingToDefaultAddresses => GetString("BindingToDefaultAddresses");

    internal static string CertNotFoundInStore => GetString("CertNotFoundInStore");

    internal static string EndpointMissingUrl => GetString("EndpointMissingUrl");

    internal static string NoCertSpecifiedNoDevelopmentCertificateFound => GetString("NoCertSpecifiedNoDevelopmentCertificateFound");

    internal static string MultipleCertificateSources => GetString("MultipleCertificateSources");

    internal static string WritingToResponseBodyAfterResponseCompleted => GetString("WritingToResponseBodyAfterResponseCompleted");

    internal static string BadRequest_RequestBodyTimeout => GetString("BadRequest_RequestBodyTimeout");

    internal static string ConnectionAbortedByApplication => GetString("ConnectionAbortedByApplication");

    internal static string ConnectionAbortedDuringServerShutdown => GetString("ConnectionAbortedDuringServerShutdown");

    internal static string ConnectionTimedBecauseResponseMininumDataRateNotSatisfied => GetString("ConnectionTimedBecauseResponseMininumDataRateNotSatisfied");

    internal static string ConnectionTimedOutByServer => GetString("ConnectionTimedOutByServer");

    internal static string Http2ErrorFrameOverLimit => GetString("Http2ErrorFrameOverLimit");

    internal static string Http2ErrorMinTlsVersion => GetString("Http2ErrorMinTlsVersion");

    internal static string Http2ErrorInvalidPreface => GetString("Http2ErrorInvalidPreface");

    internal static string InvalidEmptyHeaderName => GetString("InvalidEmptyHeaderName");

    internal static string ConnectionOrStreamAbortedByCancellationToken => GetString("ConnectionOrStreamAbortedByCancellationToken");

    internal static string Http2ErrorInitialWindowSizeInvalid => GetString("Http2ErrorInitialWindowSizeInvalid");

    internal static string Http2ErrorWindowUpdateSizeInvalid => GetString("Http2ErrorWindowUpdateSizeInvalid");

    internal static string Http2ConnectionFaulted => GetString("Http2ConnectionFaulted");

    internal static string Http2StreamResetByClient => GetString("Http2StreamResetByClient");

    internal static string Http2StreamAborted => GetString("Http2StreamAborted");

    internal static string Http2ErrorFlowControlWindowExceeded => GetString("Http2ErrorFlowControlWindowExceeded");

    internal static string Http2ErrorConnectMustNotSendSchemeOrPath => GetString("Http2ErrorConnectMustNotSendSchemeOrPath");

    internal static string Http2ErrorMethodInvalid => GetString("Http2ErrorMethodInvalid");

    internal static string Http2StreamErrorPathInvalid => GetString("Http2StreamErrorPathInvalid");

    internal static string Http2StreamErrorSchemeMismatch => GetString("Http2StreamErrorSchemeMismatch");

    internal static string Http2StreamErrorLessDataThanLength => GetString("Http2StreamErrorLessDataThanLength");

    internal static string Http2StreamErrorMoreDataThanLength => GetString("Http2StreamErrorMoreDataThanLength");

    internal static string Http2StreamErrorAfterHeaders => GetString("Http2StreamErrorAfterHeaders");

    internal static string Http2ErrorMaxStreams => GetString("Http2ErrorMaxStreams");

    internal static string GreaterThanZeroRequired => GetString("GreaterThanZeroRequired");

    internal static string ArgumentOutOfRange => GetString("ArgumentOutOfRange");

    internal static string HPackErrorDynamicTableSizeUpdateNotAtBeginningOfHeaderBlock => GetString("HPackErrorDynamicTableSizeUpdateNotAtBeginningOfHeaderBlock");

    internal static string HPackErrorNotEnoughBuffer => GetString("HPackErrorNotEnoughBuffer");

    internal static string HPackErrorIntegerTooBig => GetString("HPackErrorIntegerTooBig");

    internal static string ConnectionAbortedByClient => GetString("ConnectionAbortedByClient");

    internal static string Http2ErrorStreamAborted => GetString("Http2ErrorStreamAborted");

    internal static string FormatBadRequest()
    {
        return GetString("BadRequest");
    }

    internal static string FormatBadRequest_BadChunkSizeData()
    {
        return GetString("BadRequest_BadChunkSizeData");
    }

    internal static string FormatBadRequest_BadChunkSuffix()
    {
        return GetString("BadRequest_BadChunkSuffix");
    }

    internal static string FormatBadRequest_ChunkedRequestIncomplete()
    {
        return GetString("BadRequest_ChunkedRequestIncomplete");
    }

    internal static string FormatBadRequest_FinalTransferCodingNotChunked(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_FinalTransferCodingNotChunked", "detail"), detail);
    }

    internal static string FormatBadRequest_HeadersExceedMaxTotalSize()
    {
        return GetString("BadRequest_HeadersExceedMaxTotalSize");
    }

    internal static string FormatBadRequest_InvalidCharactersInHeaderName()
    {
        return GetString("BadRequest_InvalidCharactersInHeaderName");
    }

    internal static string FormatBadRequest_InvalidContentLength_Detail(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_InvalidContentLength_Detail", "detail"), detail);
    }

    internal static string FormatBadRequest_InvalidHostHeader()
    {
        return GetString("BadRequest_InvalidHostHeader");
    }

    internal static string FormatBadRequest_InvalidHostHeader_Detail(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_InvalidHostHeader_Detail", "detail"), detail);
    }

    internal static string FormatBadRequest_InvalidRequestHeadersNoCRLF()
    {
        return GetString("BadRequest_InvalidRequestHeadersNoCRLF");
    }

    internal static string FormatBadRequest_InvalidRequestHeader_Detail(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_InvalidRequestHeader_Detail", "detail"), detail);
    }

    internal static string FormatBadRequest_InvalidRequestLine()
    {
        return GetString("BadRequest_InvalidRequestLine");
    }

    internal static string FormatBadRequest_InvalidRequestLine_Detail(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_InvalidRequestLine_Detail", "detail"), detail);
    }

    internal static string FormatBadRequest_InvalidRequestTarget_Detail(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_InvalidRequestTarget_Detail", "detail"), detail);
    }

    internal static string FormatBadRequest_LengthRequired(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_LengthRequired", "detail"), detail);
    }

    internal static string FormatBadRequest_LengthRequiredHttp10(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_LengthRequiredHttp10", "detail"), detail);
    }

    internal static string FormatBadRequest_MalformedRequestInvalidHeaders()
    {
        return GetString("BadRequest_MalformedRequestInvalidHeaders");
    }

    internal static string FormatBadRequest_MethodNotAllowed()
    {
        return GetString("BadRequest_MethodNotAllowed");
    }

    internal static string FormatBadRequest_MissingHostHeader()
    {
        return GetString("BadRequest_MissingHostHeader");
    }

    internal static string FormatBadRequest_MultipleContentLengths()
    {
        return GetString("BadRequest_MultipleContentLengths");
    }

    internal static string FormatBadRequest_MultipleHostHeaders()
    {
        return GetString("BadRequest_MultipleHostHeaders");
    }

    internal static string FormatBadRequest_RequestLineTooLong()
    {
        return GetString("BadRequest_RequestLineTooLong");
    }

    internal static string FormatBadRequest_RequestHeadersTimeout()
    {
        return GetString("BadRequest_RequestHeadersTimeout");
    }

    internal static string FormatBadRequest_TooManyHeaders()
    {
        return GetString("BadRequest_TooManyHeaders");
    }

    internal static string FormatBadRequest_UnexpectedEndOfRequestContent()
    {
        return GetString("BadRequest_UnexpectedEndOfRequestContent");
    }

    internal static string FormatBadRequest_UnrecognizedHTTPVersion(object detail)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BadRequest_UnrecognizedHTTPVersion", "detail"), detail);
    }

    internal static string FormatBadRequest_UpgradeRequestCannotHavePayload()
    {
        return GetString("BadRequest_UpgradeRequestCannotHavePayload");
    }

    internal static string FormatFallbackToIPv4Any(object port)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("FallbackToIPv4Any", "port"), port);
    }

    internal static string FormatResponseStreamWasUpgraded()
    {
        return GetString("ResponseStreamWasUpgraded");
    }

    internal static string FormatBigEndianNotSupported()
    {
        return GetString("BigEndianNotSupported");
    }

    internal static string FormatMaxRequestBufferSmallerThanRequestHeaderBuffer(object requestBufferSize, object requestHeaderSize)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("MaxRequestBufferSmallerThanRequestHeaderBuffer", "requestBufferSize", "requestHeaderSize"), requestBufferSize, requestHeaderSize);
    }

    internal static string FormatMaxRequestBufferSmallerThanRequestLineBuffer(object requestBufferSize, object requestLineSize)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("MaxRequestBufferSmallerThanRequestLineBuffer", "requestBufferSize", "requestLineSize"), requestBufferSize, requestLineSize);
    }

    internal static string FormatServerAlreadyStarted()
    {
        return GetString("ServerAlreadyStarted");
    }

    internal static string FormatUnknownTransportMode(object mode)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("UnknownTransportMode", "mode"), mode);
    }

    internal static string FormatInvalidAsciiOrControlChar(object character)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("InvalidAsciiOrControlChar", "character"), character);
    }

    internal static string FormatInvalidContentLength_InvalidNumber(object value)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("InvalidContentLength_InvalidNumber", "value"), value);
    }

    internal static string FormatNonNegativeNumberOrNullRequired()
    {
        return GetString("NonNegativeNumberOrNullRequired");
    }

    internal static string FormatNonNegativeNumberRequired()
    {
        return GetString("NonNegativeNumberRequired");
    }

    internal static string FormatPositiveNumberRequired()
    {
        return GetString("PositiveNumberRequired");
    }

    internal static string FormatPositiveNumberOrNullRequired()
    {
        return GetString("PositiveNumberOrNullRequired");
    }

    internal static string FormatUnixSocketPathMustBeAbsolute()
    {
        return GetString("UnixSocketPathMustBeAbsolute");
    }

    internal static string FormatAddressBindingFailed(object address)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("AddressBindingFailed", "address"), address);
    }

    internal static string FormatBindingToDefaultAddress(object address)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BindingToDefaultAddress", "address"), address);
    }

    internal static string FormatConfigureHttpsFromMethodCall(object methodName)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("ConfigureHttpsFromMethodCall", "methodName"), methodName);
    }

    internal static string FormatConfigurePathBaseFromMethodCall(object methodName)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("ConfigurePathBaseFromMethodCall", "methodName"), methodName);
    }

    internal static string FormatDynamicPortOnLocalhostNotSupported()
    {
        return GetString("DynamicPortOnLocalhostNotSupported");
    }

    internal static string FormatEndpointAlreadyInUse(object endpoint)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("EndpointAlreadyInUse", "endpoint"), endpoint);
    }

    internal static string FormatInvalidUrl(object url)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("InvalidUrl", "url"), url);
    }

    internal static string FormatNetworkInterfaceBindingFailed(object address, object interfaceName, object error)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("NetworkInterfaceBindingFailed", "address", "interfaceName", "error"), address, interfaceName, error);
    }

    internal static string FormatOverridingWithKestrelOptions(object addresses, object methodName)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("OverridingWithKestrelOptions", "addresses", "methodName"), addresses, methodName);
    }

    internal static string FormatOverridingWithPreferHostingUrls(object settingName, object addresses)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("OverridingWithPreferHostingUrls", "settingName", "addresses"), settingName, addresses);
    }

    internal static string FormatUnsupportedAddressScheme(object address)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("UnsupportedAddressScheme", "address"), address);
    }

    internal static string FormatHeadersAreReadOnly()
    {
        return GetString("HeadersAreReadOnly");
    }

    internal static string FormatKeyAlreadyExists()
    {
        return GetString("KeyAlreadyExists");
    }

    internal static string FormatHeaderNotAllowedOnResponse(object name, object statusCode)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("HeaderNotAllowedOnResponse", "name", "statusCode"), name, statusCode);
    }

    internal static string FormatParameterReadOnlyAfterResponseStarted(object name)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("ParameterReadOnlyAfterResponseStarted", "name"), name);
    }

    internal static string FormatRequestProcessingAborted()
    {
        return GetString("RequestProcessingAborted");
    }

    internal static string FormatTooFewBytesWritten(object written, object expected)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("TooFewBytesWritten", "written", "expected"), written, expected);
    }

    internal static string FormatTooManyBytesWritten(object written, object expected)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("TooManyBytesWritten", "written", "expected"), written, expected);
    }

    internal static string FormatUnhandledApplicationException()
    {
        return GetString("UnhandledApplicationException");
    }

    internal static string FormatWritingToResponseBodyNotSupported(object statusCode)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("WritingToResponseBodyNotSupported", "statusCode"), statusCode);
    }

    internal static string FormatConnectionShutdownError()
    {
        return GetString("ConnectionShutdownError");
    }

    internal static string FormatRequestProcessingEndError()
    {
        return GetString("RequestProcessingEndError");
    }

    internal static string FormatCannotUpgradeNonUpgradableRequest()
    {
        return GetString("CannotUpgradeNonUpgradableRequest");
    }

    internal static string FormatUpgradedConnectionLimitReached()
    {
        return GetString("UpgradedConnectionLimitReached");
    }

    internal static string FormatUpgradeCannotBeCalledMultipleTimes()
    {
        return GetString("UpgradeCannotBeCalledMultipleTimes");
    }

    internal static string FormatBadRequest_RequestBodyTooLarge()
    {
        return GetString("BadRequest_RequestBodyTooLarge");
    }

    internal static string FormatMaxRequestBodySizeCannotBeModifiedAfterRead()
    {
        return GetString("MaxRequestBodySizeCannotBeModifiedAfterRead");
    }

    internal static string FormatMaxRequestBodySizeCannotBeModifiedForUpgradedRequests()
    {
        return GetString("MaxRequestBodySizeCannotBeModifiedForUpgradedRequests");
    }

    internal static string FormatPositiveTimeSpanRequired()
    {
        return GetString("PositiveTimeSpanRequired");
    }

    internal static string FormatNonNegativeTimeSpanRequired()
    {
        return GetString("NonNegativeTimeSpanRequired");
    }

    internal static string FormatMinimumGracePeriodRequired(object heartbeatInterval)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("MinimumGracePeriodRequired", "heartbeatInterval"), heartbeatInterval);
    }

    internal static string FormatSynchronousReadsDisallowed()
    {
        return GetString("SynchronousReadsDisallowed");
    }

    internal static string FormatSynchronousWritesDisallowed()
    {
        return GetString("SynchronousWritesDisallowed");
    }

    internal static string FormatPositiveNumberOrNullMinDataRateRequired()
    {
        return GetString("PositiveNumberOrNullMinDataRateRequired");
    }

    internal static string FormatConcurrentTimeoutsNotSupported()
    {
        return GetString("ConcurrentTimeoutsNotSupported");
    }

    internal static string FormatPositiveFiniteTimeSpanRequired()
    {
        return GetString("PositiveFiniteTimeSpanRequired");
    }

    internal static string FormatEndPointRequiresAtLeastOneProtocol()
    {
        return GetString("EndPointRequiresAtLeastOneProtocol");
    }

    internal static string FormatEndPointHttp2NotNegotiated()
    {
        return GetString("EndPointHttp2NotNegotiated");
    }

    internal static string FormatHPackErrorDynamicTableSizeUpdateTooLarge(object size, object maxSize)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("HPackErrorDynamicTableSizeUpdateTooLarge", "size", "maxSize"), size, maxSize);
    }

    internal static string FormatHPackErrorIndexOutOfRange(object index)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("HPackErrorIndexOutOfRange", "index"), index);
    }

    internal static string FormatHPackHuffmanErrorIncomplete()
    {
        return GetString("HPackHuffmanErrorIncomplete");
    }

    internal static string FormatHPackHuffmanErrorEOS()
    {
        return GetString("HPackHuffmanErrorEOS");
    }

    internal static string FormatHPackHuffmanErrorDestinationTooSmall()
    {
        return GetString("HPackHuffmanErrorDestinationTooSmall");
    }

    internal static string FormatHPackHuffmanError()
    {
        return GetString("HPackHuffmanError");
    }

    internal static string FormatHPackStringLengthTooLarge(object length, object maxStringLength)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("HPackStringLengthTooLarge", "length", "maxStringLength"), length, maxStringLength);
    }

    internal static string FormatHPackErrorIncompleteHeaderBlock()
    {
        return GetString("HPackErrorIncompleteHeaderBlock");
    }

    internal static string FormatHttp2ErrorStreamIdEven(object frameType, object streamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamIdEven", "frameType", "streamId"), frameType, streamId);
    }

    internal static string FormatHttp2ErrorPushPromiseReceived()
    {
        return GetString("Http2ErrorPushPromiseReceived");
    }

    internal static string FormatHttp2ErrorHeadersInterleaved(object frameType, object streamId, object headersStreamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorHeadersInterleaved", "frameType", "streamId", "headersStreamId"), frameType, streamId, headersStreamId);
    }

    internal static string FormatHttp2ErrorStreamIdZero(object frameType)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamIdZero", "frameType"), frameType);
    }

    internal static string FormatHttp2ErrorStreamIdNotZero(object frameType)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamIdNotZero", "frameType"), frameType);
    }

    internal static string FormatHttp2ErrorPaddingTooLong(object frameType)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorPaddingTooLong", "frameType"), frameType);
    }

    internal static string FormatHttp2ErrorStreamClosed(object frameType, object streamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamClosed", "frameType", "streamId"), frameType, streamId);
    }

    internal static string FormatHttp2ErrorStreamHalfClosedRemote(object frameType, object streamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamHalfClosedRemote", "frameType", "streamId"), frameType, streamId);
    }

    internal static string FormatHttp2ErrorStreamSelfDependency(object frameType, object streamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamSelfDependency", "frameType", "streamId"), frameType, streamId);
    }

    internal static string FormatHttp2ErrorUnexpectedFrameLength(object frameType, object expectedLength)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorUnexpectedFrameLength", "frameType", "expectedLength"), frameType, expectedLength);
    }

    internal static string FormatHttp2ErrorSettingsLengthNotMultipleOfSix()
    {
        return GetString("Http2ErrorSettingsLengthNotMultipleOfSix");
    }

    internal static string FormatHttp2ErrorSettingsAckLengthNotZero()
    {
        return GetString("Http2ErrorSettingsAckLengthNotZero");
    }

    internal static string FormatHttp2ErrorSettingsParameterOutOfRange(object parameter)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorSettingsParameterOutOfRange", "parameter"), parameter);
    }

    internal static string FormatHttp2ErrorWindowUpdateIncrementZero()
    {
        return GetString("Http2ErrorWindowUpdateIncrementZero");
    }

    internal static string FormatHttp2ErrorContinuationWithNoHeaders()
    {
        return GetString("Http2ErrorContinuationWithNoHeaders");
    }

    internal static string FormatHttp2ErrorStreamIdle(object frameType, object streamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamIdle", "frameType", "streamId"), frameType, streamId);
    }

    internal static string FormatHttp2ErrorTrailersContainPseudoHeaderField()
    {
        return GetString("Http2ErrorTrailersContainPseudoHeaderField");
    }

    internal static string FormatHttp2ErrorHeaderNameUppercase()
    {
        return GetString("Http2ErrorHeaderNameUppercase");
    }

    internal static string FormatHttp2ErrorTrailerNameUppercase()
    {
        return GetString("Http2ErrorTrailerNameUppercase");
    }

    internal static string FormatHttp2ErrorHeadersWithTrailersNoEndStream()
    {
        return GetString("Http2ErrorHeadersWithTrailersNoEndStream");
    }

    internal static string FormatHttp2ErrorMissingMandatoryPseudoHeaderFields()
    {
        return GetString("Http2ErrorMissingMandatoryPseudoHeaderFields");
    }

    internal static string FormatHttp2ErrorPseudoHeaderFieldAfterRegularHeaders()
    {
        return GetString("Http2ErrorPseudoHeaderFieldAfterRegularHeaders");
    }

    internal static string FormatHttp2ErrorUnknownPseudoHeaderField()
    {
        return GetString("Http2ErrorUnknownPseudoHeaderField");
    }

    internal static string FormatHttp2ErrorResponsePseudoHeaderField()
    {
        return GetString("Http2ErrorResponsePseudoHeaderField");
    }

    internal static string FormatHttp2ErrorDuplicatePseudoHeaderField()
    {
        return GetString("Http2ErrorDuplicatePseudoHeaderField");
    }

    internal static string FormatHttp2ErrorConnectionSpecificHeaderField()
    {
        return GetString("Http2ErrorConnectionSpecificHeaderField");
    }

    internal static string FormatUnableToConfigureHttpsBindings()
    {
        return GetString("UnableToConfigureHttpsBindings");
    }

    internal static string FormatAuthenticationFailed()
    {
        return GetString("AuthenticationFailed");
    }

    internal static string FormatAuthenticationTimedOut()
    {
        return GetString("AuthenticationTimedOut");
    }

    internal static string FormatInvalidServerCertificateEku(object thumbprint)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("InvalidServerCertificateEku", "thumbprint"), thumbprint);
    }

    internal static string FormatPositiveTimeSpanRequired1()
    {
        return GetString("PositiveTimeSpanRequired1");
    }

    internal static string FormatServerCertificateRequired()
    {
        return GetString("ServerCertificateRequired");
    }

    internal static string FormatBindingToDefaultAddresses(object address0, object address1)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("BindingToDefaultAddresses", "address0", "address1"), address0, address1);
    }

    internal static string FormatCertNotFoundInStore(object subject, object storeLocation, object storeName, object allowInvalid)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("CertNotFoundInStore", "subject", "storeLocation", "storeName", "allowInvalid"), subject, storeLocation, storeName, allowInvalid);
    }

    internal static string FormatEndpointMissingUrl(object endpointName)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("EndpointMissingUrl", "endpointName"), endpointName);
    }

    internal static string FormatNoCertSpecifiedNoDevelopmentCertificateFound()
    {
        return GetString("NoCertSpecifiedNoDevelopmentCertificateFound");
    }

    internal static string FormatMultipleCertificateSources(object endpointName)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("MultipleCertificateSources", "endpointName"), endpointName);
    }

    internal static string FormatWritingToResponseBodyAfterResponseCompleted()
    {
        return GetString("WritingToResponseBodyAfterResponseCompleted");
    }

    internal static string FormatBadRequest_RequestBodyTimeout()
    {
        return GetString("BadRequest_RequestBodyTimeout");
    }

    internal static string FormatConnectionAbortedByApplication()
    {
        return GetString("ConnectionAbortedByApplication");
    }

    internal static string FormatConnectionAbortedDuringServerShutdown()
    {
        return GetString("ConnectionAbortedDuringServerShutdown");
    }

    internal static string FormatConnectionTimedBecauseResponseMininumDataRateNotSatisfied()
    {
        return GetString("ConnectionTimedBecauseResponseMininumDataRateNotSatisfied");
    }

    internal static string FormatConnectionTimedOutByServer()
    {
        return GetString("ConnectionTimedOutByServer");
    }

    internal static string FormatHttp2ErrorFrameOverLimit(object size, object limit)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorFrameOverLimit", "size", "limit"), size, limit);
    }

    internal static string FormatHttp2ErrorMinTlsVersion(object protocol)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorMinTlsVersion", "protocol"), protocol);
    }

    internal static string FormatHttp2ErrorInvalidPreface()
    {
        return GetString("Http2ErrorInvalidPreface");
    }

    internal static string FormatInvalidEmptyHeaderName()
    {
        return GetString("InvalidEmptyHeaderName");
    }

    internal static string FormatConnectionOrStreamAbortedByCancellationToken()
    {
        return GetString("ConnectionOrStreamAbortedByCancellationToken");
    }

    internal static string FormatHttp2ErrorInitialWindowSizeInvalid()
    {
        return GetString("Http2ErrorInitialWindowSizeInvalid");
    }

    internal static string FormatHttp2ErrorWindowUpdateSizeInvalid()
    {
        return GetString("Http2ErrorWindowUpdateSizeInvalid");
    }

    internal static string FormatHttp2ConnectionFaulted()
    {
        return GetString("Http2ConnectionFaulted");
    }

    internal static string FormatHttp2StreamResetByClient()
    {
        return GetString("Http2StreamResetByClient");
    }

    internal static string FormatHttp2StreamAborted()
    {
        return GetString("Http2StreamAborted");
    }

    internal static string FormatHttp2ErrorFlowControlWindowExceeded()
    {
        return GetString("Http2ErrorFlowControlWindowExceeded");
    }

    internal static string FormatHttp2ErrorConnectMustNotSendSchemeOrPath()
    {
        return GetString("Http2ErrorConnectMustNotSendSchemeOrPath");
    }

    internal static string FormatHttp2ErrorMethodInvalid(object method)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorMethodInvalid", "method"), method);
    }

    internal static string FormatHttp2StreamErrorPathInvalid(object path)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2StreamErrorPathInvalid", "path"), path);
    }

    internal static string FormatHttp2StreamErrorSchemeMismatch(object requestScheme, object transportScheme)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2StreamErrorSchemeMismatch", "requestScheme", "transportScheme"), requestScheme, transportScheme);
    }

    internal static string FormatHttp2StreamErrorLessDataThanLength()
    {
        return GetString("Http2StreamErrorLessDataThanLength");
    }

    internal static string FormatHttp2StreamErrorMoreDataThanLength()
    {
        return GetString("Http2StreamErrorMoreDataThanLength");
    }

    internal static string FormatHttp2StreamErrorAfterHeaders()
    {
        return GetString("Http2StreamErrorAfterHeaders");
    }

    internal static string FormatHttp2ErrorMaxStreams()
    {
        return GetString("Http2ErrorMaxStreams");
    }

    internal static string FormatGreaterThanZeroRequired()
    {
        return GetString("GreaterThanZeroRequired");
    }

    internal static string FormatArgumentOutOfRange(object min, object max)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("ArgumentOutOfRange", "min", "max"), min, max);
    }

    internal static string FormatHPackErrorDynamicTableSizeUpdateNotAtBeginningOfHeaderBlock()
    {
        return GetString("HPackErrorDynamicTableSizeUpdateNotAtBeginningOfHeaderBlock");
    }

    internal static string FormatHPackErrorNotEnoughBuffer()
    {
        return GetString("HPackErrorNotEnoughBuffer");
    }

    internal static string FormatHPackErrorIntegerTooBig()
    {
        return GetString("HPackErrorIntegerTooBig");
    }

    internal static string FormatConnectionAbortedByClient()
    {
        return GetString("ConnectionAbortedByClient");
    }

    internal static string FormatHttp2ErrorStreamAborted(object frameType, object streamId)
    {
        return string.Format(CultureInfo.CurrentCulture, GetString("Http2ErrorStreamAborted", "frameType", "streamId"), frameType, streamId);
    }

    private static string GetString(string name, params string[] formatterNames)
    {
        string text = _resourceManager.GetString(name);
        if (formatterNames != null)
        {
            for (int i = 0; i < formatterNames.Length; i++)
            {
                text = text.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }
        }
        return text;
    }
}
