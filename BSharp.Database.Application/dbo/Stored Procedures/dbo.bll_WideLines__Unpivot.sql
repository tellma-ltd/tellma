CREATE PROCEDURE [dbo].[bll_WideLines__Unpivot]
		@WideLines dbo.[WideLineList] READONLY
AS
	DECLARE @Lines dbo.[LineList];
	DECLARE @Entries dbo.EntryList;

	INSERT INTO @Lines(
		[Id], [DocumentIndex] --[DocumentId], [LineDefinitionId], [TemplateLineId], [ScalingFactor]
		)
	SELECT 
		[Id], [DocumentIndex] --[DocumentId], [LineDefinitionId] , [TemplateLineId], [ScalingFactor]
	FROM @WideLines;
	/*
	INSERT INTO @Entries(
	[LineIndex], [DocumentIndex], [Id],
	--[LineId],
	[EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId],
	[ResourceId], [ResourceInstanceId], [BatchCode], [DueDate],
	[MonetaryValue], [Mass], [Volume], [Area], [Length], [Time], [Count], [Value])
	SELECT
	[LineIndex], [DocumentIndex], [Id],
	--[LineId],
	1, [Direction1], [AccountId1], [IfrsEntryClassificationId1],
	[ResourceId1], [InstanceId1], [BatchCode1], [DueDate1],
	[DECIMAL (19,4)Amount1], [Mass1], [Volume1], [Area1], [Length1], [Time1], [Count1], [Value1]
	FROM @WideLines
	UNION
	SELECT
	[LineIndex], [DocumentIndex], [Id],
--	[LineId], 
	2, [Direction2], [AccountId2], [IfrsEntryClassificationId2],
	[ResourceId2], [InstanceId2], [BatchCode2], [DueDate2],
	[DECIMAL (19,4)Amount2], [Mass2], [Volume2], [Area2], [Length2], [Time2], [Count2], [Value2]
	FROM @WideLines

	-- Assuming there is no circular dependency in the logic (needs a way to test it)
	DECLARE @EntryNumber INT = 1
	WHILE @EntryNumber < 6
	BEGIN
		UPDATE E
		SET
			E.Direction = CASE
				WHEN LTS.DirectionExpression = N'Constant' THEN LTS.Direction 
				ELSE E.Direction END,
			E.AccountId = CASE 
				WHEN LTS.AccountIdExpression = N'Account' THEN (
						SELECT AccountId FROM @Entries EI
						WHERE EI.EntryNumber = LTS.AccountIdEntryNumber
						AND EI.LineIndex = E.LineIndex
					)
				WHEN LTS.AccountIdExpression = N'Resource.ExpenseAccountId' THEN (
						SELECT R.ExpenseAccountId
						FROM @Entries EI
						JOIN dbo.Resources R ON EI.ResourceId = R.Id
						WHERE EI.EntryNumber = LTS.ResourceIdEntryNumber
						AND EI.LineIndex = E.LineIndex
					)
				WHEN LTS.AccountIdExpression = N'Resource.RevenueAccountId' THEN (
						SELECT R.RevenueAccountId
						FROM @Entries EI
						JOIN dbo.Resources R ON EI.ResourceId = R.Id
						WHERE EI.EntryNumber = LTS.ResourceIdEntryNumber
						AND EI.LineIndex = E.LineIndex
					)
				ELSE E.AccountId END
			--E.Quantity = CASE 
			--	WHEN LTS.QuantityExpression = N'Constant' THEN LTS.Quantity 
			--	WHEN LTS.QuantityExpression = N'Quantity' THEN (
			--			SELECT Quantity FROM @Entries EI
			--			WHERE EI.EntryNumber = LTS.QuantityEntryNumber
			--			AND EI.LineIndex = E.LineIndex
			--		)
			--	WHEN LTS.QuantityExpression = N'Net' THEN (
			--			SELECT ABS(SUM([Direction] * [Quantity])) FROM @Entries EI
			--			WHERE EI.EntryNumber <> LTS.QuantityEntryNumber
			--			AND EI.LineIndex = E.LineIndex
			--		)
			--	ELSE E.Quantity END
		FROM @Entries E
		JOIN @Lines L ON E.[LineIndex] = L.[Index]
		JOIN dbo.LineTypesSpecifications LTS
		ON L.[LineDefinitionId] = LTS.[LineDefinitionId] AND E.EntryNumber = LTS.EntryNumber
		WHERE E.EntryNumber = @EntryNumber;

		SET @EntryNumber = @EntryNumber	 + 1;
	END
	*/
	
	-- TODO: find a way to dill the dependent parameters
	--UPDATE E
	--SET
	--	E.Mass = E.[Quantity] * R.UnitMass,
	--	E.Volume = E.[Quantity] * R.UnitVolume
	--FROM @Entries E JOIN dbo.Resources R ON E.ResourceId = R.Id
	--WHERE R.[UnitId] = R.[CountUnitId];