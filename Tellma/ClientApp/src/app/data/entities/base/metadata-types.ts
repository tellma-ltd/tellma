///////////////////////////////////
//          Granularity
///////////////////////////////////

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

///////////////////////////////////
//          Calendar
///////////////////////////////////

export type Calendar = 'GC' | 'ET' | 'UQ';

export const calendarsArray: Calendar[] = ['GC', 'ET', 'UQ'];

///////////////////////////////////
//          Date Format
///////////////////////////////////

export type DateFormat = YmdFormat | YmFormat | YFormat;

export type YmdFormat =
    'M/d/yyyy' |
    'MM/dd/yyyy' |
    'yyyy-MM-dd' |
    'MMMM d, yyyy' |
    'dd-MMM-yyyy' |
    'd/M/yyyy' |
    'dd/MM/yyyy' |
    'd.M.yyyy' |
    'dd.MM.yyyy' |
    'd MMMM yyyy' |
    'dd MMMM yyyy';

export type YmFormat =
    'M/yyyy' |
    'MM/yyyy' |
    'yyyy-MM' |
    'MMMM, yyyy' |
    'MMM-yyyy' |
    'M.yyyy' |
    'MM.yyyy' |
    'MMMM yyyy';

export type YFormat = 'yyyy';

export const defaultDateFormat: YmdFormat = 'yyyy-MM-dd';

export const ymdFormatsArray: YmdFormat[] = [
    'M/d/yyyy',
    'MM/dd/yyyy',
    'yyyy-MM-dd',
    'MMMM d, yyyy',
    'dd-MMM-yyyy',
    'd/M/yyyy',
    'dd/MM/yyyy',
    'd.M.yyyy',
    'dd.MM.yyyy',
    'd MMMM yyyy',
    'dd MMMM yyyy'
];

export function yearMonthFormat(format: YmdFormat): YmFormat {
    switch (format) {
        case 'MM/dd/yyyy': return 'MM/yyyy';
        case 'yyyy-MM-dd': return 'yyyy-MM';
        case 'MMMM d, yyyy': return 'MMMM, yyyy';
        case 'M/d/yyyy': return 'M/yyyy';
        case 'dd-MMM-yyyy': return 'MMM-yyyy';
        case 'd/M/yyyy': return 'M/yyyy';
        case 'dd/MM/yyyy': return 'MM/yyyy';
        case 'd.M.yyyy': return 'M.yyyy';
        case 'dd.MM.yyyy': return 'MM.yyyy';
        case 'd MMMM yyyy': return 'MMMM yyyy';
        case 'dd MMMM yyyy': return 'MMMM yyyy';
        default: return 'yyyy-MM';
    }
}

export function yearFormat(_: YmdFormat): YFormat {
    return 'yyyy';
}

///////////////////////////////////
//          Time Format
///////////////////////////////////

export type TimeFormat = HmsFormat | HmFormat | HFormat;

export type HmsFormat =
    'HH:mm:ss' |
    'hh:mm:ss t' |
    'H:mm:ss' |
    'h:mm:ss t';

export type HmFormat =
    'HH:mm' |
    'hh:mm t' |
    'H:mm' |
    'h:mm t';

export type HFormat =
    'HH' |
    'hh t' |
    'H' |
    'h t';

export const defaultTimeFormat: HmsFormat = 'HH:mm:ss';

export const hmsFormatsArray: HmsFormat[] = [
    'HH:mm:ss',
    'hh:mm:ss t',
    'H:mm:ss',
    'h:mm:ss t'
];

export function hourMinuteFormat(format: HmsFormat): HmFormat {
    switch (format) {
        case 'HH:mm:ss': return 'HH:mm';
        case 'hh:mm:ss t': return 'hh:mm t';
        case 'H:mm:ss': return 'H:mm';
        case 'h:mm:ss t': return 'h:mm t';
        default: return 'HH:mm';
    }
}

export function hourFormat(format: HmsFormat): HFormat {
    switch (format) {
        case 'HH:mm:ss': return 'HH';
        case 'hh:mm:ss t': return 'hh t';
        case 'H:mm:ss': return 'H';
        case 'h:mm:ss t': return 'h t';
        default: return 'HH';
    }
}
