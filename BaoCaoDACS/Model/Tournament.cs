using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class Tournament
{
    public int TournamentId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Location { get; set; } = null!;

    public string HinhThucThiDau { get; set; } = null!;

    public string DoiTuongThamGia { get; set; } = null!;

    public string QuyMoiaiDa { get; set; } = null!;

    public string BanToChuc { get; set; } = null!;

    public int Phithamgia { get; set; }

    public string Status { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? ImageUrls { get; set; }

    public int LoaiHinhThiDauId { get; set; }

    public virtual LoaiHinhThiDau LoaiHinhThiDau { get; set; } = null!;

    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public virtual ICollection<TournamentRanking> TournamentRankings { get; set; } = new List<TournamentRanking>();
}
