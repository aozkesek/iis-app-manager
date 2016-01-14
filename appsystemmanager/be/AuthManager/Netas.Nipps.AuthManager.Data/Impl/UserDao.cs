using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using NLog;

using Netas.Nipps.Aspect;
using Netas.Nipps.BaseDao;
using Netas.Nipps.AuthManager.Data.Model;


namespace Netas.Nipps.AuthManager.Data.Impl
{
    
    public class UserDao : AbstractDao<User, AuthDbContext>
    {
        private static Logger mLogger = NLog.LogManager.GetLogger("UserDao");
        public const string ModuleName = "AuthManager";
        public const string ExceptionTypes = "SqlException;";

        #region abstract method implementation
        public override IQueryable<User> GetTQuery(AuthDbContext dbContext, int id)
        {
            return from u in dbContext.Users
                   where u.UserId == id
                   select u;
        }
        public override IQueryable<User> GetTQuery(AuthDbContext dbContext, User t)
        {
            return from u in dbContext.Users
                   where u.UserId == t.UserId
                   select u;
        }
        public override IQueryable<User> GetTByNameQuery(AuthDbContext dbContext, string name)
        {
            return from u in dbContext.Users
                   where u.UserName == name
                   select u;
        }
        public override IQueryable<User> UpdateTQuery(AuthDbContext dbContext, User t)
        {
            return from u in dbContext.Users
                   where u.UserId == t.UserId
                   select u;
        }
        public override IQueryable<User> ListTQuery(AuthDbContext dbContext)
        {
            return from u in dbContext.Users
                   orderby u.UserId
                   select u;
        }
        public override DbSet SetT(AuthDbContext dbContext)
        {
            return dbContext.Users;
        }
        public override void UpdateFrom(User tDest, User tSource)
        {
            tDest.UpdateDate = DateTime.Now;
            tDest.Email = tSource.Email;
            tDest.FirstName = tSource.FirstName;
            tDest.LastName = tSource.LastName;
            tDest.InvalidAttemptCount = tSource.InvalidAttemptCount;
            tDest.LastInvalidAttempt = tSource.LastInvalidAttempt;
            tDest.LastSuccessAttempt = tSource.LastSuccessAttempt;
            tDest.PasswordHash = tSource.PasswordHash;
            tDest.PasswordUpdateDate = tSource.PasswordUpdateDate;
            tDest.UserId = tSource.UserId;
            tDest.UserName = tSource.UserName;
        }
        public override Logger GetTLogger()
        {
            return mLogger;
        }
        public override object GetTId(User t)
        {
            return t.UserId;
        }
        public override AuthDbContext NewDbContext()
        {
            return new AuthDbContext(ConnectionName);
        }
        #endregion

        [PerformanceLoggingAdvice]
        public override User Get(int id)
        {
            return base.Get(id);
        }
        
        [PerformanceLoggingAdvice]
        public override User GetByName(string name)
        {
            return base.GetByName(name);
        }

        [PerformanceLoggingAdvice]
        public override User GetByT(User user)
        {
            return base.GetByT(user);
        }

        [PerformanceLoggingAdvice]
        public override List<User> List()
        {
            return base.List();
        }

        [PerformanceLoggingAdvice]
        public override List<User> List(int pageNo)
        {
            return base.List(pageNo);
        }

        [PerformanceLoggingAdvice]
        public override void Add(User t)
        {
            t.CreateDate = DateTime.Now;
            t.UpdateDate = t.CreateDate;
            t.LastInvalidAttempt = t.CreateDate;
            t.LastSuccessAttempt = t.CreateDate;
            t.PasswordUpdateDate = t.CreateDate;
            t.InvalidAttemptCount = 0;
            base.Add(t);
        }

        [PerformanceLoggingAdvice]
        public override void Update(User t)
        {
            base.Update(t);
        }

        [PerformanceLoggingAdvice]
        public override void Remove(User t)
        {
            base.Remove(t);
        }

        public override IQueryable<User> ListTQueryByName(AuthDbContext dbContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}
