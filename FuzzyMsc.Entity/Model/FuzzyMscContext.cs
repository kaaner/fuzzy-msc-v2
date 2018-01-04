namespace FuzzyMsc.Entity.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FuzzyMscContext : DbContext
    {
        public FuzzyMscContext()
            : base("name=FuzzyMscContext")
        {
        }

        public virtual DbSet<Degisken> Degiskens { get; set; }
        public virtual DbSet<Kullanici> Kullanicis { get; set; }
        public virtual DbSet<Kural> Kurals { get; set; }
        public virtual DbSet<KuralList> KuralLists { get; set; }
        public virtual DbSet<KuralListItem> KuralListItems { get; set; }
        public virtual DbSet<KuralListText> KuralListTexts { get; set; }
        public virtual DbSet<Rol> Rols { get; set; }
        public virtual DbSet<KullaniciRol> KullaniciRols { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Degisken>()
                .HasMany(e => e.KuralLists)
                .WithOptional(e => e.Degisken)
                .HasForeignKey(e => e.SonucDegiskenID);

            modelBuilder.Entity<Kullanici>()
                .HasMany(e => e.KullaniciRols)
                .WithRequired(e => e.Kullanici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kural>()
                .HasMany(e => e.Degiskens)
                .WithRequired(e => e.Kural)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kural>()
                .HasMany(e => e.KuralLists)
                .WithRequired(e => e.Kural)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kural>()
                .HasMany(e => e.KuralListTexts)
                .WithRequired(e => e.Kural)
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
