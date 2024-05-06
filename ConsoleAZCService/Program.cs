using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using FirebirdSql.Data.FirebirdClient;


namespace ConsoleAZCService
{

    internal class Program
    {

        // Simple ESTB-back to check AUTH_TOKEN 
        public static string ESTB_URL_AUTH_THOKEN = "https://httpbin.org/basic-auth/user7/passwd";   // PROD-ESTB:  /api/public/poll  - ???


        public const string AZCSericeName = "AZCService";

        public const string TopazOfficeUser= "SYSDBA";
        public const string TopazOfficePassword= "masterkey";
        public const string TopazOfficeDBName = "TopazOffice";
        public const string TopazOfficeDBPath = "C:\\Users\\garpix\\Downloads\\" + TopazOfficeDBName + ".FDB";
        public const string ConnectionString = "User = " + TopazOfficeUser + "; Password = " + TopazOfficePassword + "; Database = " + TopazOfficeDBPath + "; DataSource = localhost; Port = 3050; Dialect = 3; Charset = NONE; Role =; Connection lifetime = 15; Pooling = true; MinPoolSize = 0; MaxPoolSize = 50; Packet Size = 8192; ServerType = 0;";

        public static string SqlShowAllTables() => "SELECT a.RDB$RELATION_NAME\r\nFROM RDB$RELATIONS a\r\nWHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0 AND RDB$RELATION_TYPE = 0;";
        public static string SqlSelect(string tableName) => "SELECT * FROM \""+tableName+"\";";

        public List<string> GetSqlResultsSql(string commandText) 
        {

            FbConnection con = new FbConnection(ConnectionString);
            con.Open();

            if (con.State != ConnectionState.Open)
            {
                string message = "Fail: Connected to " + TopazOfficeDBName + " Database was fail::" + Environment.NewLine;
                throw new Exception(message);
            }

            List<string> result = new List<string>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = SqlShowAllTables();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }

            con.Close();
            return result;
        }

        public bool StartAZCService() 
        {
            Console.WriteLine("Check: Waiting for connection " + AZCSericeName + "...");

            ServiceController sc = new ServiceController(AZCSericeName);
            ServiceController[] scServices = ServiceController.GetServices();

            foreach (ServiceController scTemp in scServices)
            {
                if (scTemp.ServiceName != AZCSericeName) continue;
                if (sc.Status != ServiceControllerStatus.Running) sc.Start();
                if (sc.Status != ServiceControllerStatus.Running) {
                    return false;
                } else {
                    Console.WriteLine("Success: " + AZCSericeName + " was connected");
                    return true;
                }
            }

            return false;
        }

        static void Main(string[] args)
        {
            Program app = new Program();

            /// Connect to AC Service
            if (!app.StartAZCService()) {
                string message = "Fail: Connected to " + AZCSericeName + " was fail." + Environment.NewLine;
                throw new Exception(message);
            }

            /// Get SQL results to Firebird database
            List<string> result = app.GetSqlResultsSql(SqlShowAllTables());
            Console.WriteLine(string.Join(Environment.NewLine, result.ToArray()));
            // Dictionary<string, string> parameters = new Dictionary<string, string>();
            // parameters.ForEach(kvp => cmd.Parameters.Add(dbio.CreateParameter(kvp.Key, kvp.Value)));


            /// Send the first message to ESTB backend with Token
            string userName = "user7";  // "marychev_garpix";
            string passwd = "passwd";   // "911911";
            using (var client = new HttpClient())
            {
                var authToken = Encoding.ASCII.GetBytes($"{userName}:{passwd}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                using(var resp = client.GetAsync(ESTB_URL_AUTH_THOKEN))   // using(var resp = client.PostAsync(url, data))   
                {
                    Console.WriteLine("Get ESTB responce status: " + resp.Status);
                    // ???? if (resp.Status != TaskStatus.WaitingForActivation) throw new Exception("Authenticated was failing. Get " + resp.Status.ToString());

                    /* 
                    
                    PROCCESING 
                    
                    Return data via {"id": "srv_tzp_1_msg_15", "type":"update_cars", "result":"success"}
                    
                     */
                }

            }
  
            bool answerFromESTB = true;


            /// CASE SUCCESS 1:


            /// CASE FAIL 1:


            //var response = await client.PostAsync(url, data);
            //var result = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(result);

            /* MyEntity myEntity;
             HttpResponseMessage response;

             using (var httpClient = new HttpClient())
             {
                 httpClient.BaseAddress = new Uri("https://yourApiAdress.com");
                 //Yours string value.
                 var content = new FormUrlEncodedContent(new[]
                 {
                     new KeyValuePair<string, string>("MyStringContent", "someString")
                 });
                 //Sending http post request.

                 response = await httpClient.PostAsync($"/api/public/poll/", content);

             }
 */

            /// ... wait for few seconds as example await for DB answers ...
            // TODO: To return answer to ESTB-back via `sendInfoToAZC()` data 


            /// Method 1: sendInfoToAZC() => Return data via {"id": "srv_tzp_1_msg_15", "type":"update_cars", "result":"success"}


        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        }

    }

}