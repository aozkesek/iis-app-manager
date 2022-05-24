using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Org.Apps.BaseDao
{
    public sealed class BaseDaoHelper
    {
        private const string GO_SEPARATOR = "\nGO";

        public static void ExecuteUpgradeScript(string basePath)
        {
            string sqlScriptName = basePath + "upgrade.sql";

            if (!File.Exists(sqlScriptName))
                return;

            using (FileStream sqlScriptStream = File.OpenRead(sqlScriptName))
            {
                if (sqlScriptStream.Length < 1)
                    throw new FileNotFoundException();

                byte[] readBuffer = new byte[sqlScriptStream.Length];
                sqlScriptStream.Read(readBuffer, 0, (int)sqlScriptStream.Length);
                string sqlScript = ASCIIEncoding.ASCII.GetString(readBuffer);

                Match goMatch = Regex.Match(sqlScript, GO_SEPARATOR);

                if (!goMatch.Success)
                    throw new InvalidDataException();

                using (System.Data.Entity.DbContext upgradeDbContext = new System.Data.Entity.DbContext("UpgradeDb"))
                {
                    System.Data.Entity.DbContextTransaction upgradeTransaction = upgradeDbContext.Database.BeginTransaction();
                    int startIndex = 0;

                    try
                    {
                        while (goMatch.Success)
                        {
                            string sqlCommand = sqlScript.Substring(startIndex, goMatch.Index - startIndex);

                            upgradeDbContext.Database.ExecuteSqlCommand(sqlCommand);

                            startIndex = goMatch.Index + GO_SEPARATOR.Length;
                            goMatch = goMatch.NextMatch();
                        }

                        upgradeTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        upgradeTransaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
    }
}