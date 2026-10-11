namespace System.Runtime.CompilerServices;

/// <summary>Marks a closed hierarchy's root; the <c>net11.0</c> type the compiler requires, declared here for <c>net10.0</c>.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IsClosedTypeAttribute : Attribute;
