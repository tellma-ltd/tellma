-- Rules for signing
INSERT INTO dbo.RuleTypes([RuleType]) VALUES
(N'ByContract'),
(N'ByRole'),
(N'ByUser'),
(N'Public'); 

INSERT INTO dbo.PredicateTypes([PredicateType]) VALUES
(N'ValueGreaterOrEqual');