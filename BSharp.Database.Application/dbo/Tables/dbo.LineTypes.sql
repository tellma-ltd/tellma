CREATE TABLE [dbo].[LineTypes] (
	[Id]						NVARCHAR (255) PRIMARY KEY,
	[Description]				NVARCHAR (255),
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255),
	[CustomerLabel]				NVARCHAR (255),
	[SupplierLabel]				NVARCHAR (255),
	[EmployeeLabel]				NVARCHAR (255),
	[FromCustodyAccountLabel]	NVARCHAR (255),
	[ToCustodyAccountLabel]		NVARCHAR (255),
);