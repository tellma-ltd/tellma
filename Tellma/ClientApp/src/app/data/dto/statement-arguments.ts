export interface StatementArguments {
    select?: string;
    top?: number;
    skip?: number;
    fromDate?: string;
    toDate?: string;
    accountId?: number;
    segmentId?: number;
    custodianId?: number;
    resourceId?: number;
    entryTypeId?: number;
    centerId?: number;
    currencyId?: string;
    includeCompleted?: boolean;
}
