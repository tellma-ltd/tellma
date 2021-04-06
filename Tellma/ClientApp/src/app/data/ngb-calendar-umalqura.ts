import { Injectable } from '@angular/core';
import { NgbCalendarIslamicCivil, NgbDate } from '@ng-bootstrap/ng-bootstrap';

/**
 * Umalqura calendar is one type of Hijri calendars used in islamic countries.
 * This Calendar is used by Saudi Arabia for administrative purpose.
 * Unlike tabular calendars, the algorithm involves astronomical calculation, but it's still deterministic.
 * http://cldr.unicode.org/development/development-process/design-proposals/islamic-calendar-types
 */

const GREGORIAN_FIRST_DATE = new Date(1900, 3, 30); // 1318-01-01 H
const GREGORIAN_LAST_DATE = new Date(2077, 10, 16); // 1500-12-30 H
const HIJRI_BEGIN = 1318;
const HIJRI_END = 1500;
const ONE_DAY = 1000 * 60 * 60 * 24;

const withinGRange = (gDate: Date): boolean =>
    gDate.getTime() >= GREGORIAN_FIRST_DATE.getTime() &&
    gDate.getTime() <= GREGORIAN_LAST_DATE.getTime();

const withinHRange = (hYear: number): boolean =>
    hYear < HIJRI_BEGIN || hYear > HIJRI_END;

const MONTH_LENGTH2: number[] = [
    746, 1769, // 1318 - 1319
    3794, 3748, 3402, 2710, 1334, 2741, 3498, 2980, 2889, 2707, // 1320 - 1329
    1323, 2647, 1206, 2741, 1450, 3413, 3370, 2646, 1198, 2397, // 1330 - 1339
    748, 1749, 1706, 1365, 1195, 2395, 698, 1397, 2994, 1892, // 1340 - 1349
    1865, 1621, 683, 1371, 2778, 1748, 3785, 3474, 3365, 2637, // 1350 - 1359
    685, 1389, 2922, 2898, 2725, 2635, 1175, 2359, 694, 1397, // 1360 - 1369
    3434, 3410, 2710, 2349, 605, 1245, 2778, 1492, 3497, 3410, // 1370 - 1379
    2730, 1238, 2486, 884, 1897, 1874, 1701, 1355, 2731, 1370, // 1380 - 1389
    2773, 3538, 3492, 3401, 2709, 1325, 2653, 1370, 2773, 1706, // 1390 - 1399
    1685, 1323, 2647, 1198, 2422, 1388, 2901, 2730, 2645, 1197, // 1400 - 1409
    2397, 730, 1497, 3506, 2980, 2890, 2645, 693, 1397, 2922, // 1410 - 1419
    3026, 3012, 2953, 2709, 1325, 1453, 2922, 1748, 3529, 3474, // 1420 - 1429
    2726, 2390, 686, 1389, 874, 2901, 2730, 2381, 1181, 2397, // 1430 - 1439
    698, 1461, 1450, 3413, 2714, 2350, 622, 1373, 2778, 1748, // 1440 - 1449
    1701, 1355, 2711, 1358, 2734, 1452, 2985, 3474, 2853, 1611, // 1450 - 1459
    3243, 1370, 2901, 1746, 3749, 3658, 2709, 1325, 2733, 876, // 1460 - 1469
    1881, 1746, 1685, 1325, 2651, 1210, 2490, 948, 2921, 2898, // 1470 - 1479
    2726, 1206, 2413, 748, 1753, 3762, 3412, 3370, 2646, 1198, // 1480 - 1489
    2413, 3434, 2900, 2857, 2707, 1323, 2647, 1334, 2741, 1706, // 1490 - 1499
    3731, // 1500
];

/**
 * Retrieves the number of days in the given hijri month in the given hijri year, answer is always 29 or 30
 */
function getDaysInMonth(hMonth: number, hYear: number): 30 | 29 {
    const info = MONTH_LENGTH2[hYear - HIJRI_BEGIN];
    const mask = Math.pow(2, hMonth - 1);
    // tslint:disable-next-line:no-bitwise
    return (info & mask) > 0 ? 30 : 29;
}

function getDaysInYear(hYear: number): number {
    let daysInYear = 0;
    for (let m = 1; m <= 12; m++) {
        daysInYear += getDaysInMonth(m, hYear);
    }

    if (daysInYear !== 354 && daysInYear !== 355) {
        console.error(`Bug: Hijri days in year returned ${daysInYear}`);
    }

    return daysInYear;
}

