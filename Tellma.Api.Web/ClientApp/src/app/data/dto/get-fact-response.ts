// tslint:disable:variable-name
export type DynamicRow = any[];

export interface GetFactResponse {
    Result: DynamicRow[];
    ServerTime: string;
    TotalCount: number;
}
