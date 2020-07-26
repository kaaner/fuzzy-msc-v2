using FuzzyMsc.Entity.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class GetRuleDTO
    {
        public GetRuleDTO()
        {
            RuleListText = new List<KuralListText>();
            VariableList = new List<Degisken>();
            VariableItemList = new List<DegiskenItem>();
        }
        public Kural Kural { get; set; }
        public List<KuralListText> RuleListText { get; set; }
        public List<Degisken> VariableList { get; set; }
        public List<DegiskenItem> VariableItemList { get; set; }
    }
}
