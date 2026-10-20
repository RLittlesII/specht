namespace Specht.Tool.Features.Evidence.Contract;

public sealed record TestEvidenceFingerprintSource(string Path, string Kind, byte[] Content);
