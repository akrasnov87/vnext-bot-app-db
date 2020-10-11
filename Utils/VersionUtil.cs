using RpcSecurity.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace RpcSecurity.Utils
{
    public class VersionUtil
    {
        public static string GetLastVersion()
        {
            string version = "0.0.0.0";
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Digests
                            where !t.b_hidden
                            orderby t.id descending
                            select new
                            {
                                t.c_version
                            };

                if (query.Count() > 0)
                {
                    version = query.First().c_version;
                }
            }
            return version;
        }

        public static string GetDescriptionVersion(string version)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Digests
                            where t.c_version == version
                            select t;

                if (query.Count() == 1)
                {
                    string desc = query.First().c_description;
                    return desc == null ? "" : desc;
                }
            }

            return "";
        }

        public static void UpdateDescriptionVersion(string version, string txt)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var query = from t in db.Digests
                            where t.c_version == version
                            select t;

                if (query.Count() == 1)
                {
                    Digests item = query.First();
                    item.c_description = txt;
                    db.Digests.Update(item);
                    db.SaveChanges();
                }
            }
        }

        public static async Task<int> FileUpload(StorageFile file)
        {
            var bytes = await GetBytesAsync(file);
            using (ApplicationContext db = new ApplicationContext())
            {
                ApkReader.ApkReader apkReader = new ApkReader.ApkReader();
                MemoryStream memoryStream = new MemoryStream(bytes);
                string versionName = apkReader.Read(memoryStream).VersionName;

                db.Digests.Add(new Digests()
                {
                    c_version = versionName,
                    ba_file = bytes
                });
                return await db.SaveChangesAsync();
            }
        }

        public static async Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;
            if (file == null) return null;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
