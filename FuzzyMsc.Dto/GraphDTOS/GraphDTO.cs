namespace FuzzyMsc.Dto.GraphDTOS
{
    public class GraphDTO
    {
        public ExcelModelDTO excel { get; set; }
        public long ruleId { get; set; }
        public ScaleDTO scale { get; set; }
        public ParametersDTO parameters { get; set; }
        public GraphCountDTO numbers { get; set; }
    }
}
