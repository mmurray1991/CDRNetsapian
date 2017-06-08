using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CDRNetsapian
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string accessToken = requestAccessToken();
            var cdr = getCDR(accessToken);
            Console.Write(cdr);

        }
        public static string requestAccessToken()
        {
            var client = new RestClient("https://portal.trueipsolutions.com/ns-api/oauth2/token/");
            var request = new RestRequest("",Method.POST);
            request.AddParameter("grant_type", "password");
            request.AddParameter("client_id", "apitest");
            request.AddParameter("client_secret", "11972ff80886ecfbaa18355eff3251b6");
            request.AddParameter("username", "brianhales@trueipsolutions.com");
            request.AddParameter("password", "10658009");
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            var json = JObject.Parse(content);
            string token = json["access_token"].ToObject<string>();
            return token;
        }
        public static string getCDR(string act)
        {
            var client = new RestClient("https://portal.trueipsolutions.com/ns-api/?format=json&object=cdr&action=read");
            var request = new RestRequest("", Method.POST);
            request.AddHeader("Authorization", "Bearer " + act);
            request.AddParameter("domain", "doubleradius.com");
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);
            var content = response.Content;
            var json = JObject.Parse(content);
            string x = json.ToObject<string>();
            return x;
        }
       
    }
}
