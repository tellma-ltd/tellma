CREATE TABLE [dbo].[ReportDefinitionRoles]
(
	[Id]								INT		CONSTRAINT [PK_ReportDefinitionRoles] PRIMARY KEY IDENTITY,	
	[ReportDefinitionId]				INT	NOT NULL CONSTRAINT [FK_ReportDefinitionRoles_ReportDefinitionId] REFERENCES [dbo].[ReportDefinitions] ([Id]) ON DELETE CASCADE,
	[RoleId]							INT	NOT NULL CONSTRAINT [FK_ReportDefinitionRoles_RoleId] REFERENCES [dbo].[Roles] ([Id])
)
