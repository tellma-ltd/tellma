import { OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Calendar, DateFormat, DateTimeGranularity, TimeFormat } from '~/app/data/entities/base/metadata-types';
import { WorkspaceService } from '~/app/data/workspace.service';
import { datetimeFormat } from './date-time-format';

@Pipe({
  name: 'datetimeFormat',
  pure: false
})
export class DateTimeFormatPipe implements PipeTransform, OnDestroy {

  // As an optimization, we cache the result unless the calendar or language change
  private _subscriptions: Subscription;
  private _wsCalendar: Calendar;
  private _wsDateFormat: DateFormat;
  private _wsTimeFormat: TimeFormat;
  private _formattedValue: string;

  private _isoDate: string;
  private _granularity: DateTimeGranularity;
  private _calendar: Calendar;
  private _markForCheck = true;

  private markForCheck() {
    this._markForCheck = true;
  }

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    this._subscriptions = new Subscription();
    this._subscriptions.add(this.translate.onLangChange.subscribe(() => {
      this.markForCheck(); // In case a month name or a day period (AM/PM) is used
    }));
    this._subscriptions.add(this.workspace.stateChanged$.subscribe({
      next: () => {
        // This resets the model if the calendar changes
        if (!!this._wsCalendar && this._wsCalendar !== this.workspace.calendar) {
          this.markForCheck();
        }

        if (!!this._wsDateFormat && this._wsDateFormat !== this.workspace.dateFormat) {
          this.markForCheck();
        }

        if (!!this._wsTimeFormat && this._wsTimeFormat !== this.workspace.timeFormat) {
          this.markForCheck();
        }
      }
    }));
  }

  ngOnDestroy(): void {
    if (!!this._subscriptions) {
      this._subscriptions.unsubscribe();
    }
  }

  transform(isoDate: string, calendar?: Calendar, granularity?: DateTimeGranularity): string {
    if (this._markForCheck || this._isoDate !== isoDate || this._calendar !== calendar || this._granularity !== granularity) {
      // For caching
      this._markForCheck = false;
      this._isoDate = isoDate;
      this._calendar = calendar;
      this._granularity = granularity;

      // For change detection
      this._wsCalendar = this.workspace.calendar;
      this._wsDateFormat = this.workspace.dateFormat;
      this._wsTimeFormat = this.workspace.timeFormat;

      this._formattedValue = datetimeFormat(isoDate, this.workspace, this.translate, calendar, granularity);

    }

    return this._formattedValue;
  }
}
