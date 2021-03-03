import { NgbCalendarIslamicUmalqura, NgbDate, NgbDateNativeAdapter, NgbDateStruct, NgbTimeStruct } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { afterNoonName, beforeNoonName, monthFullName, monthShortName } from './date-time-localizations';
import { getEditDistance } from './edit-distance';
import {
    Calendar,
    DateFormat,
    DateGranularity,
    DateTimeGranularity,
    HmsFormat,
    hourFormat,
    hourMinuteFormat,
    TimeFormat,
    TimeGranularity,
    yearFormat,
    yearMonthFormat,
    YmdFormat
} from './entities/base/metadata-types';
import { NgbCalendarEthiopian } from './ngb-calendar-ethiopian';

///////////////////////////////
//            Date
///////////////////////////////

const nativeAdapter: NgbDateNativeAdapter = new NgbDateNativeAdapter();
const ethiopianCalendar: NgbCalendarEthiopian = new NgbCalendarEthiopian();
const umalquraCalendar: NgbCalendarIslamicUmalqura = new NgbCalendarIslamicUmalqura();

export function ngbDateFromDate(date: Date, calendar: Calendar): NgbDateStruct {

    let ngbDate: NgbDateStruct;
    switch (calendar) {
        case 'GC': {
            ngbDate = nativeAdapter.fromModel(date);
            break;
        }
        case 'ET': {
            ngbDate = ethiopianCalendar.fromJSDate(date);
            break;
        }
        case 'UQ': {
            ngbDate = umalquraCalendar.fromGregorian(date);
            break;
        }
    }

    return ngbDate;
}

export function dateFromNgbDate(ngbDate: NgbDateStruct, calendar: Calendar): Date {
    let date: Date;
    switch (calendar) {
        case 'GC': {
            date = nativeAdapter.toModel(ngbDate);
            date.setHours(0); // Not sure why the native adapter sets hours to 12
            break;
        }
        case 'ET': {
            date = ethiopianCalendar.toJSDate(ngbDate as NgbDate); // Only struct members are used
            break;
        }
        case 'UQ': {
            date = umalquraCalendar.toGregorian(ngbDate as NgbDate); // Only struct members are used
            break;
        }
    }

    return date;
}

export function adjustDateFormatForGranularity(format: YmdFormat, granularity: DateTimeGranularity) {
    switch (granularity) {
        case DateGranularity.years: return yearFormat(format);
        case DateGranularity.months: return yearMonthFormat(format);
        default: return format;
    }
}

const yyyy = 'yyyy';
const yyy = 'yyy';
const yy = 'yy';
const MMMM = 'MMMM';
const MMM = 'MMM';
const MM = 'MM';
const M = 'M';
const dd = 'dd';
const d = 'd';

export function formatDate(date: NgbDateStruct, format: DateFormat, trx: TranslateService, calendar: Calendar): string {
    if (!date) {
        return '';
    }

    let result: string = format;

    // (1) Format the year
    if (result.includes(yyyy)) {
        let year = date.year.toString();
        if (year.length < 4) {
            year = '000'.substring(0, 4 - year.length) + year;
        }

        result = result.replace(yyyy, year);
    } else if (result.includes(yyy)) {
        let year = date.year.toString();
        if (year.length < 3) {
            year = '00'.substring(0, 3 - year.length) + year;
        }

        result = result.replace(yyy, year);
    } else if (result.includes(yy)) {
        result = result.replace(yy, pad(date.year.toString()));
    }

    // (2) Format the day
    if (result.includes(dd)) {
        result = result.replace(dd, pad(date.day.toString()));
    } else if (result.includes(d)) {
        result = result.replace(d, date.day.toString());
    }

    // (3) Format the Month last (so that month name does not interfer with other replacements)
    if (result.includes(MMMM)) {
        result = result.replace(MMMM, monthFullName(date.month, trx, calendar, date.year));
    } else if (result.includes(MMM)) {
        result = result.replace(MMM, monthShortName(date.month, trx, calendar, date.year));
    } else if (result.includes(MM)) {
        result = result.replace(MM, pad(date.month.toString()));
    } else if (result.includes(M)) {
        result = result.replace(M, date.month.toString());
    }

    return result;
}

