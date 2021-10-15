CREATE TYPE [dbo].[NotificationTemplateAttachmentList] AS TABLE
(
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
    PRIMARY KEY CLUSTERED ([Index], [HeaderIndex]),
	[Id]						INT	NOT NULL DEFAULT 0,
	[NotificationTemplateId] INT,
	[ContextOverride] NVARCHAR (1024),
	[DownloadNameOverride] NVARCHAR (1024),
	[PrintingTemplateId] INT
)
