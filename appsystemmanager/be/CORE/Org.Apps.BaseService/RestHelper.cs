using Org.Apps.BaseDao;
using Org.Apps.LogManager.Data.Model;
using Org.Apps.LogManager.Data.Model.Request;
using Org.Apps.LogManager.Data.Model.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace Org.Apps.BaseService
{
    public static class RestHelper
    {

        public static T RestPostObject<T, P>(string uri, P postData) where T : class, new()
        {
            Uri address = new Uri(uri);

            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            // we exactly need much more robust method instead of this,
            // to handle writing especially the object has very large amount of memory
            string postString = JsonConvert.SerializeObject(postData);
            byte[] postStringBytes = UTF8Encoding.UTF8.GetBytes(postString);

            request.ContentLength = postStringBytes.Length;

            request.GetRequestStream().Write(postStringBytes, 0, (int)request.ContentLength);
            //

            //JsonWriter jsonWriter = new JsonTextWriter(new StreamWriter(request.GetRequestStream()));
            //JsonSerializer.Create().Serialize(jsonWriter, postData);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        JsonReader jsonReader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                        T t = JsonSerializer.Create().Deserialize<T>(jsonReader);
                        return t;
                }

                throw new Exception(string.Format("Response Http Status Code is {0}.", response.StatusCode));
            }

            throw new Exception("Ops! Something went wrong.");
        }

        public static T RestGet<T>(string uri) where T : class, new()
        {
            Uri address = new Uri(uri);

            HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
            request.Method = "GET";
            request.ContentLength = 0;

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        JsonReader jsonReader = new JsonTextReader(new StreamReader(response.GetResponseStream()));
                        T t = JsonSerializer.Create().Deserialize<T>(jsonReader);
                        return t;
                }

                throw new Exception(string.Format("Response Http Status Code is {0}.", response.StatusCode));
            }

            throw new Exception("Ops! Something went wrong.");
        }
    }
}