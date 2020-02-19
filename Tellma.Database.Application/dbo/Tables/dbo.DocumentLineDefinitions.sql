CREATE TABLE [dbo].[DocumentLineDefinitions]
(
	[Id]				INT					NOT NULL PRIMARY KEY IDENTITY,
	[DocumentId]		INT					NOT NULL CONSTRAINT [FK_DocumentLineDefinitions__DocumentId] REFERENCES dbo.Documents([Id]),
	[LineDefinitionId]	NVARCHAR (50)		NOT NULL CONSTRAINT [FK_DocumentLineDefinitions__LineDefinitionId] REFERENCES dbo.LineDefinitions([Id]),
	CONSTRAINT [UX_DocumentLineDefinitions__DocumentId_LineDefinitionId] UNIQUE ([DocumentId], [LineDefinitionId]),
	[Memo]				NVARCHAR (255),
	[MemoIsCommon]		BIT,
	[AgentId0]			INT,
	[AgentId0IsCommon]	BIT,
	[AgentId1]			INT,
	[AgentId1IsCommon]	BIT,
	[AgentId2]			INT,
	[AgentId2IsCommon]	BIT,
	[AgentId3]			INT,
	[AgentId3IsCommon]	BIT
)
