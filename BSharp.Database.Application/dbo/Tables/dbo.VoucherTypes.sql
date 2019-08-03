CREATE TABLE [dbo].[VoucherTypes](
-- table managed by Banan, except for the VoucherPrefix and  column
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						NVARCHAR (255) PRIMARY KEY,
	[Description]				NVARCHAR (255)	NOT NULL,
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255),
	[CustomerLabel]				NVARCHAR (50),
	[SupplierLabel]				NVARCHAR (50),
	[EmployeeLabel]				NVARCHAR (50),
	[FromCustodyAccountLabel]	NVARCHAR (50),
	[ToCustodyAccountLabel]		NVARCHAR (50)
);
GO;