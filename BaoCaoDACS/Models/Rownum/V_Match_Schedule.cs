using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaoCaoDACS.Models
{
    public class V_Match_Schedule
    {
        public string MatchId { get; set; }
        public DateTime Date { get; set; }
        public string Vongdau { get; set; }
        public string Hangcan { get; set; }
        public string SanDau { get; set; }
        public int? trangthai { get; set; }
        public string TournamentName { get; set; }
        public string LoaiHinhThiDau { get; set; }

        // Oracle đã ghép sẵn 2 võ sĩ cho bạn
        public string FighterA_Name { get; set; }
        public string FighterA_Club { get; set; }
        public string FighterB_Name { get; set; }
        public string FighterB_Club { get; set; }
    }
}