/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
DECLARE @CBEUSD INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT;

INSERT INTO dbo.Accounts([Name], [Code], [IfrsClassificationId]) VALUES
(N'CBE - USD', N'1101', N'BalancesWithBanks'),
(N'CBE - LC', N'1102', N'BalancesWithBanks'), -- reserved money to pay for LC when needed
(N'TF1903950009', N'1209', N'CurrentInventoriesInTransit'), -- Merchandise in transit, for given LC

(N'Capital - MA', N'3101', N'IssuedCapital'),
(N'Capital - AA', N'3102', N'IssuedCapital');


SELECT @CBEUSD = [Id] FROM dbo.Accounts WHERE Code = N'1101';
SELECT @CBELC = [Id] FROM dbo.Accounts WHERE Code = N'1102';
SELECT @ESL =  [Id] FROM dbo.Accounts WHERE Code = N'1209';

SELECT @CapitalMA = [Id] FROM dbo.Accounts WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.Accounts WHERE Code = N'3102';