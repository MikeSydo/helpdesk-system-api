using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.DataAccess.Configurations;

public sealed class OneTimeTokenConfiguration : IEntityTypeConfiguration<OneTimeToken>
{
    public void Configure(EntityTypeBuilder<OneTimeToken> builder)
    {
        builder.ToTable("OneTimeTokens");
        builder.HasKey(token => token.Id);
        
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.Property(token => token.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(token => token.CreatedAt).IsRequired();
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.UsedAt);

        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.Type });
        
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
