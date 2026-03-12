using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class Socre
{
    public int ScoreId { get; set; }

    public float? Diem { get; set; }

    public byte? Kq { get; set; }

    public string? KietQua { get; set; }

    public string? Danhgia { get; set; }

    public string ParticipantId { get; set; } = null!;

    public string MatchId { get; set; } = null!;

    public virtual Match Match { get; set; } = null!;

    public virtual Participant Participant { get; set; } = null!;
}
