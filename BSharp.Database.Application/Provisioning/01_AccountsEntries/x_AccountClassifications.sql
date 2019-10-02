-- Provision a chart of accounts that meet local regulatory requirements
IF NOT EXISTS(SELECT * FROM dbo.Accounts)
BEGIN
	IF @ChartOfAccounts = N'US-GAAP'
	BEGIN
		-- Add US chart of accounts
		RETURN;
	END
	ELSE IF @ChartOfAccounts = N'UK-GAAP'
	BEGIN
		-- Add US chart of accounts
		RETURN;
	END
	ELSE IF @ChartOfAccounts = N'OHADA'
	BEGIN
		RETURN;
	END
END