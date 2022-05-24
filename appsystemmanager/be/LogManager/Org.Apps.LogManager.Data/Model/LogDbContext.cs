using System;
using System.Data.Entity;

namespace Org.Apps.LogManager.Data.Model
{
    public class LogDbContext : DbContext
    {
        public string LogSchemaName { get { return "log"; } }
        public DbSet<AppsModule> AppsModules { get; set; }
        public DbSet<AppsLog> AppsLogs { get; set; }

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
