CREATE FUNCTION [dbo].[fi_Account__Balances] ( -- SELECT * FROM dbo.[fi_Account__Balances](7)
	@AccountId INT 
) RETURNS TABLE
AS
RETURN
	SELECT
		[ResponsibilityCenterId], [AgentAccountId], [ResourceId], [ExpectedSettlingDate], 
		[Quantity], [MoneyAmount], [Mass], [Volume], [Count], [Time], [Value]
	FROM dbo.AccountsBalancesView
	WHERE AccountId = @AccountId;
GO;