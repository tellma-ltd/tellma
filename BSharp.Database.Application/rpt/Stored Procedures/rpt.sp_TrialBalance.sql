CREATE PROCEDURE [rpt].[sp_TrialBalance] 
/* 
SELECT * FROM [rpt].[sp_TrialBalance] ( @fromDate = '01.01.2015', @toDate = '01.01.2020', @ByResource = 1, @ByIfrsNote = 1, @PrintQuery = 1)
*/	
	@fromDate Date = '01.01.2000', 
	@toDate Date = '01.01.2020',
	@ByResponsibilityCenter bit = 1,
	@ByResource bit = 1,
	@ByIfrsNote bit = 1,
	@PrintQuery bit = 0
AS
BEGIN
	DECLARE @Query nvarchar(max);
	SET @Query = 
	N'
		SET NOCOUNT ON;
		SELECT
			A.[Code], A.[Name] As Account,'
	IF (@ByResponsibilityCenter = 1)
		SET @Query = @Query + N'
			S.[Name] As ResponsibilityCenter,'
	IF (@ByResource = 1)
		SET @Query = @Query + N'
			R.[Name] As Resource,
			--T.[Amount], MU.[Name] As UOM,'
	IF (@ByIfrsNote = 1)
		SET @Query = @Query + N'
			T.IfrsNoteId As IfrsNote,'
	SET @Query = @Query + N'
			[MoneyAmount], [Mass], [Volume], [Area], [Length], [Time], [Count],
			(CASE WHEN T.[Value] > 0 THEN T.[Value] ELSE 0 END) As Debit,
			(CASE WHEN T.[Value] < 0 THEN -T.[Value] ELSE 0 END) As Credit
		FROM 
		(
			SELECT AccountId, '
	IF (@ByResponsibilityCenter = 1) SET @Query = @Query + N'ResponsibilityCenterId, '
	IF (@ByResource = 1) SET @Query = @Query + N'ResourceId, '
	IF (@ByIfrsNote = 1) SET @Query = @Query + N'IfrsNoteId, '
	SET @Query = @Query + N'
			CAST(SUM([Direction] * [MoneyAmount]) AS money) AS [MoneyAmount],
			CAST(SUM([Direction] * [Mass]) AS money) AS [Mass],
			CAST(SUM([Direction] * [Volume]) AS money) AS [Volume],	
			CAST(SUM([Direction] * [Area]) AS money) AS [Area],
			CAST(SUM([Direction] * [Length]) AS money) AS [Length],
			CAST(SUM([Direction] * [Time]) AS money) AS [Time],
			CAST(SUM([Direction] * [Count]) AS money) AS [Count],	
			CAST(SUM([Direction] * [Value]) AS money) AS [Value]
			FROM [dbo].[fi_Journal](@fromDate, @toDate)
			GROUP BY AccountId'
	IF (@ByResponsibilityCenter = 1) SET @Query = @Query + N', ResponsibilityCenterId'
	IF (@ByResource = 1) SET @Query = @Query + N', ResourceId'
	IF (@ByIfrsNote = 1) SET @Query = @Query + N', IfrsNoteId'
	SET @Query = @Query + N'		
			HAVING
				SUM([Direction] * [MoneyAmount]) <> 0 OR
				SUM([Direction] * [Mass]) <> 0 OR
				SUM([Direction] * [Volume]) <> 0 OR
				SUM([Direction] * [Area]) <> 0 OR
				SUM([Direction] * [Length]) <> 0 OR
				SUM([Direction] * [Time]) <> 0 OR
				SUM([Direction] * [Count]) <> 0 OR
				SUM([Direction] * [Value]) <> 0
		) T 
		JOIN [dbo].Accounts A ON T.AccountId = A.Id'
	IF (@ByResponsibilityCenter = 1) SET @Query = @Query + N'
		LEFT JOIN [dbo].[ResponsibilityCenters] S ON T.ResponsibilityCenterId = S.Id'
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
		EXEC sp_executesql @Query, @ParmDefinition, @fromDate = @fromDate, @toDate = @toDate
	END
END