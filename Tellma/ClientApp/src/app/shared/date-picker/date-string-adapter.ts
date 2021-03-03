import { Injectable } from '@angular/core';
import { NgbDateAdapter, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { dateFromNgbDate, ngbDateFromDate } from '~/app/data/date-time-formats';
import { toLocalDateTimeISOString } from '~/app/data/date-util';
import { WorkspaceService } from '~/app/data/workspace.service';

// In our DTOs we always use ISO string representation instead of native JS Date objects,
// since JSON parse and stringify are unable to handle the native JS Date object on both
// server and client side, this adapter acts as a bridge between the native JS date and
// the ISO string representation, we simply make it a thin wrapper around the existing
// implementation of NgbDateNativeAdapter to make our lives easier

@Injectable()
export class NgbDateStringAdapter implements NgbDateAdapter<string> {

    constructor(private workspace: WorkspaceService) {
    }

    fromModel(value: string): NgbDateStruct {
        if (!value) {
            return null;
        }

        const date = new Date(value);
        return ngbDateFromDate(date, this.workspace.calendarForPicker);
    }

    toModel(ngbDate: NgbDateStruct): string {
        if (!ngbDate) {
            return null;
        }

        const date = dateFromNgbDate(ngbDate, this.workspace.calendarForPicker);

        // The code below turns the JS date into a local date formatted in ISO 8601
        return toLocalDateTimeISOString(date);
    }
}
