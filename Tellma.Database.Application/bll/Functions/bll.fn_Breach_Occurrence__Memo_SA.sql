CREATE FUNCTION [bll].[fn_Breach_Occurrence__Memo_SA](
	@ResourceId INT,
	@Occurrence INT,
	@ProposedMemo NVARCHAR (255)
)
RETURNS NVARCHAR (255)
AS BEGIN
	DECLARE @MyResult NVARCHAR (255)
	DECLARE @L1 INT, @L2 INT, @L3 INT, @L4 INT;
	DECLARE @Code1 NVARCHAR (10), @Code2 NVARCHAR (10),  @Code3 NVARCHAR (10), @Code4 NVARCHAR (10);
	DECLARE @Memo1 NVARCHAR (255), @Memo2 NVARCHAR (255), @Memo3 NVARCHAR (255), @Memo4 NVARCHAR (255)
	SELECT @L1 = [Lookup1Id], @L2 = [Lookup2Id], @L3 = [Lookup3Id], @L4 = [Lookup4Id]
	FROM dbo.Resources
	WHERE [Id] = @ResourceId

	SELECT @Code1 = dal.fn_Lookup__Code(@L1), @Code2 = dal.fn_Lookup__Code(@L2), @Code3 = dal.fn_Lookup__Code(@L3), @Code4 = dal.fn_Lookup__Code(@L4);
	SELECT @Memo1 = dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Lookups WHERE [Id] = @L1
	SELECT @Memo2 = dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Lookups WHERE [Id] = @L2
	SELECT @Memo3 = dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Lookups WHERE [Id] = @L3
	SELECT @Memo4 = dbo.fn_Localize([Name], [Name2], [Name3]) FROM dbo.Lookups WHERE [Id] = @L4

	SET @MyResult =
				CASE
					WHEN @Occurrence = 1 THEN IIF(@Code1 = N'Manual', @ProposedMemo, @Memo1)
					WHEN @Occurrence = 2 THEN IIF(@Code2 = N'Manual', @ProposedMemo, @Memo2)
					WHEN @Occurrence = 3 THEN IIF(@Code3 = N'Manual', @ProposedMemo, @Memo3)
					WHEN @Occurrence >= 4 THEN IIF(@Code4 = N'Manual', @ProposedMemo, @Memo4)
					ELSE @ProposedMemo
				END


	Return @MyResult
END
GO