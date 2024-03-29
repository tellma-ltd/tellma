﻿CREATE TABLE [dbo].[DocumentAssignmentsHistory] (
-- To be filled by a trigger on table DocumentsAssignments
	[Id]			INT					CONSTRAINT [PK_DocumentAssignmentsHistory] PRIMARY KEY IDENTITY,
	[DocumentId]	INT					NOT NULL CONSTRAINT [FK_DocumentAssignmentsHistory__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[AssigneeId]	INT					NOT NULL CONSTRAINT [FK_DocumentAssignmentsHistory__AssigneeId] REFERENCES [dbo].[Users] ([Id]),
	[Comment]		NVARCHAR (1024),
	[CreatedAt]		DATETIMEOFFSET (7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT					NOT NULL CONSTRAINT [FK_DocumentAssignmentsHistory__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]	DATETIMEOFFSET (7),	-- Only set if the user modifies the comment
	[OpenedAt]		DATETIMEOFFSET (7)	
);
GO
CREATE INDEX [IX_AssignmentsHistory__DocumentId] ON [dbo].[DocumentAssignmentsHistory]([DocumentId]);
GO
CREATE INDEX [IX_AssignmentsHistory__AssigneeId] ON [dbo].[DocumentAssignmentsHistory]([AssigneeId]);
GO
CREATE INDEX [IX_AssignmentsHistory__CreatedById] ON [dbo].[DocumentAssignmentsHistory]([CreatedById]);
GO