CREATE TYPE [dbo].[IFRSConceptList] AS TABLE (
	[Index]				INT,
	[Id]				NVARCHAR (255),
	[Name]				NVARCHAR (1024) NOT NULL,
	[Code]				NVARCHAR (255)  NOT NULL,
	[IsActive]			BIT				NOT NULL,
	[AccountType]		NVARCHAR (255)	NOT NULL DEFAULT (N'Custom'),
	[IsExtensible]		BIT				NOT NULL DEFAULT (1),
	[ParentId]			NVARCHAR (255),
	PRIMARY KEY ([Index] ASC),
	INDEX IX_AccountList_Code UNIQUE CLUSTERED ([Code] ASC),
	CHECK ([AccountType] IN (N'Correction', N'Custom', N'Extension', N'Regulatory'))
);