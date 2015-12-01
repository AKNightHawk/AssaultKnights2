namespace DAL
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;

	public partial class DBM : DbContext
	{
		public DBM()
			: base( "name=DBM" )
		{
		}

		public virtual DbSet<phpap_akusers> phpap_akusers { get; set; }
		public virtual DbSet<phpap_akservers> phpap_akservers { get; set; }
		public virtual DbSet<phpap_akunits> phpap_akunits { get; set; }

		protected override void OnModelCreating( DbModelBuilder modelBuilder )
		{
			modelBuilder.Entity<phpap_akusers>()
				.Property( e => e.Username )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akusers>()
				.Property( e => e.Password )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akusers>()
				.Property( e => e.Email )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akusers>()
				.Property( e => e.Name )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akservers>()
				.Property( e => e.ServerIP )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akservers>()
				.Property( e => e.ServerName )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akservers>()
				.Property( e => e.ServerPassword )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akservers>()
				.Property( e => e.ServerMapname )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akunits>()
				.Property( e => e.UnitName )
				.IsUnicode( false );

			modelBuilder.Entity<phpap_akunits>()
				.Property( e => e.UserName )
				.IsUnicode( false );
		}
	}
}
