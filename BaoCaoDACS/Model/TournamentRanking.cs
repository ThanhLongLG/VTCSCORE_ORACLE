using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class TournamentRanking
{
    public int Id { get; set; }

    public float Rating { get; set; }

    public int Tier { get; set; }

    public int MatchesPlayed { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UserId { get; set; } = null!;

    public int TournamentId { get; set; }

    public virtual Tournament Tournament { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
