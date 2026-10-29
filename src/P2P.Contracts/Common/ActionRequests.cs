namespace P2P.Contracts.Common;

/// <summary>Body for commands that require a free-text reason (hold, reject, resolve).</summary>
public sealed record ReasonRequest(string Reason);

/// <summary>Body for commands that accept an optional comment (approve).</summary>
public sealed record CommentRequest(string? Comment);
