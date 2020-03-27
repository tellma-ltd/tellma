CREATE TABLE [dbo].[AgentUsers] (
	[Id]			INT					CONSTRAINT [PK_AgentUsers] PRIMARY KEY IDENTITY,
	[AgentId]		INT					NOT NULL CONSTRAINT [FK_AgentUsers__AgentId] REFERENCES dbo.Agents([Id]) ON DELETE CASCADE,
	[UserId]		INT					NOT NULL CONSTRAINT [FK_AgentUsers__UserId] REFERENCES dbo.Users([Id]),
	[Direction]		SMALLINT			NOT NULL CHECK([Direction] IN (-1,0,+1))
);