namespace FuzzyMsc.Dto.GraphDTOS
{
	public class ParametersDTO
    {
        public string Title { get; set; }
        public int? ResistivityRatio { get; set; }
        public int? SeismicRatio { get; set; }
        public int? ScaleX { get; set; }
        public int? ScaleY { get; set; }
        public int? ResolutionX { get; set; }
        public int? ResolutionY { get; set; }
        public bool? IsGraphsVisible { get; set; }
    }
}
