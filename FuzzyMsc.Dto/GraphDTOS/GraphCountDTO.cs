namespace FuzzyMsc.Dto.GraphDTOS
{
    public class GraphCountDTO
    {
        public int Normal { get; set; }
        /// <summary>
        /// Closure (Kapatma)
        /// </summary>
        public int Closure { get; set; }
        /// <summary>
        /// Fault (Fay)
        /// </summary>
        public int Fault { get; set; }
        public double successRate { get; set; }
    }
}
