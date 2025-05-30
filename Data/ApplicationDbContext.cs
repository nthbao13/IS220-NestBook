using System;
using System.Collections.Generic;
using BookNest.Models.MappingDBModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookNest.Data;

public partial class ApplicationDbContext : IdentityDbContext<AspNetUser,  AspNetRole, int>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookComment> BookComments { get; set; }

    public virtual DbSet<BookRating> BookRatings { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<ParentCategory> ParentCategories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Book__3213E83FA712B29A");

            entity.ToTable("Book");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnName("author");
            entity.Property(e => e.BookName)
                .HasMaxLength(100)
                .IsUnicode(true)
                .HasColumnName("book_name");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Cover)
                .HasMaxLength(15)
                .IsUnicode(true)
                .HasColumnName("cover");
            entity.Property(e => e.Description)
                .IsUnicode(true)
                .HasColumnName("description");
            entity.Property(e => e.FirstPrice)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("first_price");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(true)
                .HasColumnName("image_url");
            entity.Property(e => e.ImportPrice)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("import_price");
            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .IsUnicode(true)
                .HasColumnName("isbn");
            entity.Property(e => e.Pages).HasColumnName("pages");
            entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("rating");
            entity.Property(e => e.SecondPrice)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("second_price");
            entity.Property(e => e.YearPublished).HasColumnName("year_published");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Book__category_i__693CA210");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK__Book__publisher___6B24EA82");
        });

        modelBuilder.Entity<BookComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookComm__3213E83F1510F6A6");

            entity.ToTable("BookComment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Content)
                .HasMaxLength(300)
                .IsUnicode(true)
                .HasColumnName("content");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany(p => p.BookComments)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__BookComme__book___619B8048");
        });

        modelBuilder.Entity<BookRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookRati__3213E83F6C2BA165");

            entity.ToTable("BookRating");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany(p => p.BookRatings)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__BookRatin__book___68487DD7");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3213E83F2414FCEA");

            entity.ToTable("Cart");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Book).WithMany(p => p.Carts)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Cart__book_id__60A75C0F");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3213E83F965B5337");

            entity.ToTable("Category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnName("name");
            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.Categories)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK__Category__parent__6A30C649");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3213E83F996681EF");

            entity.ToTable("Order");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .IsUnicode(true)
                .HasColumnName("name");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnName("address");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsUnicode(true)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(true)
                .HasColumnName("status");
            entity.Property(e => e.From).HasColumnName("from");

            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3213E83F521D1FAF");

            entity.ToTable("OrderDetail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Book).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__OrderDeta__book___6754599E");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderDeta__order__66603565");
        });

        modelBuilder.Entity<ParentCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ParentCa__3213E83FE618C529");

            entity.ToTable("ParentCategory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3213E83F2B6B1401");

            entity.ToTable("Payment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentTypeId).HasColumnName("payment_type_id");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(true)
                .HasColumnName("status");
            entity.Property(e => e.TransactionRef)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("TransactionRef");
            entity.Property(e => e.VnpTransactionNo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("VnpTransactionNo");
            entity.Property(e => e.VnpResponseCode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("VnpResponseCode");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Payment__order_i__6383C8BA");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("FK__Payment__payment__628FA481");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3213E83F12E4A6E6");

            entity.ToTable("PaymentType");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(true)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Publishe__3213E83F1EA10F55");

            entity.ToTable("Publisher");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3213E83F4AFFE46F");

            entity.ToTable("Voucher");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.ExpiredAt)
                .HasColumnType("datetime")
                .HasColumnName("expired_at");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UsageLimit).HasColumnName("usage_limit");
            entity.Property(e => e.UsedCount).HasColumnName("used_count");
            entity.Property(e => e.VoucherCode)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("voucher_code");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("value");

            entity.HasMany(d => d.Orders).WithMany(p => p.Vouchers)
                .UsingEntity<Dictionary<string, object>>(
                    "OrderVoucher",
                    r => r.HasOne<Order>().WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__OrderVouc__order__6477ECF3"),
                    l => l.HasOne<Voucher>().WithMany()
                        .HasForeignKey("VoucherId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__OrderVouc__vouch__656C112C"),
                    j =>
                    {
                        j.HasKey("VoucherId", "OrderId").HasName("PK__OrderVou__04D3698A28DD1E74");
                        j.ToTable("OrderVoucher");
                        j.IndexerProperty<int>("VoucherId").HasColumnName("voucher_id");
                        j.IndexerProperty<int>("OrderId").HasColumnName("order_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
