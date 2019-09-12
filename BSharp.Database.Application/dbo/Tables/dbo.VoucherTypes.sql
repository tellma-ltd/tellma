CREATE TABLE [dbo].[VoucherTypes](
	[Id]						INT				PRIMARY KEY IDENTITY,
	[Name]						NVARCHAR (255)	NOT NULL,
	[Name2]						NVARCHAR (255),
	[Name3]						NVARCHAR (255),
	[Code]						NVARCHAR (255),
    [IsActive]					BIT				DEFAULT 1
);
GO;