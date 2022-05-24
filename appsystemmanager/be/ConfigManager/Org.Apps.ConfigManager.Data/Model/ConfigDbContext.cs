using System;
using System.Data.Entity;

namespace Org.Apps.ConfigManager.Data.Model
{
    public class ConfigDbContext : DbContext
    {
        public string ConfigSchemaName { get { return "conf"; } }

        public DbSet<SystemParameter> SystemParamaters { get; set; }

        public DbSet<ParameterCategory> ParamaterCategories { get; set; }

        public ConfigDbContext(String ConnectionName)
            : base(ConnectionName)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(ConfigSchemaName);
            base.OnModelCreating(modelBuilder);
        }
    }
}