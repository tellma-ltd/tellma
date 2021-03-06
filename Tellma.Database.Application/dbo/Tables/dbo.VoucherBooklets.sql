﻿CREATE TABLE [dbo].[VoucherBooklets] (
-- This is relevant when the document type is a copy, not a source document.
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different voucher types not 3 booklets.
	[Id]				INT				CONSTRAINT [PK_VoucherBooklets] PRIMARY KEY,
	[VoucherTypeId]		INT				CONSTRAINT [FK_VoucherBooklets__VoucherTypeId] REFERENCES [dbo].[VoucherTypes] ([Id]) ON UPDATE CASCADE, 
	[Name]				NVARCHAR (255)	NOT NULL, -- Default : Voucher Type : StringPrefix + RangeStarts - StringPrefix + RangeEnds
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[StringPrefix]		NVARCHAR (10)	NOT NULL DEFAULT (N''), -- visible for IsSourceDocument = 0
	[NumericLength]		TINYINT			DEFAULT 0, -- If 0, then no padding zeroes. Visible for IsSourceDocument = 0
	[RangeStarts]		INT				DEFAULT 1,
	[RangeEnds]			INT				DEFAULT 2147483647,
	[IsActive]			BIT				DEFAULT 1,
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_VoucherBooklets__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_VoucherBooklets__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO;