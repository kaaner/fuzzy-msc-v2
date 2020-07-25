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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DegiskenItem()
        {
            KuralListItems = new HashSet<KuralListItem>();
        }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<KuralListItem> KuralListItems { get; set; }
    }
}
