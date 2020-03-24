CREATE FUNCTION [dbo].[fw_Documents__Json] (
	@Json NVARCHAR(MAX)
)
RETURNS TABLE
AS
RETURN
SELECT *
	FROM OpenJson(@Json)
	WITH (
		[Id] INT '$.Id',
		[PostingDate] DATETIME2 (7) '$.DocumentDate',
		[State] NVARCHAR (255) '$.State',
		[SerialNumber] INT '$.SerialNumber',
		[Reference] NVARCHAR (255) '$.Reference',
		[Memo] NVARCHAR (255) '$.Memo',

		[TransactionType] NVARCHAR (255) '$.TransactionType',
		[Frequency] NVARCHAR (255) '$.Frequency',
		[Repetitions] INT '$.Repetitions',
		[EndDate] DATETIME2 (7) '$.EndDate'
	);