﻿namespace FuzzyMsc.Dto.GraphDTOS
{
    public class DrillDTO
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double? K { get; set; }
        public string T { get; set; }
        public bool Checked { get; set; }
    }
}