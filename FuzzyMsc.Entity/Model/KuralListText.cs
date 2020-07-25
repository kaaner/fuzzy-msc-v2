namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("KuralListText")]
    public partial class KuralListText
    {
        public long KuralListTextID { get; set; }

        public long KuralID { get; set; }

        public string KuralText { get; set; }

        public virtual Kural Kural { get; set; }
    }
}
