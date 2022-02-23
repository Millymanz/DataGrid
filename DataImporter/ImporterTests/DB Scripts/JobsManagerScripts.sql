USE [master]
GO

/****** Object:  Database [JobsManager]    Script Date: 14/09/2016 19:55:17 ******/
CREATE DATABASE [JobsManager]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'JobsManager', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQL2012\MSSQL\DATA\JobsManager.mdf' , SIZE = 3725312KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'JobsManager_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQL2012\MSSQL\DATA\JobsManager_log.ldf' , SIZE = 1475904KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [JobsManager] SET COMPATIBILITY_LEVEL = 110
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [JobsManager].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [JobsManager] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [JobsManager] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [JobsManager] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [JobsManager] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [JobsManager] SET ARITHABORT OFF 
GO

ALTER DATABASE [JobsManager] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [JobsManager] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [JobsManager] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [JobsManager] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [JobsManager] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [JobsManager] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [JobsManager] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [JobsManager] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [JobsManager] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [JobsManager] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [JobsManager] SET  DISABLE_BROKER 
GO

ALTER DATABASE [JobsManager] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [JobsManager] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [JobsManager] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [JobsManager] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [JobsManager] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [JobsManager] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [JobsManager] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [JobsManager] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [JobsManager] SET  MULTI_USER 
GO

ALTER DATABASE [JobsManager] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [JobsManager] SET DB_CHAINING OFF 
GO

ALTER DATABASE [JobsManager] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [JobsManager] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO

ALTER DATABASE [JobsManager] SET  READ_WRITE 
GO




USE [JobsManager]
GO
/****** Object:  StoredProcedure [dbo].[proc_BulkInsertExchangeLookUps]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





/**This proc is run manually and not by an application*/






/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_BulkInsertExchangeLookUps]
@ROWSAFFECTED int output
AS

DELETE FROM [dbo].tblSTAGING_ExchangeLookUp

IF OBJECT_ID('StageExchangeLookUp') IS NOT NULL
BEGIN
	DROP TABLE StageExchangeLookUp
END


BULK
INSERT [dbo].tblSTAGING_ExchangeLookUp
FROM 'C:\Users\Public\Downloads\Output\Test\Exchange\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
--ERRORFILE = 'C:\Users\Public\Downloads\Output\Test\RawData\Staging\myRubbishData.log', 
MAXERRORS = 99999999
)

DECLARE @GUID [nvarchar](MAX)

SET @ROWSAFFECTED = 0


----------------------------------------------------------------------------------------

BEGIN

SELECT NEWID() AS ID,
[dbo].tblSTAGING_ExchangeLookUp.[SymbolID],
[dbo].tblSTAGING_ExchangeLookUp.[Exchange] INTO StageExchangeLookUp
FROM [dbo].tblSTAGING_ExchangeLookUp;


/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY SymbolID ORDER BY SymbolID DESC) AS rn
  FROM [dbo].StageExchangeLookUp
)
DELETE FROM DistinctSet
WHERE rn > 1 


/*Ensure that there is only one batch of date with a given enddate no duplicates for when this sp is run several times within the day */
BEGIN

	DELETE [dbo].StageExchangeLookUp FROM [dbo].StageExchangeLookUp DD
	INNER JOIN [dbo].ExchangeLookUp GG ON DD.SymbolID = GG.SymbolID 

	/**delete alias as well*/
END



INSERT INTO [dbo].ExchangeLookUp (
	   ID
	  ,[SymbolID]
      ,Exchange)
SELECT DISTINCT 
	   [ID]
	  ,LOWER(LTRIM(RTRIM(StageExchangeLookUp.[SymbolID])))
      ,[Exchange] FROM StageExchangeLookUp

DROP TABLE StageExchangeLookUp

/*********************************************************************************************/



BEGIN
	/*Delete any duplicates within the main database table*/
	WITH DistinctSet AS
	(
	  SELECT row_number() OVER(PARTITION BY SymbolID ORDER BY SymbolID DESC) AS rn
	  FROM [dbo].ExchangeLookUp
	)
	DELETE FROM DistinctSet
	WHERE rn > 1 
END

END






GO
/****** Object:  StoredProcedure [dbo].[proc_CheckAllJobCompletions]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_CheckAllJobCompletions]
@DayStartDatetime datetime,
@DayEndDatetime datetime
AS

BEGIN
	/*SELECT [JobsManager].[dbo].[Jobs].JobStartTime, [JobsManager].[dbo].[Jobs].JobEndTime, [JobsManager].[dbo].[Jobs].Completed FROM [JobsManager].[dbo].[Jobs] 
	WHERE [JobsManager].[dbo].[Jobs].JobStartTime BETWEEN @DayStartDatetime AND @DayEndDatetime AND [JobsManager].[dbo].[Jobs].Completed = 0
	*/
	SELECT COUNT(*) AS Total FROM [JobsManager].[dbo].[Jobs] 
	WHERE [JobsManager].[dbo].[Jobs].JobStartTime BETWEEN @DayStartDatetime AND @DayEndDatetime AND [JobsManager].[dbo].[Jobs].Completed = 0
END







