using System;
using System.Collections.Generic;
using ConcertTicketing.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketing.Server.Data.Context;

public partial class ConcertTicketingDBContext : DbContext
{
    public ConcertTicketingDBContext() {}

    public ConcertTicketingDBContext(DbContextOptions<ConcertTicketingDBContext> options) : base(options) {}

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<ArtistRole> ArtistRoles { get; set; }

    public virtual DbSet<ArtistRolesAtConcert> ArtistRolesAtConcerts { get; set; }

    public virtual DbSet<Concert> Concerts { get; set; }

    public virtual DbSet<ConcertGroup> ConcertGroups { get; set; }

    public virtual DbSet<ConcertStatus> ConcertStatuses { get; set; }

    public virtual DbSet<Discount> Discounts { get; set; }

    public virtual DbSet<DiscountStatus> DiscountStatuses { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderTicket> OrderTickets { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketDetail> TicketDetails { get; set; }

    public virtual DbSet<TicketStatus> TicketStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //    //optionsBuilder.UseSqlServer("Data Source=192.168.1.97\\MAIN,11500;Initial Catalog=ConcertTicketingDB;User ID=DESKTOP-3I9NATQ;Password=db-admin;TrustServerCertificate=True;");

    //    //if (!optionsBuilder.IsConfigured)
    //    //{
    //    //    var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
    //    //    var connectionStringMain = config.GetConnectionString("MainSQLServerConnection");
    //    //    optionsBuilder.UseSqlServer(connectionStringMain);
    //    //}

    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Artists_ID");

            entity.HasMany(d => d.Genres).WithMany(p => p.Artists)
                .UsingEntity<Dictionary<string, object>>(
                    "GenresOfArtist",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GenresOfArtists_GenreID"),
                    l => l.HasOne<Artist>().WithMany()
                        .HasForeignKey("ArtistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GenresOfArtists_ArtistID"),
                    j =>
                    {
                        j.HasKey("ArtistId", "GenreId").HasName("PK_GenresOfArtists_ArtistID_GenreID");
                        j.ToTable("GenresOfArtists");
                        j.HasIndex(new[] { "ArtistId" }, "IX_GenresOfArtists_ArtistID");
                        j.HasIndex(new[] { "GenreId" }, "IX_GenresOfArtists_GenreID");
                        j.IndexerProperty<long>("ArtistId").HasColumnName("ArtistID");
                        j.IndexerProperty<int>("GenreId").HasColumnName("GenreID");
                    });
        });

        modelBuilder.Entity<ArtistRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ArtistRoles_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ArtistRolesAtConcert>(entity =>
        {
            entity.HasKey(e => new { e.ConcertId, e.ArtistId, e.RoleId }).HasName("PK_ArtistRolesAtConcerts_ConcertID_ArtistID_RoleID");

            entity.HasOne(d => d.Artist).WithMany(p => p.ArtistRolesAtConcerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArtistRolesAtConcerts_ArtistID");

            entity.HasOne(d => d.Concert).WithMany(p => p.ArtistRolesAtConcerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArtistRolesAtConcerts_ConcertID");

            entity.HasOne(d => d.Role).WithMany(p => p.ArtistRolesAtConcerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArtistRolesAtConcerts_RoleID");
        });

        modelBuilder.Entity<Concert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Concerts_ID");

            entity.HasOne(d => d.ConcertGroup).WithMany(p => p.Concerts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Concerts_ConcertGroupID");

            entity.HasOne(d => d.MainArtist).WithMany(p => p.Concerts).HasConstraintName("FK_Concerts_MainArtistID");

            entity.HasOne(d => d.Status).WithMany(p => p.Concerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Concerts_StatusID");

            entity.HasOne(d => d.Venue).WithMany(p => p.Concerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Concerts_VenueID");
        });

        modelBuilder.Entity<ConcertGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ConcertGroups_ID");
        });

        modelBuilder.Entity<ConcertStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ConcertStatuses_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Discount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Discounts_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DiscountCode).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Status).WithMany(p => p.Discounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Discounts_StatusID");
        });

        modelBuilder.Entity<DiscountStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DiscountStatuses_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Genres_ID");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Orders_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Discount).WithMany(p => p.Orders).HasConstraintName("FK_Orders_DiscountID");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Orders_UserID");
        });

        modelBuilder.Entity<OrderTicket>(entity =>
        {
            entity.HasKey(e => new { e.TicketId, e.OrderId }).HasName("PK_OrderTickets_TicketID_OrderID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderTickets).HasConstraintName("FK_OrderTickets_OrderID");

            entity.HasOne(d => d.Ticket).WithOne(p => p.OrderTicket).HasConstraintName("FK_OrderTickets_TicketID");
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Passwords_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tickets_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Concert).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_ConcertID");

            entity.HasOne(d => d.TicketDetail).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_TicketDetailID");

            entity.HasOne(d => d.TicketStatus).WithMany(p => p.Tickets).HasConstraintName("FK_Tickets_TicketStatusID");
        });

        modelBuilder.Entity<TicketDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketDetails_ID");

            entity.Property(e => e.StartDate).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Concert).WithMany(p => p.TicketDetails)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TicketDetails_ConcertID");
        });

        modelBuilder.Entity<TicketStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketStatuses_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Users_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.SignedIn).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Password).WithOne(p => p.User).HasConstraintName("FK_Users_PasswordID");

            entity.HasOne(d => d.UserRole).WithMany(p => p.Users).HasConstraintName("FK_Users_UserRoleID");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_UserRoles_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Venues_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
