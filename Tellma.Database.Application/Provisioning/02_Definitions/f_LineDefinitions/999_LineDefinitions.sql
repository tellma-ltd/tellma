EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DECLARE @ManualLineDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @GoodReceiptDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'GoodReceipt');
DECLARE @PurchaseExpenseDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PurchaseExpense');

DECLARE @PaymentToSupplierDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToSupplier');
DECLARE @PaymentToOtherDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToOther');
DECLARE @CashTransferExchangeDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');

DECLARE @LeaseInDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'LeaseIn');

DECLARE @PaymentFromCustomerDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromCustomer');
DECLARE @PaymentFromOtherDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromOther');

DECLARE @ServiceDeliveryDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceDelivery');
DECLARE @LeaseOutDef INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'LeaseOut');


/*
-- how to make sure that the document is not closed until the control account has zero balance
C_PaymentToSupplier
C_GoodReceiptNote
C_CSReceiptNote
C_PPPReceiptNote
 
then similar version without C_ PaymentToSupplier, GoodReceiptNote, ...

C_PaymentFromCustomer
C_GoodDeliveryNote
C_ServiceDeliveryNote

LeaseInReceiptNote
LeaseOutDeliveryNote

0-9: Cash purchase
11-19: Cash payments
21-29: Goods receipts, service receipts, Lease in
31-39: Cash sale
41-49: cash receipt
51-59: Goods delivery, service delivery, Lease out
61-69: employees payroll/
71-79: machines

*/
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Line Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;