namespace Specht.IdGeneration;

/// <summary>Where the sequence field starts within a millisecond.</summary>
public enum SequenceStart
{
    /// <summary>At zero, as the generator the owner supplied.</summary>
    Zero,

    /// <summary>At a random value, because a print-only run mints once and keeps no sequence.</summary>
    Random,
}
