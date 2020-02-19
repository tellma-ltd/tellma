	DECLARE @LineDefinitions dbo.LineDefinitionList;
	DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
	DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
	DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];

		-- Cash, Warehouse, Supplier, Customer


	-- TODO: this is still unfinished
	-- consumable with invoice
	--	durables with invoice
	-- consumables without invoice
	-- durables without invoice
	-- Mise In use (fixed asset)
	-- inventory receipt with invoice (inventory item)
	-- inventory receipt without invoice (inventory item)
	-- inventory transfer (inventory item)
	-- stock issue (inventory item)
	-- fuel consumption (fixed asset)
	-- payroll voucher (employee)
	-- overtime voucher (employee)
	-- etc...