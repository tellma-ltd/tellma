import { Component, OnDestroy, TemplateRef } from '@angular/core';
import { Subject } from 'rxjs';
import { WorkspaceService } from '~/app/data/workspace.service';
import { supportedCultures } from '~/app/data/supported-cultures';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { calendarsArray, HmsFormat, hmsFormatsArray, YmdFormat, ymdFormatsArray } from '~/app/data/entities/base/metadata-types';
import { TranslateService } from '@ngx-translate/core';
import { formatDate, formatTime } from '~/app/data/date-time-formats';
import { SettingsBaseComponent } from '~/app/shared/settings-base/settings-base';
import { NgbDateStruct, NgbModal, NgbModalRef, NgbTimeStruct } from '@ng-bootstrap/ng-bootstrap';
import { GeneralSettingsForSave } from '~/app/data/entities/general-settings';
import { ApiService } from '~/app/data/api.service';

@Component({standalone: false, 
  selector: 't-general-settings',
  templateUrl: './general-settings.component.html'
})
export class GeneralSettingsComponent extends SettingsBaseComponent implements OnDestroy {

  private _cultures: SelectorChoice[];
  private _calendars: SelectorChoice[];
  private _dateFormats: SelectorChoice[];
  private _timeFormats: SelectorChoice[];

  constructor(
    private workspace: WorkspaceService,
    private translate: TranslateService,
    private api: ApiService,
    private modalService: NgbModal) {
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

  public customFields(model: GeneralSettingsForSave) {
    model.CustomFields = model.CustomFields || {};
    return model.CustomFields;
  }

  ////////// Marmin (UAE e-invoicing)

  // SettingsBaseComponent has no destruct signal of its own, so this component owns one to keep
  // the secrets request from outliving the screen.
  private notifyDestruct$ = new Subject<void>();

  ngOnDestroy(): void {
    this.notifyDestruct$.next();
    this.notifyDestruct$.complete();
  }

  // Bound only while the modal is open. Never populated from the server: the secrets are
  // [JsonIgnore] on SettingsForClient and deliberately never reach the browser.
  public marminAeClientSecret: string;
  public marminAeWebhookSecret: string;
  public marminAeSecretsError: string;
  public marminAeSaving = false;

  /** The environment is DBA-set, so it is shown but not editable. */
  get marminAeEnvironment(): string {
    return this.workspace.currentTenant.settings.MarminAeEnvironment;
  }

  get marminAeSecretsValid(): boolean {
    return !!this.marminAeClientSecret || !!this.marminAeWebhookSecret;
  }

  public onSetMarminAeSecrets = (modalTemplate: TemplateRef<any>): void => {
    // Always start blank. Leaving a field empty means "keep the stored secret".
    this.marminAeClientSecret = null;
    this.marminAeWebhookSecret = null;
    this.marminAeSecretsError = null;
    this.marminAeSaving = false;

    this.modalService.open(modalTemplate);
  }

  public onSaveMarminAeSecrets = (modal: NgbModalRef): void => {
    if (!this.marminAeSecretsValid || this.marminAeSaving) {
      return;
    }

    this.marminAeSaving = true;
    this.marminAeSecretsError = null;

    this.api.generalSettingsApi(this.notifyDestruct$).saveMarminAeSecrets({
      ClientSecret: this.marminAeClientSecret,
      WebhookSecret: this.marminAeWebhookSecret
    }).subscribe({
      next: () => {
        // Do not keep the plaintext around any longer than the request needed it.
        this.marminAeClientSecret = null;
        this.marminAeWebhookSecret = null;
        this.marminAeSaving = false;
        modal.close();
      },
      error: (friendlyError: { error: string }) => {
        this.marminAeSaving = false;
        this.marminAeSecretsError = friendlyError.error;
      }
    });
  }

}
