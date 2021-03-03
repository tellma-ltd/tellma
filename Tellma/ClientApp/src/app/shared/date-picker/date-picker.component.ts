
import { Component, ElementRef, HostBinding, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor, Validator, ValidationErrors, AbstractControl, NG_VALIDATORS } from '@angular/forms';
import { NgbDateParserFormatter, NgbInputDatepicker } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Calendar, DateFormat, DateGranularity } from '~/app/data/entities/base/metadata-types';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-date-picker',
  templateUrl: './date-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DatePickerComponent },
  { provide: NG_VALIDATORS, multi: true, useExisting: DatePickerComponent }]
})
export class DatePickerComponent implements ControlValueAccessor, Validator, OnInit, OnChanges, OnDestroy {

  @Input()
  calendar: Calendar;

  @Input()
  granularity: DateGranularity;

  @ViewChild('d', { static: true })
  picker: NgbInputDatepicker;

  @ViewChild('i', { static: true })
  input: ElementRef<HTMLInputElement>;

  constructor(private workspace: WorkspaceService, private translate: TranslateService) { }

  @HostBinding('class.w-100')
  w100 = true;

  public isDisabled = false;
  private _subscriptions: Subscription;
  private _value: string;
  private _calendar: Calendar;
  private _dateFormat: DateFormat;

  private altCalendar: Calendar;
  private _invalid = false;

  ngOnInit() {
    this._subscriptions = new Subscription();
    this._subscriptions.add(this.translate.onLangChange.subscribe(() => {
      this.onFormatChange(); // In case a month name was used
    }));
    this._subscriptions.add(this.workspace.stateChanged$.subscribe({
      next: () => {
        // This resets the model if the calendar changes
        if (!!this._calendar && this._calendar !== this.workspace.calendar) {
          this.onCalendarChange();
        }

        if (!!this._dateFormat && this._dateFormat !== this.workspace.dateFormat) {
          this.onFormatChange();
        }
      }
    }));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.calendar && !changes.calendar.isFirstChange()) {
      this.onCalendarChange();
    }

    if (!!changes.granularity && !changes.granularity.isFirstChange()) {
      this.onFormatChange();
    }
  }

  ngOnDestroy() {
    if (!!this._subscriptions) {
      this._subscriptions.unsubscribe();
    }
  }

  private onCalendarChange() {
    this.deleteAltCalendarIfRedundant();
    this.writeValue(this._value);
    this.picker.close(); // In the unlikely event that the calendar changes while the popup is open
  }

  private onFormatChange() {
    this.writeValue(this._value);
  }

  private setOverride() {
    // This ensures that the current date picker behaves according to its calendar and format overrides
    this.workspace.calendarOverride = this.calendar || (this.disableAltCalendar ? undefined : this.altCalendar);
    this.workspace.granularityOverride = this.granularity;
  }

  ///////////////// Implementation of ControlValueAccessor
  writeValue(v: string): void {
    this.setOverride();

    this._invalid = false;
    this._value = v;
    this._calendar = this.workspace.calendar;
    this._dateFormat = this.workspace.dateFormat;
    this.picker.writeValue(v); // Format
  }

  registerOnChange(fn: (val: any) => void): void {
    this.picker.registerOnChange((v) => {

      // When NgbInputDatePicker fails to parse the user input, it sends it as is to OnChange.
      // Luckily none of the supported input formats match the parsed format, so we can use
      // this simple comparison to check if the user input was invalid
      if (!v || v === this.input.nativeElement.value) {
        this._invalid = !!v;
        delete this._value;
        delete this._calendar;
        delete this._dateFormat;
        fn(undefined);
      } else {
        this._invalid = false;
        this._value = v;
        this._calendar = this.workspace.calendar;
        this._dateFormat = this.workspace.dateFormat;
        fn(v);
      }
    });
  }

  public validate(control: AbstractControl): ValidationErrors | null {
    // We use our own validate function cause the one in the details input picker is buggy
    const { value } = control; // The user input

    if (this._invalid) {
      return { ngbDate: { invalid: value } };
    }
    return null;
  }

  registerOnTouched(fn: any): void {
    this.picker.registerOnTouched(fn);
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
    this.picker.setDisabledState(isDisabled);
  }

  registerOnValidatorChange?(fn: () => void): void {
    this.picker.registerOnValidatorChange(fn);
  }

  public get format(): string {
    return this.workspace.dateFormatForPicker;
  }

  public onFocus() {
    this.setOverride();
  }

  public onClick() {
    this.setOverride();
    this.picker.toggle();
  }

  private get getAltCalendar(): Calendar {
    if (this.workspace.isApp) {
      const tenant = this.workspace.currentTenant;
      const settings = tenant.settings;
      if (!!settings.SecondaryCalendar) {
        if ((this.altCalendar || tenant.calendar) === settings.PrimaryCalendar) {
          return settings.SecondaryCalendar;
        } else {
          return settings.PrimaryCalendar;
        }
      }
    }
  }

  public get altCalendarName(): string {
    const calendar = this.getAltCalendar;
    if (!!calendar) {
      return this.translate.instant('Calendar_' + calendar);
    }
  }

  public onAltCalendar() {
    this.altCalendar = this.getAltCalendar;
    this.onCalendarChange();
  }

  private deleteAltCalendarIfRedundant() {
    if (this.workspace.isApp) {
      const tenant = this.workspace.currentTenant;
      if (tenant.calendar === this.altCalendar) {
        delete this.altCalendar;
      }
    }
  }

  public get disableAltCalendar(): boolean {
    // If the calender is fixed, or this is the admin console, or the secondary calendar is null
    return !!this.calendar || !this.workspace.isApp || !this.workspace.currentTenant.settings.SecondaryCalendar;
  }
}

