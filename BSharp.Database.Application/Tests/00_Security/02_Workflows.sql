DECLARE @WorkflowId INT;
INSERT INTO dbo.Workflows([LineDefinitionId], FromState, ToState)
Values(N'ManualLine', N'Draft', N'Reviewed');
SELECT @WorkflowId = SCOPE_IDENTITY();

INSERT INTO dbo.[WorkflowSignatures](WorkflowId, RoleId) VALUES
(@WorkflowId, @Accountant);
--(@WorkflowId, @Comptroller);