import { NgbDatepickerI18n, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Calendar } from '~/app/data/entities/base/metadata-types';
import { monthFullName, monthShortName, weekdayVeryShortName } from '~/app/data/date-time-localizations';

@Injectable()
export class DatePickerLocalization extends NgbDatepickerI18n {

    constructor(private workspace: WorkspaceService, private translation: TranslateService) {
        super();
    }

    private get calendar(): Calendar {
        // Helper function
        return this.workspace.calendarForPicker;
    }

    getWeekdayShortName(weekday: number): string {
        return weekdayVeryShortName(weekday, this.translation, this.calendar);
    }

    getMonthShortName(month: number): string {
        return monthShortName(month, this.translation, this.calendar);
    }

    getMonthFullName(month: number): string {
        return monthFullName(month, this.translation, this.calendar);
    }

    getDayAriaLabel(date: NgbDateStruct): string {
        // TODO: Accessibility support
        return '';
    }
}
