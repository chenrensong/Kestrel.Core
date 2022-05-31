using System;

namespace Microsoft.AspNetCore.Connections;

[Flags]
public enum TransferFormat
{
	Binary = 1,
	Text = 2
}
