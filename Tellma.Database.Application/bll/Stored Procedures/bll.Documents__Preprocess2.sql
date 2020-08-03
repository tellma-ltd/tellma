/*
			UPDATE E
			SET E.CurrencyId = BE.CurrencyId
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'CurrencyId';
			UPDATE E
			SET E.[CustodyId] = BE.[CustodyId]
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'CustodyId';
			UPDATE E
			SET E.ResourceId = BE.ResourceId
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'ResourceId';
			UPDATE E
			SET E.CenterId = BE.CenterId
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'CenterId';
			UPDATE E
			SET E.EntryTypeId = BE.EntryTypeId
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'EntryTypeId';
			UPDATE E
			SET E.MonetaryValue = BE.MonetaryValue
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'MonetaryValue';
			UPDATE E
			SET E.Quantity = BE.Quantity
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'Quantity';
			UPDATE E
			SET E.UnitId = BE.UnitId
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'UnitId';
			UPDATE E
			SET E.Time1 = BE.Time1
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'Time1';
			UPDATE E
			SET E.Time2 = BE.Time2
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'Time2';
			UPDATE E
			SET E.ExternalReference = BE.ExternalReference
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'ExternalReference';
			UPDATE E
			SET E.AdditionalReference = BE.AdditionalReference
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'AdditionalReference';
			UPDATE E
			SET E.[NotedRelationId] = BE.[NotedRelationId]
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'NotedRelationId';
			UPDATE E
			SET E.NotedAgentName = BE.NotedAgentName
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'NotedAgentName';
			UPDATE E
			SET E.NotedAmount = BE.NotedAmount
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'NotedAmount';
			UPDATE E
			SET E.NotedDate = BE.NotedDate
			FROM @E E
			JOIN dbo.Entries BE ON E.Id = BE.Id
			JOIN dbo.Lines BL ON BE.[LineId] = BL.[Id]
			JOIN dbo.LineDefinitionColumns LDC ON BL.DefinitionId = LDC.LineDefinitionId AND LDC.[EntryIndex] = BE.[Index]
			WHERE (LDC.ReadOnlyState <= BL.[State] OR BL.[State] < 0)
			AND LDC.ColumnName = N'NotedDate';
*/