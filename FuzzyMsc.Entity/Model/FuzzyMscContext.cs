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

        public virtual DbSet<Degisken> Degisken { get; set; }
        public virtual DbSet<DegiskenItem> DegiskenItem { get; set; }
        public virtual DbSet<DegiskenTip> DegiskenTip { get; set; }
        public virtual DbSet<Kullanici> Kullanici { get; set; }
        public virtual DbSet<Kural> Kural { get; set; }
        public virtual DbSet<KuralList> KuralList { get; set; }
        public virtual DbSet<KuralListItem> KuralListItem { get; set; }
        public virtual DbSet<KuralListText> KuralListText { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<KullaniciRol> KullaniciRol { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Degisken>()
                .HasMany(e => e.DegiskenItem)
                .WithRequired(e => e.Degisken)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Degisken>()
                .HasMany(e => e.KuralList)
                .WithOptional(e => e.Degisken)
                .HasForeignKey(e => e.SonucDegiskenID);

            modelBuilder.Entity<DegiskenTip>()
                .HasMany(e => e.Degisken)
                .WithRequired(e => e.DegiskenTip)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kullanici>()
                .HasMany(e => e.KullaniciRol)
                .WithRequired(e => e.Kullanici)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kural>()
                .HasMany(e => e.Degisken)
                .WithRequired(e => e.Kural)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kural>()
                .HasMany(e => e.KuralList)
                .WithRequired(e => e.Kural)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Kural>()
                .HasMany(e => e.KuralListText)
                .WithRequired(e => e.Kural)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<KuralList>()
                .HasMany(e => e.KuralListItem)
                .WithRequired(e => e.KuralList)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Rol>()
                .Property(e => e.RolAdi)
                .IsFixedLength();

            modelBuilder.Entity<Rol>()
                .HasMany(e => e.KullaniciRol)
                .WithRequired(e => e.Rol)
                .WillCascadeOnDelete(false);
        }
    }
}
