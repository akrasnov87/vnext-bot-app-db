using vNextBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace vNextBot.Utils
{
    public class FileIO
    {
        public static string ACCSESS_FILE = "access.txt";
        public async static Task<bool> CreateFile(string fileName)
        {
            StorageFolder storageFolder = getLocalFolder();
            await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            return true;
        }

        public async static Task<bool> WriteToFile(string fileName, string txt)
        {
            StorageFolder storageFolder = getLocalFolder();
            StorageFile sampleFile = await storageFolder.GetFileAsync(fileName);
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, txt);
            return true;
        }

        public async static Task<string> ReadFromFile(string fileName)
        {
            StorageFolder storageFolder = getLocalFolder();
            StorageFile sampleFile = await storageFolder.GetFileAsync(fileName);
            return await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
        }

        public async static Task<bool> IsFilePresent(string fileName)
        {
            var item = await getLocalFolder().TryGetItemAsync(fileName);
            return item != null;
        }

        private static StorageFolder getLocalFolder()
        {
            return ApplicationData.Current.LocalFolder;
        }

        public static List<ConnectionString> GetConnectionStrings(string txt)
        {
            List<ConnectionString> results = new List<ConnectionString>();
            if(!string.IsNullOrEmpty(txt))
            {
                string[] lines = txt.Split('\n');
                foreach(string line in lines)
                {
                    string[] data = line.Split('|');
                    ConnectionString connectionString = new ConnectionString();
                    connectionString.host = data[0];
                    connectionString.port = data[1];
                    connectionString.dbName = data[2];
                    connectionString.login = data[3];
                    connectionString.password = data[4];

                    results.Add(connectionString);
                }
            }

            return results;
        }

        public static string ConnectionStringsToString(List<ConnectionString> connectionStrings)
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i <connectionStrings.Count; i++)
            {
                ConnectionString connectionString = connectionStrings[i]; 
                builder.Append(connectionString.host + "|" + connectionString.port + "|" + connectionString.dbName + "|" + connectionString.login + "|" + connectionString.password);
                if(i != connectionStrings.Count - 1)
                {
                    builder.Append("\n");
                }
            }
            return builder.ToString();
        }
    }
}
