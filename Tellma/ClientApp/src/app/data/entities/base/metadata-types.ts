export enum DateGranularity {
    years = 1,
    months = 2,
    days = 3,
}
export enum TimeGranularity {
    hours = 4,
    minutes = 5,
    seconds = 6,
}

export type DateTimeGranularity = DateGranularity | TimeGranularity;
