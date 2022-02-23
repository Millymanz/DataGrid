USE [master]
GO

/****** Object:  Database [FundamentalMarketData]    Script Date: 14/10/2016 12:01:45 ******/
CREATE DATABASE [FundamentalMarketData]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'FundamentalMarketData', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQL2012\MSSQL\DATA\FundamentalMarketData.mdf' , SIZE = 594944KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'FundamentalMarketData_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.SQL2012\MSSQL\DATA\FundamentalMarketData_log.ldf' , SIZE = 1475904KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

ALTER DATABASE [FundamentalMarketData] SET COMPATIBILITY_LEVEL = 110
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [FundamentalMarketData].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [FundamentalMarketData] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET ARITHABORT OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET AUTO_CREATE_STATISTICS ON 
GO

ALTER DATABASE [FundamentalMarketData] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [FundamentalMarketData] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [FundamentalMarketData] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET  DISABLE_BROKER 
GO

ALTER DATABASE [FundamentalMarketData] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [FundamentalMarketData] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET RECOVERY SIMPLE 
GO

ALTER DATABASE [FundamentalMarketData] SET  MULTI_USER 
GO

ALTER DATABASE [FundamentalMarketData] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [FundamentalMarketData] SET DB_CHAINING OFF 
GO

ALTER DATABASE [FundamentalMarketData] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [FundamentalMarketData] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO

ALTER DATABASE [FundamentalMarketData] SET  READ_WRITE 
GO












USE [FundamentalMarketData]
GO
/****** Object:  StoredProcedure [dbo].[proc_Admin_SearchAndViewKeywords_EventOrInstitutionOrHeadline]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROC [dbo].[proc_Admin_SearchAndViewKeywords_EventOrInstitutionOrHeadline]
@keywordvar nvarchar(50)
AS

BEGIN
	 SELECT EventHeadline
		  ,[Category]
		  ,[InstitutionBody]
		  ,[EventType]
		  ,[EventDescription]
		  ,[ReleaseDateTime]
		  ,AA.[Actual]
		  ,AA.[Forecast]
		  ,AA.[Previous]
		  ,AA.[Completed]
		  ,AA.[NumericType]
		  ,AA.ID AS 'EconomicValueID'
	  FROM [dbo].[tblEconomicEvents] DD
	  JOIN [dbo].[tblEconomicValues] AA ON DD.ID = AA.EventID	
	  WHERE DD.[EventType] like '%'+@keywordvar+'%' Or DD.[InstitutionBody] like '%'+@keywordvar+'%'
	  Or DD.[EventHeadline] like '%'+@keywordvar+'%'
	  ORDER BY [ReleaseDateTime] DESC
END

GO
/****** Object:  StoredProcedure [dbo].[proc_AdminCheckForDuplicates]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[proc_AdminCheckForDuplicates]
AS

BEGIN
	SELECT EventHeadline, EventType, Country, ReleaseDateTime, COUNT(EventType) FROM [dbo].tblEconomicEvents
	GROUP BY EventHeadline, EventType, Country, ReleaseDateTime
	HAVING COUNT(EventType) > 1
END







GO
/****** Object:  StoredProcedure [dbo].[proc_BulkInsertEconomicEvents]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO









/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_BulkInsertEconomicEvents]
@PATH [nvarchar](MAX),
@ROWSAFFECTED int output
AS

PRINT 'DELETE FROM [dbo].tblSTAGING_EconomicEvents';
DELETE FROM [dbo].tblSTAGING_EconomicEvents

PRINT 'DROP TABLE StageEconomicEvents';
IF OBJECT_ID('StageEconomicEvents') IS NOT NULL
BEGIN
	DROP TABLE StageEconomicEvents
END

PRINT 'Bulk Import'
DECLARE @q nvarchar(MAX);
SET @q=
    'BULK INSERT [dbo].tblSTAGING_EconomicEvents
    FROM '+char(39)+ @PATH +char(39)+'
    WITH
    (
    FIELDTERMINATOR = '','',
    ROWTERMINATOR = ''\n'', 
    MAXERRORS = 99999999  
    )'
EXEC(@q)
--Hex equivalent of \n   0x0a




/*

PRINT 'Bulk Import'
BULK
INSERT [dbo].tblSTAGING_EconomicEvents
FROM 'C:\Users\Public\Downloads\Output\Test\RawData\Staging\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
--ERRORFILE = 'C:\Users\Public\Downloads\Output\Test\RawData\Staging\myRubbishData.log', 
MAXERRORS = 99999999
)
*/

