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
            DegiskenItem = new HashSet<DegiskenItem>();
            KuralList = new HashSet<KuralList>();
            KuralListItem = new HashSet<KuralListItem>();
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
        public virtual ICollection<DegiskenItem> DegiskenItem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralList> KuralList { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralListItem> KuralListItem { get; set; }
    }
}
