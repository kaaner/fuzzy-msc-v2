using FuzzyMsc.Entity.Model;
using System.Data.Entity.ModelConfiguration;

namespace FuzzyMsc.Entity.Map
{
    public class RolMap : EntityTypeConfiguration<Rol>
    {
        public RolMap()
        {
            //Primary Key
            this.HasKey(t => t.RolID);



            //Properties
            this.Property(t => t.RolAdi)
                .HasMaxLength(50);

        }
    }
}
