CREATE TYPE [dbo].[ValidationErrorList] AS TABLE (
	[Key]			NVARCHAR (225),
	[ErrorName]		NVARCHAR (225),
	[Argument0]		NVARCHAR (255),
	[Argument1]		NVARCHAR (255),
	[Argument2]		NVARCHAR (255),
	[Argument3]		NVARCHAR (255),
	[Argument4]		NVARCHAR (255)
	UNIQUE ([Key], [ErrorName], [Argument0], [Argument1], [Argument2], [Argument3], [Argument4])
);