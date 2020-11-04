CREATE TYPE [dbo].[EmailList] AS TABLE (
	[Index]						INT					PRIMARY KEY DEFAULT 0,
	[ToEmail]					NVARCHAR (256),
	[Subject]					NVARCHAR (1024),
	[Body]						NVARCHAR (MAX),
	[State]						SMALLINT,
	[ErrorMessage]				NVARCHAR (2048)
);