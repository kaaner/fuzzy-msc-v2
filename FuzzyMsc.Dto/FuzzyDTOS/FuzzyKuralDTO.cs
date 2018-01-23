using System.Collections.Generic;

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
        public List<KuralListDTO> KuralList { get; set; }
        public List<DegiskenDTO> OzdirencList { get; set; }
        public List<DegiskenDTO> ToprakList { get; set; }
    }

    public class KuralListDTO
    {
        public string Text { get; set; }
        public KuralDTO Kural { get; set; }
    }

    public class KuralDTO
    {
        public string Ozdirenc { get; set; }
        public string Toprak { get; set; }

    }
}
