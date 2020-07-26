using FuzzyMsc.Dto.GraphDTOS;
using System.Collections.Generic;

namespace FuzzyMsc.Dto.HighchartsDTOS
{
    public class HighchartsDTO
    {
        public HighchartsDTO()
        {
            series = new List<SeriesDTO>();
            annotations = new List<AnnotationsDTO>();
            cizimBilgileri = new List<GraphDetailedDTO>();
        }
        public List<SeriesDTO> series { get; set; }
        public AxisDTO xAxis { get; set; }
        public AxisDTO yAxis { get; set; }
        public List<AnnotationsDTO> annotations { get; set; }
        public ParametersDTO parameters { get; set; }
        public GraphCountDTO sayilar { get; set; }
        public List<GraphDetailedDTO> cizimBilgileri { get; set; }
    }
}