DECLARE @GUID [nvarchar](MAX)

SET @ROWSAFFECTED = 0


----------------------------------------------------------------------------------------

BEGIN

PRINT 'SELECT INTO tblSTAGING_EconomicEvents';
SELECT NEWID() AS ID,
[dbo].tblSTAGING_EconomicEvents.[EventHeadline],
[dbo].tblSTAGING_EconomicEvents.[Category],
[dbo].tblSTAGING_EconomicEvents.[EventType],
[dbo].tblSTAGING_EconomicEvents.[ReleaseDateTime],
[dbo].tblSTAGING_EconomicEvents.[InstitutionBody],
[dbo].tblSTAGING_EconomicEvents.[Country],
[dbo].tblSTAGING_EconomicEvents.[Currency],
[dbo].tblSTAGING_EconomicEvents.[EventDescription],
[dbo].tblSTAGING_EconomicEvents.[Importance],
[dbo].tblSTAGING_EconomicEvents.[JsonSrc] INTO StageEconomicEvents
FROM [dbo].tblSTAGING_EconomicEvents;



PRINT 'Ensuring data set (grouped data) is DISITINCT';

/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY [EventHeadline], [ReleaseDateTime] ORDER BY [EventHeadline], [ReleaseDateTime] DESC) AS rn
  FROM [dbo].StageEconomicEvents
)
DELETE FROM DistinctSet
WHERE rn > 1 


PRINT 'Event Entries New';
SELECT JJ.ID,
JJ.[EventHeadline],
JJ.[Category],
JJ.[EventType],
JJ.[ReleaseDateTime],
JJ.[InstitutionBody],
JJ.[Country],
JJ.[Currency],
JJ.[EventDescription],
JJ.[Importance],
JJ.[JsonSrc] INTO NewEnteriesEventsOnly
FROM [dbo].tblEconomicEvents DD
RIGHT JOIN [dbo].StageEconomicEvents JJ ON DD.[EventHeadline] = JJ.[EventHeadline] 
AND DD.ReleaseDateTime = JJ.ReleaseDateTime
WHERE DD.ReleaseDateTime IS NULL




--PRINT 'DELETE If exist date item is in database...?';
--/*Ensure that there is only one batch of date with a given enddate no duplicates for when this sp is run several times within the day */
--BEGIN
--	DECLARE @TempEndDate datetime
--	SELECT TOP 1  @TempEndDate = [ReleaseDateTime] FROM StageEconomicEvents
--	DELETE FROM [dbo].tblEconomicEvents WHERE [dbo].tblEconomicEvents.[ReleaseDateTime] = @TempEndDate
--END


PRINT 'INSERT INTO [dbo].tblEconomicEvents';
INSERT INTO [dbo].tblEconomicEvents (
	   [ID]
      ,[EventHeadline]
      ,[Category]
      ,[EventType]
      ,[ReleaseDateTime]
      ,[InstitutionBody]
      ,[Country]
      ,[Currency]
      ,[EventDescription]
      ,[Importance]
      ,[JsonSrc])
SELECT DISTINCT 
	   NEWID() AS ID
      ,[EventHeadline]
      ,[Category]
      ,[EventType]
      ,[ReleaseDateTime]
      ,[InstitutionBody]
      ,[Country]
      ,[Currency]
      ,[EventDescription]
      ,[Importance]      
      ,[JsonSrc] FROM NewEnteriesEventsOnly

