namespace Specht.IdGeneration;

/// <summary>How formatting writes a numeric candidate.</summary>
public enum IdEncoding
{
    /// <summary>The strategy's text as it is.</summary>
    None,

    /// <summary>Lower-case Crockford base32, fixed at 13 characters for 64 bits so text order is numeric order.</summary>
    Base32,
}
