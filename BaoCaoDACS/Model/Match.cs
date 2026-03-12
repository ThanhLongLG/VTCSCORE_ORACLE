using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class Match
{
    public string MatchId { get; set; } = null!;

    public string Vongdau { get; set; } = null!;

    public string SanDau { get; set; } = null!;

    public string Hangcan { get; set; } = null!;

    public string Trongtai { get; set; } = null!;

    public int? Trangthai { get; set; }

    public DateTime Date { get; set; }

    public int LoaiHinhThiDauId { get; set; }

    public int? TournamentId { get; set; }

    public virtual LoaiHinhThiDau LoaiHinhThiDau { get; set; } = null!;

    public virtual ICollection<Socre> Socres { get; set; } = new List<Socre>();

    public virtual Tournament? Tournament { get; set; }
}
