CREATE TABLE [dbo].[AccountDesignationResourceDefinitions]
(
	[Id]					INT CONSTRAINT [PK_AccountDesignationResourceDefinitions] PRIMARY KEY IDENTITY,
	[AccountDesignationId]	INT NOT NULL CONSTRAINT [FK_AccountDesignationResourceDefinitions__AccountDesignationId] REFERENCES dbo.[AccountDesignations]([Id])  ON DELETE CASCADE,
	[ResourceDefinitionId]	INT NOT NULL CONSTRAINT [FK_AccountDesignationResourceDefinitions__ResourceDefinitionId] REFERENCES dbo.ResourceDefinitions([Id])
);