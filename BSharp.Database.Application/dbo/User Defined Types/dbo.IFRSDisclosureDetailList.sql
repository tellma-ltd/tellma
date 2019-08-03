CREATE TYPE [dbo].[IfrsDisclosureDetailList] AS TABLE (
	[Index]				INT	PRIMARY KEY IDENTITY(1, 1),
	[Id]				INT NOT NULL DEFAULT 0,
	[IfrsDisclosureId]	NVARCHAR (255)	NOT NULL,
	[Value]				NVARCHAR (255),
	[ValidSince]		Date			NOT NULL DEFAULT('0001.01.01'),
	[IsDirty]			BIT	NOT NULL DEFAULT 1
);