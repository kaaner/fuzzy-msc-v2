namespace FuzzyMsc.Dto.CizimDTOS
{
    public class GraphDTO
    {
        public ExcelModelDTO excel { get; set; }
        public long kuralID { get; set; }
        public OlcekDTO olcek { get; set; }
        public ParametersDTO parameters { get; set; }
        public CizimCountDTO sayilar { get; set; }
    }
}