SELECT @ROWSAFFECTED = COUNT(*) FROM NewEnteriesEventsOnly


PRINT 'DROP TABLE StageEconomicEvents';
DROP TABLE StageEconomicEvents
DROP TABLE NewEnteriesEventsOnly

PRINT 'Delete any duplicates in database';
BEGIN
	/*Delete any duplicates within the main database table*/
	WITH DistinctSet AS
	(
	  SELECT row_number() OVER(PARTITION BY [EventHeadline], [ReleaseDateTime] ORDER BY [EventHeadline], [ReleaseDateTime] DESC) AS rn
	  FROM [dbo].tblEconomicEvents
	)
	DELETE FROM DistinctSet
	WHERE rn > 1 
END

--PRINT 'Delete tblEconomicEventsWorkingVersion';
--DELETE FROM dbo.tblEconomicEventsWorkingVersion

--PRINT 'INSERT tblEconomicEventsWorkingVersion';
--INSERT INTO dbo.tblEconomicEventsWorkingVersion
--([ID]
--,[EventHeadline]
--,[Category]
--,[EventType]
--,[ReleaseDateTime]
--,[InstitutionBody]
--,[Country]
--,[Currency]
--,[EventDescription]
--,[Importance]
--,[JsonSrc])
--SELECT * FROM dbo.tblEconomicEvents


END






GO
/****** Object:  StoredProcedure [dbo].[proc_BulkInsertEconomicValues]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_BulkInsertEconomicValues]
@PATH [nvarchar](MAX),
@ROWSAFFECTED int output
AS


DELETE FROM [dbo].tblSTAGING_EconomicValues
PRINT 'DELETE tblSTAGING_EconomicValues';

IF OBJECT_ID('StageEconomicValues') IS NOT NULL
BEGIN
	DROP TABLE StageEconomicValues
END
PRINT 'DROP TABLE StageEconomicValues';



PRINT 'Bulk Import'
DECLARE @q nvarchar(MAX);
SET @q=
    'BULK INSERT [dbo].tblSTAGING_EconomicValues
    FROM '+char(39)+ @PATH +char(39)+'
    WITH
    (
    FIELDTERMINATOR = '','',
    ROWTERMINATOR = ''\n'', 
    MAXERRORS = 99999999  
    )'
EXEC(@q)
--Hex equivalent of \n   0x0a




/*
BULK
INSERT [dbo].tblSTAGING_EconomicValues
FROM 'C:\Users\Public\Downloads\Output\Test\RawData\Staging\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
MAXERRORS = 99999999
)
*/


PRINT 'BULK INSERT tblSTAGING_EconomicValues';

DECLARE @GUID [nvarchar](MAX)

SET @ROWSAFFECTED = 0



----------------------------------------------------------------------------------------

BEGIN

SELECT 
NEWID() AS ID
,DD.ID AS EventID
,JJ.[Actual]
,JJ.[Forecast]
,JJ.[Previous]
--,JJ.[Completed]
,JJ.[NumericType]
INTO StageEconomicValues
FROM [dbo].tblEconomicEvents DD
JOIN [dbo].tblSTAGING_EconomicValues JJ ON DD.EventHeadline = JJ.EventHeadline
AND DD.ReleaseDateTime = JJ.ReleaseDateTime;
PRINT 'SELECT INTO StageEconomicValues, joining on tblEconomicEvents';


/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY EventID, [Actual], [Forecast], [Previous] ORDER BY EventID, [Actual], [Forecast], [Previous] DESC) AS rn
  FROM [dbo].StageEconomicValues
)
DELETE FROM DistinctSet
WHERE rn > 1 
PRINT 'Ensuring data set (grouped data) is DISITINCT';


