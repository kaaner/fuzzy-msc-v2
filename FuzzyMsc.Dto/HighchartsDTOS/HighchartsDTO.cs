﻿using FuzzyMsc.Dto.CizimDTOS;
using System.Collections.Generic;

namespace FuzzyMsc.Dto.HighchartsDTOS
{
    public class HighchartsDTO
    {
        public HighchartsDTO()
        {
            series = new List<SeriesDTO>();
            annotations = new List<AnnotationsDTO>();
            cizimBilgileri = new List<CizimDetailedDTO>();
        }
        public List<SeriesDTO> series { get; set; }
        public AxisDTO xAxis { get; set; }
        public AxisDTO yAxis { get; set; }
        public List<AnnotationsDTO> annotations { get; set; }
        public ParametersDTO parameters { get; set; }
        public CizimCountDTO sayilar { get; set; }
        public List<CizimDetailedDTO> cizimBilgileri { get; set; }
    }
}
