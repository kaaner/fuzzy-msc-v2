using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto.CizimDTOS
{
    public class ParametersDTO
    {
        public string Baslik { get; set; }
        public int? OzdirencOran { get; set; }
        public int? SismikOran { get; set; }
        public int? OlcekX { get; set; }
        public int? OlcekY { get; set; }
        public int? CozunurlukX { get; set; }
        public int? CozunurlukY { get; set; }
        public bool? CizimlerGorunsunMu { get; set; }
    }
}
