namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DegiskenItem")]
    public partial class DegiskenItem
    {
        public long DegiskenItemID { get; set; }

        public long DegiskenID { get; set; }

        [Required]
        [StringLength(250)]
        public string DegiskenItemAdi { get; set; }

        [Required]
        [StringLength(250)]
        public string DegiskenItemGorunenAdi { get; set; }

        public double MinDeger { get; set; }

        public double MaxDeger { get; set; }

        public virtual Degisken Degisken { get; set; }
    }
}
