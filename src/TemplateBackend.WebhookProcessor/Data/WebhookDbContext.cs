using Microsoft.EntityFrameworkCore;
using TemplateBackend.WebhookProcessor.Models;

namespace TemplateBackend.WebhookProcessor.Data;

/// <summary>
/// Database context for webhook processing
/// </summary>
public class WebhookDbContext : DbContext
{
    public WebhookDbContext(DbContextOptions<WebhookDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Webhook events table
    /// </summary>
    public DbSet<WebhookEvent> WebhookEvents { get; set; }

    /// <summary>
    /// Webhook processing logs table
    /// </summary>
    public DbSet<WebhookProcessingLog> WebhookProcessingLogs { get; set; }

    /// <summary>
    /// Payment events table
    /// </summary>
    public DbSet<PaymentEvent> PaymentEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WebhookEvent configuration
        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.EventId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RawPayload).IsRequired();
            entity.Property(e => e.ProcessedPayload).IsRequired(false);
            entity.Property(e => e.Signature).HasMaxLength(1000);
            entity.Property(e => e.SourceIp).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            entity.HasIndex(e => e.EventId);
            entity.HasIndex(e => e.Provider);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // WebhookProcessingLog configuration
        modelBuilder.Entity<WebhookProcessingLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.WebhookEventId).IsRequired();
            entity.Property(e => e.ProcessingStep).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DurationMs).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.AdditionalData).HasMaxLength(4000);
            
            entity.HasIndex(e => e.WebhookEventId);
            entity.HasIndex(e => e.ProcessingStep);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ProcessedAt);
        });

        // PaymentEvent configuration
        modelBuilder.Entity<PaymentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.WebhookEventId).IsRequired();
            entity.Property(e => e.PaymentId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CustomerId).HasMaxLength(255);
            entity.Property(e => e.OrderId).HasMaxLength(255);
            entity.Property(e => e.TransactionId).HasMaxLength(255);
            entity.Property(e => e.Metadata).HasMaxLength(4000);
            
            entity.HasIndex(e => e.WebhookEventId);
            entity.HasIndex(e => e.PaymentId);
            entity.HasIndex(e => e.Provider);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
} 