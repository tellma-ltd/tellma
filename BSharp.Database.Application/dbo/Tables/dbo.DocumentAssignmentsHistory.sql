CREATE TABLE [dbo].[DocumentAssignmentsHistory] (
-- To be filled by a trigger on table DocumentsAssignments
	[Id]								INT PRIMARY KEY,
	[DocumentId]	INT	NOT NULL,
	[AssigneeId]	INT	NOT NULL,
	[Comment]		NVARCHAR (1024),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[OpenedAt]		DATETIMEOFFSET (7),
	CONSTRAINT [FK_DocumentAssignmentsHistory__DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_DocumentAssignmentsHistory__AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_DocumentAssignmentsHistory__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_AssignmentsHistory__DocumentId] ON [dbo].[DocumentAssignmentsHistory]([DocumentId]);
GO
CREATE INDEX [IX_AssignmentsHistory__AssigneeId] ON [dbo].[DocumentAssignmentsHistory]([AssigneeId]);
GO
CREATE INDEX [IX_AssignmentsHistory__CreatedById] ON [dbo].[DocumentAssignmentsHistory]([CreatedById]);
GO