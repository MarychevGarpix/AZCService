using System;
using System.ComponentModel;
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
using Newtonsoft.Json;


namespace ConsoleAZCService
{
    public enum ESTBResponceType
    {
        [Description("no_data_required - формируется когда нет необходимости обновлять данные. На этом ТЗП завершает текущий запрос")] NoDataRequired = 0,
        [Description("update_cars - в ответ возвращается массив всех машин с ТЗП с указанием их карт (на ТЗП одна машина это одна карта)")] UpdateCars = 1,
        [Description("update_drivers - в ответ возвращается массив всех водителей с указанием их карт (на ТЗП один водитель это одна карта)")] UpdateDrivers = 2,
        [Description("update_operators - в ответ возвращается массив всех операторов с указанием их карт (на ТЗП у одного оператора одна карта)")] UpdateOperators = 3,
        [Description("get_supplies - получает список приемов топлива с момента последнего успешного сеанса связи")] GetSupplies = 10,
        [Description("get_transactions - получает список отгрузок топлива с момента последнего успешного сеанса связи")] GetTransactions = 11,
        [Description("get_shifts - получает список смен с итогами с момента последнего успешного сеанса связи")] GetShifts = 12,
        [Description("get_current получает текущие данные, т.е. последние счетчики с ТРК или последние данные с уровнемеров")] GetCurrent = 13,
    }





    internal class Program
    {
        public const string AZCSericeName = "AZCService";

        /// Database Sample settings
        
        public const string TopazOfficeUser= "SYSDBA";
        public const string TopazOfficePassword= "masterkey";
        public const string TopazOfficeDBName = "TopazOffice";
        public const string TopazOfficeDBPath = "C:\\Users\\garpix\\Downloads\\" + TopazOfficeDBName + ".FDB";
        public const string ConnectionString = "User = " + TopazOfficeUser + "; Password = " + TopazOfficePassword + "; Database = " + TopazOfficeDBPath + "; DataSource = localhost; Port = 3050; Dialect = 3; Charset = NONE; Role =; Connection lifetime = 15; Pooling = true; MinPoolSize = 0; MaxPoolSize = 50; Packet Size = 8192; ServerType = 0;";


        /// ESTB-back Sample to check AUTH_TOKEN 
        public const string ESTBUserName = "integration_tzp1";
        public const string ESTBPassword = "12INT_admin";

        public static string ESTB_URL_AUTH_THOKEN = "http://estb.infra.garpix.com/api/v1/user/login/";  // "https://httpbin.org/basic-auth/user7/passwd";   FOR TESTING WITHOUT ESTB-Back

        // - CASE SUCCESS 1: Return data via {"id": "srv_tzp_1_msg_15", "type":"update_cars", "result":"success"}
        public static Dictionary<string, string> ESTB_SuccessResponce_Case1_update_cars = new()
        {
            {"id", "srv_tzp_1_msg_15"}, 
            {"type", "update_cars" }, 
            {"result", "success"}
        };


        /// SQL Sample 

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
        
        
        /// AZCService 
        
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

        /*
        public string PrepareBaseAuthToken64(string userName = "user7", string passwd = "passwd") // "integration_tzp1", "password": "12INT_admin"
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{passwd}"));
        }
        */


        /// API

