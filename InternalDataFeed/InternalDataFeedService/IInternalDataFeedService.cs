using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace InternalDataFeedService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.

    [ServiceContract(CallbackContract = typeof(IBroadcastorCallBack))]
    public interface IInternalDataFeedService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        List<TradeSummary> GetTradeSummaryData(String symbol, int interval, String exchange, DateTime beginDate);

        [OperationContract]
        void GetTradeSummaryDataAsynch(String clientName, String symbol, int interval, String exchange, DateTime beginDate);

        [OperationContract]
        void GetTradeSummaryDataSingleDataPointAsynch(String clientName, String symbol, int interval, String exchange, DateTime beginDate);

        [OperationContract]
        List<String> GetSymbolList(String exchange);

        [OperationContract]
        void WorkCompleteFreeResources(String clientName);

        // TODO: Add your service operations here

        [OperationContract(IsOneWay = true)]
        void RegisterClient(string clientName);

        [OperationContract(IsOneWay = true)]
        void NotifyServer(EventDataTypeCollection eventData);
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "InternalDataFeedService.ContractType".
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
    public class TradeSummary
    {
        private DateTime _dateTime;
        private double _open;
        private double _high;
        private double _low;
        private double _close;
        private double _adjustmentClose;
        private int _volume;
        private String _symbolID;
        private String _exchange;
        private String _timeFrame;


        [DataMember]
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        [DataMember]
        public double Open
        {
            get { return _open; }
            set { _open = value; }
        }

        [DataMember]
        public double High
        {
            get { return _high; }
            set { _high = value; }
        }

        [DataMember]
        public double Low
        {
            get { return _low; }
            set { _low = value; }
        }

        [DataMember]
        public double Close
        {
            get { return _close; }
            set { _close = value; }
        }

        [DataMember]
        public int Volume
        {
            get { return _volume; }
            set { _volume = value; }
        }

        [DataMember]
        public double AdjustmentClose
        {
            get { return _adjustmentClose; }
            set { _adjustmentClose = value; }
        }

        [DataMember]
        public String SymbolID
        {
            get { return _symbolID; }
            set { _symbolID = value; }
        }

        [DataMember]
        public String TimeFrame
        {
            get { return _timeFrame; }
            set { _timeFrame = value; }
        }

        [DataMember]
        public String Exchange
        {
            get { return _exchange; }
            set { _exchange = value; }
        }
    }

    [DataContract()]
    public class EventDataType
    {
        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public TradeSummary EventMessage { get; set; }
    }

    [DataContract()]
    public class EventDataTypeCollection
    {
        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public List<TradeSummary> EventMessage { get; set; }
    }

    public interface IBroadcastorCallBack
    {
        [OperationContract(IsOneWay = true)]
        void BroadcastToClient(EventDataTypeCollection eventData);
    }
    //public interface IBroadcastorCallBack
    //{
    //    [OperationContract(IsOneWay = true)]
    //    void BroadcastToClient(EventDataType eventData);
    //}

}
