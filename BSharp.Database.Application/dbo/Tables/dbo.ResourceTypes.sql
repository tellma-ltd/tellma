CREATE TABLE [dbo].[ResourceTypes] (
-- flat table, corresponds to leaves of the account classification tree.
	[Id]						NVARCHAR (255) PRIMARY KEY,
	[Code]						NVARCHAR (255)	NOT NULL,
	[Description]				NVARCHAR (255)	NOT NULL,
	[Description2]				NVARCHAR (255),
	[Description3]				NVARCHAR (255),
	-- UI Specs
	[ResourceLookup1Label]		NVARCHAR (255),
	[ResourceLookup1Label2]		NVARCHAR (255),
	[ResourceLookup1Label3]		NVARCHAR (255),
	
	[ResourceLookup2Label]		NVARCHAR (255),
	[ResourceLookup2Label2]		NVARCHAR (255),
	[ResourceLookup2Label3]		NVARCHAR (255),

	[ResourceLookup3Label]		NVARCHAR (255),
	[ResourceLookup3Label2]		NVARCHAR (255),
	[ResourceLookup3Label3]		NVARCHAR (255),

	[ResourceLookup4Label]		NVARCHAR (255),
	[ResourceLookup4Label2]		NVARCHAR (255),
	[ResourceLookup4Label3]		NVARCHAR (255)
);
GO