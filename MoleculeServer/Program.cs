using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MoleculeServer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            #if !DEBUG
                  ServiceBase[] ServicesToRun;
                  ServicesToRun = new ServiceBase[] 
			            { 
				            new MoleculeServerService()
                        };
                  ServiceBase.Run(ServicesToRun);
            #else
                        new MoleculeServerService().Run();
            #endif
        }
    }
}
