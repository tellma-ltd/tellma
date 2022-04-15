CREATE TABLE [dbo].[FeatureFlags]
(
	[FeatureCode]	NVARCHAR (255)	CONSTRAINT [PK_FeatureFlags] PRIMARY KEY,
	[IsEnabled]		BIT				NOT NULL  DEFAULT (1)
)