using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;

namespace SeerrFixarr.App.Shared;

public static class TokenValidationParametersExtensions
{
    public static TokenValidationParameters WithTimeProvider(this TokenValidationParameters @this, TimeProvider timeProvider)
    {
        UpdateTimeProvider(@this, timeProvider);
        return @this;
    }
    
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_" + nameof(TimeProvider))]
    private static extern void UpdateTimeProvider(this TokenValidationParameters @this, TimeProvider value);    
}