CREATE PROCEDURE [dal].[ResourcePicks__Save]
	@Entities [dbo].[ResourcePickList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ResourcePicks] AS t
		USING (
			SELECT 	
				[Index],
				[Id],
				[ResourceId],
				[Name],
				[Name2],
				[Name3],
				[Code],
				[AvailableSince],
				[AvailableTill],
				[MonetaryValue],
				[Mass],
				[Volume],
				[Area],
				[Length],
				[Time],
				[Count],
				[Beneficiary],
				[IssuingBankAccountId],
				[IssuingBankId],
				[Text1],
				[Text2],
				[Date1],
				[Date2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Lookup5Id]
			FROM @Entities 
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET 
				t.[ResourceId]				=	s.[ResourceId],
				t.[Name]					=	s.[Name],
				t.[Name2]					=	s.[Name2],
				t.[Name3]					=	s.[Name3],
				t.[Code]					=	s.[Code],
				t.[AvailableSince]			=	s.[AvailableSince],
				t.[AvailableTill]				=	s.[AvailableTill],
				t.[MonetaryValue]			=	s.[MonetaryValue],
				t.[Mass]					=	s.[Mass],
				t.[Volume]					=	s.[Volume],
				t.[Area]					=	s.[Area],
				t.[Length]					=	s.[Length],
				t.[Time]					=	s.[Time],
				t.[Count]					=	s.[Count],
				t.[Beneficiary]				=	s.[Beneficiary],
				t.[IssuingBankAccountId]	=	s.[IssuingBankAccountId],
				t.[IssuingBankId]			=	s.[IssuingBankId],
				t.[Text1]					=	s.[Text1],
				t.[Text2]					=	s.[Text2],
				t.[Date1]					=	s.[Date1],
				t.[Date2]					=	s.[Date2],
				t.[Lookup1Id]				=	s.[Lookup1Id],
				t.[Lookup2Id]				=	s.[Lookup2Id],
				t.[Lookup3Id]				=	s.[Lookup3Id],
				t.[Lookup4Id]				=	s.[Lookup4Id],
				t.[Lookup5Id]				=	s.[Lookup5Id],
				t.[ModifiedAt]				=	@Now,
				t.[ModifiedById]			=	@UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[ResourceId],
				[Name],
				[Name2],
				[Name3],
				[Code],
				[AvailableSince],
				[AvailableTill],
				[MonetaryValue],
				[Mass],
				[Volume],
				[Area],
				[Length],
				[Time],
				[Count],
				[Beneficiary],
				[IssuingBankAccountId],
				[IssuingBankId],
				[Text1],
				[Text2],
				[Date1],
				[Date2],
				[Lookup1Id],
				[Lookup2Id],
				[Lookup3Id],
				[Lookup4Id],
				[Lookup5Id])
			VALUES (
				s.[ResourceId],
				s.[Name],
				s.[Name2],
				s.[Name3],
				s.[Code],
				s.[AvailableSince],
				s.[AvailableTill],
				s.[MonetaryValue],
				s.[Mass],
				s.[Volume],
				s.[Area],
				s.[Length],
				s.[Time],
				s.[Count],
				s.[Beneficiary],
				s.[IssuingBankAccountId],
				s.[IssuingBankId],
				s.[Text1],
				s.[Text2],
				s.[Date1],
				s.[Date2],
				s.[Lookup1Id],
				s.[Lookup2Id],
				s.[Lookup3Id],
				s.[Lookup4Id],
				s.[Lookup5Id])
			OUTPUT s.[Index], inserted.[Id]
	) AS x
	OPTION (RECOMPILE);

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
