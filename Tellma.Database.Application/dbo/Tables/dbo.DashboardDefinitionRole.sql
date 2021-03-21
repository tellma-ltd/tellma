CREATE TABLE [dbo].[DashboardDefinitionRoles]
(
	[Id]								INT		CONSTRAINT [PK_DashboardDefinitionRoles] PRIMARY KEY IDENTITY,	
	[DashboardDefinitionId]				INT	NOT NULL CONSTRAINT [FK_DashboardDefinitionRoles_DashboardDefinitionId] REFERENCES [dbo].[DashboardDefinitions] ([Id]),
	[RoleId]							INT	NOT NULL CONSTRAINT [FK_DashboardDefinitionRoles_RoleId] REFERENCES [dbo].[Roles] ([Id])
)
