using System;
using System.Data.Entity;

namespace Netas.Nipps.AuthManager.Data.Model
{
    public class AuthDbContext : DbContext
    {

        public string AuthSchemaName { get { return "auth"; } }
        public DbSet<User> Users { get; set; }

        public AuthDbContext(String ConnectionName)
            : base(ConnectionName)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(AuthSchemaName);
            base.OnModelCreating(modelBuilder);
        }

    }
}
