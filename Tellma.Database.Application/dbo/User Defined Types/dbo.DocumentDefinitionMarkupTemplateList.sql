CREATE TYPE [dbo].[DocumentDefinitionMarkupTemplateList] AS TABLE (
	[Index]					INT		DEFAULT 0,
	[HeaderIndex]			INT		DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]					INT		DEFAULT 0,
	[MarkupTemplateId]		INT,
	UNIQUE ([HeaderIndex], [MarkupTemplateId])
	-- TODO: Other business logic configuration related to printing
);