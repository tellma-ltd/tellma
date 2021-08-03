CREATE TABLE [dbo].[Agents]
(
	[Id]				INT					CONSTRAINT [PK_Agents] PRIMARY KEY IDENTITY,
	[Name]				NVARCHAR (50)		NOT NULL CONSTRAINT [UX_Agents__Name] UNIQUE,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsRelated]			BIT					NOT NULL DEFAULT 0,
	[Category]			NVARCHAR (50), --ParentMember, JointControlOrSignificantInfluenceMember, SubsidiariesMembe, AssociatesMember, JointVenturesWhereEntityIsVenturerMember, KeyManagementPersonnelOfEntityOrParentMember, OtherRelatedPartiesMember
	[IsActive]			BIT					NOT NULL DEFAULT 1,
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL CONSTRAINT [FK_Agents__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL CONSTRAINT [FK_Agents__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);