CREATE TABLE [dbo].[ResourceTypes] (
-- table managed by Banan
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						NVARCHAR (255) PRIMARY KEY,
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
	[Code]						NVARCHAR (255)	NOT NULL,
	[Description]				NVARCHAR (255)	NOT NULL,
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255),
	-- UI Specs
	[DefaultVoucherTypeId]		NVARCHAR (255),

	[FromCustodyAccountLabel]	NVARCHAR (50),
	[ToCustodyAccountLabel]		NVARCHAR (50)
);
GO;