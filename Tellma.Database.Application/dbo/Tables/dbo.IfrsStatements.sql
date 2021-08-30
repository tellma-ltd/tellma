CREATE TABLE [dbo].[IfrsStatements]
(
	[Id]				INT							CONSTRAINT [PK_IfrsStatements] PRIMARY KEY IDENTITY,
	[IfrsRole]			NCHAR(6)		NOT NULL	CONSTRAINT [CK_IfrsStatements__IfrsRole] CHECK([IfrsRole] IN (N'210000')),
	[ParentId]			INT,
	[IfrsConceptId]		NVARCHAR(255)	NOT NULL,
	[IsSystem]			BIT				NOT NULL	DEFAULT 0, 
	[Context]			NVARCHAR(30)	NOT NULL	CHECK([Context] IN (N'Instant', N'Period', N'Beginning', N'Ending')),
	[CustomName]		NVARCHAR(255)	NOT NULL,
	[CustomName2]		NVARCHAR(255)	NOT NULL,
	[CustomName3]		NVARCHAR(255)	NOT NULL
);