GO
/****** Object:  StoredProcedure [dbo].[proc_CheckRawDataFileRegisteration]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_CheckRawDataFileRegisteration]
@FileName nvarchar(20),
@Count int output
AS


SELECT @Count = COUNT(*) FROM [dbo].[FileImports] WHERE [dbo].[FileImports].[FileName] = @FileName


GO
/****** Object:  StoredProcedure [dbo].[proc_DataConsumptionCompletionUpdate]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[proc_DataConsumptionCompletionUpdate]
@FileName nvarchar(20)
AS

DECLARE @ID nvarchar(100)

SELECT @ID = [ID] FROM dbo.FileImports D WHERE D.FileName = @FileName

UPDATE dbo.DataConsumer SET dbo.DataConsumer.DownloadCompleted = '1' WHERE dbo.DataConsumer.FileImportID = @ID





GO
/****** Object:  StoredProcedure [dbo].[proc_DeleteAllCompatiblityCheckers_Admin]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[proc_DeleteAllCompatiblityCheckers_Admin]
AS

--DELETE FROM [dbo].HistoricallyCompatibleStocks
--DELETE FROM [dbo].NonCompatibleStocks

DELETE FROM [dbo].PeriodicCompatibilityLookUp
DELETE FROM [dbo].NonCompatibleSymbols



GO
/****** Object:  StoredProcedure [dbo].[proc_DeleteAllFileImportLogs_Admin]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[proc_DeleteAllFileImportLogs_Admin]
AS

DELETE FROM [dbo].[FileImports]
DELETE FROM [dbo].[ImportLog]
DELETE FROM [dbo].[JobsWithinFile]



GO
/****** Object:  StoredProcedure [dbo].[proc_DeleteAllJobs_Admin]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_DeleteAllJobs_Admin]
AS

DELETE FROM [dbo].[Jobs]



GO
/****** Object:  StoredProcedure [dbo].[proc_EndLogIndicatorPerformanceStats]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_EndLogIndicatorPerformanceStats]
@ID int
AS

BEGIN
	UPDATE [JobsManager].[dbo].PerformanceStatistics_BatchRunLog SET [dbo].PerformanceStatistics_BatchRunLog.[EndDateTime] = GETDATE()
	WHERE [dbo].PerformanceStatistics_BatchRunLog.[BatchRunID] = @ID
END



GO
/****** Object:  StoredProcedure [dbo].[proc_GetAllExchangeLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROC [dbo].[proc_GetAllExchangeLookUp]
AS

BEGIN
	SELECT [SymbolID], Exchange FROM [dbo].[ExchangeLookUp] 
END





GO
/****** Object:  StoredProcedure [dbo].[proc_GetAllSymbolNameConversionLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE PROC [dbo].[proc_GetAllSymbolNameConversionLookUp]
AS

BEGIN
	SELECT [SymbolID], SymbolIDFriendly FROM [dbo].[SymbolNameConversionLookUp]
END







GO
/****** Object:  StoredProcedure [dbo].[proc_GetChartPatternPerformanceStats]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE  [dbo].[proc_GetChartPatternPerformanceStats]
@timeFrame [nvarchar](20)
AS

DECLARE @LatestRunID int
BEGIN
	SELECT @LatestRunID = MAX([BatchRunID]) FROM [JobsManager].[dbo].[PerformanceStatistics_BatchRunLog]
	WHERE [dbo].[PerformanceStatistics_BatchRunLog].[Type] = 'ChartPattern'
	AND [dbo].[PerformanceStatistics_BatchRunLog].[StartDateTime] IS NOT NULL
	AND [dbo].[PerformanceStatistics_BatchRunLog].[EndDateTime] IS NOT NULL


	SELECT * FROM [JobsManager].[dbo].[PerformanceStatistics_ChartPatternLog] 
	WHERE [dbo].[PerformanceStatistics_ChartPatternLog].BatchRunLogID = @LatestRunID
	AND [dbo].[PerformanceStatistics_ChartPatternLog].TimeFrame = @timeFrame
END


GO
/****** Object:  StoredProcedure [dbo].[proc_GetChartPatternPerformanceStats_Overview]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE  [dbo].[proc_GetChartPatternPerformanceStats_Overview]
@timeFrame [nvarchar](20)
AS

DECLARE @LatestRunID int
BEGIN
	SELECT @LatestRunID = MAX([BatchRunID]) FROM [JobsManager].[dbo].[PerformanceStatistics_BatchRunLog]
	WHERE [dbo].[PerformanceStatistics_BatchRunLog].[Type] = 'ChartPattern'

	SELECT [Pattern]
		  , SUM([Total]) AS [Total]
		  , SUM([CorrectRecognition]) AS [CorrectRecognition]
		  , SUM([Percentage]) / COUNT([Pattern]) AS [Percentage]
	  FROM [JobsManager].[dbo].[PerformanceStatistics_ChartPatternLog]
	  WHERE [JobsManager].[dbo].[PerformanceStatistics_ChartPatternLog].[BatchRunLogID] = @LatestRunID
	  AND [JobsManager].[dbo].[PerformanceStatistics_ChartPatternLog].TimeFrame = @timeFrame
	  GROUP BY [Pattern] 

END


GO
/****** Object:  StoredProcedure [dbo].[proc_GetChartPatternTemplate]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE  [dbo].[proc_GetChartPatternTemplate]
@SymbolID nvarchar(20),
@TimeFrame nvarchar(10),
@Exchange nvarchar(10),
@Pattern nvarchar(50)
AS

BEGIN
	SELECT [SymbolID]
		  ,[TimeFrame]
		  ,[Pattern]
		  ,[StartDateTime]
		  ,[EndDateTime]
	  FROM [dbo].[ChartPatternTemplates] DD
	  WHERE DD.[SymbolID] = @SymbolID AND DD.TimeFrame = @TimeFrame
	  AND DD.Exchange = @Exchange AND DD.Pattern = @Pattern

END

GO
/****** Object:  StoredProcedure [dbo].[proc_GetExchangeLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[proc_GetExchangeLookUp]
@keywordvar nvarchar(50)
AS

BEGIN
	SELECT Exchange FROM [dbo].[ExchangeLookUp] DD
	WHERE DD.SymbolID = @keywordvar
END




GO
/****** Object:  StoredProcedure [dbo].[proc_GetImportReadyFiles]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE  [dbo].[proc_GetImportReadyFiles]
AS
--SELECT dbo.[FileImports].[FileName] FROM dbo.[FileImports] 
--JOIN dbo.DataConsumer ON dbo.[FileImports].ID = dbo.DataConsumer.FileImportID
--WHERE dbo.[FileImports].ImportCompleted ='0' AND dbo.[FileImports].DataType ='Raw'

SELECT dbo.[FileImports].[FileName], dbo.[FileImports].[DataType] FROM dbo.[FileImports] 
JOIN dbo.DataConsumer ON dbo.[FileImports].ID = dbo.DataConsumer.FileImportID
WHERE dbo.[FileImports].ImportCompleted ='0' AND NOT dbo.[FileImports].DataType ='Computed'




GO
/****** Object:  StoredProcedure [dbo].[proc_GetIndicatorPerformanceStats]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[proc_GetIndicatorPerformanceStats]
@timeFrame [nvarchar](20)
AS

DECLARE @LatestRunID int
BEGIN
	SELECT @LatestRunID = MAX([BatchRunID]) FROM [JobsManager].[dbo].[PerformanceStatistics_BatchRunLog]
	WHERE [dbo].[PerformanceStatistics_BatchRunLog].[Type] = 'Indicator'
	AND [dbo].[PerformanceStatistics_BatchRunLog].[StartDateTime] IS NOT NULL
	AND [dbo].[PerformanceStatistics_BatchRunLog].[EndDateTime] IS NOT NULL


	SELECT * FROM [dbo].[PerformanceStatistics_IndicatorLog] DD
	WHERE DD.TimeFrame = @timeFrame AND DD.BatchRunLogID = @LatestRunID
END
GO
/****** Object:  StoredProcedure [dbo].[proc_GetSymbolNameConversionLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROC [dbo].[proc_GetSymbolNameConversionLookUp]
@keywordvar nvarchar(50)
AS

BEGIN
	SELECT SymbolIDFriendly FROM [dbo].[SymbolNameConversionLookUp] DD
	WHERE DD.SymbolID = @keywordvar
END





GO
/****** Object:  StoredProcedure [dbo].[proc_ImportLogging]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_ImportLogging]
@FileName nvarchar(100),
@ErrorMsg nvarchar(MAX),
@CSVRecordCount int,
@ImportedRecordCount int,
@PercentageSuccess float,
@PercentageFailure float,
@ImportStartTime datetime,
@ImportEndTime datetime,
@SuccessThreshold float
AS

DECLARE @ID nvarchar(100)
DECLARE @FileImportID nvarchar(100)

SET @ID = NEWID()

/* We dont want to update as it is a log database we want to see a history of activity */

