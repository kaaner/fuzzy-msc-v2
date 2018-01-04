using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class FuzzyKuralDTO
    {
        public string Ozdirenc { get; set; }
        public int? Mukavemet { get; set; }
        public int? Doygunluk { get; set; }
        public int? Operator { get; set; }
        public int? Esitlik { get; set; }
    }

    public class FuzzyKuralListDTO
    {
        public string Sonuc { get; set; }
        public List<FuzzyKuralDTO> Kurallar { get; set; }
    }

    public class KuralKumeDTO {
        public string KumeAdi { get; set; }
        public List<FuzzyKuralListDTO> KuralList { get; set; }
        public List<DegiskenDTO> OzdirencList { get; set; }
        public List<DegiskenDTO> ToprakList { get; set; }
    }
}
