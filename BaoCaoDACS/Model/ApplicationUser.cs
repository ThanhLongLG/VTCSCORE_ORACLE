using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BaoCaoDACS.Models;
using Microsoft.AspNetCore.Identity;

namespace BAO_CAO.Model
{
    public class ApplicationUser :IdentityUser
    {
        [Required]
        public string? Fullname {  get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }

        public ICollection<Participant> participant { get; set; } = new List<Participant>();

        public ICollection<TournamentRanking> TournamentRankings { get; set; } = new List<TournamentRanking>();


    }
}
