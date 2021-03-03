import { Injectable } from '@angular/core';
import { NgbCalendar, NgbPeriod, NgbDate } from '@ng-bootstrap/ng-bootstrap';

/**
 * Ethiopian calendar implementation of NgbCalendar.
 * https://en.wikipedia.org/wiki/Ethiopian_calendar
 */
@Injectable()
export class NgbCalendarEthiopian extends NgbCalendar {
    getDaysPerWeek(): number {
        return 7;
    }

    getMonths(): number[] {
        return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13];
    }

    getWeeksPerMonth(): number {
        return 6; // 30 days may end up spanning 6 weeks (1 day on first week, 1 day on last week, and 28 in the intermediate 4 weeks)
    }

    getNext(eDate: NgbDate, period: NgbPeriod = 'd', n = 1): NgbDate {
        eDate = new NgbDate(eDate.year, eDate.month, eDate.day);

        // This function is an all-in-one AddDays, AddMonths and AddYears
        switch (period) {
            case 'd':
                let day = eDate.day + n;
                let mDays = getDaysPerMonth(eDate.month, eDate.year);
                if (day <= 0) {
                    while (day <= 0) {
                        eDate = setMonthIgnoreDay(eDate, eDate.month - 1);
                        mDays = getDaysPerMonth(eDate.month, eDate.year);
                        day += mDays;
                    }
                } else if (day > mDays) {
                    while (day > mDays) {
                        day -= mDays;
                        eDate = setMonthIgnoreDay(eDate, eDate.month + 1);
                        mDays = getDaysPerMonth(eDate.month, eDate.year);
                    }
                }
                eDate.day = day;
                return eDate;
            case 'm':
                eDate = setMonthIgnoreDay(eDate, eDate.month + n);
                eDate.day = Math.min(eDate.day, getDaysPerMonth(eDate.month, eDate.year)); // To handle month differences
                return eDate;
            case 'y':
                eDate.year = eDate.year + n;
                eDate.day = Math.min(eDate.day, getDaysPerMonth(eDate.month, eDate.year)); // To handle leap years
                return eDate;
            default:
                return eDate;
        }
    }

    getPrev(eDate: NgbDate, period: NgbPeriod = 'd', n = 1): NgbDate {
        return this.getNext(eDate, period, -n);
    }

    getWeekday(eDate: NgbDate): number {
        const jsDate = this.toJSDate(eDate);
        const day = jsDate.getDay();
        // in JS Date Sun=0, in ISO 8601 Sun=7
        return day === 0 ? 7 : day;
    }

    getWeekNumber(week: readonly NgbDate[], firstDayOfWeek: number): number {
        // in JS Date Sun=0, in ISO 8601 Sun=7
        if (firstDayOfWeek === 7) {
            firstDayOfWeek = 0;
        }

        const thursdayIndex = (4 + 7 - firstDayOfWeek) % 7;
        const eDate = week[thursdayIndex];

        const jsDate = this.toJSDate(eDate);
        jsDate.setDate(jsDate.getDate() + 4 - (jsDate.getDay() || 7));  // Thursday
        const time = jsDate.getTime();
        const meskerem1 = this.toJSDate(new NgbDate(eDate.year, 1, 1));  // Compare with 1st of Meskerem
        return Math.floor(Math.round((time - meskerem1.getTime()) / 86400000) / 7) + 1;
    }

    getToday(): NgbDate {
        return this.fromJSDate(new Date());
    }

    isValid(eDate?: NgbDate | null): boolean {
        if (!eDate ||
            !isInteger(eDate.year) ||
            !isInteger(eDate.month) ||
            !isInteger(eDate.day) ||
            eDate.year <= 0 || // Only positive years are supported
            eDate.month < 1 ||
            eDate.month > yMonths ||
            eDate.day < 1 ||
            eDate.day > getDaysPerMonth(eDate.month, eDate.year)) {
            return false;
        }

        return true;
    }

    /**
     * Returns Ethiopian Date
     */
    fromJSDate(jsDate: Date): NgbDate {
        const gDate = new NgbDate(jsDate.getFullYear(), jsDate.getMonth() + 1, jsDate.getDate());
        return ethiopianFromGregorian(gDate);
    }

    /**
     * Turns an Ethiopian Date into a JS Date
     */
    toJSDate(eDate: NgbDate): Date {
        const gDate = gregorianFromEthiopian(eDate);
        const jsDate = new Date(gDate.year, gDate.month - 1, gDate.day);
        // this is done avoid 30 -> 1930 conversion
        if (!isNaN(jsDate.getTime())) {
            jsDate.setFullYear(gDate.year);
        }
        return jsDate;
    }
}

