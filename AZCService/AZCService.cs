using System;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using System.Runtime.InteropServices;
using FirebirdSql.Data.FirebirdClient;


namespace AZCService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    public partial class AZCService : ServiceBase
    {
        private int eventId = 1;


        public AZCService(string[] args)
        {
            InitializeComponent();

            Console.WriteLine(" - AZCService - ");

            string eventSourceName = "MySource";
            string logName = "MyNewLog";

            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }

            if (args.Length > 1)
            {
                logName = args[1];
            }

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            Console.WriteLine(">> OnTimer <<");

            using (var connection = new FbConnection(@"User=SYSDBA;Password=masterkey;Database=C:\Users\garpix\Downloads\TopazOffice.FDB"))
            {
                connection.Open();

                using (EventLog log = new EventLog())
                {
                    eventLog1.Source = "MySource-2";
                    eventLog1.Log = "MyLog-2";
                    string message = "Hello: " + connection.ToString();
                    log.WriteEntry(message);
                }

                Console.WriteLine(" . . . ", connection);

            }

            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart-2.");

            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Set up a timer that triggers every minute.
            Timer timer = new Timer();
            timer.Interval = 3000; // 2 seconds
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            string connectionString = "Database=C:\\Users\\garpix\\Downloads\\TopazOffice.FDB;" + "User=SYSDBA;" + "Password=masterkey;" + "Dialect=3;" + "Server=localhost";
            IDbConnection dbcon = new FbConnection(connectionString);
            dbcon.Open();
            IDbCommand dbcmd = dbcon.CreateCommand();
            string sql = "SELECT * FROM topaz";
            dbcmd.CommandText = sql;
            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                object dataValue = reader.GetValue(0);
                string sValue = dataValue.ToString();
                eventLog1.WriteEntry("Value: " + sValue);

            }

            // clean up
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbcon.Close();
            dbcon = null;

            /*using (var connection = new FbConnection(@"User=SYSDBA;Password=masterkey;Database=C:\Users\garpix\Downloads\TopazOffice.FDB"))
            {
                connection.Open();
                using (EventLog log = new EventLog())
                {
                    eventLog1.Source = "MySource-2";
                    eventLog1.Log = "MyLog-2";
                    string message = "Hello: " + connection.ToString();
                    log.WriteEntry(message);
                }
            
            }*/

            // TODO: Insert monitoring activities here.
            eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop - 2");

            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

    }
}
