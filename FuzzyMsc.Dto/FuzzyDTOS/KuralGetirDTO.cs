using FuzzyMsc.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class KuralGetirDTO
    {
        public KuralGetirDTO()
        {
            KuralListText = new List<KuralListText>();
            DegiskenList = new List<Degisken>();
            DegiskenItemList = new List<DegiskenItem>();
        }
        public Kural Kural { get; set; }
        public List<KuralListText> KuralListText { get; set; }
        public List<Degisken> DegiskenList { get; set; }
        public List<DegiskenItem> DegiskenItemList { get; set; }
    }
}
