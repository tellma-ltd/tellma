CREATE TABLE [dbo].[PrintingTemplateRoles]
(
	[Id]								INT CONSTRAINT [PK_PrintingTemplateRoles] PRIMARY KEY IDENTITY,
	[PrintingTemplateId]				INT	NOT NULL CONSTRAINT [FK_PrintingTemplateRoles_ReportDefinitionId] REFERENCES [dbo].[PrintingTemplates] ([Id]) ON DELETE CASCADE,
	[RoleId]							INT	NOT NULL CONSTRAINT [FK_PrintingTemplateRoles_RoleId] REFERENCES [dbo].[Roles] ([Id])
);
GO
