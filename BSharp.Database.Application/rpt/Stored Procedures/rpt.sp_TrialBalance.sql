CREATE PROCEDURE [rpt].[sp_TrialBalance] 
/* 
SELECT * FROM [rpt].[sp_TrialBalance] ( @fromDate = '01.01.2015', @toDate = '01.01.2020', @ByResource = 1, @ByEntryType = 1, @PrintQuery = 1)
*/	
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2020',
	@ByResource bit = 1,
	@ByEntryClassification bit = 1,
	@PrintQuery bit = 0
AS
BEGIN
	DECLARE @Query nvarchar(max);
	SET @Query = 
	N'
		SET NOCOUNT ON;
		SELECT
			A.[Code], A.[Name] As Account,'
	IF (@ByResource = 1)
		SET @Query = @Query + N'
			R.[Name] As Resource,
			--T.[Amount], MU.[Name] As UOM,'
	IF (@ByEntryClassification = 1)
		SET @Query = @Query + N'
			T.EntryTypeId As IfrsNote,'
	SET @Query = @Query + N'
			[MonetaryValue], [Mass], [Volume], [Area], [Length], [Time], [Count],
			(CASE WHEN T.[Value] > 0 THEN T.[Value] ELSE 0 END) As Debit,
			(CASE WHEN T.[Value] < 0 THEN -T.[Value] ELSE 0 END) As Credit
		FROM 
		(
			SELECT AccountId, '
	IF (@ByResource = 1) SET @Query = @Query + N'ResourceId, '
	IF (@ByEntryClassification = 1) SET @Query = @Query + N'EntryClassificationId, '
	SET @Query = @Query + N'
			CAST(SUM([Direction] * [MonetaryValue]) AS DECIMAL (19,4)) AS [MonetaryValue],
			CAST(SUM([Direction] * [Mass]) AS DECIMAL (19,4)) AS [Mass],
			CAST(SUM([Direction] * [Volume]) AS DECIMAL (19,4)) AS [Volume],	
			CAST(SUM([Direction] * [Area]) AS DECIMAL (19,4)) AS [Area],
			CAST(SUM([Direction] * [Length]) AS DECIMAL (19,4)) AS [Length],
			CAST(SUM([Direction] * [Time]) AS DECIMAL (19,4)) AS [Time],
			CAST(SUM([Direction] * [Count]) AS DECIMAL (19,4)) AS [Count],	
			CAST(SUM([Direction] * [Value]) AS DECIMAL (19,4)) AS [Value]
			FROM [dbo].[fi_Journal](@fromDate, @toDate)
			GROUP BY AccountId'
	IF (@ByResource = 1) SET @Query = @Query + N', ResourceId'
	IF (@ByEntryClassification = 1) SET @Query = @Query + N', EntryClassificationId'
	SET @Query = @Query + N'		
			HAVING
				SUM([Direction] * [MonetaryValue]) <> 0 OR
				SUM([Direction] * [Mass]) <> 0 OR
				SUM([Direction] * [Volume]) <> 0 OR
				SUM([Direction] * [Area]) <> 0 OR
				SUM([Direction] * [Length]) <> 0 OR
				SUM([Direction] * [Time]) <> 0 OR
				SUM([Direction] * [Count]) <> 0 OR
				SUM([Direction] * [Value]) <> 0
		) T 
		JOIN [dbo].Accounts A ON T.AccountId = A.Id'
	IF (@ByResource = 1) SET @Query = @Query + N'
		JOIN [dbo].[Resources] R ON T.ResourceId = R.Id
		--JOIN [dbo].[MeasurementUnits] MU ON R.MeasurementUnitId = MU.Id
		'
	SET @Query = @Query + N'
		ORDER BY A.[Code]'

	IF (@PrintQuery = 1)
		Print @Query
	ELSE BEGIN
		DECLARE @ParmDefinition nvarchar(500);
		SET @ParmDefinition = N'@fromDate Datetime, @toDate Datetime';
		EXEC master.sys.sp_executesql @Query, @ParmDefinition, @fromDate = @fromDate, @toDate = @toDate
	END
END