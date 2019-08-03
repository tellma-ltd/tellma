CREATE TABLE [dbo].[VoucherBooklets] (
-- This is relevant when the document type is a copy, not a source document.
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types not 3 booklets.
-- When different booklets of the same type have different specification, such as a specific site
-- ot a specific set of raw materials, we assign a specification code for the voucher range.
	[Id]						INT PRIMARY KEY,
	[VoucherTypeId]				NVARCHAR (255),
--	[Specification]				NVARCHAR (255)	NOT NULL DEFAULT (N''), -- multiple booklets may share the same specification, e.g., Expansion
	[StringPrefix]				NVARCHAR (255)	NOT NULL DEFAULT (N''), -- visible for IsSourceDocument = 0
	[NumericLength]				INT				DEFAULT (0), -- If 0, then no padding zeroes. Visible for IsSourceDocument = 0
	[RangeStarts]				INT				DEFAULT (1),
	[RangeEnds]					INT				DEFAULT (2147483647),
	[IsActive]					BIT				DEFAULT (1),
	CONSTRAINT [FK_VoucherBooklets__VoucherTypeId] FOREIGN KEY ([VoucherTypeId]) REFERENCES [dbo].[VoucherTypes] ([Id]) ON UPDATE CASCADE, 
);
GO;