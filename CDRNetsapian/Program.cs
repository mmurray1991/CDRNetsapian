using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace CDRNetsapian
{
    class Program
    {
        
        static void Main(string[] args)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = 
                "Data Source=TIPS-6Z6GYN1;"+
                "Initial Catalog=CallReporting;"+
                "User Id=Matt;"+
                "Password=tips;";
            conn.Open();
            CallReportingEntities cr = new CallReportingEntities();
            CALL_RECORDS_MASTER crm = new CALL_RECORDS_MASTER();
            string accessToken = requestAccessToken();
            JArray cdr = getCDR(accessToken);
            var testing = cdr[0]["term_callid"];
            
                // using the code here...
            
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
        public static JArray getCDR(string act)
        {
            var client = new RestClient("https://portal.trueipsolutions.com/ns-api/?format=json&object=cdr&action=read");
            var request = new RestRequest("", Method.POST);
            request.AddHeader("Authorization", "Bearer " + act);
            request.AddParameter("domain", "doubleradius.com");
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            JArray ja = JArray.Parse(content);
            return ja;
            /*Console.Write(ja[0].ToString(Newtonsoft.Json.Formatting.Indented));
            JToken jt = JToken.Parse(content);
            string formatted = jt.ToString(Newtonsoft.Json.Formatting.Indented);
            Console.Write(formatted);
            JArray array = (JArray)response.Content;
            dynamic stuff = JRaw.Parse(response.Content);

           // object answer =JObject.
            var json = JObject.Parse(content);
            string x = json.ToObject<string>();
            return x;

            return content;*/
        }

        public class CDRList
        {
            public List<CDRContainer> cdr { get; set; }
        }

        public class CDRContainer
        {

        }


    }
}
