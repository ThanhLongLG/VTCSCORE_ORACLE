using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaoCaoDACS.Models
{
    public class MatchTrainingSample
    {

        public float FighterA_Weight { get; set; }
        public float FighterA_Height { get; set; }
        public int FighterA_Age { get; set; }

        public float FighterB_Weight { get; set; }
        public float FighterB_Height { get; set; }
        public int FighterB_Age { get; set; }

        public float FighterA_Rating { get; set; }
        public float FighterB_Rating { get; set; }
        public float RatingDiff { get; set; }


        public int LoaiHinhThiDauId { get; set; }
        public string HangCan { get; set; } = "unknown";
        public string VongDau { get; set; } = "unknown";
        public float DiffWeight { get; set; }
        public float DiffHeight { get; set; }
        public float DiffAge { get; set; }

        public bool AWins { get; set; }
    }

}
