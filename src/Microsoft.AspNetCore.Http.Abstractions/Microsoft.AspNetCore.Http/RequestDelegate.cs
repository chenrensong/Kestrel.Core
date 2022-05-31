using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http;

public delegate Task RequestDelegate(HttpContext context);
