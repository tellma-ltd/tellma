/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

--    :r .\PredeploymentScripts\Go.sql
DROP FUNCTION IF EXISTS [bll].[ft_LD_PaymentFromCash__Preprocess];
DROP FUNCTION IF EXISTS [bll].[ft_LD_PaymentToExpenseWithInvoice__Preprocess];
DROP FUNCTION IF EXISTS [bll].[ft_LD_ReceiptToCash__Preprocess];
IF EXISTS (
	SELECT 1 
	FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_SCHEMA ='dbo' 
	AND TABLE_NAME='FeaturesFlags'
)
DROP TABLE dbo.FeaturesFlags
