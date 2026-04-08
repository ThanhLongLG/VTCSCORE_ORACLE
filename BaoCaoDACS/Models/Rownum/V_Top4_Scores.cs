using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoDACS.Models
{
    public class V_Match_Prediction
    {
        public string MatchId { get; set; }
        public DateTime Date { get; set; }
        public int? TournamentID { get; set; }
        public int? LoaiHinhThiDauId { get; set; }
        public string Hangcan { get; set; }
        public string Vongdau { get; set; }

        public float FighterA_Weight { get; set; }
        public float FighterA_Height { get; set; }
        public int FighterA_Age { get; set; }
        public float FighterA_Rating { get; set; }

        public float FighterB_Weight { get; set; }
        public float FighterB_Height { get; set; }
        public int FighterB_Age { get; set; }
        public float FighterB_Rating { get; set; }
    }
}