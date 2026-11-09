namespace Consumer.Ledger;

/// <summary>Posts and reverses amounts against a running balance.</summary>
/// <implements feature="ledger" claim="REQ-LED-001" />
public static class Postings
{
    /// <summary>Returns the balance after an amount is posted.</summary>
    /// <implements feature="ledger" claim="REQ-LED-001" />
    public static int Post(int balance, int amount) => balance + amount;

    /// <summary>Returns the balance after an amount is reversed.</summary>
    /// <implements feature="ledger" claim="REQ-LED-002" />
    public static int Reverse(int balance, int amount) => balance - amount;
}
