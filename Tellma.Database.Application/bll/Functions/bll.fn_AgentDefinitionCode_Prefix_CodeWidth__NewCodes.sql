CREATE FUNCTION bll.fn_AgentDefinitionCode_Prefix_CodeWidth__NewCodes (
    @DefinitionCode NVARCHAR(50),
    @Prefix NVARCHAR(5),
    @CodeWidth TINYINT,
    @Count INT  -- number of new codes to generate
)
RETURNS @NewCodes TABLE (
    RowNum INT,
    NewCode NVARCHAR(50)
)
AS
BEGIN
    DECLARE @DefinitionId INT = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @DefinitionCode);
    DECLARE @MaxSN NVARCHAR(50) = (
        SELECT MAX([Code])
        FROM dbo.Agents
        WHERE DefinitionId = @DefinitionId
        AND [Code] LIKE @Prefix + N'[0-9]%'
    );
    DECLARE @StartInt INT = ISNULL(CAST(RIGHT(@MaxSN, @CodeWidth) AS INT), 0) + 1;
    
    -- Generate sequential codes
    ;WITH Numbers AS (
        SELECT 1 AS n
        UNION ALL
        SELECT n + 1 FROM Numbers WHERE n < @Count
    )
    INSERT INTO @NewCodes (RowNum, NewCode)
    SELECT 
        n AS RowNum,
        @Prefix + RIGHT(REPLICATE(N'0', @CodeWidth) + CAST(@StartInt + n - 1 AS NVARCHAR(50)), @CodeWidth) AS NewCode
    FROM Numbers
    OPTION (MAXRECURSION 1000);
    
    RETURN;
END