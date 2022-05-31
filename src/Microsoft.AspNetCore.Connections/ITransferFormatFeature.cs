namespace Microsoft.AspNetCore.Connections.Features;

public interface ITransferFormatFeature
{
	TransferFormat SupportedFormats { get; }

	TransferFormat ActiveFormat { get; set; }
}
