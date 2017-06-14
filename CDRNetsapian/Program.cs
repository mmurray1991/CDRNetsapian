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
        //private static 
        private static CallReportingEntities cr = new CallReportingEntities();
        private static SqlConnection openCon = new SqlConnection("Data Source=TIPS-6Z6GYN1;" +
            "Initial Catalog=CallReporting;" +
            "User id=Matt;" +
            "Password=tips;");

        static void Main(string[] args)
        {
            string accessToken = requestAccessToken();
            JArray cdr = getCDR(accessToken);
            var testing = cdr[0]["term_callid"];
            //Console.Write(cdr);
            openCon.Open();
            AddToDB(cdr);
            openCon.Close();
            // using the code here...

            //Console.Write(cdr);
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
            request.AddParameter("type", "OutBound");
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

        public static void AddToDB(JArray cdrData)
        {
            CALL_RECORDS_MASTER crm = new CALL_RECORDS_MASTER();
            
               

                for (int x = 0; x < cdrData.Count; x++)
                {
                    crm.Dialed__ = cdrData[x]["orig_to_user"].ToString();
                    crm.From_Device = cdrData[x]["orig_from_uri"].ToString();
                    crm.Orig_Call_ID = cdrData[x]["orig_callid"].ToString();
                    crm.From_User = cdrData[x]["orig_domain"].ToString();
                    crm.To_Device = cdrData[x]["term_to_uri"].ToString();
                    crm.To_User = cdrData[x]["domain"].ToString();
                    crm.Call_Time = cdrData[x]["batch_tim_beg"].ToString();
                    crm.Ringing_Time = "";
                    crm.Answer_Time = cdrData[x]["time_answer"].ToString();
                    crm.Hangup_Time = cdrData[x]["time_release"].ToString();

                    if (crm.Answer_Time == "" || crm.Answer_Time == "0000-00-00 00:00:00")
                    {
                        crm.Talking_Time = 0;
                        crm.Pre_Talk_Time = "";
                    }
                    else
                    {
                        DateTime aTDate = Convert.ToDateTime(crm.Answer_Time);
                        DateTime hTDate = Convert.ToDateTime(crm.Hangup_Time);
                        int talk_Ti = Convert.ToInt16((hTDate - aTDate).TotalSeconds);
                        crm.Talking_Time = talk_Ti;

                        DateTime batchTimeBeg = Convert.ToDateTime(crm.Call_Time);
                        crm.Pre_Talk_Time = (aTDate - batchTimeBeg).TotalSeconds.ToString();
                    }

                    crm.Hold_Time = 0;
                    crm.Duration__Sec_ = crm.Talking_Time;
                    crm.ACW = 0;
                    crm.Release_Reason = cdrData[x]["release_text"].ToString();
                    crm.Disposition = cdrData[x]["disposition"].ToString();
                    crm.Reason = cdrData[x]["reason"].ToString();
                    Console.WriteLine(x);
                //cr.CALL_RECORDS_MASTER.Add(crm);

                
                    string saveRecord = "Insert into CallReporting.dbo.CALL_RECORDS_MASTER ([Dialed #], [From Device], [Orig Call-ID], [From User], [To Device], [To User], [Call Time], [Ringing Time], [Answer Time], [Hangup Time], [Talking Time], [Hold Time], [Duration (Sec)], ACW, [Pre-Talk Time], [Release Reason], Disposition, Reason) VALUES (@dialed, @fromDevice, @origCallId, @fromUser, @toDevice, @toUser, @callTime, @ringingTime, @answerTime, @hangupTime, @talkingTIme, @holdTime, @duration, @acw, @preTalkTime, @releaseReason, @disposition, @reason)";
                    SqlCommand querySaveRecord = new SqlCommand(saveRecord, openCon);
                    
                        querySaveRecord.CommandType = CommandType.Text;
                        querySaveRecord.Parameters.AddWithValue("@dialed", crm.Dialed__);
                        querySaveRecord.Parameters.AddWithValue("@fromDevice", crm.From_Device);
                        querySaveRecord.Parameters.AddWithValue("@origCallId", crm.Orig_Call_ID);
                        querySaveRecord.Parameters.AddWithValue("@fromUser", crm.From_User);
                        querySaveRecord.Parameters.AddWithValue("@toDevice", crm.To_Device);
                        querySaveRecord.Parameters.AddWithValue("@toUser", crm.To_User);
                        querySaveRecord.Parameters.AddWithValue("@callTime", crm.Call_Time);
                        querySaveRecord.Parameters.AddWithValue("@ringingTime", crm.Ringing_Time);
                        querySaveRecord.Parameters.AddWithValue("@answerTime", crm.Answer_Time);
                        querySaveRecord.Parameters.AddWithValue("@hangupTime", crm.Hangup_Time);
                        querySaveRecord.Parameters.AddWithValue("@talkingTIme", crm.Talking_Time);
                        querySaveRecord.Parameters.AddWithValue("@holdTime", crm.Hold_Time);
                        querySaveRecord.Parameters.AddWithValue("@duration", crm.Duration__Sec_);
                        querySaveRecord.Parameters.AddWithValue("@acw", crm.ACW);
                        querySaveRecord.Parameters.AddWithValue("@preTalkTime", crm.Pre_Talk_Time);
                        querySaveRecord.Parameters.AddWithValue("@releaseReason", crm.Release_Reason);
                        querySaveRecord.Parameters.AddWithValue("@disposition", crm.Disposition);
                        querySaveRecord.Parameters.AddWithValue("@reason", crm.Reason);
                        querySaveRecord.ExecuteNonQuery();
                        
                    
                
                    
                
            }
            
            //cr.SaveChanges();
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
