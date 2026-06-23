using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;

namespace PaymentAPI.Data;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(builder =>
        {
            builder.ToTable("Payments");
            builder.HasKey(payment => payment.Id);
            builder.HasIndex(payment => payment.OrderId)
                .IsUnique();

            builder.Property(payment => payment.UserId)
                .HasColumnName("UsuarioId")
                .IsRequired();

            builder.Property(payment => payment.GameId)
                .IsRequired();

            builder.Property(payment => payment.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(payment => payment.Type)
                .HasColumnName("Tipo")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(payment => payment.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(payment => payment.Installments)
                .HasColumnName("Vezes")
                .IsRequired();

            builder.Property(payment => payment.PaymentDate)
                .HasColumnName("DataPagamento")
                .IsRequired();
        });
    }
}
