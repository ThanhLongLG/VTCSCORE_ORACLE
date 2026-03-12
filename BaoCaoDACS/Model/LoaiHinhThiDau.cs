using System;
using System.Collections.Generic;

namespace BaoCaoDACS.Model;

public partial class LoaiHinhThiDau
{
    public int LoaiHinhThiDauId { get; set; }

    public string Name { get; set; } = null!;

    public string MonVo { get; set; } = null!;

    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();

    public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
}
