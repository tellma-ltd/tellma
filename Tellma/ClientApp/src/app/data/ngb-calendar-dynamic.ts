import { Injectable } from '@angular/core';
import { NgbCalendar, NgbPeriod, NgbDate } from '@ng-bootstrap/ng-bootstrap';

/**
 * This is a wrapper calendar that behaves accordingly to the currently selected calendar
 */
@Injectable()
export class NgbCalendarDynamic extends NgbCalendar {
    getDaysPerWeek(): number {
        throw new Error('Method not implemented.');
    }
    getMonths(year?: number): number[] {
        throw new Error('Method not implemented.');
    }
    getWeeksPerMonth(): number {
        throw new Error('Method not implemented.');
    }
    getWeekday(date: NgbDate): number {
        throw new Error('Method not implemented.');
    }
    getNext(date: NgbDate, period: NgbPeriod = 'd', n = 1): NgbDate {
        throw new Error('Method not implemented.');
    }
    getPrev(date: NgbDate, period: NgbPeriod = 'd', n = 1): NgbDate {
        throw new Error('Method not implemented.');
    }
    getWeekNumber(week: readonly NgbDate[], firstDayOfWeek: number): number {
        throw new Error('Method not implemented.');
    }
    getToday(): NgbDate {
        throw new Error('Method not implemented.');
    }
    isValid(date?: NgbDate): boolean {
        throw new Error('Method not implemented.');
    }
}
