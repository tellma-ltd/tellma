import { TranslateService } from '@ngx-translate/core';
import { Calendar } from './entities/base/metadata-types';

///////////////////////////////
//            Date
///////////////////////////////

export function monthShortName(month: number, trx: TranslateService, calendar: Calendar, year?: number): string {
    if (isValidMonth(month, calendar, year)) {
        return trx.instant('ShortMonth' + monthPostfix(month, calendar));
    }
}

export function monthFullName(month: number, trx: TranslateService, calendar: Calendar, year?: number): string {
    if (isValidMonth(month, calendar, year)) {
        return trx.instant('FullMonth' + monthPostfix(month, calendar));
    }
}

export function weekdayVeryShortName(weekday: number, trx: TranslateService, calendar: Calendar): string {
    if (isValidWeekday(weekday, calendar)) {
        return trx.instant('VeryShortDay' + weekday);
    }
}

export function weekdayShortName(weekday: number, trx: TranslateService, calendar: Calendar): string {
    if (isValidWeekday(weekday, calendar)) {
        return trx.instant('ShortDay' + weekday);
    }
}

export function weekdayFullName(weekday: number, trx: TranslateService, calendar: Calendar): string {
    if (isValidWeekday(weekday, calendar)) {
        return trx.instant('FullDay' + weekday);
    }
}

// Helper functions

function isValidMonth(month: number, calendar: Calendar, year: number): boolean {
    return month >= 1 && (month <= 12 || (calendar === 'ET' && month <= 13));
}

function isValidWeekday(weekday: number, _: Calendar): boolean {
    // weekday: 1 = Monday (ISO 8601)
    return weekday >= 1 && weekday <= 7;
}

function monthPostfix(month: number, calendar: Calendar): string {
    switch (calendar) {
        case 'GC':
            return '' + month;
        case 'ET':
            return 'Et' + month;
        case 'UQ':
            return 'Uq' + month;
    }
}

///////////////////////////////
//            Time
///////////////////////////////

export function beforeNoonName(trx: TranslateService): string {
    return trx.instant('Time_AM');
}

export function afterNoonName(trx: TranslateService): string {
    return trx.instant('Time_PM');
}