export function parseDate(input: string, format: DateFormat, trx: TranslateService, calendar: Calendar): NgbDateStruct {
    if (!input) {
        return;
    }

    let parts: string[];
    let yearPiece: string;
    let monthPiece: string;
    let dayPiece: string;

    switch (format) {
        case 'MM/dd/yyyy':
        case 'M/d/yyyy':
            parts = splitMXX(input, '/');
            if (!parts) { return; }
            [monthPiece, dayPiece, yearPiece] = parts;
            break;

        case 'd/M/yyyy':
        case 'dd/MM/yyyy':
            parts = splitXMX(input, '/');
            if (!parts) { return; }
            [dayPiece, monthPiece, yearPiece] = parts;
            break;

        case 'M/yyyy':
        case 'MM/yyyy':
            parts = splitMY(input, '/');
            if (!parts) { return; }
            [monthPiece, yearPiece] = parts;
            dayPiece = '1';
            break;

        case 'MMMM d, yyyy': {
            // Special case, has more than 1 separator
            let pieces = input.split(',');
            if (pieces.length < 2) {
                return;
            }
            yearPiece = pieces.pop().trim();
            pieces = pieces.join(',').split(' ');
            if (pieces.length < 2) {
                return;
            }
            dayPiece = pieces.pop().trim();
            monthPiece = pieces.join(' ').trim();
            break;
        }
        case 'MMMM, yyyy':
            parts = splitMY(input, ',');
            if (!parts) { return; }
            [monthPiece, yearPiece] = parts;
            dayPiece = '1';
            break;

        case 'd.M.yyyy':
        case 'dd.MM.yyyy':
            parts = splitXMX(input, '.');
            if (!parts) { return; }
            [dayPiece, monthPiece, yearPiece] = parts;
            break;

        case 'M.yyyy':
        case 'MM.yyyy':
            parts = splitMY(input, '.');
            if (!parts) { return; }
            [monthPiece, yearPiece] = parts;
            dayPiece = '1';
            break;

        case 'd MMMM yyyy':
        case 'dd MMMM yyyy':
            parts = splitXMX(input, ' ');
            if (!parts) { return; }
            [dayPiece, monthPiece, yearPiece] = parts;
            break;
        case 'MMMM yyyy':
            parts = splitMY(input, ' ');
            if (!parts) { return; }
            [monthPiece, yearPiece] = parts;
            dayPiece = '1';
            break;

        case 'dd-MMM-yyyy':
            parts = splitXMX(input, '-');
            if (!parts) { return; }
            [dayPiece, monthPiece, yearPiece] = parts;
            break;
        case 'MMM-yyyy':
            parts = splitMY(input, '-');
            if (!parts) { return; }
            [monthPiece, yearPiece] = parts;
            dayPiece = '1';
            break;

        case 'yyyy':
            yearPiece = input;
            monthPiece = '1';
            dayPiece = '1';
            break;

        case 'yyyy-MM':
            parts = splitYM(input, '-');
            if (!parts) { return; }
            [monthPiece, yearPiece] = parts;
            dayPiece = '1';
            break;
        case 'yyyy-MM-dd':
        default: // Default is 'yyyy-MM-dd'
            parts = splitXMX(input, '-');
            if (!parts) { return; }
            [yearPiece, monthPiece, dayPiece] = parts;
            break;
    }

    const year = parseInt(yearPiece, 10);
    if (isNaN(year)) {
        return;
    }

    const day = parseInt(dayPiece, 10);
    if (isNaN(day)) {
        return;
    }

    let month = parseInt(monthPiece, 10);
    if (isNaN(month)) {
        // The month can be specified by name
        // (1) Collect all month names in a an array
        const shortNames: string[] = [];
        const fullNames: string[] = [];
        let m = 1;
        while (true) {
            // Get the display name of the month
            const fullName = monthFullName(m, trx, calendar, year);
            const shortName = monthShortName(m, trx, calendar, year);
            if (!shortName || !fullName) {
                break; // No more months
            }

            fullNames.push(fullName.toLowerCase());
            shortNames.push(shortName.toLowerCase());
            m++;
        }

        // (2) Find the month name with the lowest distance from the user input
        const monthPieceLower = monthPiece.toLowerCase();
        let minDistance = Infinity;
        let dist: number; // convenience variable
        for (let i = 0; i < shortNames.length; i++) {

            // Compare with the full month name
            const fullName = fullNames[i];
            const fullCap = Math.floor(fullName.length / 2) + 1; // The longer the name, the more spelling errors we allow
            dist = getEditDistance(fullName, monthPieceLower, fullCap);
            if (dist < fullCap && dist < minDistance) {
                month = i + 1;
                minDistance = dist;
            }

            // Compare with the short month name
            const shortName = shortNames[i];
            const shortCap =  Math.floor(shortName.length / 2) + 1;
            dist = getEditDistance(shortName, monthPieceLower, shortCap);
            if (dist < shortCap && dist < minDistance) {
                month = i + 1;
                minDistance = dist;
            }

            if (minDistance === 0) {
                break; // Optimizaton for perfect match
            }
        }

        if (isNaN(month)) {
            return; // No good match was found
        }
    }

    return { day, month, year };
}

