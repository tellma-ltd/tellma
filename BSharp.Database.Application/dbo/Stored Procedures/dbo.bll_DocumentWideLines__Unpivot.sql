CREATE PROCEDURE [dbo].[bll_DocumentWideLines__Unpivot]
		@WideLines dbo.[DocumentWideLineList] READONLY
AS
	DECLARE @Lines dbo.[DocumentLineList];
	DECLARE @Entries dbo.DocumentLineEntryList;

	INSERT INTO @Lines(
		[Id], [DocumentIndex], [DocumentId], [LineTypeId], [TemplateLineId], [ScalingFactor]
		)
	SELECT 
		[Id], [DocumentIndex], [DocumentId], [LineTypeId], [TemplateLineId], [ScalingFactor]
	FROM @WideLines;

	INSERT INTO @Entries(
	[DocumentLineIndex], [DocumentIndex], [Id],
	[DocumentLineId], [EntryNumber], [Direction], [AccountId], [IfrsEntryClassificationId],
	[ResourceId], [ResourcePickId], [BatchCode], [DueDate], [Quantity],
	[MonetaryValue], [Mass], [Volume], [Area], [Length], [Time], [Count], [Value], [Memo],
	[ExternalReference], [AdditionalReference], [RelatedResourceId], [RelatedAgentId],
	[RelatedQuantity], [RelatedMoneyAmount])
	SELECT
	[DocumentLineIndex], [DocumentIndex], [Id],
	[DocumentLineId], 1, [Direction1], [AccountId1], [IfrsEntryClassificationId1],
	[ResourceId1], [InstanceId1], [BatchCode1], [DueDate1], [Quantity1],
	[MoneyAmount1], [Mass1], [Volume1], [Area1], [Length1], [Time1], [Count1], [Value1], [Memo1],
	[ExternalReference1], [AdditionalReference1], [RelatedResourceId1], [RelatedAgentId1],
	[RelatedQuantity1], [RelatedMoneyAmount1]
	FROM @WideLines
	UNION
	SELECT
	[DocumentLineIndex], [DocumentIndex], [Id],
	[DocumentLineId], 2, [Direction2], [AccountId2], [IfrsEntryClassificationId2],
	[ResourceId2], [InstanceId2], [BatchCode2], [DueDate2], [Quantity2],
	[MoneyAmount2], [Mass2], [Volume2], [Area2], [Length2], [Time2], [Count2], [Value2], [Memo2],
	[ExternalReference2], [AdditionalReference2], [RelatedResourceId2], [RelatedAgentId2],
	[RelatedQuantity2], [RelatedMoneyAmount2]
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
						AND EI.DocumentLineIndex = E.DocumentLineIndex
					)
				WHEN LTS.AccountIdExpression = N'Resource.ExpenseAccountId' THEN (
						SELECT R.ExpenseAccountId
						FROM @Entries EI
						JOIN dbo.Resources R ON EI.ResourceId = R.Id
						WHERE EI.EntryNumber = LTS.ResourceIdEntryNumber
						AND EI.DocumentLineIndex = E.DocumentLineIndex
					)
				WHEN LTS.AccountIdExpression = N'Resource.RevenueAccountId' THEN (
						SELECT R.RevenueAccountId
						FROM @Entries EI
						JOIN dbo.Resources R ON EI.ResourceId = R.Id
						WHERE EI.EntryNumber = LTS.ResourceIdEntryNumber
						AND EI.DocumentLineIndex = E.DocumentLineIndex
					)
				ELSE E.AccountId END,
			E.Quantity = CASE 
				WHEN LTS.QuantityExpression = N'Constant' THEN LTS.Quantity 
				WHEN LTS.QuantityExpression = N'Quantity' THEN (
						SELECT Quantity FROM @Entries EI
						WHERE EI.EntryNumber = LTS.QuantityEntryNumber
						AND EI.DocumentLineIndex = E.DocumentLineIndex
					)
				WHEN LTS.QuantityExpression = N'Net' THEN (
						SELECT ABS(SUM([Direction] * [Quantity])) FROM @Entries EI
						WHERE EI.EntryNumber <> LTS.QuantityEntryNumber
						AND EI.DocumentLineIndex = E.DocumentLineIndex
					)
				ELSE E.Quantity END
		FROM @Entries E
		JOIN @Lines L ON E.[DocumentLineIndex] = L.[Index]
		JOIN dbo.LineTypesSpecifications LTS
		ON L.[LineTypeId] = LTS.[LineTypeId] AND E.EntryNumber = LTS.EntryNumber
		WHERE E.EntryNumber = @EntryNumber;

		SET @EntryNumber = @EntryNumber	 + 1;
	END

	UPDATE E
	SET
		E.Mass = E.[Quantity] * R.UnitMass,
		E.Volume = E.[Quantity] * R.UnitVolume
	FROM @Entries E JOIN dbo.Resources R ON E.ResourceId = R.Id
	WHERE R.[UnitId] = R.[CountUnitId];