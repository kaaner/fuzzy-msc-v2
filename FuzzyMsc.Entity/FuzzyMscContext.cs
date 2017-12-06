using FuzzyMsc.Entity.Map;
using FuzzyMsc.Entity.Model;
using FuzzyMsc.Pattern.EF6;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyMsc.Entity
{
    public class FuzzyMscContext : DataContext
    {
        public FuzzyMscContext() : base("name=ProjeContext")
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
