using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Domain.Common;
using P2P.Domain.Vendors;

namespace P2P.Application.Vendors.Commands.AddBankAccount;

internal sealed class AddBankAccountCommandHandler(IApplicationDbContext context)
    : ICommandHandler<AddBankAccountCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        AddBankAccountCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var vendor = await context.Vendors
            .Include(v => v.BankAccounts)
            .SingleOrDefaultAsync(v => v.Id == request.VendorId, ct);

        if (vendor is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(nameof(Vendor), request.VendorId));
        }

        var result = vendor.AddBankAccount(
            request.BankName,
            request.AccountName,
            request.AccountNumber,
            request.SortCodeOrSwift,
            request.Currency,
            request.MakePrimary);

        return result.IsFailure
            ? Result.Failure<Guid>(result.Error)
            : result.Value.Id;
    }
}
