using System.Globalization;

namespace SeerrFixarr.App.Runners.Webhook;

internal class CultureScopeFactory
{
    internal CultureScope FromLocale(string username, string locale)
    { 
        var original = Thread.CurrentThread.CurrentUICulture;
        var newCulture = new CultureInfo(locale);
        Thread.CurrentThread.CurrentUICulture = newCulture;
        return new CultureScope(() => Thread.CurrentThread.CurrentUICulture = original);
    }
    
    internal class CultureScope(Action reset) : IDisposable
    {
        public void Dispose() => reset();
    }
}