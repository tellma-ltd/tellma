import { OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { TimeGranularity, TimeFormat } from '~/app/data/entities/base/metadata-types';
import { WorkspaceService } from '~/app/data/workspace.service';
import { timeFormat } from './date-time-format';

@Pipe({
  name: 'timeFormat',
  pure: false
})
export class TimeFormatPipe implements PipeTransform, OnDestroy {

  // As an optimization, we cache the result unless the calendar or language change
  private _subscriptions: Subscription;
  private _wsTimeFormat: TimeFormat;
  private _formattedValue: string;

  private _isoDate: string;
  private _granularity: TimeGranularity;
  private _markForCheck = true;

  private markForCheck() {
    this._markForCheck = true;
  }

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    this._subscriptions = new Subscription();
    this._subscriptions.add(this.translate.onLangChange.subscribe(() => {
      this.markForCheck(); // In case day period (AM/PM) was used
    }));
    this._subscriptions.add(this.workspace.stateChanged$.subscribe({
      next: () => {

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

  transform(isoDate: string, granularity?: TimeGranularity): string {
    if (this._markForCheck || this._isoDate !== isoDate || this._granularity !== granularity) {
      // For caching
      this._markForCheck = false;
      this._isoDate = isoDate;
      this._granularity = granularity;

      // For change detection
      this._wsTimeFormat = this.workspace.timeFormat;

      this._formattedValue = timeFormat(isoDate, this.workspace, this.translate, granularity);

    }

    return this._formattedValue;
  }
}
