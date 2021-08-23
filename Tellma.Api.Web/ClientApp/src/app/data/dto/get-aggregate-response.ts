// tslint:disable:variable-name
export type DynamicRow = any[];

export interface DimensionAncestors {
    IdIndex: number;
    MinIndex: number;
    Result: DynamicRow[];
}

export interface GetAggregateResponse {
    Result: DynamicRow[];
    DimensionAncestors: DimensionAncestors[];
    ServerTime: string;
}
