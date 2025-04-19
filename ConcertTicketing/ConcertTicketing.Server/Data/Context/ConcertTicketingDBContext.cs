using System;
using System.Collections.Generic;
using ConcertTicketing.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConcertTicketing.Server.Data.Context;

public partial class ConcertTicketingDBContext : DbContext
{
    public ConcertTicketingDBContext() {}

    public ConcertTicketingDBContext(DbContextOptions<ConcertTicketingDBContext> options) : base(options) {}

    public virtual DbSet<Artist> Artists { get; set; }

    public virtual DbSet<Concert> Concerts { get; set; }

    public virtual DbSet<ConcertGroup> ConcertGroups { get; set; }

    public virtual DbSet<ConcertRole> ConcertRoles { get; set; }

    public virtual DbSet<ConcertStatus> ConcertStatuses { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<GenresOfArtist> GenresOfArtists { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderTicket> OrderTickets { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketCategory> TicketCategories { get; set; }

    public virtual DbSet<TicketStatus> TicketStatuses { get; set; }

    public virtual DbSet<Venue> Venues { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //    //optionsBuilder.UseSqlServer("Data Source=192.168.1.84\\MAIN,11500;Initial Catalog=ConcertTicketingDB;User ID=DESKTOP-3I9NATQ;Password=db-admin;TrustServerCertificate=True;");

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
        });

        modelBuilder.Entity<Concert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Concerts_ID");

            entity.HasOne(d => d.ConcertGroup).WithMany(p => p.Concerts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Concerts_ConcertGroupID");

            entity.HasOne(d => d.MainArtist).WithMany(p => p.Concerts).HasConstraintName("FK_Concerts_MainArtistID");

            entity.HasOne(d => d.Status).WithMany(p => p.Concerts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Concerts_StatusID");

            entity.HasOne(d => d.Venue).WithMany(p => p.Concerts).HasConstraintName("FK_Concerts_VenueID");
        });

        modelBuilder.Entity<ConcertGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ConcertGroups_ID");
        });

        modelBuilder.Entity<ConcertRole>(entity =>
        {
            entity.HasKey(e => new { e.ConcertId, e.ArtistId, e.RoleId }).HasName("PK_ConcertRoles_ConcertID_ArtistID_RoleID");

            entity.HasOne(d => d.Artist).WithMany(p => p.ConcertRoles).HasConstraintName("FK_ConcertRoles_ArtistID");

            entity.HasOne(d => d.Concert).WithMany(p => p.ConcertRoles).HasConstraintName("FK_ConcertRoles_ConcertID");

            entity.HasOne(d => d.Role).WithMany(p => p.ConcertRoles).HasConstraintName("FK_ConcertRoles_RoleID");
        });

        modelBuilder.Entity<ConcertStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ConcertStatuses_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Customers_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Password).WithMany(p => p.Customers)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Customers_PasswordID");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Genres_ID");
        });

        modelBuilder.Entity<GenresOfArtist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_GenresOfArtists_ID");

            entity.HasOne(d => d.Artist).WithMany(p => p.GenresOfArtists)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GenresOfArtists_ArtistID");

            entity.HasOne(d => d.Genre).WithMany(p => p.GenresOfArtists)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GenresOfArtists_GenreID");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Orders_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Orders_CustomerID");
        });

        modelBuilder.Entity<OrderTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OrderTickets_ID");

            entity.HasOne(d => d.Orders).WithMany(p => p.OrderTickets)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderTickets_OrdersID");

            entity.HasOne(d => d.Ticket).WithMany(p => p.OrderTickets)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderTickets_TicketID");
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Passwords_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Roles_ID");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Tickets_ID");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Concert).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Tickets_ConcertID");

            entity.HasOne(d => d.TicketCategory).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Tickets_TicketCategoryID");

            entity.HasOne(d => d.TicketStatus).WithMany(p => p.Tickets)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Tickets_TicketStatusID");
        });

        modelBuilder.Entity<TicketCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketCategories_ID");

            entity.Property(e => e.StartDate).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Concert).WithMany(p => p.TicketCategories)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_TicketCategories_ConcertID");
        });

        modelBuilder.Entity<TicketStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TicketStatuses_ID");

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
