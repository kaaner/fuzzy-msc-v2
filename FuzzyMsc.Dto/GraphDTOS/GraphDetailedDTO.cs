using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto.GraphDTOS
{
    public class GraphDetailedDTO
    {
        public string FirstNode { get; set; }
        public string SecondNode { get; set; }
        public string Connection { get; set; }
        public bool Normal { get; set; }
        /// <summary>
        /// Closure (Kapatma)
        /// </summary>
        public bool Closure { get; set; }
        /// <summary>
        /// Fault (Fay)
        /// </summary>
        public bool Fault { get; set; }
    }
}
