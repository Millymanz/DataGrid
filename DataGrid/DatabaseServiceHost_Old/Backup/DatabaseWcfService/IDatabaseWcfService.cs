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
        List<DayTradeSummary> GetDayTradeSummaries(List<String> symbolList, DateTime startDate, DateTime endDate, String stockExchange);
    }
}
