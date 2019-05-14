using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Company.WebService.Models.ServiceCenter
{
    public partial class ServiceCenterContext : DbContext
    {
        public ServiceCenterContext()
        {
        }

        public ServiceCenterContext(DbContextOptions<ServiceCenterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CustomerComment> CustomerComment { get; set; }
        public virtual DbSet<CustomerMapping> CustomerMapping { get; set; }
        public virtual DbSet<CustomerWarning> CustomerWarning { get; set; }
        public virtual DbSet<DeliveryPaymentButton> DeliveryPaymentButton { get; set; }
        public virtual DbSet<Invoice> Invoice { get; set; }
        public virtual DbSet<InvoiceBillingAddress> InvoiceBillingAddress { get; set; }
        public virtual DbSet<InvoiceDeliveryAddress> InvoiceDeliveryAddress { get; set; }
        public virtual DbSet<InvoiceItem> InvoiceItem { get; set; }
        public virtual DbSet<PznAvailabilityTypeOverride> PznAvailabilityTypeOverride { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.1-servicing-10028");

            modelBuilder.Entity<CustomerComment>(entity =>
            {
                entity.HasKey(e => e.CustomerCommentPk);

                entity.ToTable("CustomerComment", "ServiceCenter");

                entity.HasIndex(e => new { e.CustomerId, e.CommentSent });

                entity.Property(e => e.ChangedUserName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ChangedUtc)
                    .HasColumnName("ChangedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUtc)
                    .HasColumnName("CreatedUTC")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerComment)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerComment_CustomerId");
            });

            modelBuilder.Entity<CustomerMapping>(entity =>
            {
                entity.HasKey(e => e.CustomerId);

                entity.ToTable("CustomerMapping", "ServiceCenter");

                entity.HasIndex(e => e.KundenNr)
                    .IsUnique();

                entity.Property(e => e.CustomerId).ValueGeneratedNever();
            });

            modelBuilder.Entity<CustomerWarning>(entity =>
            {
                entity.HasKey(e => e.CustomerWarningPk)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("CustomerWarning", "ServiceCenter");

                entity.HasIndex(e => e.KundenNr)
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.WarningType)
                    .IsRequired()
                    .HasMaxLength(16);
            });

            modelBuilder.Entity<DeliveryPaymentButton>(entity =>
            {
                entity.HasKey(e => e.DeliveryPaymentButtonPk);

                entity.ToTable("DeliveryPaymentButton", "ServiceCenter");

                entity.Property(e => e.DisplayFromTotal).HasColumnType("money");

                entity.Property(e => e.DisplayText)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FromDate).HasColumnType("date");

                entity.Property(e => e.ItemPrice).HasColumnType("money");

                entity.Property(e => e.Pzn).HasColumnName("PZN");

                entity.Property(e => e.UntilDate).HasColumnType("date");
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoicePk)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("Invoice", "ServiceCenter");

                entity.HasIndex(e => new { e.AffiliateId, e.CustomerId, e.InvoiceId })
                    .HasName("IX_Invoice_AffiliateId_CustomerId")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.AccountOwner).HasMaxLength(128);

                entity.Property(e => e.Bic).HasColumnName("BIC");

                entity.Property(e => e.ChangedUserName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ChangedUtc)
                    .HasColumnName("ChangedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUtc)
                    .HasColumnName("CreatedUTC")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CustomerGroupDiscount).HasColumnType("money");

                entity.Property(e => e.Iban).HasColumnName("IBAN");
            });

            modelBuilder.Entity<InvoiceBillingAddress>(entity =>
            {
                entity.HasKey(e => e.InvoiceBillingAddressPk)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("InvoiceBillingAddress", "ServiceCenter");

                entity.HasIndex(e => e.InvoiceFk)
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.AdditionalLine).HasMaxLength(256);

                entity.Property(e => e.ChangedUserName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ChangedUtc)
                    .HasColumnName("ChangedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.City).HasMaxLength(256);

                entity.Property(e => e.CompanyName).HasMaxLength(256);

                entity.Property(e => e.Country).HasMaxLength(2);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnName("CreatedUTC")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.CustomerNumber).HasMaxLength(16);

                entity.Property(e => e.DoB).HasColumnType("date");

                entity.Property(e => e.EmailAddress).HasMaxLength(256);

                entity.Property(e => e.FaxNumber).HasMaxLength(128);

                entity.Property(e => e.FirstName).HasMaxLength(256);

                entity.Property(e => e.LastName).HasMaxLength(256);

                entity.Property(e => e.MobileNumber).HasMaxLength(128);

                entity.Property(e => e.Street).HasMaxLength(256);

                entity.Property(e => e.TelephoneNumber).HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(64);

                entity.Property(e => e.Zip).HasMaxLength(16);

                entity.HasOne(d => d.InvoiceFkNavigation)
                    .WithOne(p => p.InvoiceBillingAddress)
                    .HasForeignKey<InvoiceBillingAddress>(d => d.InvoiceFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InvoiceBillingAddress_InvoicePk");
            });

            modelBuilder.Entity<InvoiceDeliveryAddress>(entity =>
            {
                entity.HasKey(e => e.InvoiceDeliveryAddressPk)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("InvoiceDeliveryAddress", "ServiceCenter");

                entity.HasIndex(e => e.InvoiceFk)
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.AdditionalLine).HasMaxLength(256);

                entity.Property(e => e.ChangedUserName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ChangedUtc)
                    .HasColumnName("ChangedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.City).HasMaxLength(256);

                entity.Property(e => e.CompanyName).HasMaxLength(256);

                entity.Property(e => e.Country).HasMaxLength(2);

                entity.Property(e => e.CreatedUtc)
                    .HasColumnName("CreatedUTC")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.FirstName).HasMaxLength(256);

                entity.Property(e => e.LastName).HasMaxLength(256);

                entity.Property(e => e.Street).HasMaxLength(256);

                entity.Property(e => e.Title).HasMaxLength(64);

                entity.Property(e => e.Zip).HasMaxLength(16);

                entity.HasOne(d => d.InvoiceFkNavigation)
                    .WithOne(p => p.InvoiceDeliveryAddress)
                    .HasForeignKey<InvoiceDeliveryAddress>(d => d.InvoiceFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InvoiceDeliveryAddress_InvoicePk");
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.HasKey(e => e.InvoiceItemPk)
                    .ForSqlServerIsClustered(false);

                entity.ToTable("InvoiceItem", "ServiceCenter");

                entity.HasIndex(e => e.InvoiceFk)
                    .ForSqlServerIsClustered();

                entity.Property(e => e.ChangedUserName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ChangedUtc)
                    .HasColumnName("ChangedUTC")
                    .HasColumnType("datetime");

                entity.Property(e => e.CreatedUtc)
                    .HasColumnName("CreatedUTC")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ItemPrice).HasColumnType("money");

                entity.Property(e => e.ItemSavings).HasColumnType("money");

                entity.Property(e => e.Pzn).HasColumnName("PZN");

                entity.Property(e => e.Vat)
                    .HasColumnName("VAT")
                    .HasColumnType("money");

                entity.HasOne(d => d.InvoiceFkNavigation)
                    .WithMany(p => p.InvoiceItem)
                    .HasForeignKey(d => d.InvoiceFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InvoiceItem_InvoicePk");
            });

            modelBuilder.Entity<PznAvailabilityTypeOverride>(entity =>
            {
                entity.HasKey(e => e.PznAvailabilityTypeOverridePk);

                entity.ToTable("PznAvailabilityTypeOverride", "ServiceCenter");

                entity.HasIndex(e => e.Pzn)
                    .IsUnique();

                entity.Property(e => e.Pzn).HasColumnName("PZN");
            });

            modelBuilder.HasSequence<int>("CustomerId").StartsAt(1000);

            modelBuilder.HasSequence<int>("Einlesen_Kopfdaten_PKorder_id").StartsAt(505000000);
        }
    }
}
