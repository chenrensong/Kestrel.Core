namespace Microsoft.AspNetCore.Http.Features;

public interface IHttpBufferingFeature
{
	void DisableRequestBuffering();

	void DisableResponseBuffering();
}
