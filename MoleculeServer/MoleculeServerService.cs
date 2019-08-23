using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoleculeServer
{
    public partial class MoleculeServerService : ServiceBase
    {
        IP_Listener Listener;

        public MoleculeServerService()
        {
            InitializeComponent();

            this.CanStop = true;
            this.CanPauseAndContinue = false;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            Listener = new IP_Listener();
            Thread ListenerThread = new Thread(new ThreadStart(Listener.Start));
            ListenerThread.Start();
        }

        protected override void OnStop()
        {
            Listener.Stop();
        }

        internal void Run()
        {
            Listener = new IP_Listener();
            Thread ListenerThread = new Thread(new ThreadStart(Listener.Start));
            ListenerThread.Start();
        }
    }
}
