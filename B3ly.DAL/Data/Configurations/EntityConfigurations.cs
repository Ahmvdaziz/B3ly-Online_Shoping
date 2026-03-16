using B3ly.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace B3ly.DAL.Data.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.HasOne(a => a.User)
                   .WithMany(u => u.Addresses)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasIndex(c => c.Name).IsUnique();
        }
    }

    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.HasIndex(p => p.SKU).IsUnique();
            // Product name must be unique within a category
            builder.HasIndex(p => new { p.CategoryId, p.Name }).IsUnique();
        }
    }

    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasOne(o => o.User)
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(o => o.ShippingAddress)
                   .WithMany(a => a.Orders)
                   .HasForeignKey(o => o.ShippingAddressId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.Property(oi => oi.ProductName).HasMaxLength(200).IsRequired();
            builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");
        }
    }
}
