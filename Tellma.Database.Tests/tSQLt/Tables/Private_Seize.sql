CREATE TABLE [tSQLt].[Private_Seize] (
    [Kaput] BIT NOT NULL,
    CONSTRAINT [Private_Seize:PK] PRIMARY KEY CLUSTERED ([Kaput] ASC),
    CONSTRAINT [Private_Seize:CHK] CHECK ([Kaput]=(1))
);


GO
CREATE TRIGGER tSQLt.Private_Seize_Stop ON tSQLt.Private_Seize INSTEAD OF DELETE,UPDATE
AS
BEGIN 
  RAISERROR('This is a private table that you should not mess with!',16,10);
END;
