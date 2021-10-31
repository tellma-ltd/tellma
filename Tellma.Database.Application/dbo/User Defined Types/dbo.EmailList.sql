CREATE TYPE [dbo].[EmailList] AS TABLE (
	[Index]						INT					PRIMARY KEY DEFAULT 0,
	[To]						NVARCHAR (2048),
	[Cc]						NVARCHAR (2048),
	[Bcc]						NVARCHAR (2048),
	[Subject]					NVARCHAR (1024),
	[BodyBlobId]				NVARCHAR (50),
	[State]						SMALLINT,
	[ErrorMessage]				NVARCHAR (2048)
);