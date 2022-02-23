using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace RTChartPatternDataService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRTChartPatternDataService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        List<tblPattern> GetPatterns(List<String> patternList, String exchange, String timeFrame);


        // TODO: Add your service operations here
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "RTChartPatternDataService.ContractType".
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

    //------------------------------//


    [DataContract]
    public class tblPattern
    {
        [DataMember]
        public String Pattern_ID;

        [DataMember]
        public String TimeFrame;

        [DataMember]
        public String DestinationExchange;

        [DataMember]
        public String SourceExchange;

        [DataMember]
        public Double MatchLevel;

        [DataMember]
        public Double MatchLevelPercentage;

        [DataMember]
        public DateTime StartDateTime;

        [DataMember]
        public DateTime EndDateTime;

        [DataMember]
        public String SymbolID;

        [DataMember]
        public String PatternType;

    }


}
