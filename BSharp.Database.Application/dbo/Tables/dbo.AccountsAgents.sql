CREATE TABLE [dbo].[AccountsAgents]
(
	[AccountId]		INT				CONSTRAINT [FK_AccountsAgents__AccountId] FOREIGN KEY ([AccountId]) REFERENCES dbo.[Accounts]([Id]),
	[AgentId]		INT				CONSTRAINT [FK_AccountsAgents__AgentId] FOREIGN KEY ([AgentId]) REFERENCES dbo.[Agents]([Id]) ON UPDATE CASCADE,
	[IsActive]		BIT	NOT NULL DEFAULT 1
	CONSTRAINT [PK_AccountsAgents]	PRIMARY KEY ([AccountId], [AgentId])
);