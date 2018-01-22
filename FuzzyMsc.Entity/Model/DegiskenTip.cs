namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DegiskenTip")]
    public partial class DegiskenTip
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DegiskenTip()
        {
            Degisken = new HashSet<Degisken>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte DegiskenTipID { get; set; }

        [Required]
        [StringLength(50)]
        public string DegiskenTipAdi { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Degisken> Degisken { get; set; }
    }
}
