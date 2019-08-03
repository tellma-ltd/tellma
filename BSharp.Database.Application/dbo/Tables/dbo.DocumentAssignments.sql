CREATE TABLE [dbo].[DocumentAssignments] (
-- When document is assigned to someone, who in turn can only post or assign to someone else
-- if the assignee does not open the document, any user can still forward it to someone else
	[DocumentId]	INT Primary Key,
-- If the document gets posted or void, this record will be deleted, and recorded in DocumentAssignmentsHistory
-- If it is in draft, it will be automatically assigned to the person who moved it to draft.
	[AssigneeId]	INT	NOT NULL,
-- When moved to draft mode, the comment is automatically To be completed in the user primary language
	[Comment]		NVARCHAR (1024),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
-- The first time the assignee calls the API to select the document, OpenedAt gets set
	[OpenedAt]		DATETIMEOFFSET (7),
	CONSTRAINT [FK_DocumentAssignments__DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_DocumentAssignments__AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_DocumentAssignments__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_Assignments__AssigneeId] ON [dbo].[DocumentAssignments]([AssigneeId]);
GO
CREATE INDEX [IX_Assignments__CreatedBy] ON [dbo].[DocumentAssignments]([CreatedById]);
GO