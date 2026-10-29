namespace P2P.Infrastructure.Persistence;

public sealed class DocumentSequence
{
    public required string DocumentType { get; set; }

    public required int Year { get; set; }

    public required long NextValue { get; set; }
}
