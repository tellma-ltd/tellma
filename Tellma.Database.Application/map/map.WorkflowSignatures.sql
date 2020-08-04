CREATE FUNCTION [map].[WorkflowSignatures]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[WorkflowSignatures]
);
