using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ComputedDataService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IComputedDataService
    {
        [OperationContract]
        string GetData(int value);

        //[OperationContract]
        //List<tblCorrelation> GetCorrelations(List<String> symbolList, DateTime startDate, DateTime endDate, String entity, String exchange, String timeFrame);

        [OperationContract]
        List<tblCorrelation> GetCorrelations(List<String> symbolList, String entity, String exchange, String timeFrame);
        
        [OperationContract]
        List<PerformanceStats> GetPerformanceStatistics(string operation, string type, string timeframe, string exchange);

        [OperationContract]
        List<PerformanceStats> GetIndicatorPerformanceStatistics(List<String> symbolList, string category, string timeframe, string exchange);

        [OperationContract]
        string GetIndicatorWithBestPerformanceStatistics(List<String> symbolList, string type, string timeframe, string exchange);

    }


    //DataContracts

    [DataContract]
    public class tblCorrelation
    {
        [DataMember]
        public String HC_ID;
        
        [DataMember]
        public String CorrelatingEntityID;

        [DataMember]
        public String DestinationExchange;
        
        [DataMember]
        public String SourceExchange;

        [DataMember]
        public Double Distance;
        
        [DataMember]
        public Double CorrelationRatio;

        [DataMember]
        public DateTime StartDateTime;

        [DataMember]
        public DateTime EndDateTime;

        [DataMember]
        public String SymbolID;

        [DataMember]
        public String Event;

        [DataMember]
        public String TimeFrame;
        
        [DataMember]
        public String ResultantSymbolID;      
    }

    [DataContract]
    public class PerformanceStats
    {
        private List<string> _headers = new List<string>();
        private List<List<string>> _statsLog = new List<List<string>>();
        private string _description = "";
        private string _performanceStatsType = "";

        [DataMember]
        public List<string> Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }

        [DataMember]
        public List<List<string>> StatsLog
        {
            get { return _statsLog; }
            set { _statsLog = value; }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [DataMember]
        public string PerformanceStatsType
        {
            get { return _performanceStatsType; }
            set { _performanceStatsType = value; }
        }
    }
}
