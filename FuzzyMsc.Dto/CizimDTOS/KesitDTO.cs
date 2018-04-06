using System.Collections.Generic;

namespace FuzzyMsc.Dto.CizimDTOS
{
    public class KesitDTO
    {
        public KesitDTO()
        {
            RezGenelList = new List<List<RezistiviteDTO>>();
            SisGenelList = new List<List<SismikDTO>>();
            SonGenelList = new List<List<SondajDTO>>();
        }
        public List<List<RezistiviteDTO>> RezGenelList { get; set; }
        public List<List<SismikDTO>> SisGenelList { get; set; }
        public List<List<SondajDTO>> SonGenelList { get; set; }
    }
}
