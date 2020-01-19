CREATE PROCEDURE [rpt].[Ifrs_520000]
--[520000] Statement of cash flows, indirect method
	@fromDate DATE, 
	@toDate DATE
AS
-- Apparently, there is a mapping between our AccountType and the concepts in that report
-- It does require thinking to see if we can get them all this way
-- Otherwise, the only option is to map the ACCOUNTS to the concepts in 520000
SELECT 1