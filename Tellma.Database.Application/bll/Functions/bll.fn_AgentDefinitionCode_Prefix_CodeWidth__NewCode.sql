CREATE FUNCTION bll.fn_AgentDefinitionCode_Prefix_CodeWidth__NewCode (
	@DefinitionCode NVARCHAR (50),
	@Prefix NVARCHAR (5),
	@CodeWidth TINYINT
)
RETURNS NVARCHAR (50)
AS
BEGIN
	DECLARE @DefinitionId INT = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @DefinitionCode);
	DECLARE @MaxSN NVARCHAR (50) = (
		SELECT MAX([Code])
		FROM dbo.Agents
		WHERE DefinitionId = @DefinitionId
		AND [Code] LIKE @Prefix + N'[0-9]%'
	);
	DECLARE @NewInt INT = ISNULL(CAST(RIGHT(@MaxSN, @CodeWidth) AS INT), 0) +1;
	DECLARE @NewSN NVARCHAR (50) = RIGHT(REPLICATE(N'0',  @CodeWidth) + CAST(@NewInt AS NVARCHAR(50)), @CodeWidth);
	RETURN @Prefix + @NewSN
END