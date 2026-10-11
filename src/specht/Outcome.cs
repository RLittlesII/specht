namespace Specht;

/// <summary>What a step over a check's input gives: its value, or the <see cref="InputFailure"/> that stopped it.</summary>
/// <typeparam name="T">The value the step gives when the input is accepted.</typeparam>
public union Outcome<T>(T, InputFailure);
