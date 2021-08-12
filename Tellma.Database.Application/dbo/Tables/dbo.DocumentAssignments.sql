CREATE TABLE [dbo].[DocumentAssignments] (
-- When document is assigned to someone, who in turn can only close or assign to someone else
-- if the assignee does not open the document, any user can still forward it to someone else
	[DocumentId]	INT					CONSTRAINT [PK_DocumentAssignments__DocumentId] PRIMARY KEY
										CONSTRAINT [FK_DocumentAssignments__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
-- If the document becomes (Closed), this record will be deleted, and recorded in DocumentAssignmentsHistory
-- If it is in (Active) state, it will be automatically assigned to the person who moved it to (Active)
	[AssigneeId]	INT					NOT NULL CONSTRAINT [FK_DocumentAssignments__AssigneeId] REFERENCES [dbo].[Users] ([Id]),
-- When moved to (Active) state, the comment is automatically To be completed in the user primary language
	[Comment]		NVARCHAR (1024),
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT					NOT NULL CONSTRAINT [FK_DocumentAssignments__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]	DATETIMEOFFSET(7),
-- The first time the assignee calls the API to select the document, OpenedAt gets set
	[OpenedAt]		DATETIMEOFFSET (7)
);
GO
CREATE INDEX [IX_Assignments__AssigneeId] ON [dbo].[DocumentAssignments]([AssigneeId]);
GO
CREATE INDEX [IX_Assignments__CreatedBy] ON [dbo].[DocumentAssignments]([CreatedById]);
GO