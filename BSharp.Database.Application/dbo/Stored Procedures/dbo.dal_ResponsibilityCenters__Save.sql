CREATE PROCEDURE [dbo].[dal_ResponsibilityCenters__Save]
	@Entities [ResponsibilityCenterList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[ResponsibilityCenters] AS t
	USING (
		SELECT [Index], [Id], [ResponsibilityDomain], [Name], [Name2], [Name3], [ParentId], [Code],
		[OperationId], [ProductCategoryId], [GeographicRegionId], [CustomerSegmentId], [TaxSegmentId]
		FROM @Entities 
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[ResponsibilityDomain]= s.[ResponsibilityDomain],
			t.[Name]				= s.[Name],
			t.[Name2]				= s.[Name2],
			t.[Name3]				= s.[Name3],
			t.[ParentId]			= s.[ParentId],
			t.[Code]				= s.[Code],
			t.[OperationId]			= s.[OperationId],
			t.[ProductCategoryId]	= s.[ProductCategoryId],
			t.[GeographicRegionId]	= s.[GeographicRegionId],
			t.[CustomerSegmentId]	= s.[CustomerSegmentId],
			t.[TaxSegmentId]		= s.[TaxSegmentId],
			t.[ModifiedAt]			= @Now,
			t.[ModifiedById]		= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([ResponsibilityDomain], [Name],	[Name2], [Name3], [Code], 
				[OperationId], [ProductCategoryId], [GeographicRegionId], [CustomerSegmentId], [TaxSegmentId])
		VALUES (s.[ResponsibilityDomain], s.[Name], s.[Name2], s.[Name3], s.[Code], 
				s.[OperationId], s.[ProductCategoryId], s.[GeographicRegionId], s.[CustomerSegmentId], s.[TaxSegmentId])
		OUTPUT s.[Index], inserted.[Id];