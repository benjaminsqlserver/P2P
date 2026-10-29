using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using P2P.Domain.Common;

namespace P2P.Infrastructure.Persistence.Configurations;

internal static class MoneyExtensions
{
    /// <summary>
    /// Maps a <see cref="Money"/> complex property to a pair of columns
    /// named <paramref name="columnPrefix"/> and
    /// <paramref name="columnPrefix"/>Currency.
    /// </summary>
    public static void ConfigureMoney<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Money>> property,
        string columnPrefix)
        where TEntity : class
    {
        builder.ComplexProperty(property, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName(columnPrefix)
                .HasPrecision(18, 4);

            money.Property(m => m.Currency)
                .HasColumnName($"{columnPrefix}Currency")
                .HasMaxLength(3);
        });
    }
}
