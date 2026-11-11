using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;

public partial class GameShopAppDbContext : DbContext
{
    public GameShopAppDbContext()
    {
    }

    public GameShopAppDbContext(DbContextOptions<GameShopAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<ConsoleCategory> ConsoleCategories { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameConsole> GameConsoles { get; set; }

    public virtual DbSet<GameSession> GameSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=GameShopDb;User Id=sa;Password=sasa@123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bookings__3214EC07029DEA9A");

            entity.Property(e => e.BookingStatus).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalHours).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Console).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ConsoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_GameConsoles");
        });

        modelBuilder.Entity<ConsoleCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ConsoleC__3214EC07D0D3906E");

            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Games__3214EC071F6EF7E9");

            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Category).WithMany(p => p.Games)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Games_ConsoleCategories");
        });

        modelBuilder.Entity<GameConsole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameCons__3214EC07F3110803");

            entity.HasIndex(e => e.ConsoleNumber, "UQ__GameCons__749C9D56C737B748").IsUnique();

            entity.Property(e => e.ConsoleNumber).HasMaxLength(20);
            entity.Property(e => e.CurrentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Category).WithMany(p => p.GameConsoles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GameConsoles_ConsoleCategories");
        });

        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GameSess__3214EC07F082BB3C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SessionStatus).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalHours).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Booking).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_GameSessions_Bookings");

            entity.HasOne(d => d.Console).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.ConsoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GameSessions_GameConsoles");

            entity.HasOne(d => d.Game).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK_GameSessions_Games");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
