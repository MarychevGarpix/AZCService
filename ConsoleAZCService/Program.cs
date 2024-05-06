using System.Data;
using FirebirdSql.Data.FirebirdClient;


namespace ConsoleAZCService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello ConsoleAZCSercice!");

            string connectionString = "Database=C:\\Users\\garpix\\Downloads\\TopazOffice.FDB;" + "User=SYSDBA;" + "Password=masterkey;" + "Dialect=3;" + "Server=localhost";
            var con = new FbConnection(connectionString);
            con.Open();


            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = "SELECT * FROM dcAmounts";

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // parameters.ForEach(kvp => cmd.Parameters.Add(dbio.CreateParameter(kvp.Key, kvp.Value)));
            // LogSqlStatement(sqlQuery);
            IDataReader reader = cmd.ExecuteReader();

            Console.WriteLine(reader);

            /* 
            * var con = new FbConnection(connectionString);
            con.Open();

            Console.WriteLine(con.DataSource);

            IDbCommand dbcmd = con.CreateCommand();
            dbcmd.CommandText = "SELECT * FROM topaz;";
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("reader: " + reader);
            }
           */

            /*
             * string connectionString = "Database=C:\\Users\\garpix\\Downloads\\TopazOffice.FDB;" + "User=SYSDBA;" + "Password=masterkey;" + "Dialect=3;" + "Server=localhost";
            IDbConnection dbcon = new FbConnection(connectionString);
            dbcon.Open();

            IDbCommand dbcmd = dbcon.CreateCommand();
            dbcmd.CommandText = "SELECT * FROM topaz";

            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                object dataValue = reader.GetValue(0);
                string sValue = dataValue.ToString();

                // eventLog1.WriteEntry("Value: " + sValue);

                Console.WriteLine("Value: " + sValue);
            }

            // clean up
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbcon.Close();
            dbcon = null;*/

        }
    }
}