// Maps every hijri year to gregorian date matching Muharram 1 of that year
const startsOfHYears: { [hYear: number]: Date } = {};

/**
 * Efficiently retrieves the gregorian date matching Muharram 1st of the given hijri year
 */
function getStartOfHYear(hYear: number): Date {
    if (!withinHRange(hYear)) {
        console.error(`Hijri Year ${hYear} is out of range.`);
        return;
    }

    if (!startsOfHYears[hYear]) {
        const date = new Date(GREGORIAN_FIRST_DATE.getTime());
        let year = HIJRI_BEGIN;
        while (year < hYear) {
            date.setDate(date.getDate() + getDaysInYear(year));
            year++;
        }

        startsOfHYears[hYear] = date;
    }

    // Clone it
    const cache = startsOfHYears[hYear];
    return new Date(cache.getTime()); // Return a copy
}

let _startOfYears: number[]; // Unix dates
let _hijriYears: number[];

/**
 * Efficiently retrieves the hijri date matching January 1st of Jan of the given gregorian year
 */
function getStartOfGYear(gDate: Date): { muharram1: Date, hYear: number } {
    if (!_startOfYears) {
        _startOfYears = [];
        _hijriYears = [];
        const date = new Date(GREGORIAN_FIRST_DATE.getTime());
        let year = HIJRI_BEGIN;
        while (year <= HIJRI_END) {
            _startOfYears.push(date.getTime());
            _hijriYears.push(year);
            date.setDate(date.getDate() + getDaysInYear(year++));
        }
    }

    // Find the index where _startOfYears[index] < time && _startOfYears[index + 1] > time or is null
    const time = gDate.getTime(); // Clone it
    let low = 0;
    let high = _startOfYears.length;
    let mid: number;
    let midValue: number;

    while (low < high - 1) {
        mid = Math.floor((high + low) / 2);
        midValue = _startOfYears[mid];
        if (time >= midValue) {
            low = mid;
        } else { // time < midValue
            high = mid;
        }
    }

    return { muharram1: new Date(_startOfYears[low]), hYear: _hijriYears[low] };
}

function getDaysDiff(date1: Date, date2: Date): number {
    // Ignores the time part in date1 and date2:
    const time1 = Date.UTC(date1.getFullYear(), date1.getMonth(), date1.getDate());
    const time2 = Date.UTC(date2.getFullYear(), date2.getMonth(), date2.getDate());
    const diff = Math.abs(time1 - time2);
    return Math.round(diff / ONE_DAY);
}

@Injectable()
export class NgbCalendarUmAlQura extends NgbCalendarIslamicCivil {
    /**
     * Returns the equivalent UmAlQura date value for a give input Gregorian date.
     * `gdate` is s JS Date to be converted to Hijri.
     */
    fromGregorian(gDate: Date): NgbDate {
        if (withinGRange(gDate)) {
            const { muharram1, hYear } = getStartOfGYear(gDate);
            let hDay = getDaysDiff(muharram1, gDate) + 1;
            let hMonth = 1;
            while (true) {
                const daysInMonth = getDaysInMonth(hMonth, hYear);
                if (hDay > daysInMonth) {
                    hDay -= daysInMonth;
                } else {
                    break;
                }

                hMonth++;
            }

            return new NgbDate(hYear, hMonth, hDay);
        } else {
            return super.fromGregorian(gDate); // Fallback to Islamic Civil
        }
    }

    /**
     * Converts the current Hijri date to Gregorian.
     */
    toGregorian(hDate: NgbDate): Date {
        if (withinHRange(hDate.year)) {
            const startOfYear = getStartOfHYear(hDate.year);
            let month = 1;
            let days = hDate.day - 1;
            while (month < hDate.month) {
                days += getDaysInMonth(month, hDate.year);
                month++;
            }

            startOfYear.setDate(startOfYear.getDate() + days);
            return startOfYear;
        } else {
            return super.toGregorian(hDate); // Fallback to Islamic Civil
        }
    }

    /**
     * Returns the number of days in a specific Hijri hMonth.
     * `hMonth` is 1 for Muharram, 2 for Safar, etc.
     * `hYear` is any Hijri hYear.
     */
    getDaysPerMonth(hMonth: number, hYear: number): number {
        if (withinHRange(hYear)) {
            return getDaysInMonth(hMonth, hYear);
        }

        // Fallback to Islamic Civil
        return super.getDaysPerMonth(hMonth, hYear);
    }
}
