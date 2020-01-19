import { NgbDatepickerI18n, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable()
export class DatePickerLocalization extends NgbDatepickerI18n {

    constructor(private translation: TranslateService) {
        super();
    }

    getWeekdayShortName(weekday: number): string {
        // weekday: 1 = Monday
        return this.translation.instant('VeryShortDay' + weekday);
    }

    getMonthShortName(month: number): string {
        return this.translation.instant('ShortMonth' + month);
    }

    getMonthFullName(month: number): string {
        return 'يناير';
    }

    getDayAriaLabel(date: NgbDateStruct): string {
        // TODO: Accessibility support
        return '';
    }
}
