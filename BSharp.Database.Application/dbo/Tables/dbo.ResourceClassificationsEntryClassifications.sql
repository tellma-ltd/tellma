CREATE TABLE [dbo].[ResourceClassificationsEntryClassifications]
(
	[ResourceClassificationId]	INT CONSTRAINT [FK_ResourceClassificationEntryClassifications__ResourceClassificationeId] REFERENCES [dbo].[ResourceClassifications] ([Id]), 
	[EntryClassificationId]		INT CONSTRAINT [FK_ResourceClassificationsEntryClassifications__EntryClassificationId] REFERENCES [dbo].[EntryClassifications] ([Id]),
	CONSTRAINT [PK_ResourceClassificationsEntryClassifications] PRIMARY KEY ([ResourceClassificationId], [EntryClassificationId])
);
GO;