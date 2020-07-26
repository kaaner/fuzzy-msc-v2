using System;
using System.Collections.Generic;

namespace FuzzyMsc.Dto.FuzzyDTOS
{
    public class FuzzyRuleDTO
    {
        /// <summary>
        /// Resistivity (Özdirenç)
        /// </summary>
        public string Resistivity { get; set; }
        /// <summary>
        /// Resistance (Mukavemet)
        /// </summary>
        public int? Resistance { get; set; }
        /// <summary>
        /// Saturation (Doygunluk)
        /// </summary>
        public int? Saturation { get; set; }
        public int? Operator { get; set; }
        /// <summary>
        /// Equality (Eşitlik)
        /// </summary>
        public int? Equality { get; set; }
    }

    public class FuzzyRuleListDTO
    {
        public string Result { get; set; }
        public List<FuzzyRuleDTO> Rules { get; set; }
    }

    public class RuleClusterDTO {
        public string RuleName { get; set; }
        public List<RuleListDTO> RuleList { get; set; }
        public List<VariableDTO> ResistivityList { get; set; }
        public List<VariableDTO> SoilList { get; set; }
    }

    public class RuleListDTO
    {
        public string Text { get; set; }
        public RuleDTO Rule { get; set; }
    }

    public class RuleDTO
    {
        public string Resistivity { get; set; }
        public string Soil { get; set; }
    }

    public class RuleEntityDTO
    {
        public long RuleId { get; set; }
        public string RuleName { get; set; }
        public DateTime? AddDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class RuleTextEntityDTO
    {
        public long RuleId { get; set; }
        public string RuleText { get; set; }

    }
}
