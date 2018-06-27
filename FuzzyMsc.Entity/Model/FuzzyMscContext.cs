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

        public virtual DbSet<Degisken> Degiskens { get; set; }
        public virtual DbSet<DegiskenItem> DegiskenItems { get; set; }
        public virtual DbSet<DegiskenTip> DegiskenTips { get; set; }
        public virtual DbSet<Kullanici> Kullanicis { get; set; }
        public virtual DbSet<Kural> Kurals { get; set; }
        public virtual DbSet<KuralList> KuralLists { get; set; }
        public virtual DbSet<KuralListItem> KuralListItems { get; set; }
        public virtual DbSet<KuralListText> KuralListTexts { get; set; }
        public virtual DbSet<Rol> Rols { get; set; }
        public virtual DbSet<KullaniciRol> KullaniciRols { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<FuzzyMscContext>(null); //Migration Sonrasý Eklendi 
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Degisken>()
                .HasMany(e => e.DegiskenItems)
                .WithRequired(e => e.Degisken)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DegiskenTip>()
                .HasMany(e => e.Degiskens)
                .WithRequired(e => e.DegiskenTip)
                .WillCascadeOnDelete(false);

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

            modelBuilder.Entity<KuralList>()
                .HasMany(e => e.KuralListItems)
                .WithRequired(e => e.KuralList)
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
