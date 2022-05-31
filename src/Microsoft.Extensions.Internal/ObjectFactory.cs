using System;

namespace Microsoft.Extensions.Internal;

internal delegate object ObjectFactory(IServiceProvider serviceProvider, object[] arguments);
