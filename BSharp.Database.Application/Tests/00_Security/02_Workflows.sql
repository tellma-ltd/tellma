DECLARE @WorkflowId INT;
INSERT INTO dbo.Workflows([LineDefinitionId], FromState, ToState)
Values(N'manual-journals', N'Draft', N'Posted');
SELECT @WorkflowId = SCOPE_IDENTITY();

INSERT INTO dbo.[WorkflowSignatures](WorkflowId, RoleId) VALUES
(@WorkflowId, @Accountant);
--(@WorkflowId, @Comptroller);