--SELECT DD.ID
--	,DD.EventID
--	,DD.[Actual]
--	,DD.[Forecast]
--	,DD.[Previous]
--	,DD.[NumericType] INTO NewEnteriesOnly
--FROM [dbo].StageEconomicValues DD
--LEFT JOIN [dbo].tblEconomicValues JJ ON DD.EventID = JJ.EventID
--WHERE JJ.EventID IS NULL
--PRINT 'New enteries only';


SELECT JJ.ID
	,JJ.EventID
	,JJ.[Actual]
	,JJ.[Forecast]
	,JJ.[Previous]
	,JJ.[NumericType] INTO NewEnteriesOnly
FROM [dbo].tblEconomicValues DD
RIGHT JOIN [dbo].StageEconomicValues JJ ON DD.EventID = JJ.EventID
WHERE DD.EventID IS NULL





INSERT INTO [dbo].tblEconomicValues (
	 ID
	,EventID
	,[Actual]
	,[Forecast]
	,[Previous]
	,[Completed]
	,[NumericType])
	SELECT DISTINCT 
	 ID
	,EventID
	,[Actual]
	,[Forecast]
	,[Previous]
	,0
	--,[Completed]
	,[NumericType]
	FROM NewEnteriesOnly
	--FROM StageEconomicValues
PRINT 'INSERT INTO [dbo].tblEconomicValues';


BEGIN
	/*Delete any duplicates within the main database table*/
	WITH DistinctSet AS
	(
	 SELECT row_number() OVER(PARTITION BY EventID, [Actual], [Forecast], [Previous] ORDER BY EventID, [Actual], [Forecast], [Previous] DESC) AS rn
	  FROM [dbo].tblEconomicValues
	)
	DELETE FROM DistinctSet
	WHERE rn > 1 
END
PRINT 'Delete any duplicates within the main database table';

DECLARE @tempC int
SELECT @tempC = COUNT(*) FROM [dbo].tblEconomicValues
PRINT @tempC;
PRINT 'Counting tblEconomicValues';

UPDATE [dbo].tblEconomicValues SET [dbo].tblEconomicValues.Actual = StageEconomicValues.Actual,
[dbo].tblEconomicValues.Forecast = StageEconomicValues.Forecast,
[dbo].tblEconomicValues.Previous = StageEconomicValues.Previous,
[dbo].tblEconomicValues.[NumericType] = StageEconomicValues.[NumericType] 
FROM [dbo].tblEconomicValues 
INNER JOIN StageEconomicValues ON [dbo].tblEconomicValues.EventID = StageEconomicValues.EventID
PRINT 'UPDATE [dbo].tblEconomicValues';


DECLARE @tempB int
SELECT @tempB = COUNT(*) FROM [dbo].tblEconomicValues
PRINT @tempB;
PRINT 'Counting tblEconomicValues';

SELECT @ROWSAFFECTED = COUNT(*) FROM NewEnteriesOnly

DROP TABLE StageEconomicValues
DROP TABLE NewEnteriesOnly

PRINT 'DROP TABLE StageEconomicValues';




--DELETE FROM dbo.tblEconomicValuesWorkingVersion
--PRINT 'DELETE FROM dbo.tblEconomicValuesWorkingVersion';

--INSERT INTO dbo.tblEconomicValuesWorkingVersion
--(ID
--,EventID
--,[Actual]
--,[Forecast]
--,[Previous]
--,[Completed]
--,[NumericType]
--)
--SELECT * FROM dbo.tblEconomicValues
--PRINT 'INSERT INTO dbo.tblEconomicValuesWorkingVersion';

END








