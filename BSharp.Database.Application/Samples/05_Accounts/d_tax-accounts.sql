--INSERT INTO dbo.[AccountGroups]
--([Id],				[TitleSingular],	[TitlePlural],	[RelatedAgentVisibility],	[RelatedAgentLabel],			[RelatedAgentRelationDefinitionList],
--					[RelatedMonetaryAmountVisibility],	[RelatedMonetaryAmountLabel],[ExternalReferenceVisibility], [ExternalReferenceLabel]) VALUES
--(N'tax-accounts',	N'Tax Account',		N'Tax Accounts',N'RequiredInEntries',		N'Customer/Supplier',			N'customers,suppliers',
--					N'RequiredInEntries',				N'Taxable Amount',			N'OptionalInEntries',			N'Invoice #');

--DECLARE @TaxAccounts dbo.AccountList;
--INSERT INTO @TaxAccounts([Index],
--	[AccountTypeId],				[AccountClassificationId],	[Name],								[Code]) VALUES
--(13,N'CurrentAssets',				@Debtors_AC,				N'VAT Input',						N'1401'),
--(14,N'CurrentLiabilities',			@Liabilities_AC,			N'VAT Output',						N'2401');

--EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
--	@DefinitionId = N'tax-accounts',
--	@Entities = @TaxAccounts,
--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--IF @ValidationErrorsJson IS NOT NULL 
--BEGIN
--	Print 'Inserting Tax Accounts'
--	GOTO Err_Label;
--END;

--SELECT @VATInput = [Id] FROM dbo.[Accounts] WHERE Code = N'1401';
--SELECT @VATOutput = [Id] FROM dbo.[Accounts] WHERE Code = N'2401';