﻿CREATE TYPE [dbo].[AgentBalanceList] AS TABLE --- used in bll.[fi_EmployeesIncomeTaxes]
(
	AgentId INT, 
	Balance DECIMAL (19,4)
)
