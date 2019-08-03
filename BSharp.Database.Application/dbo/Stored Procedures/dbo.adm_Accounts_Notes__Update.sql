CREATE PROCEDURE [dbo].[adm_Accounts_Notes__Update]
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Code NVARCHAR (255);
/*
	SELECT @Code = min(Code) FROM [dbo].[IfrsConcepts];
	WHILE @Code IS NOT NULL
	BEGIN
		IF EXISTS(SELECT * FROM [dbo].[IfrsConcepts] WHERE Code Like @Code + '%' AND Code <> @Code AND [ConceptType] IN  (N'Regulatory', N'Correction'))
			UPDATE [dbo].[IfrsConcepts] SET IsExtensible = 0 WHERE Code = @Code AND IsExtensible = 1;
		ELSE
			UPDATE [dbo].[IfrsConcepts] SET IsExtensible = 1 WHERE Code = @Code AND IsExtensible = 0;
		SELECT @Code = min(Code) FROM [dbo].[IfrsConcepts] WHERE Code > @Code;
	END;

	SELECT @Code = min(Code) FROM [dbo].Notes;
	WHILE @Code IS NOT NULL
	BEGIN
		IF EXISTS(SELECT * FROM [dbo].Notes WHERE Code Like @Code + '%' AND Code <> @Code AND NoteType IN  (N'Regulatory', N'Correction'))
			UPDATE [dbo].Notes SET IsExtensible = 0 WHERE Code = @Code AND IsExtensible = 1;
		ELSE
			UPDATE [dbo].Notes SET IsExtensible = 1 WHERE Code = @Code AND IsExtensible = 0;
		SELECT @Code = min(Code) FROM [dbo].Notes WHERE Code > @Code;
	END;
	/*
	SELECT A.[Id] As AccountId, N.[Id] AS [NoteId]
	FROM dbo.[Accounts] A
	CROSS JOIN dbo.[Notes] N
	WHERE A.IsExtensible = 1 AND N.IsExtensible = 1
	AND (
		(A.Code Like N'' AND N.Code Like N'') OR
		(A.Code Like N'' AND N.Code Like N'') OR
		(A.Code Like N'' AND N.Code Like N'') OR
		(A.Code Like N'' AND N.Code Like N'') OR
		(A.Code Like N'' AND N.Code Like N'') OR
		(A.Code Like N'' AND N.Code Like N'') OR
		(A.Code Like N'' AND N.Code Like N'')
	)

	UPDATE [dbo].Accounts -- Agent
	SET [AccountSpecification] = N'PropertyPlantAndEquipment'
	WHERE Code Like '1101%' AND IsExtensible = 1

	UPDATE [dbo].Accounts -- 
	SET [AccountSpecification] = N'InvestmentProperty'
	WHERE Code Like '1102%' AND IsExtensible = 1

	UPDATE [dbo].Accounts -- Agent
	SET [AccountSpecification] = N'IntangibleAssetsOtherThanGoodwill'
	WHERE Code Like '1104%' AND IsExtensible = 1

	UPDATE [dbo].Accounts -- Agent
	SET [AccountSpecification] = N'Investments'
	WHERE Code Like '1106%' AND IsExtensible = 1

	UPDATE [dbo].Accounts
	SET [AccountSpecification] = N'BiologicalAssets'
	WHERE (Code Like '1107%' OR Code Like '1214%') AND IsExtensible = 1

	UPDATE [dbo].Accounts
	SET [AccountSpecification] = N'Agents' -- Agent
	WHERE (Code Like '1108%' OR Code Like '1212%' OR Code Like '312%' OR Code Like '3212%') AND IsExtensible = 1
	
	UPDATE [dbo].Accounts
	SET [AccountSpecification] = N'Inventories' -- Place
	WHERE (Code Like '1109%' OR Code Like '1211%') AND IsExtensible = 1

	UPDATE [dbo].Accounts
	SET [AccountSpecification] = N'Cash' -- Place
	WHERE (Code Like '1217') AND IsExtensible = 1

	UPDATE [dbo].Accounts
	SET [AccountSpecification] = N'Capital' -- Place
	WHERE (Code Like '211%') AND IsExtensible = 1

	UPDATE [dbo].Accounts
	SET [AccountSpecification] = N'Basic' -- Place
	WHERE [AccountSpecification] IS NULL
	*/
*/
END