/**
 * Helper function: splits in the input into 3 parts assuming the month is the middle part
 */
function splitXMX(input: string, separator: string): string[] {
    const pieces = input.split(separator);
    if (pieces.length < 3) {
        return undefined; // Invalid
    }

    const firstPiece = pieces.shift().trim(); // day or year
    const lastPiece = pieces.pop().trim(); // day or year
    const monthPiece = pieces.join(separator).trim();

    return [firstPiece, monthPiece, lastPiece];
}

/**
 * Helper function: splits in the input into 3 parts assuming the month is the first part
 */
function splitMXX(input: string, separator: string): string[] {
    const pieces = input.split(separator);
    if (pieces.length < 3) {
        return undefined; // Invalid
    }

    const lastPiece = pieces.pop().trim(); // day or year
    const secondToLastPiece = pieces.pop().trim(); // day or year
    const monthPiece = pieces.join(separator).trim();

    return [monthPiece, secondToLastPiece, lastPiece];
}

function splitMY(input: string, separator: string): string[] {
    const pieces = input.split(separator);
    if (pieces.length < 2) {
        return undefined; // Invalid
    }

    const yearPiece = pieces.pop().trim(); // day or year
    const monthPiece = pieces.join(separator).trim();

    return [monthPiece, yearPiece];
}

function splitYM(input: string, separator: string): string[] {
    const pieces = input.split(separator);
    if (pieces.length < 2) {
        return undefined; // Invalid
    }

    const yearPiece = pieces.shift().trim(); // day or year
    const monthPiece = pieces.join(separator).trim();

    return [monthPiece, yearPiece];
}

///////////////////////////////
//            Time
///////////////////////////////

export function ngbTimeFromDate(date: Date): NgbTimeStruct {
    return {
        hour: date.getHours(),
        minute: date.getMinutes(),
        second: date.getSeconds()
    };
}

const HH = 'HH';
const hh = 'hh';
const H = 'H';
const h = 'h';
const mm = 'mm';
const ss = 'ss';
const t = 't';

export function formatTime(time: NgbTimeStruct, format: TimeFormat, trx: TranslateService): string {
    if (!time) {
        return '';
    }

    let result: string = format;

    // (2) Format the hour
    if (result.includes(HH)) {
        result = result.replace(HH, pad(time.hour.toString()));
    } else if (result.includes(H)) {
        result = result.replace(H, time.hour.toString());
    } else if (result.includes(hh)) {
        result = result.replace(hh, pad(((time.hour % 12) || 12).toString()));
    } else if (result.includes(h)) {
        result = result.replace(h, ((time.hour % 12) || 12).toString());
    }

    // (2) Format the minute
    if (result.includes(mm)) {
        result = result.replace(mm, pad(time.minute.toString()));
    }

    // (3) Format the second
    if (result.includes(ss)) {
        result = result.replace(ss, pad(time.second.toString()));
    }

    // (4) Format the period (AM/PM)
    if (result.includes(t)) {
        result = result.replace(t, time.hour < 12 ? beforeNoonName(trx) : afterNoonName(trx));
    }

    return result;
}

export function adjustTimeFormatForGranularity(format: HmsFormat, granularity: DateTimeGranularity): TimeFormat {
    switch (granularity) {
        case DateGranularity.years:
        case DateGranularity.months:
        case DateGranularity.days: return; // no time format
        case TimeGranularity.hours: return hourFormat(format);
        case TimeGranularity.minutes: return hourMinuteFormat(format);
        default: return format;
    }
}

/**
 * Helper function: adds a '0' before the string if it's less than 2 characters
 */
function pad(s: string): string {
    if (s.length < 2) {
        s = '0' + s;
    }
    return s;
}
