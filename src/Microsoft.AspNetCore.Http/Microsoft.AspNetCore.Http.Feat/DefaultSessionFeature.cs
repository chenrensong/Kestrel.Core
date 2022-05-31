namespace Microsoft.AspNetCore.Http.Features;

public class DefaultSessionFeature : ISessionFeature
{
	public ISession Session { get; set; }
}