        public Task<string> ApiGetESTBAuthToken(ConsoleAZCService.Program app) {
            using HttpClient client = new();
            
            Dictionary<string, string> body = new() {
                {"username", "integration_tzp1"}, 
                {"password", "12INT_admin"}
            };

            string jsonValue = JsonConvert.SerializeObject(body);
            StringContent content = new (jsonValue, Encoding.UTF8, "application/json");

            var response = client.PostAsync(ESTB_URL_AUTH_THOKEN, content).Result;
            // var message = response.Content.ReadAsStringAsync().Result;
            return response.Content.ReadAsStringAsync();

            /* 
            -------------------------------------
            * Examples GET II
            -------------------------------------
            using HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", app.PrepareBaseAuthToken64("integration_tzp1", "12INT_admin"));
            var result = client.GetAsync(ESTB_URL_AUTH_THOKEN).GetAwaiter().GetResult();
            if (HttpStatusCode.OK != result.StatusCode) throw new Exception("Authenticated was failing. Get: " + result);
            return result.Content.ReadAsStringAsync();
            =============================================================================================
            
            -------------------------------------
             * Examples POST II
            -------------------------------------
            using (var _client = new HttpClient())
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", app.PrepareBaseAuthToken64());
                using (var responce = _client.GetAsync(ESTB_URL_AUTH_THOKEN))   // using(var resp = client.PostAsync(url, data)) {
                    if (responce.IsCompletedSuccessfully) throw new Exception("Authenticated was failing. Get: " + responce.Result);
                    Console.WriteLine("Succes after check auth token: " + responce.Result);
                }
            }
            =============================================================================================
            */
        }


        static void Main()
        {
            Program app = new Program();

            /// Connect to AC Service
            if (!app.StartAZCService()) throw new Exception("Fail: Connected to " + AZCSericeName + " was fail." + Environment.NewLine);

            /// Get some SQL results of DB Firebird
            List<string> sqlResult = app.GetSqlResultsSql(SqlSelect("ProgConfig"));
            Console.WriteLine("[DB_TOPAZ]: SQL result = " + string.Join(Environment.NewLine, sqlResult.ToArray()));

            /// Send the first message to ESTB with Token
            Task.Delay(3).ContinueWith(t => Console.WriteLine("[ESTB]: .. waiting for ESTB-back responce ... "));
            string apiGetESTBAuthToken = app.ApiGetESTBAuthToken(app).Result;
            Console.WriteLine("[ESTB]: Responce = " + apiGetESTBAuthToken + Environment.NewLine);

            /// ---- 
            Console.WriteLine("[AZCService] CASE SUCCESS Token: Returned data = {"+ apiGetESTBAuthToken+"}");


            // Console.WriteLine(Environment.NewLine + "[AZCService] CASE SUCCESS Token: Returned data = {" + string.Join(",", ESTB_SuccessResponce_Case1_update_cars.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}");

            /*
            Console.WriteLine("[AZCService] ... proccessing CASE SUCCESS 1 and type `"+ ESTB_SuccessResponce_Case1_update_cars["type"] + "` ... ");
            string update_cars = ESTBResponceType.update_cars.ToString();

            switch (ESTB_SuccessResponce_Case1_update_cars["type"]) {
                case ESTBResponceType.update_cars:
                    List<string> dcCardsSqlResult = app.GetSqlResultsSql(SqlSelect("dcCards"));
                    List<string> dcCardsExtSqlResult = app.GetSqlResultsSql(SqlSelect("dcCardsExt"));
                    List<string> npCardsSqlResult = app.GetSqlResultsSql(SqlSelect("npCards"));
                    
                    Console.WriteLine("[DB_TOPAZ]: >> dcCards = " + string.Join(Environment.NewLine, dcCardsSqlResult.ToArray()));
                    Console.WriteLine("[DB_TOPAZ]: >> dcCardsExt = " + string.Join(Environment.NewLine, dcCardsExtSqlResult.ToArray()));
                    Console.WriteLine("[DB_TOPAZ]: >> npCards = " + string.Join(Environment.NewLine, npCardsSqlResult.ToArray()));
                    break;
                default:
                    throw new Exception("TODO: ТИП НЕ ОПРЕДЕЛЕН");
            }

            Console.WriteLine("[AZCService]: Finished proccessing" + Environment.NewLine);

            Console.WriteLine(ESTBResponceType.update_cars.ToString());
            */
            // TODO: update_cars >> массив всех машин с ТЗП с указанием их карт(на ТЗП одна машина это одна карта):


            //Console.WriteLine(ReadDescriptionESTBResponceType(case1UpdateCars));



            /// TODO: CASE FAIL 1:
            /// 




        }

    }

}