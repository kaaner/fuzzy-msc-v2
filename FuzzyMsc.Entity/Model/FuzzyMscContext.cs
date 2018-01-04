namespace FuzzyMsc.Entity.Model
{
    using FuzzyMsc.Pattern.EF6;
    using System.Data.Entity;

    public partial class FuzzyMscContext : DataContext
    {
        public FuzzyMscContext()
            : base("name=FuzzyMscContext")
        {
        }

        public virtual DbSet<Kullanici> Kullanicis { get; set; }
        public virtual DbSet<Rol> Rols { get; set; }
        public virtual DbSet<KullaniciRol> KullaniciRols { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kullanici>()
                .HasMany(e => e.KullaniciRols)
                .WithRequired(e => e.Kullanici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rol>()
                .Property(e => e.RolAdi)
                .IsFixedLength();

            modelBuilder.Entity<Rol>()
                .HasMany(e => e.KullaniciRols)
                .WithRequired(e => e.Rol)
                .WillCascadeOnDelete(false);
        }
    }
}
