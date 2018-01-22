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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KuralList()
        {
            KuralListItem = new HashSet<KuralListItem>();
        }

        public long KuralListID { get; set; }

        public long KuralID { get; set; }

        public long? SonucDegiskenID { get; set; }

        public virtual Degisken Degisken { get; set; }

        public virtual Kural Kural { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralListItem> KuralListItem { get; set; }
    }
}
