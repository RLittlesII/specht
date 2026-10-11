namespace Specht.Discovery;

/// <summary>
/// The zero-based segments of a specification's root-relative path whose directories carry its identity, as a
/// <c>layouts</c> entry's <c>identity</c> declares them (<c>0001-F6</c> B-004, decision 0009).
/// </summary>
/// <param name="Epic">The segment whose directory carries the epic id, or <see langword="null"/> when the entry declares none.</param>
/// <param name="Feature">The segment whose directory carries the Feature id, or <see langword="null"/> when the entry declares none.</param>
public sealed record SpecPathIdentity(int? Epic, int? Feature);
