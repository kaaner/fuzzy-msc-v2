using Proje.Entity.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proje.Entity.Map
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