SELECT @FileImportID = [JobsManager].[dbo].FileImports.ID FROM [JobsManager].[dbo].FileImports 
WHERE [JobsManager].[dbo].FileImports.[FileName] = @FileName

INSERT INTO [JobsManager].[dbo].ImportLog (ID, FileImportID, ErrorMsg, CSVRecordCount, ImportedRecordCount, PercentageSuccess, PercentageFailure, ImportStartTime, ImportEndTime)
VALUES (@ID, @FileImportID, @ErrorMsg, @CSVRecordCount, @ImportedRecordCount, @PercentageSuccess, @PercentageFailure, @ImportStartTime, @ImportEndTime)

IF @PercentageSuccess > @SuccessThreshold
	BEGIN
		UPDATE [JobsManager].[dbo].FileImports SET [JobsManager].[dbo].FileImports.ImportCompleted = 1
		WHERE [JobsManager].[dbo].FileImports.ID = @FileImportID
	END
ELSE
	BEGIN
		UPDATE [JobsManager].[dbo].FileImports SET [JobsManager].[dbo].FileImports.ImportCompleted = 0
		WHERE [JobsManager].[dbo].FileImports.ID = @FileImportID
	END


GO
/****** Object:  StoredProcedure [dbo].[proc_InsertIndicatorPerformanceStats]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_InsertIndicatorPerformanceStats]
@BatchRunId int,
@SymbolID nvarchar(20),
@TimeFrame nvarchar(10),
@Indicator nvarchar(50),
@Categorization nvarchar(20),
@Scenario nvarchar(10),
@Returns decimal(18, 2),
@AverageReturns decimal(18, 7),
@MedianReturn decimal(18, 7),
@PercentPositive decimal(18, 2),
@PercentNegative decimal(18, 2),
@Description nvarchar(250)
AS

BEGIN
	INSERT [dbo].[PerformanceStatistics_IndicatorLog] ([BatchRunLogID], [SymbolID], [TimeFrame], [Indicator], [Categorization], [Scenario], [Returns], [Average Return], [Median Return], [Percent Positive], [Percent Negative], [Description]) 
	VALUES (@BatchRunId, @SymbolID, @TimeFrame, @Indicator, @Categorization, @Scenario, @Returns, @AverageReturns, @MedianReturn, @PercentPositive, @PercentNegative, @Description)
END



GO
/****** Object:  StoredProcedure [dbo].[proc_InsertJobTickets]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[proc_InsertJobTickets]
@JobTicket nvarchar(MAX),
@JobId nvarchar(200),
@SymbolID nvarchar(20),
@ProcessType nvarchar(20),
@Exchange nvarchar(20),
@CompatibilityStartDate smallDateTime,
@EndDate smallDateTime,
@JobTicketCreationDate date
AS

DECLARE @NoJobs INT
DECLARE @ID nvarchar(100)

SELECT @NoJobs = COUNT(*) FROM [dbo].Jobs DD 
WHERE DD.SymbolID = @SymbolID AND DD.StockExchange = @Exchange AND DD.ProcessType = @ProcessType AND DD.CompatibilityStartDate = @CompatibilityStartDate
AND DD.EndDate = @EndDate


	IF @NoJobs = 0
	BEGIN 

	SET @ID = NEWID()
		INSERT INTO [dbo].Jobs (ID, JobTicket, JobId, SymbolID, ProcessType, StockExchange, CompatibilityStartDate, EndDate, JobTicketCreationDate)
		VALUES (@ID, @JobTicket, @JobId, @SymbolID, @ProcessType, @Exchange, @CompatibilityStartDate, @EndDate, @JobTicketCreationDate)
	END



GO
/****** Object:  StoredProcedure [dbo].[proc_InsertNonCompatibleStocksAndDates]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[proc_InsertNonCompatibleStocksAndDates]
@ID nvarchar(100),
@CriteriaSymbolID nvarchar(20),
@Exchange nchar(20),
@NonMatchingSymbolID nvarchar(20),
@NonMatchingStockExchange nchar(20),
@TimeFrame  nvarchar(10),
--@RefID nvarchar(100),
@DateTime smalldatetime

AS

BEGIN
	INSERT INTO dbo.NonCompatibleSymbols (ID, CriteriaSymbolID, Exchange, NonMatchingSymbolID, NonMatchingStockExchange, TimeFrame, [DateTime]) 
	VALUES(@ID, @CriteriaSymbolID, @Exchange, @NonMatchingSymbolID, @NonMatchingStockExchange, @TimeFrame, @DateTime)

	--INSERT INTO dbo.NonCompatibleDates_Diagnostics (RefID, [DateTime]) VALUES(@RefID, @DateTime)
END



GO
/****** Object:  StoredProcedure [dbo].[proc_JobsTickets]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_JobsTickets]
AS

DELETE FROM [dbo].STAGING_Jobs

BULK
INSERT [dbo].STAGING_Jobs
FROM 'C:\Users\Public\Downloads\Output\Test\JobTickets\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
MAXERRORS = 99999999
)

DECLARE @GUID [nvarchar](MAX)


BEGIN

--IF OBJECT_ID('tempdb..StageJobs') IS NOT NULL
--    DROP TABLE StageJobs;


SELECT NEWID() AS ID
	  ,[JobTicket]
      ,[JobId]
      ,[SymbolID]
      ,[StockExchange]
      ,[CompatibilityStartDate]
      ,[EndDate]
      ,[JobTicketCreationDate]
      ,[ProcessType]
	   INTO StageJobs
FROM [dbo].STAGING_Jobs;



/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY 
       [SymbolID]
      ,[StockExchange]
      ,[CompatibilityStartDate]
	  ,[EndDate]
	   ORDER BY 	   
       [SymbolID]
      ,[StockExchange]
      ,[CompatibilityStartDate]
	  ,[EndDate] DESC) AS rn
  FROM [dbo].StageJobs
)
DELETE FROM DistinctSet
WHERE rn > 1 


/*Which symbols do not exist in the tblTrades?*/
INSERT INTO [dbo].[Jobs]
           ([ID]
		  ,[JobTicket]
		  ,[JobId]
		  ,[SymbolID]
		  ,[StockExchange]
		  ,[CompatibilityStartDate]
		  ,[EndDate]
		  ,[JobTicketCreationDate]
		  ,[ProcessType])
