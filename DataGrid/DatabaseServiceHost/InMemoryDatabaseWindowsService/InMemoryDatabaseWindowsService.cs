using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace InMemoryDatabaseWindowsService
{
    public partial class InMemoryDatabaseWindowsService : ServiceBase
{
        public ServiceHost serviceHost = null;

        public InMemoryDatabaseWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the CalculatorService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(DatabaseWcfService.DatabaseWcfService));

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();

        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
