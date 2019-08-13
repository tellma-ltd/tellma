CREATE TABLE [dbo].[DocumentSignatures] (
-- After each sign/revoke, the system recalculates the new Document state, based on the workflow and account rules
-- Redundant signatures (where actor/role is not specified in the workflow or the accounts) are discarded
-- Duplicate last signatures are discarded.
-- The signatures can only be revoked in the reverse order they were made in
-- A signature can only be removed by the signatory or by an IT administrator
	[Id]						INT PRIMARY KEY IDENTITY,
	[DocumentId]				INT	NOT NULL,
	
	[State]						NVARCHAR (30)		NOT NULL,
	[ReasonId]					INT,	-- Especially important for states: Rejected/Failed/Declined.
	[ReasonDetails]				NVARCHAR(1024),		-- especially useful when Reason Id = Other.
	-- For a source document, SignedAt = Now(). For a copy, it is manually entered.
	[SignedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	-- For a source document, ActorId is the userId. Else, it is editable.
	[AgentId]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	-- Role Id is selected from a choice list of the actor's roles of the actor that are compatible with workflow
	[RoleId]					INT,

	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	
	[RevokedAt]					DATETIMEOFFSET(7),
	[RevokedById]				INT,

	CONSTRAINT [FK_DocumentSignatures__Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [CK_DocumentSignatures__State] CHECK ([State] IN (N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Posted')),
	CONSTRAINT [FK_DocumentSignatures__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]),
	CONSTRAINT [FK_DocumentSignatures__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_DocumentSignatures__RevokedById] FOREIGN KEY ([RevokedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_DocumentSignatures__DocumentId] ON [dbo].[DocumentSignatures]([DocumentId]);
GO