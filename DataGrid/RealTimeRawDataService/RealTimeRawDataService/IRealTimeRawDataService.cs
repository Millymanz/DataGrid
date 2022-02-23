using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Receiver;

namespace RealTimeRawDataService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRealTimeRawDataService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        List<TradeSummary> GetTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame);

        [OperationContract]
        List<TradeSummary> GetFilteredRealTimeTradeSummaries(List<String> symbolList, String exchange, String timeframe, DateTime startDateTime, DateTime endDateTime);

        [OperationContract]
        List<TradeSummary> GetRealTimeTradeSummaries(List<String> symbolList, List<int> dataPoints, bool bLast, String entity, String exchange, String timeFrame);

        [OperationContract]
        void SetTradeSummary(TradeSummary tradeSummary);

        [OperationContract]
        List<UpdatedTimeFrame> GetUpdatedTimeFrameList();

        [OperationContract]
        DateTime GetFirstDateTime(String symbolID, String exchange, String timeFrame);


        // TODO: Add your service operations here
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "RealTimeRawDataService.ContractType".
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }



    [DataContract]
    public class UpdatedTimeFrame
    {
        private String _timeFrame;
        private String _symbolID;

        [DataMember]
        public String TimeFrame
        {
            get { return _timeFrame; }
            set { _timeFrame = value; }
        }

        [DataMember]
        public String SymbolID
        {
            get { return _symbolID; }
            set { _symbolID = value; }
        }
    }


}