SELECT DISTINCT 
		   [ID]
		  ,[JobTicket]
		  ,[JobId]
		  ,[SymbolID]
		  ,[StockExchange]
		  ,[CompatibilityStartDate]
		  ,[EndDate]
		  ,[JobTicketCreationDate]
		  ,[ProcessType] FROM StageJobs

DROP TABLE StageJobs

BEGIN
	/*Delete any duplicates within the main database table*/
	WITH DistinctSet AS
	(
	  SELECT row_number() OVER(PARTITION BY 
		   [SymbolID]
		  ,[StockExchange]
		  ,[CompatibilityStartDate]
		  ,[EndDate]
		   ORDER BY 
		   [SymbolID]
		  ,[StockExchange]
		  ,[CompatibilityStartDate]
		  ,[EndDate] DESC) AS rn
	  FROM [dbo].Jobs
	)
	DELETE FROM DistinctSet
	WHERE rn > 1 
END

END




GO
/****** Object:  StoredProcedure [dbo].[proc_JobTicketCompleted]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_JobTicketCompleted]
@JobTicket nvarchar(MAX),
@EndTime datetime,
@FileNameOutput nvarchar(100),
@CalculationType nvarchar(20)
AS


UPDATE [dbo].Jobs SET [dbo].Jobs.JobEndTime = @EndTime, [dbo].Jobs.Completed = 1, 
[dbo].Jobs.TotalDuration = datediff(s,[dbo].Jobs.JobStartTime, @EndTime) 
WHERE [dbo].Jobs.JobTicket = @JobTicket

/*SELECT ([dbo].Jobs.JobEndTime - [dbo].Jobs.JobStartTime)
*/

DECLARE @FileNameID nvarchar(MAX)
DECLARE @ID nvarchar(100)


IF NOT EXISTS (SELECT 1 FROM [dbo].FileImports WHERE [dbo].FileImports.[FileName] = @FileNameOutput)
	BEGIN
		SET @FileNameID = NEWID()
		INSERT INTO [dbo].FileImports (ID, [FileName], [DataType], [CalculationType])
		VALUES (@FileNameID, @FileNameOutput, 'Computed', @CalculationType)

		SELECT @ID = [dbo].Jobs.ID FROM [dbo].Jobs WHERE [dbo].Jobs.JobTicket = @JobTicket 

		INSERT INTO [dbo].JobsWithinFile ([FileNameID], JobTicketID) VALUES (@FileNameID, @ID)
	END

ELSE
	BEGIN
		SELECT @FileNameID = [dbo].FileImports.[FileName] FROM [dbo].FileImports WHERE [dbo].FileImports.[FileName] = @FileNameOutput 

		SELECT @ID = [dbo].Jobs.ID FROM [dbo].Jobs WHERE [dbo].Jobs.JobTicket = @JobTicket 

		INSERT INTO [dbo].JobsWithinFile ([FileNameID], JobTicketID)
		VALUES (@FileNameID, @ID)
	END	





