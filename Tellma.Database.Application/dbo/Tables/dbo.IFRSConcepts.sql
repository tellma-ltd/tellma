CREATE TABLE [dbo].[IfrsConcepts] (
	[Id]						INT				CONSTRAINT [PK_IfrsConcepts] PRIMARY KEY IDENTITY,
	[Code]						NVARCHAR (255)	CONSTRAINT [UQ_IfrsConcepts__Code] UNIQUE,
	[Label]						NVARCHAR (1024)		NOT NULL,
	[Label2]					NVARCHAR (1024),
	[Label3]					NVARCHAR (1024),
	[Documentation]				NVARCHAR (MAX),
	[Documentation2]			NVARCHAR (MAX),
	[Documentation3]			NVARCHAR (MAX)
);