using System.Collections.Generic;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class KuralTextVeOzdirencDTO
    {
        public KuralTextVeOzdirencDTO()
        {
            kuralTextList = new List<KuralTextEntityDTO>();
            ozdirencList = new List<DegiskenDTO>();
        }
        public List<KuralTextEntityDTO> kuralTextList { get; set; }
        public List<DegiskenDTO> ozdirencList { get; set; }
    }
}
