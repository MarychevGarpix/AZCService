using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using FirebirdSql.Data.FirebirdClient;


namespace ConsoleAZCService
{

    internal class Program
    {

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
            bool is_run = app.StartAZCService();

            if (!is_run) {
                string message = "Fail: Connected to " + AZCSericeName + " was fail" + Environment.NewLine;
                throw new Exception(message);
            }

            /// Get SQL results to Firebird database
            List<string> result = app.GetSqlResultsSql(SqlShowAllTables());
            
            Console.WriteLine(string.Join(Environment.NewLine, result.ToArray()));
            Console.WriteLine(result.ToString());


            // Dictionary<string, string> parameters = new Dictionary<string, string>();
            // parameters.ForEach(kvp => cmd.Parameters.Add(dbio.CreateParameter(kvp.Key, kvp.Value)));

        }



    }

}