/////////////// Helper Functions

function isInteger(value: any): value is number {
    return typeof value === 'number' && isFinite(value) && Math.floor(value) === value;
}

/**
 * Number of months in every Ethiopian year
 */
const yMonths = 13;

/**
 * Number of days in a certain month of the Ethiopian calendar (handles Pagume leap years)
 */
function getDaysPerMonth(month: number, year: number) {
    if (month === 13) { // Pagume
        const isLeap = year % 4 === 3;
        return isLeap ? 6 : 5; // 6 days in leap years, 5 otherwise
    } else {
        return 30; // All other months are always 30 days
    }
}

/**
 * Sets the month of an Ethiopian date taking care of month overflow into year, does not care if day becomes invalid
 */
function setMonthIgnoreDay(eDate: NgbDate, month: number): NgbDate {
    eDate.year = eDate.year + Math.floor((month - 1) / yMonths);
    eDate.month = Math.floor(((month - 1) % yMonths + yMonths) % yMonths) + 1;
    return eDate;
}

/**
 * Converts from Gregorian Calendar to Ethiopian Calendar.
 */
export function ethiopianFromGregorian(date: NgbDate): NgbDate {
    const jdn = gregorianToJdn(date);
    return jdnToEthiopian(jdn);
}

/**
 * Converts from Ethiopian Calendar to Gregorian Calendar.
 */
export function gregorianFromEthiopian(date: NgbDate): NgbDate {
    const jdn = ethiopianToJdn(date);
    return jdnToGregorian(jdn);
}

/**
 * Converts from Gregorian Calendar to Julian Day Number.
 * https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html
 */
function gregorianToJdn(date: NgbDate): number {
    const d = date.day;
    let m = date.month;
    let y = date.year;

    if (m <= 2) {
        y--;
        m += 12;
    }

    const a = Math.floor(y / 100);
    const b = Math.floor(a / 4);
    const c = 2 - a + b;
    const e = Math.floor(365.25 * (y + 4716));
    const f = Math.floor(30.6001 * (m + 1));

    const jdn = c + d + e + f - 1524;
    return jdn;
}

/**
 * Converts from Julian Day Number (JDN) to Gregorian Calendar.
 * https://quasar.as.utexas.edu/BillInfo/JulianDatesG.html
 */
function jdnToGregorian(jdn: number): NgbDate {
    const z = jdn;
    const w = Math.floor((z - 1867216.25) / 36524.25);
    const x = Math.floor(w / 4);
    const a = z + 1 + w - x;
    const b = a + 1524;
    const c = Math.floor((b - 122.1) / 365.25);
    const d = Math.floor(365.25 * c);
    const e = Math.floor((b - d) / 30.6001);
    const f = Math.floor(30.6001 * e);

    const day = b - d - f; // + (q - z);
    const month = e <= 13 ? e - 1 : e - 13;
    const year = month <= 2 ? c - 4715 : c - 4716;

    return new NgbDate(year, month, day);
}

/**
 * Converts from Ethiopian Calendar to Julian Day Number (JDN).
 * http://www.geez.org/Calendars/
 */
function ethiopianToJdn(date: NgbDate): number {

    const jdOffset = 1723856;
    const jdn = (jdOffset + 365)
        + 365 * (date.year - 1)
        + Math.floor(date.year / 4)
        + 30 * date.month
        + date.day - 31;

    return jdn;
}

/**
 * Converts from Julian Day Number (JDN) to Ethiopian Calendar.
 * http://www.geez.org/Calendars/
 */
function jdnToEthiopian(jdn: number): NgbDate {
    const jdOffset = 1723856;
    const r = (jdn - jdOffset) % 1461;
    const n = (r % 365) + 365 * Math.floor(r / 1460);

    const year = 4 * Math.floor((jdn - jdOffset) / 1461)
        + Math.floor(r / 365)
        - Math.floor(r / 1460);

    const month = Math.floor(n / 30) + 1;
    const day = (n % 30) + 1;

    return new NgbDate(year, month, day);
}
