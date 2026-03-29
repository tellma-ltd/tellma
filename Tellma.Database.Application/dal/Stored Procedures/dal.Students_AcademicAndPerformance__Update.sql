CREATE PROCEDURE [dal].[Students_AcademicAndPerformance__Update]
AS
    WITH AcademicsHistory AS (
        SELECT L.[PostingDate], E.AgentId AS StudentId, E.[NotedAgentId] AS MajorId, D.Lookup2Id AS [DegreeAndYear]
        FROM dbo.Documents D
        JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
        JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
        JOIN dbo.LineDefinitions LD ON LD.[Id] = L.DefinitionId
        JOIN dbo.Entries E ON E.[LineId] = L.[Id]
        WHERE DD.[Code] = N'StudentContractVoucher'
        AND LD.[Code] = N'ToOtherCurrentReceivablesFromRevenues.E'
        AND L.[State] = 4
        AND E.[Index] = 1
    ),
    PerformanceHistory AS (
        SELECT L.[PostingDate], E.AgentId AS StudentId, L.[Text1] AS ScholarshipStatus, L.[Decimal2] AS [CumulativeGPA]
        FROM dbo.Lines L
        JOIN dbo.Entries E ON E.[LineId] = L.[Id]
        JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
        WHERE LD.[Code] = N'StudentGPA.E'
        AND L.[State] = 4
    ),
    ScholarshipStatuses AS (
        SELECT LK.[Id], LK.[Name]
        FROM dbo.Lookups LK
        JOIN dbo.LookupDefinitions LKD ON LKD.[Id] = LK.[DefinitionId]
        WHERE LKD.[Code] = N'ScholarshipStatuses'
    ),
    LatestAcademics AS (
        SELECT *,
            ROW_NUMBER() OVER (PARTITION BY StudentId ORDER BY PostingDate DESC) AS RN
        FROM AcademicsHistory
    ),
    LatestPerformance AS (
        SELECT *,
            ROW_NUMBER() OVER (PARTITION BY StudentId ORDER BY PostingDate DESC) AS RN
        FROM PerformanceHistory
    ),
    AcademicAndPerformanceCurrent AS (
        SELECT
            A.StudentId         AS [StudentId],
            A.MajorId           AS [CurrentMajorId],
            A.DegreeAndYear     AS [CurrentDegreeAndYear],
            SS.[Id]             AS [CurrentScholarshipStatusId],
            P.CumulativeGPA     AS [CurrentCumulativeGPA]
        FROM LatestAcademics A
        LEFT JOIN LatestPerformance P ON P.StudentId = A.StudentId
                                 AND P.RN = 1
        LEFT JOIN ScholarshipStatuses SS ON SS.[Name] = P.ScholarshipStatus
        WHERE A.RN = 1
    ) --  select * from AcademicAndPerformanceCurrent where studentid = 20784
    UPDATE S
    SET
        S.[Agent2Id]   = C.[CurrentMajorId],
        S.[Lookup6Id]  = C.[CurrentDegreeAndYear],
        S.[Lookup8Id]  = ISNULL(C.[CurrentScholarshipStatusId], S.[Lookup8Id]),
        S.[Decimal2]   = ISNULL(C.[CurrentCumulativeGPA],  S.[Decimal2])
    FROM dbo.Agents AS S
    JOIN AcademicAndPerformanceCurrent AS C ON C.[StudentId] = S.[Id];