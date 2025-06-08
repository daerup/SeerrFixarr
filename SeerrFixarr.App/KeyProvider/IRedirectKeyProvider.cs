using CSharpFunctionalExtensions;

namespace SeerrFixarr.App.KeyProvider;

public interface IRedirectKeyProvider
{
    Maybe<string> GetNext();
}