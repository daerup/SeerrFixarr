using System.Reflection;
using SeerrFixarr.App.Shared;

namespace SeerrFixarr.Test.Infrastructure;

internal static class TokenCreatorExtensions
{
    internal static void InvokeOnTokenRevoked(this TokenCreator @this, string token)
    {
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        if (typeof(TokenCreator).GetField("OnTokenRevoked", bindingFlags) is not { } field) return;
        var @event = field.GetValue(@this) as Action<string>;
        @event?.Invoke(token);
    }
}