using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(250);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.EmailConfirmed).IsRequired();
        
        builder.HasIndex(u => u.Email).IsUnique();
    }
}