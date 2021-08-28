export interface StatementArguments {
    select?: string;
    top?: number;
    skip?: number;
    fromDate?: string;
    toDate?: string;
    accountId?: number;
    agentId?: number;
    resourceId?: number;
    notedAgentId?: number;
    entryTypeId?: number;
    centerId?: number;
    currencyId?: string;
    includeCompleted?: boolean;
}
