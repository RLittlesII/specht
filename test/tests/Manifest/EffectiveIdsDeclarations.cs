namespace Specht.Tests.Manifest;

/// <summary>
/// The <c>ids</c> a manifest pinned to schema 0.4.0 declares, as JSON, or <see langword="null"/> for none (<c>0001-F9</c>
/// B-003, B-004; <c>0001-F5</c> B-097): no <c>ids</c> key, a claim record holding only its <c>form</c>, and a constraint
/// record holding only its <c>plain</c> beside a claim record holding its <c>id</c> and a <c>mint</c> the pin's default
/// records do not hold.
/// </summary>
public sealed class EffectiveIdsDeclarations : TheoryData<string?>
{
    public EffectiveIdsDeclarations()
    {
        Add((string?) null);
        Add("""{"claim":{"form":"{prefix}:{id}"}}""");
        Add("""
            {"constraint":{"plain":"K-{id}"},
             "claim":{"id":"[0-9]{4}","mint":{"strategy":"nanoid","nanoid":{"alphabet":"0123456789abcdefghjkmnpqrstvwxyz","size":6}}}}
            """);
    }
}
