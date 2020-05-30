CREATE TABLE [dbo].[AccountDesignations]
(
	[Id]				INT				CONSTRAINT [PK_AccountDesignations] PRIMARY KEY,-- IDENTITY,
	-- -1 No mapping. or when several accounts can map to the same designation.
	-- Applies to basic B/S and basic P/L
	-- 0 Set Value, 1 By Contract, 2 By Resource, 3 By Center
	-- 21: By Resource Lookup1 22: By Resource Lookup1 and Contract Id
	[MapFunction]		SMALLINT		NOT NULL DEFAULT -1,
	CONSTRAINT [UX_AccountDesignations] UNIQUE([Id], [MapFunction]),
--	[IsFinancial]		BIT				NOT NULL DEFAULT 1,
	[ShowOCE]			BIT				NOT NULL DEFAULT 1,
	[Code]				NVARCHAR (50)	NOT NULL,
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	-- Audit details
	[CreatedAt]							DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]						INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountDesignations__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]						INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AccountDesignations__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);