CREATE PROCEDURE [dbo].[dal_Accounts__Save]
	@Entities [AccountList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedUuidList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	MERGE INTO [dbo].Accounts AS t
	USING (
		SELECT
			[Index], [Id], [CustomClassificationId], [IfrsAccountId],
			[Name], [Name2], [Name3], [Code], [PartyReference], [AgentId],
			[IfrsNoteIsFixed], [IfrsNoteId],
			[ResponsibilityCenterId],
			[ResourceIsFixed], [ResourceId]
		FROM @Entities 
		WHERE [EntityState] IN (N'Inserted', N'Updated')
	) AS s ON (t.Id = s.Id)
	WHEN MATCHED 
	THEN
		UPDATE SET
			t.[CustomClassificationId]		= s.[CustomClassificationId], 
			t.[IfrsAccountId]				= s.[IfrsAccountId],
			t.[Name]						= s.[Name],
			t.[Name2]						= s.[Name2],
			t.[Name3]						= s.[Name3],
			t.[Code]						= s.[Code],
			t.[PartyReference]			= s.[PartyReference],
			t.[AgentId]						= s.[AgentId],
			t.[IfrsNoteIsFixed]				= s.[IfrsNoteIsFixed],
			t.[IfrsNoteId]					= s.[IfrsNoteId],
			t.[ResponsibilityCenterId]		= s.[ResponsibilityCenterId],
			t.[ResourceIsFixed]				= s.[ResourceIsFixed],
			t.[ResourceId]					= s.[ResourceId],
			t.[ModifiedAt]					= @Now,
			t.[ModifiedById]				= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([CustomClassificationId], [IfrsAccountId],
			[Name], [Name2], [Name3], [Code], [PartyReference], [AgentId], [IfrsNoteIsFixed], [IfrsNoteId],
			[ResponsibilityCenterId],
			[ResourceIsFixed], [ResourceId])
		VALUES (s.[CustomClassificationId], s.[IfrsAccountId],
			s.[Name], s.[Name2], s.[Name3], s.[Code], s.[PartyReference], s.[AgentId], s.[IfrsNoteIsFixed], s.[IfrsNoteId],
			s.[ResponsibilityCenterId], 
			s.[ResourceIsFixed], s.[ResourceId]);