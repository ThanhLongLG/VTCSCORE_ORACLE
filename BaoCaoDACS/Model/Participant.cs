using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class Participant
{
    public string ParticipantId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Club { get; set; } = null!;

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public float? CanNang { get; set; }

    public float? ChieuCao { get; set; }

    public int? Tuoi { get; set; }

    public string? Diachi { get; set; }

    public string? Thanhtoan { get; set; }

    public string? UserId { get; set; }

    public int? TournamentId { get; set; }

    public virtual ICollection<Socre> Socres { get; set; } = new List<Socre>();

    public virtual Tournament? Tournament { get; set; }

    public virtual AspNetUser? User { get; set; }
}
