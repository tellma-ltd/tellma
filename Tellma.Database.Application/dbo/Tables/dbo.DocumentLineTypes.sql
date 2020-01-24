CREATE TABLE [dbo].[DocumentLineTypes] (
	[TenantId]				INT					DEFAULT CONVERT(INT, SESSION_CONTEXT(N'TenantId')),
	[Id]					INT					IDENTITY,
	[DocumentId]			INT					NOT NULL,
	[LineTypeId]			NVARCHAR (255)		NOT NULL
)
