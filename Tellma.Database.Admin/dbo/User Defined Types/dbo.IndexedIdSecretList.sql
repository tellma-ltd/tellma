CREATE TYPE [dbo].[IndexedIdSecretList] AS TABLE (
	[Index]	INT PRIMARY KEY,
	[Id]	INT NOT NULL,
	[ClientSecret]	NVARCHAR(255) NOT NULL
);