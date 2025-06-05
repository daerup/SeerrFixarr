using System.Reflection;
using SeerrFixarr.App.Shared;

namespace SeerrFixarr.Test.Infrastructure;

internal static class RedirectKeyManagerExtensions
{
    internal static void InvokeOnRedirectionKeyCreated(this RedirectKeyManager @this, string key)
    {
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        if (typeof(RedirectKeyManager).GetField("OnRedirectionKeyCreated", bindingFlags) is not { } field) return;
        var @event = field.GetValue(@this) as Action<string>;
        @event?.Invoke(key);
    }
    
    internal static void InvokeOnRedirectionKeyDestroyed(this RedirectKeyManager @this, string key)
    {
        const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        if (typeof(RedirectKeyManager).GetField("OnRedirectionKeyDestroyed", bindingFlags) is not { } field) return;
        var @event = field.GetValue(@this) as Action<string>;
        @event?.Invoke(key);
    }
}