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
		[DocumentDate] DATETIME2 (7) '$.DocumentDate',
		[DocumentState] NVARCHAR (255) '$.DocumentState',
		[SerialNumber] INT '$.SerialNumber',
		[Reference] NVARCHAR (255) '$.Reference',
		[Memo] NVARCHAR (255) '$.Memo',

		[TransactionType] NVARCHAR (255) '$.TransactionType',
		[Frequency] NVARCHAR (255) '$.Frequency',
		[Repetitions] INT '$.Repetitions',
		[EndDate] DATETIME2 (7) '$.EndDate',

		[EntityState] NVARCHAR (255) '$.EntityState'
	);