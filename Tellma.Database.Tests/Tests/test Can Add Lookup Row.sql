CREATE PROCEDURE [Lookups].[test can add lookup row]
AS
 --Assemble
  declare @expected dbo.LookupList = 'Lookup 1'
 
  EXEC tSQLt.FakeTable @TableName = 'Lookups', @Identity = 1;
  EXEC tSQLt.FakeTable @TableName = 'LookupDefinitions', @Identity = 0
 
 INSERT INTO dbo.LookupDefinitions([Id], [TitlePlural], [TitleSingular])
 VALUES(1, N'Colours', N'Color');
  --Act
  DECLARE @Colours dbo.LookupList;
  INSERT INTO @Colours([Index], [Id], [Name]) VALUES(0, 0, N'Red'),(0, 0, N'Green'),(0, 0, N'Blue');
  exec api.Lookups__Save
	@DefinitionId = 1,
	@Entities = @Colours,
	@ReturnIds = 0,
	@ValidateOnly = 0,
	@Top = 100,
	@UserId = 1;

  declare @actual dbo.LookupList;-- @Colours ;-- INSER= (SELECT * from [audit_log])
 
  --Assert
  exec tSQLt.AssertEqualsTable @expected = @expected, @actual = @actual, @message = 'Audit message didn''t match expected result'
