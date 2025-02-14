using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Acceloka.Entities;

public partial class AccelokaContext : DbContext
{
    public AccelokaContext()
    {
    }

    public AccelokaContext(DbContextOptions<AccelokaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookedTicket> BookedTickets { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost; Initial Catalog=Acceloka; User id=sa; Pwd=brainless; Encrypt=false");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Book__3DE0C207D4948ADE");

            entity.ToTable("Book");

            entity.Property(e => e.BookingDate).HasDefaultValueSql("(sysdatetimeoffset())");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<BookedTicket>(entity =>
        {
            entity.HasKey(e => e.BookedTicketId).HasName("PK__BookedTi__9110472FC012D719");

            entity.ToTable("BookedTicket");

            entity.Property(e => e.TicketCode)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Book).WithMany(p => p.BookedTickets)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_BookedTicket_Book");

            entity.HasOne(d => d.TicketCodeNavigation).WithMany(p => p.BookedTickets)
                .HasForeignKey(d => d.TicketCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookedTicket_Ticket");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketCode).HasName("PK__Ticket__598CF7A24B7E1826");

            entity.ToTable("Ticket");

            entity.Property(e => e.TicketCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TicketName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TicketPrice).HasColumnType("decimal(18, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
