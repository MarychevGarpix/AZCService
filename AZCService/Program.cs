using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;



namespace AZCService
{

    internal static class Program
    {
        public const string AZCSericeName = "AZCService";

        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AZCService(args)
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
