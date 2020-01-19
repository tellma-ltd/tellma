import { Injectable } from '@angular/core';
import { NgbDateAdapter, NgbDateStruct, NgbDateNativeAdapter } from '@ng-bootstrap/ng-bootstrap';

// in our DTOs we always use ISO string representation instead of native JS Date objects,
// since JSON parse and stringify are unable to handle the native JS Date object on both
// server and client side, this adapter acts as a bridge between the the native JS date
// and the ISO string representation, we simply make it a thin wrapper around the existing
// implementation of NgbDateNativeAdapter to make our lives easier

@Injectable()
export class NgbDateStringAdapter implements NgbDateAdapter<string> {

    private nativeAdapter: NgbDateNativeAdapter = new NgbDateNativeAdapter();

    fromModel(value: string): NgbDateStruct {
        if (!value) {
            return null;
        }

        return this.nativeAdapter.fromModel(new Date(value));
    }

    toModel(date: NgbDateStruct): string {
        if (!date) {
            return null;
        }

        return this.nativeAdapter.toModel(date).toISOString().split('T')[0];
    }
}
