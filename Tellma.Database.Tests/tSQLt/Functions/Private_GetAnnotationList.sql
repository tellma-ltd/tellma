CREATE FUNCTION [tSQLt].[Private_GetAnnotationList]
(@ProcedureDefinition NVARCHAR (MAX) NULL)
RETURNS 
     TABLE (
        [AnnotationNo] INT            NULL,
        [Annotation]   NVARCHAR (MAX) NULL)
AS
 EXTERNAL NAME [tSQLtCLR].[tSQLtCLR.Annotations].[GetAnnotationList]

