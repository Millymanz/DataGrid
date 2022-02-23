using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using System.IO;


namespace RTChartPatternDataService
{
    public enum MODE
    {
        Live = 0,
        Test = 1,
        LiveTest = 2
    }


    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class RTChartPatternDataService : IRTChartPatternDataService
    {
        public static MODE Mode;


        public RTChartPatternDataService()
        {

        }


        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public List<tblPattern> GetPatterns(List<String> patternList, String exchange, String timeFrame)
        {
            return null;
        }

    }
}
