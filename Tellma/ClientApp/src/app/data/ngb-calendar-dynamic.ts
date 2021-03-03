import { Injectable } from '@angular/core';
import { NgbCalendar, NgbPeriod, NgbDate, NgbCalendarGregorian, NgbCalendarIslamicUmalqura } from '@ng-bootstrap/ng-bootstrap';
import { NgbCalendarEthiopian } from './ngb-calendar-ethiopian';
import { WorkspaceService } from './workspace.service';

/**
 * This is a wrapper calendar that behaves accordingly to the currently selected calendar
 */
@Injectable()
export class NgbCalendarDynamic extends NgbCalendar {

    constructor(
        private workspace: WorkspaceService) {
        super();
    }

    private _gregorian = new NgbCalendarGregorian();
    private _ethiopian = new NgbCalendarEthiopian();
    private _umalqura = new NgbCalendarIslamicUmalqura();

    private get calendar(): NgbCalendar {
        switch (this.workspace.calendarForPicker) {
            case 'GC': return this._gregorian;
            case 'ET': return this._ethiopian;
            case 'UQ': return this._umalqura;
        }
    }

    getDaysPerWeek(): number {
        return this.calendar.getDaysPerWeek();
    }
    getMonths(year?: number): number[] {
        return this.calendar.getMonths(year);
    }
    getWeeksPerMonth(): number {
        return this.calendar.getWeeksPerMonth();
    }
    getWeekday(date: NgbDate): number {
        return this.calendar.getWeekday(date);
    }
    getNext(date: NgbDate, period: NgbPeriod = 'd', n = 1): NgbDate {
        return this.calendar.getNext(date, period, n);
    }
    getPrev(date: NgbDate, period: NgbPeriod = 'd', n = 1): NgbDate {
        return this.calendar.getPrev(date, period, n);
    }
    getWeekNumber(week: readonly NgbDate[], firstDayOfWeek: number): number {
        return this.calendar.getWeekNumber(week, firstDayOfWeek);
    }
    getToday(): NgbDate {
        return this.calendar.getToday();
    }
    isValid(date?: NgbDate): boolean {
        return this.calendar.isValid(date);
    }
}
