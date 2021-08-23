export interface StatementArguments {
    select?: string;
    top?: number;
    skip?: number;
    fromDate?: string;
    toDate?: string;
    accountId?: number;
    relationId?: number;
    resourceId?: number;
    notedRelationId?: number;
    entryTypeId?: number;
    centerId?: number;
    currencyId?: string;
    includeCompleted?: boolean;
}