GO
/****** Object:  StoredProcedure [dbo].[proc_JobTicketCompletedBulk]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_JobTicketCompletedBulk]
AS

DELETE FROM [dbo].STAGING_JobTicketCompletedUpdate

BULK
INSERT [dbo].STAGING_JobTicketCompletedUpdate
FROM 'C:\Users\Public\Downloads\Output\Test\JobsTicketCompletedOutput\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
MAXERRORS = 99999999
)




SELECT DISTINCT [JobTicket]
      ,[EndDate]
      ,[FileNameOutput]
      ,[ProcessType]
	   INTO StageJobTicketCompletedUpdate
FROM [dbo].STAGING_JobTicketCompletedUpdate;


/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY 
       [JobTicket]
      ,[EndDate]
      ,[FileNameOutput]
      ,[ProcessType]
	   ORDER BY 	   
       [JobTicket]
      ,[EndDate]
      ,[FileNameOutput]
      ,[ProcessType] DESC) AS rn
  FROM [dbo].StageJobTicketCompletedUpdate
)
DELETE FROM DistinctSet
WHERE rn > 1 




BEGIN

	UPDATE [dbo].Jobs SET [dbo].Jobs.JobEndTime = [dbo].StageJobTicketCompletedUpdate.EndDate, [dbo].Jobs.Completed = 1, 
	[dbo].Jobs.TotalDuration = datediff(s,[dbo].Jobs.JobStartTime, [dbo].StageJobTicketCompletedUpdate.EndDate) FROM [dbo].Jobs
	JOIN [dbo].StageJobTicketCompletedUpdate ON [dbo].Jobs.JobTicket = [dbo].StageJobTicketCompletedUpdate.JobTicket


	DECLARE @FileNameID nvarchar(MAX)
	DECLARE @ID nvarchar(100)


	BEGIN
		--SET @FileNameID = NEWID()

		INSERT INTO [dbo].FileImports (ID, [FileName], [DataType], [CalculationType])		
		SELECT NEWID(), [FileNameOutput], 'Computed', [dbo].StageJobTicketCompletedUpdate.ProcessType
		FROM [JobsManager].[dbo].StageJobTicketCompletedUpdate  
		GROUP BY [dbo].StageJobTicketCompletedUpdate.FileNameOutput, [dbo].StageJobTicketCompletedUpdate.ProcessType
		

		INSERT INTO [dbo].JobsWithinFile ([FileNameID], JobTicketID) 
		SELECT [dbo].FileImports.[ID], [dbo].Jobs.ID FROM [dbo].Jobs 
		JOIN [dbo].StageJobTicketCompletedUpdate ON [dbo].Jobs.JobTicket = [dbo].StageJobTicketCompletedUpdate.JobTicket 
		JOIN [dbo].FileImports ON [dbo].FileImports.[FileName] = [dbo].StageJobTicketCompletedUpdate.[FileNameOutput]

		--INSERT INTO [dbo].JobsWithinFile ([FileNameID], JobTicketID) VALUES (@FileNameID, @ID)
	END




	--IF NOT EXISTS (SELECT 1 FROM [dbo].FileImports JOIN [dbo].StageJobTicketCompletedUpdate ON [dbo].FileImports.[FileName] = [dbo].StageJobTicketCompletedUpdate.FileNameOutput
	-- WHERE [dbo].FileImports.[FileName] = [dbo].StageJobTicketCompletedUpdate.FileNameOutput)
	--	BEGIN
	--		SET @FileNameID = NEWID()
	--		INSERT INTO [dbo].FileImports (ID, [FileName], [DataType], [CalculationType])
	--		SELECT @FileNameID,
	--		[dbo].StageJobTicketCompletedUpdate.FileNameOutput,
	--		'Computed',
	--		[dbo].StageJobTicketCompletedUpdate.ProcessType FROM [dbo].StageJobTicketCompletedUpdate


	--		SELECT @ID = [dbo].Jobs.ID FROM [dbo].Jobs 
	--		JOIN [dbo].StageJobTicketCompletedUpdate ON [dbo].Jobs.JobTicket = [dbo].StageJobTicketCompletedUpdate.JobTicket 

	--		INSERT INTO [dbo].JobsWithinFile ([FileNameID], JobTicketID) VALUES (@FileNameID, @ID)
	--	END

	--ELSE
	--	BEGIN
	--		SELECT @FileNameID = [dbo].FileImports.[FileName] FROM [dbo].FileImports 
	--		JOIN [dbo].StageJobTicketCompletedUpdate ON [dbo].FileImports.[FileName] = [dbo].StageJobTicketCompletedUpdate.FileNameOutput 


	--		SELECT @ID = [dbo].Jobs.ID FROM [dbo].Jobs 
	--		JOIN [dbo].StageJobTicketCompletedUpdate ON [dbo].Jobs.JobTicket = [dbo].StageJobTicketCompletedUpdate.JobTicket 

	--		INSERT INTO [dbo].JobsWithinFile ([FileNameID], JobTicketID)
	--		VALUES (@FileNameID, @ID)
	--	END	

END


DROP TABLE StageJobTicketCompletedUpdate

GO
/****** Object:  StoredProcedure [dbo].[proc_LogError]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_LogError]
@Thread varchar(255),
@Level varchar(50),
@Application varchar(255),
@Msg varchar(4000),
@Exception varchar(2000),
@MachineName varchar(50)
AS

BEGIN
	INSERT INTO dbo.ApplicationErrorLogs ([Date], Thread, [Level], Logger, [Message], Exception, MachineName)
	VALUES (GETDATE(), @Thread, @Level, @Application, @Msg, @Exception, @MachineName)
END 


GO
/****** Object:  StoredProcedure [dbo].[proc_PeriodicComp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_PeriodicComp]
AS

DELETE FROM [dbo].STAGING_PeriodicCompatibility

BULK
INSERT [dbo].STAGING_PeriodicCompatibility
FROM 'C:\Users\Public\Downloads\Output\Test\PeriodicComp\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
MAXERRORS = 99999999
)

DECLARE @GUID [nvarchar](MAX)


BEGIN

SELECT NEWID() AS ID
           ,[dbo].STAGING_PeriodicCompatibility.[CriteriaSymbolID]
           ,[dbo].STAGING_PeriodicCompatibility.[Exchange]
           ,[dbo].STAGING_PeriodicCompatibility.[CompatibleSymbolID]
           ,[dbo].STAGING_PeriodicCompatibility.[CompatibleExchange]
           ,[dbo].STAGING_PeriodicCompatibility.[StartDateTime]
           ,[dbo].STAGING_PeriodicCompatibility.[EndDateTime]
           ,[dbo].STAGING_PeriodicCompatibility.[TimeFrame] INTO StagePeriodicCompatibility
FROM [dbo].STAGING_PeriodicCompatibility;



/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY 
	   [CriteriaSymbolID]
      ,[Exchange]
      ,[CompatibleSymbolID]
      ,[CompatibleExchange]
      ,[StartDateTime]
      ,[EndDateTime]
      ,[TimeFrame] 
	   ORDER BY 	   
	   [CriteriaSymbolID]
      ,[Exchange]
      ,[CompatibleSymbolID]
      ,[CompatibleExchange]
      ,[StartDateTime]
      ,[EndDateTime]
      ,[TimeFrame] DESC) AS rn
  FROM [dbo].StagePeriodicCompatibility
)
DELETE FROM DistinctSet
WHERE rn > 1 


/*Ensure that there is only one batch of date with a given enddate no duplicates for when this sp is run several times within the day */
/*Check bulkinsert to confirm if this is correct, this shoyuld also include symbolid to ensure unqiueness*/
/*BEGIN
	DECLARE @TempEndDate datetime
	SELECT TOP 1  @TempEndDate = [DateTime] FROM StageTempForex
	DELETE FROM [dbo].tblTrades WHERE [dbo].tblTrades.[DateTime] = @TempEndDate
END*/


