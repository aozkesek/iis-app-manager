using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Netas.Nipps.BaseService
{
    public sealed class NippsZipHelper
    {
        public static void CreatePathFolder(string path)
        {
            StringBuilder folder = new StringBuilder();
            string[] folders = path.Split('\\');
            Array.Resize(ref folders, folders.Length - 1);

            foreach (string folderPart in folders)
                if (!Directory.Exists(folder.Append(folderPart + "\\").ToString()))
                    Directory.CreateDirectory(folder.ToString());
        }

        public static void Unzip(string basePath, byte[] packageZipBuffer)
        {
            using (Stream streamPackageZip = new MemoryStream(packageZipBuffer))
            {
                //decompress zip
                using (var zipReader = SharpCompress.Reader.ReaderFactory.Open(streamPackageZip))
                {
                    while (zipReader.MoveToNextEntry())
                    {
                        if (!zipReader.Entry.IsDirectory)
                        {
                            //delete if exist
                            if (File.Exists(basePath + zipReader.Entry.Key))
                                File.Delete(basePath + zipReader.Entry.Key);

                            //make sure all folder and sub folder is exist, if not create
                            CreatePathFolder(basePath + zipReader.Entry.Key);

                            //then recreate new
                            using (Stream entryStream = File.Create(basePath + zipReader.Entry.Key))
                            {
                                zipReader.WriteEntryTo(entryStream);
                                entryStream.Close();
                            }
                        }
                        else
                        {
                            if (!Directory.Exists(basePath + zipReader.Entry.Key))
                                Directory.CreateDirectory(basePath + zipReader.Entry.Key);
                        }
                    }
                }
            }
        }

        public static void Unzip(string basePath, string zipFileName)
        {
            Unzip(basePath, File.ReadAllBytes(zipFileName));
        }

        //take the folder name and a function delegate to use as file-filter instead of the fileNames array
        public static bool ZipFiles(string basePath, string zipName, string[] fileNames, bool deleteIfExist = true)
        {
            if (deleteIfExist)
                if (File.Exists(basePath + zipName))
                    File.Delete(basePath + zipName);

            using (FileStream zipArchiveStream = File.Create(zipName))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipArchiveStream, ZipArchiveMode.Update))
                {
                    foreach (string fileName in fileNames)
                    {
                        string entryName = fileName.Substring(basePath.Length);
                        ZipAddEntry(zipArchive, entryName, fileName, NLog.LogManager.GetCurrentClassLogger());
                    }

                    return true;
                }
            }
        }

        public static bool ZipFolder(string basePath, string zipName, bool deleteIfExist = true)
        {
            if (deleteIfExist)
                if (File.Exists(basePath + zipName))
                    File.Delete(basePath + zipName);

            using (FileStream zipArchiveStream = File.Create(zipName))
            {
                using (ZipArchive zipArchive = new ZipArchive(zipArchiveStream, ZipArchiveMode.Update))
                {
                    ZipAdd(zipArchive, basePath, NLog.LogManager.GetCurrentClassLogger());

                    return true;
                }
            }
        }

        private static void ZipAdd(ZipArchive zipArchive, string basePath, NLog.Logger logger)
        {
            foreach (string fileName in Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories))
            {
                if (File.GetAttributes(fileName).HasFlag(FileAttributes.Directory))
                {
                    ZipAdd(zipArchive, basePath + fileName, logger);
                }
                else
                {
                    string entryName = fileName.Substring(basePath.Length);
                    logger.Debug(entryName, fileName);
                    ZipAddEntry(zipArchive, entryName, fileName, logger);
                }
            }
        }

        private static void ZipAddEntry(ZipArchive zipArchive, string entryName, string fileName, NLog.Logger logger)
        {
            if (entryName.StartsWith("\\"))
                entryName = entryName.TrimStart('\\');

            ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(entryName);

            using (FileStream fileStream = File.OpenRead(fileName))
            {
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);

                using (Stream stream = zipArchiveEntry.Open())
                {
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Close();
                }
            }
        }
    }
}