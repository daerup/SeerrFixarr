using System.Runtime.CompilerServices;
using SeerrFixarr.App.Shared;

namespace SeerrFixarr.Test.Infrastructure;

internal static class TokenCreatorAccessor
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IsRevoked")]
    public static extern bool IsRevoked(this TokenCreator creator, string token);
}