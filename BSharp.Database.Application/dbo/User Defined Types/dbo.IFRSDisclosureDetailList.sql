CREATE TYPE [dbo].[IfrsDisclosureDetailList] AS TABLE (
	[Index]				INT,
	[IfrsDisclosureId]	NVARCHAR (255),
	[Concept]			NVARCHAR (255),
	[ValidSince]		Date				NOT NULL DEFAULT '0001.01.01',
	[Value]				NVARCHAR (255),
	PRIMARY KEY ([IfrsDisclosureId], [Concept], [ValidSince])
);