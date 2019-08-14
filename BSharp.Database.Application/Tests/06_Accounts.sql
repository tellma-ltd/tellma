DECLARE @CBEUSD INT, @CapitalMA INT, @CapitalAA INT;

INSERT INTO dbo.Accounts([Name], [Code], [IfrsAccountId]) VALUES
(N'CBE - USD', N'1101', N'BalancesWithBanks'),
(N'Capital - MA', N'3101', N'IssuedCapital'),
(N'Capital - AA', N'3102', N'IssuedCapital');


SELECT @CBEUSD = [Id] FROM dbo.Accounts WHERE Code = N'1101';
SELECT @CapitalMA = [Id] FROM dbo.Accounts WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.Accounts WHERE Code = N'3102';