CREATE TABLE [dbo].[Keys] (
    [Id]                NVARCHAR (450) NOT NULL,
    [Version]           INT            NOT NULL,
    [Created]           DATETIME2      NOT NULL,
    [Use]               NVARCHAR (450) NULL,
    [Algorithm]         NVARCHAR (100) NOT NULL,
    [IsX509Certificate] BIT            NOT NULL,
    [DataProtected]     BIT            NOT NULL,
    [Data]              NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_Keys] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Keys_Use]
    ON [dbo].[Keys]([Use] ASC);
