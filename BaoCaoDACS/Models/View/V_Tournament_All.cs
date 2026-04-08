using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Models
{
    public partial class V_Tournament_All
    {
        public int TournamentID { get; set; }

        public string Name { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Location { get; set; } = null!;

        public string HinhThucThiDau { get; set; } = null!;

        public string Status { get; set; } = null!;

        public int Phithamgia { get; set; }

        public string LoaiHinhName { get; set; } = null!;

        public string MonVo { get; set; } = null!;
    }

}

