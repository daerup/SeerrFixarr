using CSharpFunctionalExtensions;
using Mono.Cecil;
using NetArchTest.Rules;

namespace SeerrFixarr.Test;

internal class AsyncSuffixForMethodsRule : ICustomRule
{
    private readonly MethodType _methodType;

    private enum MethodType
    {
        Async,
        Sync
    }

    private AsyncSuffixForMethodsRule(MethodType methodType) => _methodType = methodType;

    public static AsyncSuffixForMethodsRule ForAsyncMethods() => new(MethodType.Async);
    public static AsyncSuffixForMethodsRule ForSyncMethods() => new(MethodType.Sync);

    public bool MeetsRule(TypeDefinition type) => _methodType switch
    {
        MethodType.Async => AsyncMeetsRule(type),
        MethodType.Sync => SyncMeetsRule(type)
    };

    private bool AsyncMeetsRule(TypeDefinition type)
    {
        return type.Methods
                   .Where(m => IsAwaitable(m.ReturnType))
                   .Where(m => !HasTransitiveAttribute<Refit.GetAttribute>(m))
                   .Where(m => !HasTransitiveAttribute<Refit.PostAttribute>(m))
                   .Where(m => !HasTransitiveAttribute<Refit.DeleteAttribute>(m))
                   .All(m => m.Name.EndsWith("Async"));
    }

    private bool HasTransitiveAttribute<T>(MethodDefinition method) where T : Attribute
    {
        return GetInterfaceMethod(method).Match(
            HasTransitiveAttribute<T>,
            () => method.CustomAttributes.Any(attr => attr.AttributeType.FullName == typeof(T).FullName)
        );
    }

    private Maybe<MethodDefinition> GetInterfaceMethod(MethodDefinition method)
    {
        if (!method.DeclaringType.HasInterfaces)
        {
            return null;
        }

        return method.DeclaringType.Interfaces
                     .SelectMany(i => i.InterfaceType.Resolve().Methods)
                     .FirstOrDefault(m => m.Name == method.Name && m.Parameters.Count == method.Parameters.Count);
    }

    private bool SyncMeetsRule(TypeDefinition type)
    {
        return type.Methods
                   .Where(m => !IsAwaitable(m.ReturnType))
                   .All(m => !m.Name.EndsWith("Async"));
    }

    private bool IsAwaitable(TypeReference returnType)
    {
        List<Type> asyncReturnTypes =
        [
            typeof(IAsyncEnumerable<>),
            typeof(Task),
            typeof(Task<>),
            typeof(ValueTask),
            typeof(ValueTask<>)
        ];


        if (returnType is GenericInstanceType genericType)
        {
            returnType = genericType.ElementType;
        }

        return asyncReturnTypes.Any(asyncType =>
                                        returnType.Name == asyncType.Name &&
                                        returnType.Namespace == asyncType.Namespace);
    }
}