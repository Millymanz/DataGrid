using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Data;

namespace DatabaseWcfService
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in App.config.
    [ServiceContract]
    public interface IDatabaseWcfService
    {
        [OperationContract]
        List<DataTable> GetDatabaseData(String query);

        [OperationContract]
        List<TradeSummary> GetTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame);
        
        [OperationContract]
        List<EconomicFundamentals> GetEconomicFundamentals(List<string> symbolList, List<string> eventTypeList, DateTime startDate, DateTime endDate, string category, string country, List<string> economicItemHelpers);

        [OperationContract]
        bool DataExistForTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String exchange, String timeFrame);

    }
}
