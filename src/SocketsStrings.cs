// Microsoft.AspNetCore.Server.Kestrel.Core.CoreStrings
using System.Reflection;
using System.Resources;

internal static class SocketsStrings
{
    private static readonly ResourceManager _resourceManager =
        new ResourceManager("SocketsStrings", typeof(SocketsStrings).GetTypeInfo().Assembly);

    internal static string OnlyIPEndPointsSupported => GetString("OnlyIPEndPointsSupported");

    internal static string TransportAlreadyBound => GetString("TransportAlreadyBound");

    internal static string FormatOnlyIPEndPointsSupported()
    {
        return GetString("OnlyIPEndPointsSupported");
    }

    internal static string FormatTransportAlreadyBound()
    {
        return GetString("TransportAlreadyBound");
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
