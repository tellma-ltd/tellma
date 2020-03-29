CREATE PROCEDURE [dal].[AgentDefinitions__Save]
	@Entities [AgentDefinitionList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].[AgentDefinitions] AS t
	USING (
		SELECT [Index], [Id], [TitleSingular], [TitleSingular2], [TitleSingular3],
			[TitlePlural], [TitlePlural2], [TitlePlural3], 
			[TaxIdentificationNumberVisibility],
			[ImageVisibility],
			[StartDateVisibility],
			[StartDateLabel],
			[StartDateLabel2],
			[StartDateLabel3],
			--[Prefix],
			--[CodeWidth],
			--[IsActive],

			[JobVisibility],
			[RatesVisibility],
			[RatesLabel],
			[RatesLabel2],
			[RatesLabel3],
			[BankAccountNumberVisibility],
			[UserVisibility],
			[AllowMultipleUsers],

			[MainMenuIcon],
			[MainMenuSection], [MainMenuSortKey]
		FROM @Entities 
	) AS s ON (t.Id = s.[Id])
	WHEN MATCHED 
	THEN
		UPDATE SET 
			t.[TitleSingular]				= s.[TitleSingular],
			t.[TitleSingular2]				= s.[TitleSingular2],
			t.[TitleSingular3]				= s.[TitleSingular3],
			t.[TitlePlural]					= s.[TitlePlural],
			t.[TitlePlural2]				= s.[TitlePlural2],
			t.[TitlePlural3]				= s.[TitlePlural3],

			t.[TaxIdentificationNumberVisibility]
											= s.[TaxIdentificationNumberVisibility],
			t.[ImageVisibility]				= s.[ImageVisibility],
			t.[StartDateVisibility]			= s.[StartDateVisibility],
			t.[StartDateLabel]				= s.[StartDateLabel],
			t.[StartDateLabel2]				= s.[StartDateLabel2],
			t.[StartDateLabel3]				= s.[StartDateLabel3],

			t.[JobVisibility]				= s.[JobVisibility],
			t.[RatesVisibility]				= s.[RatesVisibility],
			t.[RatesLabel]					= s.[RatesLabel],
			t.[RatesLabel2]					= s.[RatesLabel2],
			t.[RatesLabel3]					= s.[RatesLabel3],
			t.[BankAccountNumberVisibility]	= s.[BankAccountNumberVisibility],
			t.[UserVisibility]				= s.[UserVisibility],
			t.[AllowMultipleUsers]			= s.[AllowMultipleUsers],

			t.[MainMenuIcon]				= s.[MainMenuIcon],
			t.[MainMenuSection]				= s.[MainMenuSection],
			t.[MainMenuSortKey]				= s.[MainMenuSortKey],
			t.[SavedById]					= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([Id],	[TitleSingular],	[TitleSingular2], [TitleSingular3],		[TitlePlural],	[TitlePlural2],		[TitlePlural3], 
			[TaxIdentificationNumberVisibility],
			[ImageVisibility],
			[StartDateVisibility],
			[StartDateLabel],
			[StartDateLabel2],
			[StartDateLabel3],
			--[Prefix],
			--[CodeWidth],
			--[IsActive],

			[JobVisibility],
			[RatesVisibility],
			[RatesLabel],
			[RatesLabel2],
			[RatesLabel3],
			[BankAccountNumberVisibility],
			[UserVisibility],
			[AllowMultipleUsers],
		
			[MainMenuIcon],		[MainMenuSection], [MainMenuSortKey])
		VALUES (s.[Id], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3], 
			s.[TaxIdentificationNumberVisibility],
			s.[ImageVisibility],
			s.[StartDateVisibility],
			s.[StartDateLabel],
			s.[StartDateLabel2],
			s.[StartDateLabel3],
			--s.[Prefix],
			--s.[CodeWidth],
			--s.[IsActive],

			s.[JobVisibility],
			s.[RatesVisibility],
			s.[RatesLabel],
			s.[RatesLabel2],
			s.[RatesLabel3],
			s.[BankAccountNumberVisibility],
			s.[UserVisibility],
			s.[AllowMultipleUsers],

			s.[MainMenuIcon], s.[MainMenuSection], s.[MainMenuSortKey]);