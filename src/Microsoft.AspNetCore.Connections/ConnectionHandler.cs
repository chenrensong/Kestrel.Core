using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Connections;

public abstract class ConnectionHandler
{
    public abstract Task OnConnectedAsync(ConnectionContext connection);
}
