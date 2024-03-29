﻿CREATE TABLE [dbo].[ExternalEntries] (
	[Id]					INT				CONSTRAINT [PK_ExternalEntries] PRIMARY KEY IDENTITY,
	[PostingDate]			DATE			CONSTRAINT [CK_ExternalEntries__PostingDate] CHECK ([PostingDate] < DATEADD(DAY, 1, GETDATE())),
	[Direction]				SMALLINT		NOT NULL CONSTRAINT [CK_ExternalEntries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]				INT				CONSTRAINT [FK_ExternalEntries__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	[AgentId]				INT				CONSTRAINT [FK_ExternalEntries__AgentId] REFERENCES dbo.[Agents]([Id]),
	[MonetaryValue]			DECIMAL (19,4),
	[ExternalReference]		NVARCHAR (50),
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT					NOT NULL CONSTRAINT [FK_ExternalEntries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT					NOT NULL CONSTRAINT [FK_ExternalEntries__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_ExternalEntries__AccountId] ON [dbo].[ExternalEntries]([AccountId]);
GO
CREATE INDEX [IX_ExternalEntries__AgentId] ON [dbo].[ExternalEntries]([AgentId]);
GO