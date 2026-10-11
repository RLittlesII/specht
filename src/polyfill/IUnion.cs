namespace System.Runtime.CompilerServices;

/// <summary>The value a union holds; the <c>net11.0</c> type the compiler requires, declared here for <c>net10.0</c>.</summary>
public interface IUnion
{
    /// <summary>Gets the case the union holds, or <see langword="null"/>.</summary>
    object? Value { get; }
}
