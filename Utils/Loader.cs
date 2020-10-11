using RpcSecurity.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace RpcSecurity.app.Utils
{
    public class Loader
    {
        private const bool DEBUG = false;
        private static Loader loader;

        public static void CreateInstanse(string host, string login, string password)
        {
            loader = new Loader(host, login, password);
        }

        public static Loader GetInstanse()
        {
            return loader;
        }

        private string token;
        private string host;

        public string getToken()
        {
            return token;
        }

        private Loader(string host, string login, string password)
        {
            this.host = host;

            WebRequest request = WebRequest.Create(host + "/auth");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            String authData = "UserName=" + login + "&Password=" + password;
            Stream dataStream = request.GetRequestStream();
            byte[] bytes = Encoding.UTF8.GetBytes(authData);
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseFromServer);
                token = data.token;
            }

            // Close the response.  
            response.Close();
        }

        public bool isAuthorized()
        {
            return token != null;
        }

        public dynamic RPC(string action, string method, string body)
        {
            WebRequest request = WebRequest.Create(host + "/rpc");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("rpc-authorization", "Token " + token);
            Stream dataStream = request.GetRequestStream();
            byte[] bytes = Encoding.UTF8.GetBytes("[{\"action\":\"" + action + "\", \"method\":\"" + method + "\", \"tid\":1, \"data\":" + body + ", \"type\":\"rpc\"}]");
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            dynamic data;
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseFromServer);
            }

            // Close the response.  
            response.Close();
            return data;
        }

        public static List<String> getRemoteObjects()
        {
            List<String> objects = new List<string>();
            WebRequest request = WebRequest.Create(getWebUrl() + "/rpc/meta?rpc-authorization=" + getWebToken());
            request.Method = "GET";
            request.ContentType = "application/json";
            Stream dataStream;
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseFromServer);
                var actions = data.actions;
                string _namespace = data.@namespace;
                foreach (var action in actions)
                {
                    string name = action.Name;
                    foreach (var item in action.Value) {
                        string method = item.name;
                        objects.Add(_namespace + "." + name + "." + method);
                    }
                }
            }

            // Close the response.  
            response.Close();

            return objects;
        }

        public static string updateAccessCache()
        {
            string result = "FAIL";
            WebRequest request = WebRequest.Create(getWebUrl() + "/cache/reload?rpc-authorization=" + getWebToken());
            request.Method = "GET";
            request.ContentType = "text/plain";
            Stream dataStream;
            WebResponse response = request.GetResponse();
            // Get the stream containing content returned by the server.  
            // The using block ensures the stream is automatically closed.
            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  

                result = reader.ReadToEnd();
            }

            // Close the response.  
            response.Close();

            return result;
        }


        public async Task<int> FileUpload(StorageFile file)
        {
            var bytes = await GetBytesAsync(file);
            using(ApplicationContext db = new ApplicationContext())
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

        public async Task<string> GetVersion()
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.GetAsync(host + "/upload/version").Result;

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string sd = await response.Content.ReadAsStringAsync();

            dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject(sd);
            return d.version;
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

        public static string getWebUrl()
        {
            //return "http://localhost:3000";
            using(ApplicationContext db = new ApplicationContext())
            {
                Setting item = db.Setting.FirstOrDefault(t => t.c_key == "ALL_URL");
                return item != null ? item.c_value : null;
            }
        }

        public static string getWebToken()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Setting item = db.Setting.FirstOrDefault(t => t.c_key == "ALL_DEFAULT_WEB_TOKEN");
                return item != null ? item.c_value : null;
            }
        }

        public static string getBirthDay()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                Setting item = db.Setting.FirstOrDefault(t => t.c_key == "ALL_BIRTH_DAY");
                return item != null ? item.c_value : null;
            }
        }
    }
}
