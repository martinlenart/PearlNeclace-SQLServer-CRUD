using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using NecklaceModels;

namespace NecklaceDB
{
    public class NecklaceDbContext:DbContext
    {
        #region Uncomment to create the Data model
        public DbSet<Pearl> Pearls { get; set; }
        public DbSet<Necklace> Necklaces { get; set; }
        #endregion

        public NecklaceDbContext() { }
        public NecklaceDbContext(DbContextOptions options) : base(options)
        { }

        #region Uncomment to create the Data model
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = DBConnection.ConfigurationRoot.GetConnectionString("SQLServer_necklace");
                optionsBuilder.UseSqlServer(connectionString);
            }
            base.OnConfiguring(optionsBuilder);
        }
        #endregion
    }
}
