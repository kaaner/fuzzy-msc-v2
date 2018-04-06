using System.Collections.Generic;

namespace FuzzyMsc.Dto.HighchartsDTOS
{
    public class HighchartsDTO
    {
        public HighchartsDTO()
        {
            series = new List<SeriesDTO>();
            annotations = new List<AnnotationsDTO>();
        }
        public List<SeriesDTO> series { get; set; }
        public AxisDTO xAxis { get; set; }
        public AxisDTO yAxis { get; set; }
        public List<AnnotationsDTO> annotations { get; set; }
    }
}
