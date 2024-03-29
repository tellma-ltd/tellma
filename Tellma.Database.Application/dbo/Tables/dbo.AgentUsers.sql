﻿CREATE TABLE [dbo].[AgentUsers] (
	[Id]				INT					CONSTRAINT [PK_AgentUsers] PRIMARY KEY IDENTITY,
	[AgentId]			INT					NOT NULL CONSTRAINT [FK_AgentUsers__AgentId] REFERENCES dbo.[Agents]([Id]) ON DELETE CASCADE,
	[UserId]			INT					NOT NULL CONSTRAINT [FK_AgentUsers__UserId] REFERENCES dbo.Users([Id]),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL CONSTRAINT [FK_AgentUsers__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL CONSTRAINT [FK_AgentUsers__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);