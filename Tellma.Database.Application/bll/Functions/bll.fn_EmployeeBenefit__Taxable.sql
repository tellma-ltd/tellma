﻿CREATE FUNCTION bll.[fn_EmployeeBenefit__Taxable](
-- The problem with this approach is in SD, the EIT is applied on taxable pool after SS deduction.
-- Also, in some countries, there is no EIT. So, we better remove this from the logic altogether.
	@ResourceId INT,
	@MonetaryValue DECIMAL (19, 4),
	@CurrencyId NCHAR (3),
	@PeriodEnding DATE,
	@BasicSalary DECIMAL (19,4) -- In functional
)
RETURNS DECIMAL (19, 4)
AS
BEGIN
	DECLARE @ResourceCode NVARCHAR (50), @AmountExempt DECIMAL (19,4), @BasicPercentExempt DECIMAL (19,4);
	SELECT @ResourceCode = [Code], @AmountExempt = ISNULL([Decimal1], 0), @BasicPercentExempt = ISNULL([Decimal2], 0)
	FROM dbo.Resources
	WHERE [Id] = @ResourceId;

	DECLARE @E TINYINT = (SELECT [E] FROM dbo.[Currencies] WHERE [Id] = @CurrencyId);

	DECLARE @Exemption DECIMAL (19,4) = ROUND (
		IIF(@AmountExempt > 0, @AmountExempt,  @BasicPercentExempt * ISNULL(@BasicSalary, 0) / 100.0),
		@E);

	DECLARE @AmountInResourceCurrency DECIMAL (19,4) =
		[bll].[fn_ConvertToFunctional](@PeriodEnding, @CurrencyId, ABS(@MonetaryValue)); -- deductions come negative

	DECLARE @TaxableAmount DECIMAL (19,4) = IIF(@Exemption > @AmountInResourceCurrency, 0, @AmountInResourceCurrency - @Exemption)

	RETURN @TaxableAmount * SIGN(@MonetaryValue);
END;