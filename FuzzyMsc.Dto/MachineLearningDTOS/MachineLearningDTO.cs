using System.Collections.Generic;

namespace FuzzyMsc.Dto.MachineLearningDTOS
{
	public class MachineLearningDTO
	{
		public MachineLearningDTO()
		{
			features = new List<string>();
			droppedFeatures = new List<string>();
		}
		public string path { get; set; }
		public string algorithm { get; set; }
		public List<string> allFeatures { get; set; }
		public List<string> features { get; set; }
		public List<string> droppedFeatures { get; set; }
		public string target { get; set; }
	}
}
