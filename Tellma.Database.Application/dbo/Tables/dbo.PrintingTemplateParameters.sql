CREATE TABLE [dbo].[PrintingTemplateParameters]
(
	[Id]				INT					CONSTRAINT [PK_PrintingTemplateParameters] PRIMARY KEY IDENTITY,
	[Index]						INT				NOT NULL,
	[PrintingTemplateId] INT				NOT NULL CONSTRAINT [FK_PrintingTemplateParameters__PrintingTemplateId] REFERENCES [dbo].[PrintingTemplates] ([Id]) ON DELETE CASCADE,
	[Key]				NVARCHAR (255)	NOT NULL,
	[Label]				NVARCHAR (255),
	[Label2]			NVARCHAR (255),
	[Label3]			NVARCHAR (255),
	[IsRequired]		BIT				NOT NULL,
	[Control]			NVARCHAR (50),  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]	NVARCHAR (1024)
);
GO
