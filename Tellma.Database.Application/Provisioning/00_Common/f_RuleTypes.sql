IF NOT EXISTS (SELECT * FROM dbo.RuleTypes)
INSERT INTO dbo.RuleTypes([RuleType]) VALUES
(N'ByAgent'),
(N'ByRole'),
(N'ByUser'),
(N'Public');

IF NOT EXISTS (SELECT * FROM dbo.PredicateTypes)
INSERT INTO dbo.PredicateTypes([PredicateType]) VALUES
(N'ValueAtLeast');