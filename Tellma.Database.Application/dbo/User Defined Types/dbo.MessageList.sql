CREATE TYPE [dbo].[MessageList] AS TABLE (
	[Index]				INT					PRIMARY KEY DEFAULT 0,
	[PhoneNumber]				NVARCHAR (15),
	[Content]					NVARCHAR (1600),
	[State]						SMALLINT,
	[ErrorMessage]				NVARCHAR (2048)
);