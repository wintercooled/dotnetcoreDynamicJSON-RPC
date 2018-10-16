using System;
using System.Dynamic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace dotnetcoreDynamicJSON_RPC
{
    class dotnetcoreDynamicJSON_RPC : DynamicObject
    {
        private string rpcUrl;
        private string rpcPort;
        private string rpcUser;
        private string rpcPassword;

        public dotnetcoreDynamicJSON_RPC(string rpcUrl, string rpcPort, string rpcUser, string rpcPassword)
        {
            this.rpcUrl = rpcUrl;
            this.rpcPort = rpcPort;
            this.rpcUser = rpcUser;
            this.rpcPassword = rpcPassword;
        }

        public bool DaemonIsRunning(string parameterlessTestCommand = "getblockcount")
        {
            try
            {
                string check = SendRPC(parameterlessTestCommand, null);
                return true;
            }
            catch { return false; }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type type = typeof(dotnetcoreDynamicJSON_RPC);
            
            try {
            result = type.InvokeMember(
                "SendRPC",
                BindingFlags.InvokeMethod,
                null,
                this,
                new object[] { binder.Name, args }
                );
                return true;
            }
            catch (Exception) {
                throw new Exception("An error occured executing the RPC command \"" + binder.Name + "\"." +
                "Check the daemon is running with the same RPC credentials passed into dotnetcoreDynamicJSON_RPC and that the command and parameters are correctly formed.");
            }
        }

        public string SendRPC(string method, object[] args)
        {
            string jsonResponse;

            Console.WriteLine(method);

            JsonRPCRequest jsonRpcRequest = new JsonRPCRequest(method, args);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.rpcUrl + ":" + this.rpcPort);

            //SetAuthorizationHeader(webRequest, this.rpcUser, this.rpcPassword);

            webRequest.Credentials = new NetworkCredential(this.rpcUser, this.rpcPassword);
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";

            byte[] requestBytes = jsonRpcRequest.GetBytes();

            webRequest.ContentLength = jsonRpcRequest.GetBytes().Length;

            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
            }

            using (WebResponse webResponse = webRequest.GetResponse())
            using (Stream responseStream = webResponse.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream))
            {
                jsonResponse = reader.ReadToEnd();
                reader.Close();
            }

            return jsonResponse;

        }

        public class JsonRPCRequest
        {
            public JsonRPCRequest(string method, object[] args)
            {
                Method = method;
                Parameters = args?.ToList() ?? new List<object>();
            }

            [JsonProperty(PropertyName = "method", Order = 0)]
            public string Method { get; set; }

            [JsonProperty(PropertyName = "params", Order = 1)]
            public IList<object> Parameters { get; set; }

            public string GetString()
            {
                return JsonConvert.SerializeObject(this);
            }

            public byte[] GetBytes()
            {
                return Encoding.UTF8.GetBytes(GetString());
            }
        }

        private static void SetAuthorizationHeader(WebRequest webRequest, string user, string password)
        {
            string credentials = user + ":" + password;
            credentials = Convert.ToBase64String(Encoding.Default.GetBytes(credentials));
            webRequest.Headers["Authorization"] = "Basic" + " " + credentials;
        }
    }

    public static class RPCResultExtensions
    {
        /// <summary>
        /// Returns a string value for the property provided from the JSON string this extension method is applied to.
        /// </summary>
        /// <param name="property">The JSON property name path whos value we want returned. Example: from getblock result select "result.weight"</param>
        /// <returns></returns>
        public static string GetProperty(this String str, string property)
        {
            var jObject = JObject.Parse(str);
            string result = (string)jObject.SelectToken(property);
            return result;
        }

        /// <summary>
        /// Returns an IList of strings from the array of JSON data located at the 'path' specified.  
        /// </summary>
        /// <param name="path">The path to the array of objects you want returned from the JSON string this extension method is used on. Example: from getblock result select "result.tx"</param>
        /// <returns>An IList of strings</returns>
        public static IList<string> GetStringList(this String str, string path)
        {
            var jObject = JObject.Parse(str);

            IList<string> items = jObject.SelectToken("$." + path).Select(s => (string)s).ToList();

            return items;
        }

        /// <summary>
        /// Returns an IList of objects from the array of JSON data located at the 'path' specified.  
        /// </summary>
        /// <param name="path">The path to the array of objects you want returned from the JSON string this extension method is used on. Example: from decoderawtransaction result select "result.vout"</param>
        /// <returns>An IList of objects</returns>
        public static IList<object> GetObjectList(this String str, string path)
        {
            var jObject = JObject.Parse(str);

            IList<object> items = jObject.SelectToken("$." + path).Select(s => (object)s).ToList();

            return items;
        }
    }
}