GO
/****** Object:  StoredProcedure [dbo].[proc_BulkInsertEventKeyPersonnel]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_BulkInsertEventKeyPersonnel]
@PATH [nvarchar](MAX),
@ROWSAFFECTED int output
AS

DELETE FROM [dbo].tblSTAGING_EventKeyPersonnel

IF OBJECT_ID('StageEventKeyPersonnel') IS NOT NULL
BEGIN
	DROP TABLE StageEventKeyPersonnel
END

PRINT 'Bulk Import'
DECLARE @q nvarchar(MAX);
SET @q=
    'BULK INSERT [dbo].tblSTAGING_EventKeyPersonnel
    FROM '+char(39)+ @PATH +char(39)+'
    WITH
    (
    FIELDTERMINATOR = '','',
    ROWTERMINATOR = ''\n'', 
    MAXERRORS = 99999999  
    )'
EXEC(@q)


/*
BULK
INSERT [dbo].tblSTAGING_EventKeyPersonnel
FROM 'C:\Users\Public\Downloads\Output\Test\RawData\Staging\Temp.csv'
WITH
(
FIELDTERMINATOR = ',',
ROWTERMINATOR = '\n', --Hex equivalent of \n   0x0a
MAXERRORS = 99999999
)
*/
DECLARE @GUID [nvarchar](MAX)

SET @ROWSAFFECTED = 0

BEGIN

SELECT 
NEWID() AS ID,
DD.ID AS EventID,
JJ.FirstName,
JJ.LastName
INTO StageEventKeyPersonnel
FROM [dbo].tblEconomicEvents DD
JOIN [dbo].tblSTAGING_EventKeyPersonnel JJ ON DD.EventHeadline = JJ.EventHeadline
AND DD.ReleaseDateTime = JJ.ReleaseDateTime;




--SELECT NEWID() AS ID,
--[dbo].tblSTAGING_EventKeyPersonnel.[EventID],
--[dbo].tblSTAGING_EventKeyPersonnel.[FirstName],
--[dbo].tblSTAGING_EventKeyPersonnel.[LastName] INTO StageEventKeyPersonnel
--FROM [dbo].tblSTAGING_EventKeyPersonnel;


/*Ensuring data set (grouped data) is DISITINCT */
WITH DistinctSet AS
(
  SELECT row_number() OVER(PARTITION BY FirstName, LastName ORDER BY FirstName, LastName DESC) AS rn
  FROM [dbo].StageEventKeyPersonnel
)
DELETE FROM DistinctSet
WHERE rn > 1 



SELECT JJ.ID
	,JJ.EventID
	,JJ.FirstName
	,JJ.LastName INTO NewEnteriesOnly
FROM [dbo].tblEventKeyPersonnel DD
RIGHT JOIN [dbo].StageEventKeyPersonnel JJ ON DD.EventID = JJ.EventID
WHERE DD.EventID IS NULL



INSERT INTO [dbo].tblEventKeyPersonnel (
	   	[ID],
		[EventID],
		[FirstName],
		[LastName])
	SELECT DISTINCT 
		[ID],
		[EventID],
		[FirstName],
		[LastName] 
		FROM NewEnteriesOnly

BEGIN
	/*Delete any duplicates within the main database table*/
	WITH DistinctSet AS
	(
	  SELECT row_number() OVER(PARTITION BY [EventID], [FirstName], [LastName] ORDER BY [EventID], [FirstName], [LastName] DESC) AS rn
	  FROM [dbo].tblEventKeyPersonnel
	)
	DELETE FROM DistinctSet
	WHERE rn > 1 
END

SELECT @ROWSAFFECTED = COUNT(*) FROM NewEnteriesOnly

DROP TABLE StageEventKeyPersonnel
DROP TABLE NewEnteriesOnly

--DELETE FROM dbo.[tblEventKeyPersonnelWorkingVersion]

--INSERT INTO dbo.[tblEventKeyPersonnelWorkingVersion]
--([ID]
--,[EventID]
--,[FirstName]
--,[LastName])
--SELECT * FROM dbo.tblEventKeyPersonnel


END







