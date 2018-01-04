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
            DegiskenItems = new HashSet<DegiskenItem>();
            KuralLists = new HashSet<KuralList>();
            KuralListItems = new HashSet<KuralListItem>();
        }

        public long DegiskenID { get; set; }

        public long KuralID { get; set; }

        public byte DegiskenTipID { get; set; }

        [StringLength(250)]
        public string DegiskenAdi { get; set; }

        [StringLength(250)]
        public string DegiskenGorunenAdi { get; set; }

        public virtual DegiskenTip DegiskenTip { get; set; }

        public virtual Kural Kural { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DegiskenItem> DegiskenItems { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralList> KuralLists { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralListItem> KuralListItems { get; set; }
    }
}
