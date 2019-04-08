using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MoleculeServer
{
    public partial class MoleculeServerService : ServiceBase
    {
        IP_Listener Listener;

        public MoleculeServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Listener = new IP_Listener();
            Listener.Start();
        }

        protected override void OnStop()
        {
            Listener.Stop();
        }

        internal void Run()
        {
            Listener = new IP_Listener();
            Listener.Start();
        }
    }
}
