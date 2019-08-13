CREATE TYPE [dbo].[IfrsDisclosureDetailList] AS TABLE (
	[Index]				INT	IDENTITY(0,1),
	[IfrsDisclosureId]	NVARCHAR (255)	NOT NULL,
	[ValidSince]		Date			NOT NULL DEFAULT('0001.01.01'),
	[Value]				NVARCHAR (255),
	PRIMARY KEY ([IfrsDisclosureId], [ValidSince])
);