/*Which symbols do not exist in the tblTrades?*/
INSERT INTO [dbo].[PeriodicCompatibilityLookUp]
           ([ID]
           ,[CriteriaSymbolID]
           ,[Exchange]
           ,[CompatibleSymbolID]
           ,[CompatibleExchange]
           ,[StartDateTime]
           ,[EndDateTime]
           ,[TimeFrame])
SELECT DISTINCT 
			[ID]
           ,[CriteriaSymbolID]
           ,[Exchange]
           ,[CompatibleSymbolID]
           ,[CompatibleExchange]
           ,[StartDateTime]
           ,[EndDateTime]
           ,[TimeFrame] FROM StagePeriodicCompatibility

DROP TABLE StagePeriodicCompatibility

BEGIN
	/*Delete any duplicates within the main database table*/
	WITH DistinctSet AS
	(
	  SELECT row_number() OVER(PARTITION BY 
	        [CriteriaSymbolID]
           ,[Exchange]
           ,[CompatibleSymbolID]
           ,[CompatibleExchange]
           ,[StartDateTime]
           ,[EndDateTime]
           ,[TimeFrame] 
		   ORDER BY 
		    [CriteriaSymbolID]
           ,[Exchange]
           ,[CompatibleSymbolID]
           ,[CompatibleExchange]
           ,[StartDateTime]
           ,[EndDateTime]
           ,[TimeFrame] DESC) AS rn
	  FROM [dbo].PeriodicCompatibilityLookUp
	)
	DELETE FROM DistinctSet
	WHERE rn > 1 
END

END



