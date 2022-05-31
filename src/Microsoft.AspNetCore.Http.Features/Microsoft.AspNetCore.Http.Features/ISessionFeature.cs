namespace Microsoft.AspNetCore.Http.Features;

public interface ISessionFeature
{
	ISession Session { get; set; }
}
