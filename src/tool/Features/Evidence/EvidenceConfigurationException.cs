using System;

namespace Specht.Tool.Features.Evidence;

/// <implements epic="0201" feature="F5" claim="B-001" />
internal sealed class EvidenceConfigurationException(string message) : Exception(message);
