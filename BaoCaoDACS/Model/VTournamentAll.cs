using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class VTournamentAll
{
    public int TournamentId { get; set; }

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
