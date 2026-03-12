using System;
using System.Collections.Generic;
using BAO_CAO.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BaoCaoDACS.Model;

public partial class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<LoaiHinhThiDau> LoaiHinhThiDaus { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Participant> Participants { get; set; }

    public virtual DbSet<Socre> Socres { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<TournamentRanking> TournamentRankings { get; set; }

    public virtual DbSet<VTournamentAll> VTournamentAlls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseOracle("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=FREEPDB1)));User Id=NHOM4;Password=123;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasDefaultSchema("NHOM4")
            .UseCollation("USING_NLS_COMP");

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.Property(e => e.Id).HasPrecision(10);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.Property(e => e.AccessFailedCount).HasPrecision(10);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.LockoutEnd).HasPrecision(7);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.Property(e => e.Id).HasPrecision(10);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<LoaiHinhThiDau>(entity =>
        {
            entity.ToTable("loaiHinhThiDau");

            entity.Property(e => e.LoaiHinhThiDauId).HasPrecision(10);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("match");

            entity.HasIndex(e => e.LoaiHinhThiDauId, "IX_match_LoaiHinhThiDauId");

            entity.HasIndex(e => e.TournamentId, "IX_match_TournamentID");

            entity.Property(e => e.Date).HasPrecision(7);
            entity.Property(e => e.LoaiHinhThiDauId).HasPrecision(10);
            entity.Property(e => e.TournamentId)
                .HasPrecision(10)
                .HasColumnName("TournamentID");
            entity.Property(e => e.Trangthai)
                .HasPrecision(10)
                .HasColumnName("trangthai");

            entity.HasOne(d => d.LoaiHinhThiDau).WithMany(p => p.Matches).HasForeignKey(d => d.LoaiHinhThiDauId);

            entity.HasOne(d => d.Tournament).WithMany(p => p.Matches).HasForeignKey(d => d.TournamentId);
        });

        modelBuilder.Entity<Participant>(entity =>
        {
            entity.HasIndex(e => e.TournamentId, "IX_Participants_TournamentID");

            entity.HasIndex(e => new { e.UserId, e.TournamentId }, "IX_Participants_UserId_TournamentID").IsUnique();

            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Sdt).HasColumnName("sdt");
            entity.Property(e => e.TournamentId)
                .HasPrecision(10)
                .HasColumnName("TournamentID");
            entity.Property(e => e.Tuoi)
                .HasPrecision(10)
                .HasColumnName("tuoi");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Participants)
                .HasForeignKey(d => d.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.User).WithMany(p => p.Participants).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Socre>(entity =>
        {
            entity.HasKey(e => e.ScoreId);

            entity.ToTable("socre");

            entity.HasIndex(e => e.MatchId, "IX_socre_MatchId");

            entity.HasIndex(e => e.ParticipantId, "IX_socre_ParticipantId");

            entity.Property(e => e.ScoreId).HasPrecision(10);
            entity.Property(e => e.Kq).HasPrecision(3);

            entity.HasOne(d => d.Match).WithMany(p => p.Socres).HasForeignKey(d => d.MatchId);

            entity.HasOne(d => d.Participant).WithMany(p => p.Socres).HasForeignKey(d => d.ParticipantId);
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasIndex(e => e.LoaiHinhThiDauId, "IX_Tournaments_LoaiHinhThiDauId");

            entity.Property(e => e.TournamentId)
                .HasPrecision(10)
                .HasColumnName("TournamentID");
            entity.Property(e => e.EndDate).HasPrecision(7);
            entity.Property(e => e.LoaiHinhThiDauId).HasPrecision(10);
            entity.Property(e => e.Phithamgia).HasPrecision(10);
            entity.Property(e => e.StartDate).HasPrecision(7);

            entity.HasOne(d => d.LoaiHinhThiDau).WithMany(p => p.Tournaments)
                .HasForeignKey(d => d.LoaiHinhThiDauId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TournamentRanking>(entity =>
        {
            entity.HasIndex(e => e.TournamentId, "IX_TournamentRankings_TournamentId");

            entity.HasIndex(e => e.UserId, "IX_TournamentRankings_UserId");

            entity.Property(e => e.Id).HasPrecision(10);
            entity.Property(e => e.Losses).HasPrecision(10);
            entity.Property(e => e.MatchesPlayed).HasPrecision(10);
            entity.Property(e => e.Tier).HasPrecision(10);
            entity.Property(e => e.TournamentId).HasPrecision(10);
            entity.Property(e => e.UpdatedAt).HasPrecision(7);
            entity.Property(e => e.Wins).HasPrecision(10);

            entity.HasOne(d => d.Tournament).WithMany(p => p.TournamentRankings).HasForeignKey(d => d.TournamentId);

            entity.HasOne(d => d.User).WithMany(p => p.TournamentRankings).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<VTournamentAll>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_TOURNAMENT_ALL");

            entity.Property(e => e.EndDate).HasPrecision(7);
            entity.Property(e => e.Phithamgia).HasPrecision(10);
            entity.Property(e => e.StartDate).HasPrecision(7);
            entity.Property(e => e.TournamentId)
                .HasPrecision(10)
                .ValueGeneratedOnAdd()
                .HasColumnName("TournamentID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
