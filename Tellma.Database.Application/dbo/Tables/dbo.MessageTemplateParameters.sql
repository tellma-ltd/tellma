CREATE TABLE [dbo].[MessageTemplateParameters]
(
	[Id] INT CONSTRAINT [PK_MessageTemplateParameters] PRIMARY KEY IDENTITY,
	[Index] INT NOT NULL,
	[MessageTemplateId] INT NOT NULL CONSTRAINT [FK_MessageTemplateParameters__MessageTemplateId] REFERENCES [dbo].[MessageTemplates] ([Id]) ON DELETE CASCADE,
	[Key] NVARCHAR (255) NOT NULL,
	[Label] NVARCHAR (255) NOT NULL,
	[Label2] NVARCHAR (255),
	[Label3] NVARCHAR (255),
	[IsRequired] BIT NOT NULL,
	[Control] NVARCHAR (50) NOT NULL,  -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions] NVARCHAR (1024)
);
GO
