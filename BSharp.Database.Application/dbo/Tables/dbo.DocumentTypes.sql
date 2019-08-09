CREATE TABLE [dbo].[DocumentTypes] (
-- table managed by Banan
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						NVARCHAR (50) PRIMARY KEY,
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
	[IsSourceDocument]			BIT				DEFAULT (1), -- <=> IsVoucherReferenceRequired
	[Description]				NVARCHAR (255),
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255),
	-- UI Specs
	[SortKey]					DECIMAL (9,4),
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT (3), -- For presentation purposes
	[DefaultVoucherTypeId]		NVARCHAR (30),
	[CustomerLabel]				NVARCHAR (50),
	[SupplierLabel]				NVARCHAR (50),
	[EmployeeLabel]				NVARCHAR (50),
	[FromCustodyAccountLabel]	NVARCHAR (50),
	[ToCustodyAccountLabel]		NVARCHAR (50)
);
GO;