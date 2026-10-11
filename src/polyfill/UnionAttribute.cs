namespace System.Runtime.CompilerServices;

/// <summary>Marks a type as a union; the <c>net11.0</c> type the compiler requires, declared here for <c>net10.0</c>.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute;
