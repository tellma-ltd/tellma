import { Injectable } from '@angular/core';
import { NgbDateParserFormatter, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { formatDate, parseDate } from '~/app/data/date-time-formats';
import { WorkspaceService } from '~/app/data/workspace.service';

@Injectable()
export class DatePickerParserFormatter extends NgbDateParserFormatter {

    constructor(private workspace: WorkspaceService, private translate: TranslateService) {
        super();
    }

    parse(value: string): NgbDateStruct | null {
        return parseDate(value, this.workspace.dateFormatForPicker, this.translate, this.workspace.calendarForPicker);
    }

    format(date: NgbDateStruct | null): string {
        return formatDate(date, this.workspace.dateFormatForPicker, this.translate, this.workspace.calendarForPicker);
    }
}