GO
/****** Object:  StoredProcedure [dbo].[proc_RegisterProcessingEngine]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[proc_RegisterProcessingEngine]
@Ticket nvarchar(MAX),
@MachineProcessedOn nvarchar(20),
@FMDA_EngineName nvarchar(20),
@JobStartTime datetime

AS

UPDATE [dbo].Jobs SET [dbo].Jobs.MachineProcessedOn = @MachineProcessedOn, 
[dbo].Jobs.FMDA_EngineName = @FMDA_EngineName,
[dbo].Jobs.JobStartTime = @JobStartTime
WHERE [dbo].Jobs.JobTicket = @Ticket




GO
/****** Object:  StoredProcedure [dbo].[proc_RegisterProcessingEngineBulk]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_RegisterProcessingEngineBulk]
AS

DELETE FROM [dbo].STAGING_RegisterEngine

BULK
INSERT [dbo].STAGING_RegisterEngine
FROM 'C:\Users\Public\Downloads\Output\Test\RegisterProcessingEngineOutput\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
MAXERRORS = 99999999
)



SELECT DISTINCT [JobTicket]
      ,[JobStartTime]
      ,[MachineProcessedOn]
      ,[FMDA_EngineName]
	   INTO StageRegisterEngine
FROM [dbo].STAGING_RegisterEngine;


/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY 
       [JobTicket]
      ,[JobStartTime]
      ,[MachineProcessedOn]
      ,[FMDA_EngineName]
	   ORDER BY 	   
       [JobTicket]
      ,[JobStartTime]
      ,[MachineProcessedOn]
      ,[FMDA_EngineName] DESC) AS rn
  FROM [dbo].StageRegisterEngine
)
DELETE FROM DistinctSet
WHERE rn > 1 




BEGIN

	UPDATE [dbo].Jobs SET [dbo].Jobs.JobStartTime = [dbo].StageRegisterEngine.[JobStartTime],
	[dbo].Jobs.MachineProcessedOn = [dbo].StageRegisterEngine.[MachineProcessedOn],
	[dbo].Jobs.FMDA_EngineName = [dbo].StageRegisterEngine.[FMDA_EngineName]
	FROM [dbo].Jobs
	JOIN [dbo].StageRegisterEngine ON [dbo].Jobs.JobTicket = [dbo].StageRegisterEngine.[JobTicket]

END

DROP TABLE StageRegisterEngine





GO
/****** Object:  StoredProcedure [dbo].[proc_RegisterRawDataFile]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_RegisterRawDataFile]
@FileName nvarchar(100),
@FileTypeNum int
AS

DECLARE @ID nvarchar(100)
DECLARE @DCPrimaryID nvarchar(100)
DECLARE @FileType nvarchar(10)

SET @ID = NewID()
SET @DCPrimaryID = NewID()

	IF (@FileTypeNum = 0)
	BEGIN
		SET @FileType = 'Raw'
	END
	ELSE IF (@FileTypeNum = 1)
	BEGIN
		SET @FileType = 'EventDesc'
	END
	ELSE IF (@FileTypeNum = 2)
	BEGIN
		SET @FileType = 'EventData'
	END
	ELSE 
	BEGIN
		SET @FileType = 'KeyPers'
	END


	DECLARE @Count int;

	BEGIN
		SELECT @Count = COUNT(*) FROM [dbo].FileImports FF
			 WHERE FF.[FileName] = @FileName
		 	 
			IF @Count = 0
			BEGIN
				INSERT INTO [dbo].FileImports (ID, [FileName], ImportCompleted, DataType, [CalculationType])
				VALUES (@ID, @FileName, '0', @FileType, 'None')

				INSERT INTO [dbo].DataConsumer (ID, [FileImportID], [DownloadCompleted])
				VALUES (@DCPrimaryID, @ID,'0')
			END
	END











GO
/****** Object:  StoredProcedure [dbo].[proc_StartLogIndicatorPerformanceStats]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[proc_StartLogIndicatorPerformanceStats]
AS

DECLARE @startDateTime smalldatetime

SET @startDateTime = GETDATE()

BEGIN
	INSERT INTO [JobsManager].[dbo].PerformanceStatistics_BatchRunLog ([Type], [StartDateTime], [EndDateTime])
	VALUES ('Indicator', @startDateTime, NULL)

	SELECT MAX(BatchRunID) AS 'BatchRunID' FROM [JobsManager].[dbo].PerformanceStatistics_BatchRunLog 
	WHERE [JobsManager].[dbo].PerformanceStatistics_BatchRunLog.[Type] = 'Indicator'
END



GO
/****** Object:  StoredProcedure [dbo].[proc_ViewComputedFileImports]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE  [dbo].[proc_ViewComputedFileImports]
@ImportStartTime date,
@ImportEndTime date
AS
SELECT [dbo].[FileImports].[ID]
      ,[dbo].[ImportLog].[FileImportID]
      ,[ErrorMsg]
      ,[CSVRecordCount]
      ,[ImportedRecordCount]
      ,[PercentageSuccess]
      ,[PercentageFailure]
      ,[ImportStartTime]
      ,[ImportEndTime]
	  ,[FileName]
      ,[ImportCompleted]
      ,[DataType]
	  ,[CalculationType]
  FROM [dbo].[ImportLog]
  JOIN [dbo].[FileImports] ON [dbo].[FileImports].ID = [dbo].[ImportLog].FileImportID
  WHERE [dbo].[ImportLog].ImportStartTime BETWEEN  @ImportStartTime AND @ImportEndTime
  AND [dbo].[FileImports].[DataType] = 'Computed'
  ORDER BY [JobsManager].[dbo].[ImportLog].ImportStartTime DESC


  --WHERE [dbo].[ImportLog].ImportStartTime > @ImportStartTime 





GO
/****** Object:  StoredProcedure [dbo].[proc_ViewFileImports]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE  [dbo].[proc_ViewFileImports]
@ImportStartTime date,
@ImportEndTime date
AS
SELECT [dbo].[FileImports].[ID]
      ,[dbo].[ImportLog].[FileImportID]
      ,[ErrorMsg]
      ,[CSVRecordCount]
      ,[ImportedRecordCount]
      ,[PercentageSuccess]
      ,[PercentageFailure]
      ,[ImportStartTime]
      ,[ImportEndTime]
	  ,[FileName]
      ,[ImportCompleted]
      ,[DataType]
	  ,[DownloadCompleted]
  FROM [dbo].[ImportLog]
  JOIN [dbo].[FileImports] ON [dbo].[FileImports].ID = [dbo].[ImportLog].FileImportID
  JOIN [dbo].[DataConsumer] ON [dbo].[FileImports].ID = [dbo].[DataConsumer].FileImportID
  WHERE [dbo].[ImportLog].ImportStartTime BETWEEN  @ImportStartTime AND @ImportEndTime
  ORDER BY [JobsManager].[dbo].[ImportLog].ImportStartTime DESC


  --WHERE [dbo].[ImportLog].ImportStartTime > @ImportStartTime 



GO
/****** Object:  Table [dbo].[ApplicationErrorLogs]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ApplicationErrorLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[Thread] [varchar](255) NOT NULL,
	[Level] [varchar](50) NOT NULL,
	[Logger] [varchar](255) NOT NULL,
	[Message] [varchar](4000) NOT NULL,
	[Exception] [varchar](2000) NULL,
	[MachineName] [varchar](50) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ChartPatternTemplates]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ChartPatternTemplates](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SymbolID] [nvarchar](20) NOT NULL,
	[TimeFrame] [nvarchar](10) NOT NULL,
	[Pattern] [nvarchar](50) NOT NULL,
	[StartDateTime] [smalldatetime] NOT NULL,
	[EndDateTime] [smalldatetime] NOT NULL,
	[Exchange] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_tblChartPatternTemplate] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DataConsumer]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataConsumer](
	[ID] [nvarchar](100) NOT NULL,
	[FileImportID] [nvarchar](100) NOT NULL,
	[DownloadCompleted] [bit] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ExchangeLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExchangeLookUp](
	[ID] [nvarchar](100) NOT NULL,
	[SymbolID] [nvarchar](20) NOT NULL,
	[Exchange] [nvarchar](20) NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[FileImports]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FileImports](
	[ID] [nvarchar](100) NOT NULL,
	[FileName] [nvarchar](100) NULL,
	[ImportCompleted] [bit] NOT NULL,
	[DataType] [nvarchar](20) NULL,
	[CalculationType] [nvarchar](20) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[HistoricallyCompatibleStocks]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HistoricallyCompatibleStocks](
	[ID] [nvarchar](100) NOT NULL,
	[CriteriaSymbolID] [nvarchar](20) NULL,
	[StockExchange] [nchar](20) NULL,
	[CompatibleSymbolID] [nvarchar](20) NULL,
	[CompatibleStockExchange] [nvarchar](20) NULL,
	[Date] [date] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ImportLog]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ImportLog](
	[ID] [nvarchar](100) NOT NULL,
	[FileImportID] [nvarchar](100) NOT NULL,
	[ErrorMsg] [nvarchar](max) NULL,
	[CSVRecordCount] [int] NULL,
	[ImportedRecordCount] [int] NULL,
	[PercentageSuccess] [float] NULL,
	[PercentageFailure] [float] NULL,
	[ImportStartTime] [datetime] NULL,
	[ImportEndTime] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Jobs]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jobs](
	[ID] [nvarchar](100) NOT NULL,
	[JobTicket] [nvarchar](max) NULL,
	[JobId] [nvarchar](200) NULL,
	[SymbolID] [nvarchar](20) NULL,
	[StockExchange] [nvarchar](20) NULL,
	[CompatibilityStartDate] [smalldatetime] NULL,
	[EndDate] [smalldatetime] NULL,
	[JobTicketCreationDate] [date] NULL,
	[ProcessType] [nvarchar](20) NULL,
	[JobStartTime] [datetime] NULL,
	[JobEndTime] [datetime] NULL,
	[TotalDuration] [decimal](18, 4) NULL,
	[MachineProcessedOn] [nvarchar](20) NULL,
	[FMDA_EngineName] [nvarchar](20) NULL,
	[Completed] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[JobsWithinFile]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobsWithinFile](
	[FileNameID] [nvarchar](100) NULL,
	[JobTicketID] [nvarchar](100) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[NonCompatibleDates_Diagnostics]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NonCompatibleDates_Diagnostics](
	[RefID] [nvarchar](100) NOT NULL,
	[DateTime] [smalldatetime] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[NonCompatibleInstruments]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NonCompatibleInstruments](
	[ID] [nvarchar](100) NOT NULL,
	[SymbolID] [nvarchar](20) NULL,
	[Exchange] [nchar](20) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[NonCompatibleStocks_Diagnostics]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NonCompatibleStocks_Diagnostics](
	[ID] [nvarchar](100) NOT NULL,
	[CriteriaSymbolID] [nvarchar](20) NULL,
	[StockExchange] [nchar](20) NULL,
	[NonMatchingSymbolID] [nvarchar](20) NULL,
	[NonMatchingStockExchange] [nchar](20) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[NonCompatibleSymbols]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NonCompatibleSymbols](
	[ID] [nvarchar](100) NOT NULL,
	[CriteriaSymbolID] [nvarchar](20) NULL,
	[Exchange] [nchar](20) NULL,
	[NonMatchingSymbolID] [nvarchar](20) NULL,
	[NonMatchingStockExchange] [nvarchar](20) NULL,
	[TimeFrame] [nvarchar](10) NULL,
	[DateTime] [smalldatetime] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Notify]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notify](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Exchange] [nvarchar](50) NULL,
	[Notify] [bit] NOT NULL,
 CONSTRAINT [PK_Notify] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PerformanceStatistics_BatchRunLog]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PerformanceStatistics_BatchRunLog](
	[BatchRunID] [int] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](25) NULL,
	[StartDateTime] [smalldatetime] NULL,
	[EndDateTime] [smalldatetime] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PerformanceStatistics_ChartPatternLog]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PerformanceStatistics_ChartPatternLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchRunLogID] [int] NOT NULL,
	[SymbolID] [nvarchar](20) NULL,
	[Pattern] [nvarchar](50) NULL,
	[TimeFrame] [nvarchar](10) NOT NULL,
	[Total] [int] NULL,
	[CorrectRecognition] [int] NULL,
	[Percentage] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PerformanceStatistics_IndicatorLog]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PerformanceStatistics_IndicatorLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BatchRunLogID] [int] NULL,
	[SymbolID] [nvarchar](20) NULL,
	[TimeFrame] [nvarchar](10) NULL,
	[Indicator] [nvarchar](50) NULL,
	[Categorization] [nvarchar](20) NULL,
	[Scenario] [nvarchar](10) NULL,
	[Returns] [int] NULL,
	[Average Return] [decimal](18, 7) NULL,
	[Median Return] [decimal](18, 7) NULL,
	[Percent Positive] [decimal](18, 2) NULL,
	[Percent Negative] [decimal](18, 2) NULL,
	[Description] [nvarchar](250) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PeriodicCompatibilityLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PeriodicCompatibilityLookUp](
	[ID] [nvarchar](100) NOT NULL,
	[CriteriaSymbolID] [nvarchar](20) NULL,
	[Exchange] [nchar](20) NULL,
	[CompatibleSymbolID] [nvarchar](20) NULL,
	[CompatibleExchange] [nvarchar](20) NULL,
	[StartDateTime] [smalldatetime] NULL,
	[EndDateTime] [smalldatetime] NULL,
	[TimeFrame] [nvarchar](10) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[StageRegisterEngine]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StageRegisterEngine](
	[JobTicket] [nvarchar](max) NULL,
	[JobStartTime] [datetime] NULL,
	[MachineProcessedOn] [nvarchar](20) NULL,
	[FMDA_EngineName] [nvarchar](20) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[STAGING_Jobs]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STAGING_Jobs](
	[JobTicket] [nvarchar](max) NULL,
	[JobId] [nvarchar](200) NULL,
	[SymbolID] [nvarchar](20) NULL,
	[StockExchange] [nvarchar](20) NULL,
	[CompatibilityStartDate] [smalldatetime] NULL,
	[EndDate] [smalldatetime] NULL,
	[JobTicketCreationDate] [date] NULL,
	[ProcessType] [nvarchar](20) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[STAGING_JobTicketCompletedUpdate]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STAGING_JobTicketCompletedUpdate](
	[JobTicket] [nvarchar](max) NULL,
	[EndDate] [smalldatetime] NULL,
	[FileNameOutput] [nvarchar](100) NULL,
	[ProcessType] [nvarchar](20) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[STAGING_PeriodicCompatibility]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STAGING_PeriodicCompatibility](
	[CriteriaSymbolID] [nvarchar](20) NULL,
	[Exchange] [nchar](20) NULL,
	[CompatibleSymbolID] [nvarchar](20) NULL,
	[CompatibleExchange] [nvarchar](20) NULL,
	[StartDateTime] [smalldatetime] NULL,
	[EndDateTime] [smalldatetime] NULL,
	[TimeFrame] [nvarchar](10) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[STAGING_RegisterEngine]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[STAGING_RegisterEngine](
	[JobTicket] [nvarchar](max) NULL,
	[JobStartTime] [datetime] NULL,
	[MachineProcessedOn] [nvarchar](20) NULL,
	[FMDA_EngineName] [nvarchar](20) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[SymbolNameConversionLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SymbolNameConversionLookUp](
	[ID] [nvarchar](100) NOT NULL,
	[SymbolID] [nvarchar](20) NOT NULL,
	[SymbolIDFriendly] [nvarchar](20) NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblSTAGING_ExchangeLookUp]    Script Date: 14/09/2016 19:55:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSTAGING_ExchangeLookUp](
	[SymbolID] [nvarchar](20) NOT NULL,
	[Exchange] [nvarchar](20) NOT NULL
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[FileImports] ADD  CONSTRAINT [DF_FileImports_ImportCompleted]  DEFAULT ((0)) FOR [ImportCompleted]
GO
ALTER TABLE [dbo].[Jobs] ADD  CONSTRAINT [DF_Jobs_Completed]  DEFAULT ((0)) FOR [Completed]
GO
