using System.Collections.Generic;

namespace FuzzyMsc.Dto.GraphDTOS
{
    public class SectionDTO
    {
        public SectionDTO()
        {
            ResistivityGeneralList = new List<List<ResistivityDTO>>();
            SeismicGeneralList = new List<List<SeismicDTO>>();
            DrillGeneralList = new List<List<DrillDTO>>();
        }
        public List<List<ResistivityDTO>> ResistivityGeneralList { get; set; }
        public List<List<SeismicDTO>> SeismicGeneralList { get; set; }
        public List<List<DrillDTO>> DrillGeneralList { get; set; }
    }
}
