using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using FirebirdSql.Data.FirebirdClient;


namespace ConsoleAZCService
{

    public class PauseAndExecuter
    {
        public async Task Execute(Action action, int timeoutInMilliseconds)
        {
            await Task.Delay(timeoutInMilliseconds);
            action();
        }
    }

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
        
        /// CASE SUCCESS 1: Return data via {"id": "srv_tzp_1_msg_15", "type":"update_cars", "result":"success"}
        public static Dictionary<string, string> ESTB_SuccessResponce_Case1_update_cars = new()
        {
            {"id", "srv_tzp_1_msg_15"}, 
            {"type", "update_cars" }, 
            {"result", "success"}
        };

        public static Task<string> resultGetESTBAuthToken;

        public static string SqlShowAllTables() => "SELECT a.RDB$RELATION_NAME\r\nFROM RDB$RELATIONS a\r\nWHERE COALESCE(RDB$SYSTEM_FLAG, 0) = 0 AND RDB$RELATION_TYPE = 0;";
        public static string SqlSelect(string tableName) => "SELECT * FROM \""+tableName+"\";";

        public List<string> GetSqlResultsSql(string commandText) 
        {
            FbConnection con = new (ConnectionString);
            con.Open();

            if (con.State != ConnectionState.Open) {
                string message = "Fail: Connected to " + TopazOfficeDBName + " Database was fail::" + Environment.NewLine;
                throw new Exception(message);
            }

            List<string> result = new List<string>();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = commandText;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }

            con.Close();

            // Dictionary<string, string> parameters = new Dictionary<string, string>();
            // parameters.ForEach(kvp => cmd.Parameters.Add(dbio.CreateParameter(kvp.Key, kvp.Value)));

            return result;
        }

        public bool StartAZCService() 
        {
            Console.WriteLine("[AZCService] ... waiting for connection " + AZCSericeName + "...");

            ServiceController sc = new ServiceController(AZCSericeName);
            ServiceController[] scServices = ServiceController.GetServices();

            foreach (ServiceController scTemp in scServices)
            {
                if (scTemp.ServiceName != AZCSericeName) continue;
                if (sc.Status != ServiceControllerStatus.Running) sc.Start();
                if (sc.Status != ServiceControllerStatus.Running) {
                    return false;
                } else {
                    Console.WriteLine("[AZCService]: Success. " + AZCSericeName + " was connected" + Environment.NewLine + Environment.NewLine);
                    return true;
                }
            }

            return false;
        }

        public string PrepareBaseAuthToken64(string userName = "user7", string passwd = "passwd") /* "marychev_garpix" / "911911"; */
        {
            var byteToken = Encoding.ASCII.GetBytes($"{userName}:{passwd}");
            return Convert.ToBase64String(byteToken);
        }
        
        public Task<string> ApiGetESTBAuthToken(ConsoleAZCService.Program app) {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", app.PrepareBaseAuthToken64());
            
            var result = client.GetAsync(ESTB_URL_AUTH_THOKEN).GetAwaiter().GetResult();
            if (HttpStatusCode.OK != result.StatusCode) throw new Exception("Authenticated was failing. Get: " + result);

            return result.Content.ReadAsStringAsync();

            /* Examples II
            using (var _client = new HttpClient())
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", app.PrepareBaseAuthToken64());
                using (var responce = _client.GetAsync(ESTB_URL_AUTH_THOKEN))   // using(var resp = client.PostAsync(url, data)) {
                    if (responce.IsCompletedSuccessfully) throw new Exception("Authenticated was failing. Get: " + responce.Result);
                    Console.WriteLine("Succes after check auth token: " + responce.Result);
                }
            */

        }

        static void Main()
        {
            Program app = new Program();

            /// Connect to AC Service
            if (!app.StartAZCService()) throw new Exception("Fail: Connected to " + AZCSericeName + " was fail." + Environment.NewLine);

            /// Get SQL results to Firebird database
            List<string> sqlResult = app.GetSqlResultsSql(SqlSelect("ProgConfig"));
            Console.WriteLine("[DB_TOPAZ]: SQL result = " + string.Join(Environment.NewLine, sqlResult.ToArray()));

            /// Send the first message to ESTB backend with Token
            Task.Delay(3).ContinueWith(t => Console.WriteLine("[ESTB]: .. waiting for ESTB-back responce ... "));
            Console.WriteLine("[ESTB]: Responce = " + app.ApiGetESTBAuthToken(app).Result);

            Console.WriteLine("CASE SUCCESS 1: Returned data = "+Environment.NewLine+"{" + string.Join(",", ESTB_SuccessResponce_Case1_update_cars.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}");


            /// TODO: CASE FAIL 1:

        }

    }

}