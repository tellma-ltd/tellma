EXEC tSQLt.NewTestClass @ClassName = N'[Lookups]'
 EXEC tSQLt.DropClass @ClassName = N'[Lookups]'
 GO

CREATE SCHEMA [Lookups]
GO
EXECUTE sp_addextendedproperty
	@name = N'tSQLt.TestClass',
	@value = 1, @level0type = N'SCHEMA', @level0name = N'Lookups';