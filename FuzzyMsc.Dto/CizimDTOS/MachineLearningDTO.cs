using System.Collections.Generic;

namespace FuzzyMsc.Dto.CizimDTOS
{
	public class MachineLearningDTO
	{
		public MachineLearningDTO()
		{
			features = new List<string>();
			droppedFeatures = new List<string>();
		}
		public ExcelModelDTO excel { get; set; }
		public string algorithm { get; set; }
		public List<string> features { get; set; }
		public List<string> droppedFeatures { get; set; }
	}
}
