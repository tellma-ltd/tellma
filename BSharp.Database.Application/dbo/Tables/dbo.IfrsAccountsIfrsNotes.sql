CREATE TABLE [dbo].[IfrsAccountClassificationsEntryClassifications] (
	[Id]							INT				CONSTRAINT [PK_IfrsAccountClassificationsEntryClassifications] PRIMARY KEY,
	[IfrsAccountClassificationId]	NVARCHAR (255)	NOT NULL CONSTRAINT [FK_IACEC_IfrsAccountClassificationId] FOREIGN KEY ([IfrsAccountClassificationId]) REFERENCES [dbo].[IfrsAccountClassifications] ([Id]) ON DELETE CASCADE, 
	[IfrsEntryClassificationId]		NVARCHAR (255)	NOT NULL CONSTRAINT [FK_IACEC_IfrsEntryClassificationId] FOREIGN KEY ([IfrsEntryClassificationId]) REFERENCES [dbo].[IfrsEntryClassifications] ([Id]),
	[Direction]						SMALLINT		NOT NULL CONSTRAINT [CK_IACEC_Direction] CHECK ([Direction] IN (-1, 0, +1)),
);
GO;
CREATE UNIQUE INDEX [IX_IACEC__IfrsAccountClassificationId_IfrsEntryClassificationId_Direction]
  ON [dbo].[IfrsAccountClassificationsEntryClassifications]([IfrsAccountClassificationId], [IfrsEntryClassificationId], [Direction]);
GO;