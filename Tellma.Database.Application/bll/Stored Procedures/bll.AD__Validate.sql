CREATE PROCEDURE [bll].[AD__Validate]
	@Entities AgentList READONLY,
	@Top INT
AS
DECLARE @ValidationErrors ValidationErrorList;
DECLARE @ErrorNames dbo.ErrorNameList;
SET NOCOUNT ON;
INSERT INTO @ErrorNames([ErrorIndex], [Language], [ErrorName]) VALUES
(0, N'en',  N'Code cannot be changed'), (0, N'ar',  N'لا يمكن تغيير الكود'),
(1, N'en',  N'Code is required'), (1, N'ar',  N'الكود إلزامي'),
(2, N'en',  N'Resp. Center must be a leaf'), (2, N'ar',  N'مركز المسؤولية لا بد أن يكون في أسفل الهيكل التنظيمي');

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 0) AS ErrorMessage
FROM @Entities FE
JOIN dbo.Agents BE ON BE.[Id] = FE.[Id]
WHERE FE.[Code] <> BE.[Code];

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].Code',
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 1) AS ErrorMessage
FROM @Entities FE
WHERE FE.[Code] IS NULL;

INSERT INTO @ValidationErrors([Key], [ErrorName])
SELECT DISTINCT TOP (@Top)
	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].CenterId',
	dal.fn_ErrorNames_Index___Localize(@ErrorNames, 2) AS ErrorMessage
FROM @Entities FE
JOIN dbo.Centers C ON C.[Id] = FE.[CenterId]
WHERE C.[IsLeaf] = 0

SELECT * FROM @ValidationErrors;