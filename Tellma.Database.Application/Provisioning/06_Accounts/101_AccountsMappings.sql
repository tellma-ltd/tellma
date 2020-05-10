--[AccountDesignationId],
--[MapFunction],
--[CenterId],
--[ContractId],
--[ResourceId],
--[ResourceLookup1Id],
--[CurrencyId],
--[AccountId]
INSERT INTO dbo.AccountMappings([MapFunction],
[AccountDesignationId],		[AccountId]) VALUES
(0,@document_controlADef,	@1DocumentControl),
(0,@vat_receivableADef,		@1VATInput),
(0,@vat_payableADef,		@1VATOutput),
(0,@exchange_gain_lossADef,	@1ExchangeGainLoss),
(0,@exchange_varianceADef,	@1ExchangeVariance)

INSERT INTO dbo.AccountMappings([MapFunction],
[AccountDesignationId], [ContractId],		[AccountId]) VALUES
(1,@cashADef,			@GMSafe,			@1GMFund),
(1,@cashADef,			@AdminPettyCash,	@1AdminPC),
(1,@cashADef,			@KRTBank,			@1BOK),
(1,@supplierADef,		@GenericSupplier,	@1CashSuppliers),
(1,@supplierADef,		@FamilyShawarma,	@1CashSuppliers)

;
/*
@general_BSADef
@general_PLADef
@cashADef
@inventoryADef 
@in_transitADef
@in_progressADef
@ppeADef
@supplierADef
@customerADef
@employeeADef
@creditorADef
@debtorADef
@quant_emp_expenseADef
@revenueADef
@depreciation_expenseADef
@COSADef
@partnerADef
@vat_payableADef

@purchase_expenseADef
@eitaxADef
@estaxADef
*/