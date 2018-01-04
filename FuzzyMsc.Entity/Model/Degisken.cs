namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Degisken")]
    public partial class Degisken
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Degisken()
        {
            KuralLists = new HashSet<KuralList>();
            KuralListItems = new HashSet<KuralListItem>();
        }

        public long DegiskenID { get; set; }

        public long KuralID { get; set; }

        [StringLength(250)]
        public string DegiskenAdi { get; set; }

        public int? MinDeger { get; set; }

        public int? MaxDeger { get; set; }

        public virtual Kural Kural { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralList> KuralLists { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralListItem> KuralListItems { get; set; }
    }
}
