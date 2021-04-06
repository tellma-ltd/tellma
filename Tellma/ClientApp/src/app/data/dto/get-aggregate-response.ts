// tslint:disable:variable-name
export type DynamicRow = any[];

export interface TreeDimensionResult {
    IdIndex: number;
    MinIndex: number;
    Result: DynamicRow[];
}

export interface GetAggregateResponse {
    Result: DynamicRow[];
    DimensionAncestors: TreeDimensionResult[];
    ServerTime: string;
    IsPartial: boolean;
}
