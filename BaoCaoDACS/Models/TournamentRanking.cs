using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BAO_CAO.Models; 

namespace BaoCaoDACS.Models
{
    public class TournamentRanking
    {
        [Key]
        public int Id { get; set; }
      
        public float Rating { get; set; } = 1000f;
        public int Tier { get; set; } = 0;

        public int MatchesPlayed { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public int TournamentId { get; set; }

        [ForeignKey(nameof(TournamentId))]
        public Tournament Tournament { get; set; }
    }
}