GO
/****** Object:  StoredProcedure [dbo].[proc_GetEconomicFundamentals]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_GetEconomicFundamentals]
@StartDateTime smallDateTime,
@Category nvarchar(70)
AS

BEGIN
	--SELECT * FROM [FundamentalMarketData].[dbo].[tblEconomicEvents] DD
	--JOIN [FundamentalMarketData].[dbo].[tblEconomicValues] AA ON DD.ID = AA.EventID	
	--WHERE DD.[ReleaseDateTime] > @StartDateTime AND DD.Category = @Category
	--ORDER BY DD.Country, DD.[ReleaseDateTime] ASC

	SELECT 
	DISTINCT DD.EventDescription, 
	DD.Category,DD.Country, DD.Currency, DD.EventHeadline, DD.EventType, DD.Importance, 
	DD.InstitutionBody, DD.ReleaseDateTime, AA.Actual, AA.Forecast, AA.Previous, AA.NumericType 
	FROM [FundamentalMarketData].[dbo].[tblEconomicEvents] DD
	JOIN [FundamentalMarketData].[dbo].[tblEconomicValues] AA ON DD.ID = AA.EventID	
	WHERE DD.[ReleaseDateTime] > @StartDateTime AND DD.Category = @Category
	ORDER BY DD.Country, DD.[ReleaseDateTime] ASC 
END





GO
/****** Object:  StoredProcedure [dbo].[proc_GetEconomicFundamentalsByEventHeadLine]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_GetEconomicFundamentalsByEventHeadLine]
@EventHeadline nvarchar(100)
AS
BEGIN
		SELECT 
		  [EventHeadline]
		  ,[Category]
		  ,[EventType]
		  ,[ReleaseDateTime]
		  ,[InstitutionBody]
		  ,[Country]
		  ,[Currency]
		  ,[EventDescription]
		  ,[Importance]
			,AA.[ID]
		  ,[EventID]
		  ,[Actual]
		  ,[Forecast]
		  ,[Previous]
		  ,[Completed]
		  ,[NumericType]
	  FROM [FundamentalMarketData].[dbo].[tblEconomicValues] DD
	  JOIN [FundamentalMarketData].[dbo].tblEconomicEvents AA ON DD.EventID = AA.ID 
	  WHERE AA.EventHeadline = @EventHeadline ORDER BY AA.ReleaseDateTime DESC
	 
END






GO
/****** Object:  StoredProcedure [dbo].[proc_GetEconomicFundamentalsCount]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_GetEconomicFundamentalsCount]
@StartDateTime smallDateTime,
@Category nvarchar(70)
AS

BEGIN
	SELECT COUNT(*) AS 'DataCount' FROM [FundamentalMarketData].[dbo].[tblEconomicEvents] DD
	JOIN [FundamentalMarketData].[dbo].[tblEconomicValues] AA ON DD.ID = AA.EventID 
	WHERE DD.[ReleaseDateTime] > @StartDateTime AND DD.Category = @Category
END





GO
/****** Object:  StoredProcedure [dbo].[proc_GetEconomicFundamentalsEssentials]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_GetEconomicFundamentalsEssentials]
AS

BEGIN
	 SELECT DISTINCT 
	  [EventHeadline],
	  [EventType],
	  [Category],
	  [Country],
	  [Currency],
	  [InstitutionBody],
	  [EventDescription]
	  FROM [FundamentalMarketData].[dbo].[tblEconomicEvents]
	 
END






GO
/****** Object:  StoredProcedure [dbo].[proc_GetFutureEconomicFundamentals]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_GetFutureEconomicFundamentals]
@StartDateTime smallDateTime
AS

BEGIN
	SELECT
	DD.[EventHeadline]
	,DD.[Category]
	,DD.[EventType]
	,DD.[ReleaseDateTime]
	,DD.[InstitutionBody]
	,DD.[Country]
	,DD.[Currency]
	,DD.[EventDescription]
	,DD.[Importance]
	,DD.[JsonSrc],
	AA.[Actual],
	AA.[Forecast],
	AA.[Previous],
	AA.[Completed],
	AA.[NumericType]
	FROM [FundamentalMarketData].[dbo].[tblEconomicEvents] DD
	JOIN [FundamentalMarketData].[dbo].[tblEconomicValues] AA ON DD.ID = AA.EventID 
	WHERE AA.[Forecast] != 'null' AND AA.[Actual] = 'null' AND DD.[ReleaseDateTime] > @StartDateTime
	ORDER BY DD.Country, DD.[ReleaseDateTime] ASC
	 
END






GO
/****** Object:  StoredProcedure [dbo].[proc_GetHeadLinesOnly]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











/*************************************************************************/

