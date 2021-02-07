import { Injectable } from '@angular/core';
import { NgbDateAdapter, NgbDateStruct, NgbDateNativeAdapter } from '@ng-bootstrap/ng-bootstrap';
import { toLocalDateISOString } from '~/app/data/util';

// In our DTOs we always use ISO string representation instead of native JS Date objects,
// since JSON parse and stringify are unable to handle the native JS Date object on both
// server and client side, this adapter acts as a bridge between the native JS date and
// the ISO string representation, we simply make it a thin wrapper around the existing
// implementation of NgbDateNativeAdapter to make our lives easier

@Injectable()
export class NgbDateStringAdapter implements NgbDateAdapter<string> {

    private nativeAdapter: NgbDateNativeAdapter = new NgbDateNativeAdapter();

    fromModel(value: string): NgbDateStruct {
        if (!value) {
            return null;
        }

        const ngbDate = this.nativeAdapter.fromModel(new Date(value));
        return ngbDate;
    }

    toModel(ngbDate: NgbDateStruct): string {
        if (!ngbDate) {
            return null;
        }

        // The code below turns the JS date into a local date formatted in ISO 8601
        const date = this.nativeAdapter.toModel(ngbDate);
        return toLocalDateISOString(date);
    }
}
