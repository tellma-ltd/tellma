CREATE TABLE [dbo].[Lines] (
--	These are for transactions only. If there are Lines from requests or inquiries, etc=> other tables
	[Id]					INT					CONSTRAINT [PK_Lines] PRIMARY KEY IDENTITY,
	[DocumentId]			INT					NOT NULL CONSTRAINT [FK_Lines__DocumentId] REFERENCES [dbo].[Documents] ([Id]) ON DELETE CASCADE,
	[DefinitionId]			INT					NOT NULL CONSTRAINT [FK_Lines__DefinitionId] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[State]					SMALLINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_Lines__State] CHECK ([State] BETWEEN -4 AND +4),
	[PostingDate]			DATE,
	CONSTRAINT [CK_Lines__State_PostingDate] CHECK([State] < 4 OR ([PostingDate] IS NOT NULL AND [PostingDate] < DATEADD(DAY, 1, GETDATE()))),

	[Memo]					NVARCHAR (255), -- a textual description for statements and reports
	[Index]					INT				NOT NULL,
	[Boolean1]				BIT,
	[Decimal1]				DECIMAL (19,4),
	[Text1]					NVARCHAR(10),
	[EmployeeId]			INT CONSTRAINT [FK_Lines__EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Agents] ([Id]),
	[CustomerId]			INT CONSTRAINT [FK_Lines__CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Agents] ([Id]),
	[SupplierId]			INT CONSTRAINT [FK_Lines__SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Agents] ([Id]),
-- for auditing
	[CreatedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]			INT	NOT NULL CONSTRAINT [FK_Lines__CreatedById]	FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]			INT	NOT NULL CONSTRAINT [FK_Lines__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
);
GO
CREATE INDEX [IX_Lines__DocumentId] ON [dbo].[Lines]([DocumentId]);
GO
CREATE INDEX [IX_Lines__DefinitionId] ON [dbo].[Lines]([DefinitionId]);
GO
CREATE INDEX [IX_Lines__EmployeeId] ON dbo.Lines([EmployeeId]);
GO
CREATE INDEX [IX_Lines__CustomerId] ON dbo.Lines([CustomerId]);
GO
CREATE INDEX [IX_Lines__SupplierId] ON dbo.Lines([SupplierId]);
GO