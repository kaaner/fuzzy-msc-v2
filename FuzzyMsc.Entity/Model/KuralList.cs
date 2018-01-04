namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("KuralList")]
    public partial class KuralList
    {
        public long KuralListID { get; set; }

        public long KuralID { get; set; }

        public long? SonucDegiskenID { get; set; }

        public virtual Degisken Degisken { get; set; }

        public virtual Kural Kural { get; set; }
    }
}
