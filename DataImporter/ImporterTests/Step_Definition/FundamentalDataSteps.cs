using System;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImporterTests.Step_Definition
{
    [Binding]
    public class FundamentalDataSteps
    {
        private DatabaseManagerService.DatabaseManagerServiceClient _dbmClient = null;

        [Given(@"I had prepared new non-existant fundamental data for importing into already populated database")]
        public void GivenIHadPreparedNewNon_ExistantFundamentalDataForImportingIntoAlreadyPopulatedDatabase()
        {
            var connectionName = "JobsManager_TEST";

            DatabaseUtility.DeleteFileRegisteration(connectionName);

            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicEvents");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicValues");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEventKeyPersonnel");

            //setup jobsmanager with file to be imported criteria
            var secondFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDescriptionFile_Random_NoADP_2016", connectionName, 1);
            var firstFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_Random_NoADP_2016", connectionName, 2);
            var thirdFileReg = DatabaseUtility.RegisterFile("FundamentalData_KeyPersonnelFile_Random_NoADP_2016", connectionName, 3);

            try
            {
                var dbmClientTemp = new DatabaseManagerService.DatabaseManagerServiceClient();
                dbmClientTemp.RetrieveRawDataFiles("", 1);

                System.Threading.Thread.Sleep(120000);
            }
            catch (Exception ex)
            {
                ScenarioContext.Current.Add("Exception_OperationalContractCall", ex);
            }

            
            var additionalsA = DatabaseUtility.RegisterFile("FundamentalData_EventDescriptionFile_Random_ADP2016", connectionName, 1);
            var additionalsB = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_Random_ADP2016", connectionName, 2);
            var additionalsC = DatabaseUtility.RegisterFile("FundamentalData_KeyPersonnelFile_Random_ADP2016", connectionName, 3);

            object exception = null;
            ScenarioContext.Current.TryGetValue("Exception_OperationalContractCall", out exception);
            Assert.AreEqual(exception, null);

            Assert.AreEqual(2, firstFileReg);
            Assert.AreEqual(2, secondFileReg);
            Assert.AreEqual(2, thirdFileReg);

            Assert.AreEqual(2, additionalsA);
            Assert.AreEqual(2, additionalsB);
            Assert.AreEqual(2, additionalsC);
        }

        [Given(@"I had prepared the latest fundamental data for importing into already populated database")]
        public void GivenIHadPreparedTheLatestFundamentalDataForImportingIntoAlreadyPopulatedDatabase()
        {
            var connectionName = "JobsManager_TEST";

            DatabaseUtility.DeleteFileRegisteration(connectionName);

            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicEvents");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicValues");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEventKeyPersonnel");

            //setup jobsmanager with file to be imported criteria
            var secondFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDescriptionFile_Random", connectionName, 1);
            var firstFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_Random", connectionName, 2);
            var thirdFileReg = DatabaseUtility.RegisterFile("FundamentalData_KeyPersonnelFile_Random", connectionName, 3);

            try
            {
                var dbmClientTemp = new DatabaseManagerService.DatabaseManagerServiceClient();
                dbmClientTemp.RetrieveRawDataFiles("", 1);

                System.Threading.Thread.Sleep(120000);
            }
            catch (Exception ex)
            {
                ScenarioContext.Current.Add("Exception_OperationalContractCall", ex);
            }
            DatabaseUtility.DeleteFileRegisteration(connectionName);

            var additionalsB = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_RandomUpdate", connectionName, 2);

            object exception = null;
            ScenarioContext.Current.TryGetValue("Exception_OperationalContractCall", out exception);
            Assert.AreEqual(exception, null);

            Assert.AreEqual(2, additionalsB);

            Assert.AreEqual(2, firstFileReg);
            Assert.AreEqual(2, secondFileReg);
            Assert.AreEqual(2, thirdFileReg);
        }
        
        [Given(@"I have prepared random fundamental data for importing")]
        public void GivenIHavePreparedRandomFundamentalDataForImporting()
        {
            var connectionName = "JobsManager_TEST";

            DatabaseUtility.DeleteFileRegisteration(connectionName);

            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicEvents");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicValues");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEventKeyPersonnel");

            //setup jobsmanager with file to be imported criteria
            var secondFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDescriptionFile_Random", connectionName, 1);
            var firstFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_Random", connectionName, 2);
            var thirdFileReg = DatabaseUtility.RegisterFile("FundamentalData_KeyPersonnelFile_Random", connectionName, 3);

            //setup 
            Assert.AreEqual(2, firstFileReg);
            Assert.AreEqual(2, secondFileReg);
            Assert.AreEqual(2, thirdFileReg);
        }

        [Given(@"I have prepared the fundamental data for double importation")]
        public void GivenIHavePreparedTheFundamentalDataForDoubleImportation()
        {
            var connectionName = "JobsManager_TEST";

            DatabaseUtility.DeleteFileRegisteration(connectionName);

            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicEvents");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEconomicValues");
            DatabaseUtility.ExecuteNonQuery("TRADES_TEST_FundamentalData", "DELETE FROM FundamentalMarketData.dbo.tblEventKeyPersonnel");

            var secondFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDescriptionFile_Random", connectionName, 1);
            var firstFileReg = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_Random", connectionName, 2);
            var thirdFileReg = DatabaseUtility.RegisterFile("FundamentalData_KeyPersonnelFile_Random", connectionName, 3);


            try
            {
                var dbmClientTemp = new DatabaseManagerService.DatabaseManagerServiceClient();
                dbmClientTemp.RetrieveRawDataFiles("", 1);

                System.Threading.Thread.Sleep(120000);
            }
            catch (Exception ex)
            {
                ScenarioContext.Current.Add("Exception_OperationalContractCall", ex);
            }

            var additionalsA = DatabaseUtility.RegisterFile("FundamentalData_EventDescriptionFile_RandomA", connectionName, 1);
            var additionalsB = DatabaseUtility.RegisterFile("FundamentalData_EventDataFile_RandomA", connectionName, 2);
            var additionalsC = DatabaseUtility.RegisterFile("FundamentalData_KeyPersonnelFile_RandomA", connectionName, 3);

            object exception = null;
            ScenarioContext.Current.TryGetValue("Exception_OperationalContractCall", out exception);
            Assert.AreEqual(exception, null);

            //setup 
            Assert.AreEqual(2, firstFileReg);
            Assert.AreEqual(2, secondFileReg);
            Assert.AreEqual(2, thirdFileReg);

            Assert.AreEqual(2, additionalsA);
            Assert.AreEqual(2, additionalsB);
            Assert.AreEqual(2, additionalsC);
        }


        [When(@"I have connected to the Database Importer Service")]
        public void WhenIHaveConnectedToTheDatabaseImporterService()
        {
            _dbmClient = new DatabaseManagerService.DatabaseManagerServiceClient();          
        }

        [When(@"I have called for this data to be imported")]
        public void WhenIHaveCalledForThisDataToBeImported()
        {
            try
            {
                _dbmClient.RetrieveRawDataFiles("", 1);
                
                System.Threading.Thread.Sleep(120000);
            }
            catch (Exception ex)
            {
                ScenarioContext.Current.Add("Exception_OperationalContractCall", ex);
            }
        }

        [Then(@"the result should be (.*) rows in the database")]
        public void ThenTheResultShouldBeRowsInTheDatabase(int p0)
        {
            object exception = null;

            ScenarioContext.Current.TryGetValue("Exception_OperationalContractCall", out exception);

            var rows = DatabaseUtility.GetRowCount("TRADES_TEST_FundamentalData", "SELECT COUNT(*) FROM FundamentalMarketData.dbo.tblEconomicEvents");

            Assert.AreEqual(p0, rows);
            Assert.AreEqual(exception, null);
        }

        [Then(@"a total of (.*) ADP Report updates for the year (.*)")]
        public void ThenATotalOfADPReportUpdatesForTheYear(int p0, int p1)
        {
            object exception = null;

            ScenarioContext.Current.TryGetValue("Exception_OperationalContractCall", out exception);

            var rows = DatabaseUtility.GetRowCount("TRADES_TEST_FundamentalData",
                "SELECT "
                + " COUNT([EventHeadline])"
                + "  FROM [FundamentalMarketData].[dbo].[tblEconomicValues] DD"
                + "    JOIN [FundamentalMarketData].[dbo].tblEconomicEvents AA ON DD.EventID = AA.ID "
                + "   WHERE AA.EventHeadline = N'U.S. ADP Nonfarm Employment Change' AND YEAR(AA.ReleaseDateTime) = '"+p1+"'"
            );

            Assert.AreEqual(p0, rows);
            Assert.AreEqual(exception, null);
        }

        [Then(@"a total of (.*) ADP Report data")]
        public void ThenATotalOfADPReportData(int p0)
        {
            object exception = null;

            ScenarioContext.Current.TryGetValue("Exception_OperationalContractCall", out exception);

            var rows = DatabaseUtility.GetRowCount("TRADES_TEST_FundamentalData",
                "SELECT COUNT(*) FROM [FundamentalMarketData].[dbo].[tblEconomicValues] DD "
                +"JOIN [FundamentalMarketData].[dbo].tblEconomicEvents AA ON DD.EventID = AA.ID "
                +"WHERE AA.EventHeadline = N'U.S. ADP Nonfarm Employment Change' " 
                +"AND DD.Actual = '700' AND DD.Forecast = '194' AND DD.Previous = '200' "
                +"OR AA.EventHeadline = N'U.S. ADP Nonfarm Employment Change' "
                +"AND DD.Actual = '200' AND DD.Forecast = '200' AND DD.Previous = '175' "
                +"OR AA.EventHeadline = N'U.S. ADP Nonfarm Employment Change' "
                +"AND DD.Actual = '200' AND DD.Forecast = '225' AND DD.Previous = '214'"
            );

            Assert.AreEqual(p0, rows);
            Assert.AreEqual(exception, null);
        }

    }
}
