namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("KuralListItem")]
    public partial class KuralListItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long KuralListItemID { get; set; }

        public long KuralListID { get; set; }

        public long? DegiskenID { get; set; }

        public long? Mukavemet { get; set; }

        public long? Doygunluk { get; set; }

        public long? Operator { get; set; }

        public long? Esitlik { get; set; }

        public virtual Degisken Degisken { get; set; }
    }
}
