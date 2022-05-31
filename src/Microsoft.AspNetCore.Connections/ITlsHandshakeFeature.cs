using System.Security.Authentication;

namespace Microsoft.AspNetCore.Connections.Features;

public interface ITlsHandshakeFeature
{
	SslProtocols Protocol { get; }

	CipherAlgorithmType CipherAlgorithm { get; }

	int CipherStrength { get; }

	HashAlgorithmType HashAlgorithm { get; }

	int HashStrength { get; }

	ExchangeAlgorithmType KeyExchangeAlgorithm { get; }

	int KeyExchangeStrength { get; }
}
