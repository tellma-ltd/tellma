 IF @DB = N'101' -- Banan SD, USD, en
 BEGIN
	INSERT INTO @Agents
	([Index], [Name]) VALUES
	(0,		N'el-Amin Al-Tayyib');

	INSERT INTO @AgentUsers([Index], [HeaderIndex],
		[UserId]) VALUES
	(0,0,@amtaam);
END

EXEC [api].[Agents__Save]
	@Entities = @Agents,
	@AgentUsers = @AgentUsers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Agents: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @elAminAgent INT = (SELECT [Id] FROM dbo.[Agents] WHERE [Name] = N'el-Amin Al-Tayyib');

