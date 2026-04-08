using BAO_CAO.Models;
using BaoCaoDACS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; // Thêm using này
using System.Text.Json; // Thêm using này

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Participant>()
        .HasIndex(p => new { p.UserId, p.TournamentID })
        .IsUnique();

        modelBuilder.Entity<Participant>()
        .HasOne(p => p.Tournament)
        .WithMany(t => t.participant)
        .HasForeignKey(p => p.TournamentID)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tournament>()
       .HasOne(t => t.LoaiHinhThiDau)
       .WithMany(l => l.tournament)
       .HasForeignKey(t => t.LoaiHinhThiDauId)
       .OnDelete(DeleteBehavior.Restrict);


        // Cấu hình ValueConverter cho ImageUrls
        modelBuilder.Entity<Tournament>()
            .Property(t => t.ImageUrls)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null), // Chuyển List<string> thành chuỗi JSON
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null), // Chuyển chuỗi JSON thành List<string>
                new ValueComparer<List<string>>( // So sánh List<string>
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));


        modelBuilder.Entity<V_Tournament_All>(entity =>
        {
            entity.HasNoKey(); 
            entity.ToView("V_TOURNAMENT_ALL", "NHOM4"); 

        });
        modelBuilder.Entity<V_Top4_Scores>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("V_TOP4_SCORES"); 
        });

        modelBuilder.Entity<V_Match_Schedule>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("V_MATCH_SCHEDULE");
        });

        modelBuilder.Entity<V_Match_Prediction>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("V_MATCH_PREDICTION");
        });

        modelBuilder.Entity<V_Match_All>().ToView("V_MATCH_ALL");        

    }
    public DbSet<V_Match_All> V_Match_Alls { get; set; }
    public DbSet<V_Match_Prediction> V_Match_Predictions { get; set; }
    public DbSet<V_Match_Schedule> V_Match_Schedules { get; set; }
    public DbSet<V_Top4_Scores> V_Top4_Scores { get; set; }
    public virtual DbSet<V_Tournament_All> V_Tournament_All { get; set; }

    //bảng
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Match> match { get; set; }
    public DbSet<Socre> socre { get; set; }
    public DbSet<LoaiHinhThiDau> loaiHinhThiDau { get; set; }
    public DbSet<TournamentRanking> TournamentRankings { get; set; }


}
