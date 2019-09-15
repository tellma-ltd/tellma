CREATE TABLE [dbo].[DocumentDefinitions] (
-- table managed by Banan
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						NVARCHAR (50) PRIMARY KEY, -- Kebab case
-- The choice of booleans should form a connected tree. For example, in Cut to Size, and
-- assuming that the B# document is not a source document, the true values are: 
-- (starting) (Draft), IsPosted, IsDeclined.
-- The list of possible states can be also deduced from the workflow (ToState).
/*
	[IsRequestedOrVoid]			BIT				DEFAULT (1),
	[IsAuthorizedOrRejected]	BIT				DEFAULT (1),
	[IsCompletedOrFailed]		BIT				DEFAULT (1),
	[IsPostedOrInvalid]			BIT				DEFAULT (1),
*/
	[IsSourceDocument]			BIT				DEFAULT 1, -- <=> IsVoucherReferenceRequired
	[FinalState]				NVARCHAR (30)	NOT NULL DEFAULT N'Posted',
	[DescriptionSingular]		NVARCHAR (255),
	[DescriptionSingular2]		NVARCHAR (255),
	[DescriptionSingular3]		NVARCHAR (255),
	[DescriptionPlural]			NVARCHAR (255),
	[DescriptionPlural2]		NVARCHAR (255),
	[DescriptionPlural3]		NVARCHAR (255),

	[IsImmutable]				BIT				NOT NULL DEFAULT 0, -- 1 <=> Cannot change without invalidating signatures
	-- UI Specs
	[SortKey]					DECIMAL (9,4),
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[NumericalLength]			TINYINT			DEFAULT 3, -- For presentation purposes
	[CustomerLabel]				NVARCHAR (50),
	[SupplierLabel]				NVARCHAR (50),
	[EmployeeLabel]				NVARCHAR (50),
	[FromCustodyAccountLabel]	NVARCHAR (50),
	[ToCustodyAccountLabel]		NVARCHAR (50)
);
GO;