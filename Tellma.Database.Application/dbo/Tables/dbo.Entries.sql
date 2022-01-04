CREATE TABLE [dbo].[Entries] (
--	These are for transactions only. If there are entries from requests or inquiries, etc=> other tables
	[Id]						INT				CONSTRAINT [PK_Entries] PRIMARY KEY IDENTITY,
	[LineId]					INT				NOT NULL CONSTRAINT [FK_Entries__LineId] REFERENCES [dbo].[Lines] ([Id]) ON DELETE CASCADE,
	[Index]						INT				NOT NULL DEFAULT 0,
	CONSTRAINT [UQ_Entries__LineId_Index] UNIQUE([LineId], [Index]),
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_Entries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				NULL CONSTRAINT [FK_Entries__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	[CurrencyId]				NCHAR (3)		NOT NULL CONSTRAINT [FK_Entries__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[AgentId]					INT				CONSTRAINT [FK_Entries__AgentId] REFERENCES dbo.[Agents]([Id]),
	[NotedAgentId]				INT				CONSTRAINT [FK_Entries__NotedAgentId] REFERENCES dbo.[Agents]([Id]),
	[ResourceId]				INT				CONSTRAINT [FK_Entries__ResourceId] REFERENCES dbo.[Resources]([Id]),
	[CenterId]					INT				NOT NULL CONSTRAINT [FK_Entries__CentertId] REFERENCES dbo.[Centers]([Id]),
	-- Entry Type Id is Required in Entries only if we have Parent Entry type in AccountTypes
	[EntryTypeId]				INT				CONSTRAINT [FK_Entries__EntryTypeId] REFERENCES [dbo].[EntryTypes] ([Id]),
	[MonetaryValue]				DECIMAL (19,4), --			NOT NULL DEFAULT 0,
-- Tracking additive measures
	-- Quantity & Unit are the ones in which the transaction is held (purchase, sales, production)
	[Quantity]					DECIMAL (19,4),
	[UnitId]					INT				CONSTRAINT [FK_Entries__UnitId] REFERENCES [dbo].[Units] ([Id]),
	[Value]						DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in functional currency
	[RValue]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- re-instated in functional currency
	[PValue]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in presentation currency
-- The following are sort of dynamic properties that capture information for reporting purposes
	[Time1]						DATETIME2 (2),	-- from time
	[Duration]					DECIMAL (19,4),
	[DurationUnitId]			INT				CONSTRAINT [FK_Entries__DurationUnitId] REFERENCES [dbo].[Units] ([Id]),
	[Time2]						DATETIME2 (2),	-- to time
	[ExternalReference]			NVARCHAR (50),
	[ReferenceSourceId]			INT				CONSTRAINT [FK_Entries__ReferenceSourceId] REFERENCES dbo.[Agents]([Id]),
	[InternalReference]			NVARCHAR (50),
	[NotedAgentName]			NVARCHAR (255), -- In case, it is not necessary to define the agent, we simply capture the agent name.
	[NotedAmount]				DECIMAL (19,4),		-- e.g., amount subject to tax, or Control Quantity for poultry
	[NotedDate]					DATE,
	[NotedResourceId]			INT				CONSTRAINT [FK_Entries__NotedResourceId] REFERENCES dbo.[Resources]([Id]),
-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT					NOT NULL CONSTRAINT [FK_Entries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT					NOT NULL CONSTRAINT [FK_Entries__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE INDEX [IX_Entries__LineId] ON [dbo].[Entries]([LineId]);
GO
CREATE INDEX [IX_Entries__AccountId] ON [dbo].[Entries]([AccountId]);
GO
CREATE INDEX [IX_Entries__CurrencyId] ON [dbo].[Entries]([CurrencyId]);
GO
CREATE INDEX [IX_Entries__CenterId] ON [dbo].[Entries]([CenterId]);
GO
CREATE INDEX [IX_Entries__AgentId] ON [dbo].[Entries]([AgentId]);
GO
CREATE INDEX [IX_Entries__ResourceId] ON [dbo].[Entries]([ResourceId]);
GO
CREATE INDEX [IX_Entries__UnitId] ON [dbo].[Entries]([UnitId]);
GO
CREATE INDEX [IX_Entries__NotedAgentId] ON [dbo].[Entries]([NotedAgentId]);
GO
CREATE INDEX [IX_Entries__EntryTypeId] ON [dbo].[Entries]([EntryTypeId]);
GO
CREATE INDEX [IX_Entries__NotedResourceId] ON [dbo].[Entries]([NotedResourceId]);
GO
CREATE TRIGGER dbo.traiu_Entries ON [dbo].[Entries]
AFTER INSERT, UPDATE
AS 
BEGIN
	-- Customer or Sales Invioce
	UPDATE L
	SET L.CustomerId = T.CustomerId
	FROM dbo.Lines L
	LEFT JOIN (
		SELECT E.LineId, MIN(E.AgentId) As CustomerId, COUNT(DISTINCT E.AgentId) As CustomerCount
		FROM Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE LineId IN (SELECT DISTINCT LineId FROM Inserted)
		AND (
			AC.[Concept] IN (
				N'CRMExtension',
				N'NoncurrentTradeReceivables', N'NoncurrentReceivablesDueFromRelatedParties',
				N'NoncurrentReceivablesFromSaleOfProperties', N'NoncurrentReceivablesFromRentalOfProperties',
				N'CurrentTradeReceivables', N'TradeAndOtherCurrentReceivablesDueFromRelatedParties',
				N'CurrentReceivablesFromRentalOfProperties'
			)
		)
		GROUP BY E.LineId
		HAVING COUNT(DISTINCT E.AgentId) = 1
	) T
	ON L.[Id] = T.[LineId]
	WHERE (L.CustomerId IS NULL OR T.CustomerId IS NULL OR L.CustomerId <> T.CustomerId)
	AND Id IN (SELECT DISTINCT LineId FROM Inserted)
	-- Supplier or Purchase Invoice
	UPDATE L
	SET L.SupplierId = T.SupplierId
	FROM dbo.Lines L
	LEFT JOIN (
		SELECT E.LineId, MIN(E.AgentId) As SupplierId, COUNT(DISTINCT E.AgentId) As SupplierCount
		FROM Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE LineId IN (SELECT DISTINCT LineId FROM Inserted)
		AND (
			AC.[Concept] IN (
				N'NoncurrentPayablesToTradeSuppliers', N'NoncurrentPayablesToRelatedParties',
				N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'TradeAndOtherCurrentPayablesToRelatedParties'
			)
		)
		GROUP BY E.LineId
		HAVING COUNT(DISTINCT E.AgentId) = 1
	) T
	ON L.[Id] = T.[LineId]
	WHERE (L.SupplierId IS NULL OR T.SupplierId IS NULL OR L.SupplierId <> T.SupplierId)
	AND Id IN (SELECT DISTINCT LineId FROM Inserted)
	--- Employee
	UPDATE L
	SET L.EmployeeId = T.EmployeeId
	FROM dbo.Lines L
	LEFT JOIN (
		SELECT E.LineId, MIN(E.AgentId) As EmployeeId, COUNT(DISTINCT E.AgentId) As EmployeeCount
		FROM Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE LineId IN (SELECT DISTINCT LineId FROM Inserted)
		AND (
			AC.[Concept] IN (
				N'PayrollExtension', N'HRExtension',
				N'ShorttermEmployeeBenefitsAccruals'
			)
		)
		GROUP BY E.LineId
		HAVING COUNT(DISTINCT E.AgentId) = 1
	) T
	ON L.[Id] = T.[LineId]
	WHERE (L.EmployeeId IS NULL OR T.EmployeeId IS NULL OR L.EmployeeId <> T.EmployeeId)
	AND Id IN (SELECT DISTINCT LineId FROM Inserted)
END
GO

CREATE TRIGGER dbo.trad_Entries ON [dbo].[Entries]
AFTER DELETE
AS 
BEGIN
	-- Customer or Sales Invoice
	UPDATE L
	SET L.CustomerId = T.CustomerId
	FROM dbo.Lines L
	LEFT JOIN (
		SELECT E.LineId, MIN(E.AgentId) As CustomerId, COUNT(DISTINCT E.AgentId) As CustomerCount
		FROM Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE LineId IN (SELECT DISTINCT LineId FROM Deleted)
		AND (
			AC.[Concept] IN (
				N'CRMExtension',
				N'NoncurrentTradeReceivables', N'NoncurrentReceivablesDueFromRelatedParties',
				N'NoncurrentReceivablesFromSaleOfProperties', N'NoncurrentReceivablesFromRentalOfProperties',
				N'CurrentTradeReceivables', N'TradeAndOtherCurrentReceivablesDueFromRelatedParties',
				N'CurrentReceivablesFromRentalOfProperties'
			)
		)
		GROUP BY E.LineId
		HAVING COUNT(DISTINCT E.AgentId) = 1
	) T
	ON L.[Id] = T.[LineId]
	WHERE (L.CustomerId IS NULL OR T.CustomerId IS NULL OR L.CustomerId <> T.CustomerId)
	AND Id IN (SELECT DISTINCT LineId FROM Deleted)
	-- Supplier or Purchase Invoice
	UPDATE L
	SET L.SupplierId = T.SupplierId
	FROM dbo.Lines L
	LEFT JOIN (
		SELECT E.LineId, MIN(E.AgentId) As SupplierId, COUNT(DISTINCT E.AgentId) As SupplierCount
		FROM Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE LineId IN (SELECT DISTINCT LineId FROM Deleted)
		AND (
			AC.[Concept] IN (
				N'NoncurrentPayablesToTradeSuppliers', N'NoncurrentPayablesToRelatedParties',
				N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'TradeAndOtherCurrentPayablesToRelatedParties'
			)
		)
		GROUP BY E.LineId
		HAVING COUNT(DISTINCT E.AgentId) = 1
	) T
	ON L.[Id] = T.[LineId]
	WHERE (L.SupplierId IS NULL OR T.SupplierId IS NULL OR L.SupplierId <> T.SupplierId)
	AND Id IN (SELECT DISTINCT LineId FROM Deleted)
	--- Employee
		UPDATE L
	SET L.EmployeeId = T.EmployeeId
	FROM dbo.Lines L
	LEFT JOIN (
		SELECT E.LineId, MIN(E.AgentId) As EmployeeId, COUNT(DISTINCT E.AgentId) As EmployeeCount
		FROM Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE LineId IN (SELECT DISTINCT LineId FROM Deleted)
		AND (
			AC.[Concept] IN (
				N'PayrollExtension', N'HRExtension',
				N'ShorttermEmployeeBenefitsAccruals'
			)
		)
		GROUP BY E.LineId
		HAVING COUNT(DISTINCT E.AgentId) = 1
	) T
	ON L.[Id] = T.[LineId]
	WHERE (L.EmployeeId IS NULL OR T.EmployeeId IS NULL OR L.EmployeeId <> T.EmployeeId)
	AND Id IN (SELECT DISTINCT LineId FROM Deleted)
END
GO