CREATE PROCEDURE [dbo].[proc_GetHeadLinesOnly]
@Category nvarchar(100)
AS

BEGIN
	SELECT DISTINCT EventHeadline FROM [FundamentalMarketData].[dbo].[tblEconomicEvents] DD
	WHERE DD.Category = @Category
END






GO
/****** Object:  Table [dbo].[StageNotInEconomicEventsTable]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StageNotInEconomicEventsTable](
	[EventHeadline] [nvarchar](300) NULL,
	[Category] [nvarchar](50) NULL,
	[EventType] [nvarchar](200) NULL,
	[ReleaseDateTime] [smalldatetime] NULL,
	[InstitutionBody] [nvarchar](100) NULL,
	[Country] [nvarchar](50) NULL,
	[Currency] [nvarchar](20) NULL,
	[EventDescription] [nvarchar](max) NULL,
	[Importance] [nvarchar](20) NULL,
	[JsonSrc] [nvarchar](100) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblEconomicEvents]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEconomicEvents](
	[ID] [uniqueidentifier] NOT NULL,
	[EventHeadline] [nvarchar](300) NULL,
	[Category] [nvarchar](50) NULL,
	[EventType] [nvarchar](200) NULL,
	[ReleaseDateTime] [smalldatetime] NULL,
	[InstitutionBody] [nvarchar](100) NULL,
	[Country] [nvarchar](50) NULL,
	[Currency] [nvarchar](20) NULL,
	[EventDescription] [nvarchar](max) NULL,
	[Importance] [nvarchar](20) NULL,
	[JsonSrc] [nvarchar](100) NULL,
 CONSTRAINT [PK_tblEconomicEvents] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblEconomicValues]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEconomicValues](
	[ID] [uniqueidentifier] NOT NULL,
	[EventID] [uniqueidentifier] NULL,
	[Actual] [nvarchar](20) NULL,
	[Forecast] [nvarchar](20) NULL,
	[Previous] [nvarchar](20) NULL,
	[Completed] [bit] NULL,
	[NumericType] [nvarchar](20) NULL,
 CONSTRAINT [PK_tblEconomicValues] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblEventKeyPersonnel]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEventKeyPersonnel](
	[ID] [uniqueidentifier] NOT NULL,
	[EventID] [uniqueidentifier] NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
 CONSTRAINT [PK_EventKeyPersonnel] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblSTAGING_EconomicEvents]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSTAGING_EconomicEvents](
	[EventHeadline] [nvarchar](300) NULL,
	[Category] [nvarchar](50) NULL,
	[EventType] [nvarchar](200) NULL,
	[ReleaseDateTime] [smalldatetime] NULL,
	[InstitutionBody] [nvarchar](100) NULL,
	[Country] [nvarchar](50) NULL,
	[Currency] [nvarchar](20) NULL,
	[EventDescription] [nvarchar](max) NULL,
	[Importance] [nvarchar](20) NULL,
	[JsonSrc] [nvarchar](100) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblSTAGING_EconomicValues]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSTAGING_EconomicValues](
	[EventHeadline] [nvarchar](300) NULL,
	[ReleaseDateTime] [smalldatetime] NULL,
	[Actual] [nvarchar](20) NULL,
	[Forecast] [nvarchar](20) NULL,
	[Previous] [nvarchar](20) NULL,
	[NumericType] [nvarchar](20) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblSTAGING_EventKeyPersonnel]    Script Date: 14/10/2016 12:00:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSTAGING_EventKeyPersonnel](
	[EventHeadline] [nvarchar](300) NULL,
	[ReleaseDateTime] [smalldatetime] NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL
) ON [PRIMARY]

GO
