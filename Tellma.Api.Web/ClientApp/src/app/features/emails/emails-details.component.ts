// tslint:disable:member-ordering
import { Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '~/app/data/api.service';
import { EmailPreview } from '~/app/data/dto/email-command-preview';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { EmailForQuery, EmailState, metadata_Email } from '~/app/data/entities/email';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 't-emails-details',
  templateUrl: './emails-details.component.html',
  styles: []
})
export class EmailsDetailsComponent extends DetailsBaseComponent implements OnDestroy {

  private emailApi = this.api.emailsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Attachments';
  private _stateDesc: ChoicePropDescriptor;
  public extraParams = { includeBody: true };
  private body: string;

  constructor(private workspace: WorkspaceService, private translate: TranslateService, private api: ApiService) {
    super();
    this.emailApi = api.emailsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public handleFreshExtras = (extras: { [key: string]: any }) => {
    if (!!extras) {
      this.body = extras.Body as string;
    }
  }

  public stateDisplay(state: EmailState) {
    if (!this._stateDesc) {
      const meta = metadata_Email(null, this.translate);
      this._stateDesc = meta.properties.State as ChoicePropDescriptor;
    }

    return this._stateDesc.format(state);
  }

  public stateColor(state: EmailState) {
    if (!this._stateDesc) {
      const meta = metadata_Email(null, this.translate);
      this._stateDesc = meta.properties.State as ChoicePropDescriptor;
    }

    return this._stateDesc.color(state);
  }

  // NEW!

  private _email: EmailForQuery;
  private _body: string;
  private _emailPreviewResult: EmailPreview;

  public emailPreview(email: EmailForQuery): EmailPreview {
    if (this._email !== email ||
      this._body !== this.body) {
      this._email = email;
      this._body = this.body;

      const preview: EmailPreview = {};
      if (!!email.To) {
        preview.To = email.To.split(';').filter(e => !!e).map(e => e.trim());
        preview.Cc = email.Cc.split(';').filter(e => !!e).map(e => e.trim());
        preview.Bcc = email.Bcc.split(';').filter(e => !!e).map(e => e.trim());
        preview.Subject = email.Subject;
        preview.Body = this.body;
        preview.Attachments = (email.Attachments || []).map(a => ({
          DownloadName: a.Name,
          bodyResolver: this.emailApi.getAttachment(email.Id, a.Id)
        }));
      }

      this._emailPreviewResult = preview;
    }

    return this._emailPreviewResult;
  }
}
