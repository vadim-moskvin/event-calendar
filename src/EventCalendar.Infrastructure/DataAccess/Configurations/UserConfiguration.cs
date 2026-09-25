using EventCalendar.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventCalendar.Infrastructure.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Login).IsRequired();
        builder.HasIndex(x => x.Login).IsUnique();

        builder.Property(x => x.PasswordHash).IsRequired();
    }
}