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
using Brisebois.WindowsAzure.Sql;
using Microsoft.WindowsAzure;

namespace CDRNetsapian
{
    class Program
    {
        //private static 
        private static CallReportingEntities cr = new CallReportingEntities();
        private static SqlConnection openCon = new SqlConnection("Data Source=tipsdb.database.windows.net;" +
            "Initial Catalog=LOA;" +
            "User id=tipsadmin;" +
            "Password=^arBM)qy;");

        static void Main(string[] args)
        {
            string accessToken = requestAccessToken();
            JArray cdr = getCDR(accessToken);
            //var testing = cdr[0]["term_callid"];
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
            request.AddParameter("password", "L2C93vTy");
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
            List<CALL_RECORDS_MASTER> crmList = new List<CALL_RECORDS_MASTER>();
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Dialed #", typeof(string)));
            table.Columns.Add(new DataColumn("From Device", typeof(string)));
            table.Columns.Add(new DataColumn("Orig Call-ID", typeof(string)));
            table.Columns.Add(new DataColumn("From User", typeof(string)));
            table.Columns.Add(new DataColumn("To Device", typeof(string)));
            table.Columns.Add(new DataColumn("To User", typeof(string)));
            table.Columns.Add(new DataColumn("Call Time", typeof(string)));
            table.Columns.Add(new DataColumn("Ringing Time", typeof(string)));
            table.Columns.Add(new DataColumn("Answer Time", typeof(string)));
            table.Columns.Add(new DataColumn("Hangup Time", typeof(string)));
            table.Columns.Add(new DataColumn("Talking Time", typeof(int)));
            table.Columns.Add(new DataColumn("Hold Time", typeof(int)));
            table.Columns.Add(new DataColumn("Duration (Sec)", typeof(int)));
            table.Columns.Add(new DataColumn("ACW", typeof(int)));
            table.Columns.Add(new DataColumn("Pre-Talk Time", typeof(string)));
            table.Columns.Add(new DataColumn("Release Reason", typeof(string)));
            table.Columns.Add(new DataColumn("Disposition", typeof(string)));
            table.Columns.Add(new DataColumn("Reason", typeof(string)));

            for (int x = 0; x < cdrData.Count; x++)
            {
                crm.Dialed__ = cdrData[x]["orig_to_user"].ToString();
                crm.From_Device = cdrData[x]["orig_from_uri"].ToString();
                crm.Orig_Call_ID = cdrData[x]["orig_callid"].ToString();
                if (cdrData[x]["orig_sub"].ToString() == "")
                {
                    crm.From_User = "";
                }
                else
                {
                    crm.From_User = cdrData[x]["orig_sub"].ToString() + "@" + cdrData[x]["orig_domain"].ToString();
                }
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
                //Console.WriteLine(x);
                DataRow row = table.NewRow();
                row["Dialed #"] = crm.Dialed__;
                row["From Device"] = crm.From_Device;
                row["Orig Call-ID"] = crm.Orig_Call_ID;
                row["From User"] = crm.From_User;
                row["To Device"] = crm.To_Device;
                row["To User"] = crm.To_User;
                row["Call Time"] = crm.Call_Time;
                row["Ringing Time"] = crm.Ringing_Time;
                row["Answer Time"] = crm.Answer_Time;
                row["Hangup Time"] = crm.Hangup_Time;
                row["Talking Time"] = crm.Talking_Time;
                row["Hold Time"] = crm.Hold_Time;
                row["Duration (Sec)"] = crm.Duration__Sec_;
                row["ACW"] = crm.ACW;
                row["Pre-Talk Time"] = crm.Pre_Talk_Time;
                row["Release Reason"] = crm.Release_Reason;
                row["Disposition"] = crm.Disposition;
                row["Reason"] = crm.Reason;
                table.Rows.Add(row);
                //crmList.Add(crm);


            //    string saveRecord = "Insert into dbo.CALL_RECORDS_MASTER ([Dialed #], [From Device], [Orig Call-ID], [From User], [To Device], [To User], [Call Time], [Ringing Time], [Answer Time], [Hangup Time], [Talking Time], [Hold Time], [Duration (Sec)], ACW, [Pre-Talk Time], [Release Reason], Disposition, Reason) VALUES (@dialed, @fromDevice, @origCallId, @fromUser, @toDevice, @toUser, @callTime, @ringingTime, @answerTime, @hangupTime, @talkingTIme, @holdTime, @duration, @acw, @preTalkTime, @releaseReason, @disposition, @reason)";
            //    SqlCommand querySaveRecord = new SqlCommand(saveRecord, openCon);

            //    querySaveRecord.CommandType = CommandType.Text;
            //    querySaveRecord.Parameters.AddWithValue("@dialed", crm.Dialed__);
            //    querySaveRecord.Parameters.AddWithValue("@fromDevice", crm.From_Device);
            //    querySaveRecord.Parameters.AddWithValue("@origCallId", crm.Orig_Call_ID);
            //    querySaveRecord.Parameters.AddWithValue("@fromUser", crm.From_User);
            //    querySaveRecord.Parameters.AddWithValue("@toDevice", crm.To_Device);
            //    querySaveRecord.Parameters.AddWithValue("@toUser", crm.To_User);
            //    querySaveRecord.Parameters.AddWithValue("@callTime", crm.Call_Time);
            //    querySaveRecord.Parameters.AddWithValue("@ringingTime", crm.Ringing_Time);
            //    querySaveRecord.Parameters.AddWithValue("@answerTime", crm.Answer_Time);
            //    querySaveRecord.Parameters.AddWithValue("@hangupTime", crm.Hangup_Time);
            //    querySaveRecord.Parameters.AddWithValue("@talkingTIme", crm.Talking_Time);
            //    querySaveRecord.Parameters.AddWithValue("@holdTime", crm.Hold_Time);
            //    querySaveRecord.Parameters.AddWithValue("@duration", crm.Duration__Sec_);
            //    querySaveRecord.Parameters.AddWithValue("@acw", crm.ACW);
            //    querySaveRecord.Parameters.AddWithValue("@preTalkTime", crm.Pre_Talk_Time);
            //    querySaveRecord.Parameters.AddWithValue("@releaseReason", crm.Release_Reason);
            //    querySaveRecord.Parameters.AddWithValue("@disposition", crm.Disposition);
            //    querySaveRecord.Parameters.AddWithValue("@reason", crm.Reason);

            //    querySaveRecord.ExecuteNonQuery();

            }
            //var bulk = new BulkWriter("EventLog", new Dictionary<string, string>
            //    {
            //        {"Dialed #", "Dialed #"},
            //        {"From Device", "From Device"},
            //        {"Orig Call-ID", "Orig Call-ID"},
            //        {"From User", "From User"},
            //         {"To Device", "To Device"},
            //         {"To User", "To User"},
            //         {"Call Time", "Call Time"},
            //        {"Ringing Time", "Ringing Time"},
            //        {"Answer Time", "Answer Time"},
            //         {"Hangup Time", "Hangup Time"},
            //         {"Talking Time", "Talking Time"},
            //        {"Hold Time", "Hold Time"},
            //         {"Duration (Sec)", "Duration (Sec)"},
            //         {"ACW", "ACW"},
            //         {"Pre-Talk Time", "Pre-Talk Time"},
            //        {"Release Reason", "Release Reason"},
            //        {"Disposition", "Disposition"},
            //         {"Reason", "Reason"}
            //    });

            //bulk.WriteWithRetries(table);
            //string tmpTable = "CREATE TABLE temp([Dialed #] [varchar](50) NOT NULL,[From Device][varchar](100) NOT NULL,[Orig Call - ID][varchar](100) NOT NULL,[From User][varchar](100) NOT NULL,[To Device][varchar](100) NOT NULL,[To User][varchar](100) NOT NULL,[Call Time][varchar](25) NOT NULL,[Ringing Time][varchar](25) NOT NULL,[Answer Time][varchar](25) NOT NULL,[Hangup Time][varchar](25) NOT NULL,[Talking Time][int] NOT NULL,[Hold Time][int] NOT NULL,[Duration(Sec)][int] NOT NULL,[ACW][int] NOT NULL,[Pre - Talk Time][varchar](25) NOT NULL,[Release Reason][varchar](100) NOT NULL,[Disposition][varchar](50) NULL,[Reason][varchar](50) NULL,)";
            //SqlCommand cmd = new SqlCommand(tmpTable, openCon);
            //cmd.ExecuteNonQuery();
            using (SqlBulkCopy bulk = new SqlBulkCopy(openCon))
            {
                bulk.DestinationTableName = "CALL_RECORDS_STAGING";
                //Console.Write(bulk.ColumnMappings.ToString()); ;
                bulk.WriteToServer(table);
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
