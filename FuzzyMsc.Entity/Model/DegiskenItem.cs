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

        [StringLength(250)]
        public string DegiskenItemAdi { get; set; }

        [StringLength(250)]
        public string DegiskenItemGorunenAdi { get; set; }

        public int? MinDeger { get; set; }

        public int? MaxDeger { get; set; }

        public virtual Degisken Degisken { get; set; }
    }
}
