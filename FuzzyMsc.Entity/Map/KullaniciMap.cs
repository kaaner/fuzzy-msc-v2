using FuzzyMsc.Entity.Model;
using System.Data.Entity.ModelConfiguration;

namespace FuzzyMsc.Entity.Map
{
    public class KullaniciMap : EntityTypeConfiguration<Kullanici>
    {
        public KullaniciMap()
        {

            //Primary Key
            this.HasKey(t => t.KullaniciID);


            //Properties
            this.Property(t => t.Ad)
                .IsRequired()
                .HasMaxLength(50);


            this.Property(t => t.Soyad)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.TcNo)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(11);

            this.Property(t => t.Telefon)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(11);

            this.Property(t => t.Sifre)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.Eposta)
                .IsRequired()
                .HasMaxLength(50);


            //Relationships




        }
    }
}
