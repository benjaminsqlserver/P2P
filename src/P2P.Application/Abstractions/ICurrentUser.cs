namespace P2P.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }

    string UserName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);
}
