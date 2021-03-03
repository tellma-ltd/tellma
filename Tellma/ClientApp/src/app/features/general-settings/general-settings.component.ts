import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { supportedCultures } from '~/app/data/supported-cultures';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { calendarsArray, HmsFormat, hmsFormatsArray, YmdFormat, ymdFormatsArray } from '~/app/data/entities/base/metadata-types';
import { TranslateService } from '@ngx-translate/core';
import { formatDate, formatTime } from '~/app/data/date-time-formats';
import { SettingsBaseComponent } from '~/app/shared/settings-base/settings-base';
import { NgbDateStruct, NgbTimeStruct } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 't-general-settings',
  templateUrl: './general-settings.component.html'
})
export class GeneralSettingsComponent extends SettingsBaseComponent {

  private _cultures: SelectorChoice[];
  private _calendars: SelectorChoice[];
  private _dateFormats: SelectorChoice[];
  private _timeFormats: SelectorChoice[];

  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  ////////// UI Bindings

  get primaryPostfix(): string {
    return this.workspace.currentTenant.primaryPostfix;
  }

  get secondaryPostfix(): string {
    return this.workspace.currentTenant.secondaryPostfix;
  }

  get ternaryPostfix(): string {
    return this.workspace.currentTenant.ternaryPostfix;
  }

  public cultureName(culture: string): string {
    return supportedCultures[culture];
  }

  get cultures(): SelectorChoice[] {

    if (!this._cultures) {
      this._cultures = Object.keys(supportedCultures)
        .map(key => ({ name: () => supportedCultures[key], value: key }));
    }

    return this._cultures;
  }

  public calendarName(calendar: string): string {
    return !!calendar ? this.translate.instant('Calendar_' + calendar) : null;
  }

  get calendars(): SelectorChoice[] {

    if (!this._calendars) {
      this._calendars = calendarsArray
        .map(c => ({ name: () => this.calendarName(c), value: c }));
    }

    return this._calendars;
  }

  public dateFormatDisplay(format: YmdFormat): string {
    const date: NgbDateStruct = { day: 1, month: 2, year: new Date().getFullYear() };
    return formatDate(date, format, this.translate, 'GC');
  }

  get dateFormats(): SelectorChoice[] {
    if (!this._dateFormats) {
      this._dateFormats = ymdFormatsArray
      .map(f => ({ name: () => this.dateFormatDisplay(f), value: f}));
    }

    return this._dateFormats;
  }

  public timeFormatDisplay(format: HmsFormat): string {
    const time: NgbTimeStruct = { hour: 13, minute: 5, second: 27 };
    return formatTime(time, format, this.translate);
  }

  get timeFormats(): SelectorChoice[] {
    if (!this._timeFormats) {
      this._timeFormats = hmsFormatsArray
      .map(f => ({ name: () => this.timeFormatDisplay(f), value: f}));
    }

    return this._timeFormats;
  }
}
