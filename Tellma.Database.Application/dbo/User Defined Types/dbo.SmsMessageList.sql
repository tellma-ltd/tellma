CREATE TYPE [dbo].[SmsMessageList] AS TABLE (
	[Index]				INT					PRIMARY KEY DEFAULT 0,
	[ToPhoneNumber]				NVARCHAR (15),
	[Message]					NVARCHAR (1600),
	[State]						SMALLINT,
	[ErrorMessage]				NVARCHAR (2048)
);