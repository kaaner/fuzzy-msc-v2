using Proje.Entity.Map;
using Proje.Entity.Model;
using Proje.Pattern.EF6;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proje.Entity
{
    public class ProjeContext : DataContext
    {
        public ProjeContext() : base("name=ProjeContext")
        {

        }


        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Rol> Roller { get; set; }
        public DbSet<KullaniciRolMntm> KullaniciRolMntm { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            modelBuilder.Configurations.Add(new KullaniciMap());
            modelBuilder.Configurations.Add(new RolMap());
            modelBuilder.Configurations.Add(new KullaniciRolMntmMap());

            base.OnModelCreating(modelBuilder);
        }
    }
}
