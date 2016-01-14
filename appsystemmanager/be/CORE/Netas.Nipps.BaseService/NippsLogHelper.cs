using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Netas.Nipps.BaseService
{
    public sealed class NippsLogHelper
    {
        public static string TranslateZipFileName(string zipFileName, string fileExt, DateTime startDate, DateTime finishDate)
        {
            return zipFileName
                + "_" + fileExt.Replace(".", "")
                + "_" + startDate.ToShortDateString().Replace("/", "-").Replace("\\", "-")
                + "_" + finishDate.ToShortDateString().Replace("/", "-").Replace("\\", "-")
                + ".zip";
        }

        public static bool ZipFiles(string zipFileName, string sourcePath, string fileExt, DateTime startDate, DateTime finishDate)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            try
            {
                string zipFileNameFull = sourcePath + TranslateZipFileName(zipFileName, fileExt, startDate, finishDate);

                if (File.Exists(zipFileNameFull))
                    File.Delete(zipFileNameFull);

                using (FileStream zipArchiveStream = File.Create(zipFileNameFull))
                {
                    using (ZipArchive zipArchive = new ZipArchive(zipArchiveStream, ZipArchiveMode.Update))
                    {
                        foreach (string fileName in Directory.GetFiles(sourcePath))
                        {
                            if (fileName.EndsWith(fileExt) && Match(fileName, startDate, finishDate))
                            {
                                string entryName = fileName.Substring(sourcePath.Length);
                                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(entryName);

                                try
                                {
                                    using (FileStream fileStream = File.OpenRead(fileName))
                                    {
                                        byte[] buffer = new byte[fileStream.Length];
                                        fileStream.Read(buffer, 0, buffer.Length);

                                        try
                                        {
                                            using (Stream stream = zipArchiveEntry.Open())
                                            {
                                                stream.Write(buffer, 0, buffer.Length);
                                                stream.Close();
                                            }
                                        }
                                        catch (Exception ex) { logger.Error(ex); }
                                    }
                                }
                                catch (Exception ex) { logger.Error(ex); }
                            }
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex) { logger.Error(ex); }

            return false;
        }

        public static bool Match(string logFileName, DateTime logStartDate, DateTime logFinishDate)
        {
            string nlogDate = Regex.Match(logFileName, "\\.([0-9])\\w+-([0-9])\\w+-([0-9])\\w+\\.").Value;

            if (String.IsNullOrEmpty(nlogDate))
                return false;

            nlogDate = nlogDate.Replace(".", "");
            try
            {
                DateTime logDate = DateTime.Parse(nlogDate);

                if (logDate < logStartDate || logDate > logFinishDate)
                    return false;
            }
            catch (Exception ex) { return false; }

            return true;
        }

        public static List<String> ListLogFileNames()
        {
            List<String> logFiles = new List<string>();

            foreach (NLog.Targets.Target target in NLog.LogManager.Configuration.ConfiguredNamedTargets)
                if (target.GetType().Name.Equals("FileTarget"))
                {
                    NLog.Targets.FileTarget fileTarget = target as NLog.Targets.FileTarget;
                    logFiles.Add(fileTarget.FileName.Render(new NLog.LogEventInfo() { TimeStamp = DateTime.Now }).Replace("\\\\", "\\"));
                }

            return logFiles;
        }
    }
}