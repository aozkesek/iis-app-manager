using System;
using System.Data.Entity;

namespace Netas.Nipps.LogManager.Data.Model
{
    public class LogDbContext : DbContext
    {
        public string LogSchemaName { get { return "log"; } }
        public DbSet<NippsModule> NippsModules { get; set; }
        public DbSet<NippsLog> NippsLogs { get; set; }

        public LogDbContext(String ConnectionName)
            : base(ConnectionName)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(LogSchemaName);
            base.OnModelCreating(modelBuilder);
        }
    }
}
