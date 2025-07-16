using NetArchTest.Rules;
using Shouldly;

namespace SeerrFixarr.Test;

internal static class ConditionListExtensions
{
    public static void Assert(this ConditionList conditionList)
    {
        var result = conditionList.GetResult();
        (result.FailingTypes ?? []).ShouldBeEmpty();
        result.IsSuccessful.ShouldBeTrue();
    }
}