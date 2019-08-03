CREATE PROCEDURE [dbo].[ui_Documents_Lines_Entries__Json]
	@Json NVARCHAR(MAX)
AS
	SELECT * FROM [dbo].[fw_Documents__Json] (@Json)
	SELECT * FROM [dbo].[fw_TransactionLines__Json] (@Json)
	SELECT * FROM [dbo].[fw_TransactionEntries__Json] (@Json)
RETURN 0;