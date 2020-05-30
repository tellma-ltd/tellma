--DECLARE @AccountDesignations dbo.AccountDesignationList;
	-- 0 Set Value, 1 By Contract, 2 By Resource, 3 By Center
	-- 21: By Resource Lookup1 22: By Resource Lookup1 and Contract Id
	-- By Center, By Currency, 
INSERT INTO dbo.[AccountDesignations]([Id],[ShowOCE],[MapFunction],
	[Code],						[Name]) VALUES
(8,1,1,N'supplier',				N'Supplier account'),
(9,1,1,N'customer',				N'Customer account'),
(10,1,1,N'employee',			N'Employee account'),
(11,1,1,N'creditor',			N'Creditor account'),
(12,1,1,N'debtor',				N'Debtor account'),
(17,1,1,N'partner',				N'Partner account'),
(20,1,3,N'purchase-expense',	N'Purchase expenses account'),-- materials and services
(24,1,0,N'exchange-gain-loss',	N'Exchange Loss (Gain) account'),
(25,1,0,N'exchange-variance',	N'Exchange Variance account')
;