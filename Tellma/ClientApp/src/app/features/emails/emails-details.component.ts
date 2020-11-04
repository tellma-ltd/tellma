import { Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { StatePropDescriptor } from '~/app/data/entities/base/metadata';
import { EmailState, metadata_Email } from '~/app/data/entities/email';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 't-emails-details',
  templateUrl: './emails-details.component.html',
  styles: []
})
export class EmailsDetailsComponent extends DetailsBaseComponent implements OnDestroy {

  public expand = '';
  private _stateDesc: StatePropDescriptor;
  private _url: string;
  private _body: string;
  private _iframe: ElementRef;

  @ViewChild('iframe')
  public set iframe(v: ElementRef) {
    if (this._iframe !== v) {
      if (!!this._iframe) {
        (this._iframe.nativeElement as HTMLIFrameElement).contentWindow.location.replace(undefined);
      }
      this._iframe = v;
      if (!!this._iframe) {
        (this._iframe.nativeElement as HTMLIFrameElement).contentWindow.location.replace(this._url);
      }
    }
  }

  public get iframe(): ElementRef {
    return this._iframe;
  }


  constructor(private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  ngOnDestroy(): void {
    this.emailBodyCleanup();
    super.ngOnDestroy();
  }

  private emailBodyCleanup() {
    if (!!this._url) {
      window.URL.revokeObjectURL(this._url);
      delete this._url;
    }
    delete this._body;
    if (!!this.iframe) {
      (this.iframe.nativeElement as HTMLIFrameElement).contentWindow.location.replace(undefined);
    }
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public stateDisplay(state: EmailState) {
    if (!this._stateDesc) {
      const meta = metadata_Email(null, this.translate);
      this._stateDesc = meta.properties.State as StatePropDescriptor;
    }

    return this._stateDesc.format(state);
  }

  public stateColor(state: EmailState) {
    if (!this._stateDesc) {
      const meta = metadata_Email(null, this.translate);
      this._stateDesc = meta.properties.State as StatePropDescriptor;
    }

    return this._stateDesc.color(state);
  }

//   public viewBody(body: string): void {

//     // Wrap the content inside a simple UTF-8 document and launch it in a new tab
//     const html = `<!DOCTYPE html><html><head><meta charset="utf-8"></head><body style="margin: 0; padding: 1rem;">
//       ${body || ''}
// </body></html>`;
//     const blob = new Blob([html], { type: 'text/html' });
//     const url = window.URL.createObjectURL(blob) + '#toolbar=0&navpanes=0&scrollbar=0';
//     window.open(url);
//   }

  public watch(body: string) {
    if (!body) {
      this.emailBodyCleanup();
    } else if (body !== this._body) {
      this._body = body;

      const html = `<!DOCTYPE html>
<html>
<head>
  <meta charset="utf-8">
</head>
<body style="margin: 0; padding: 1rem;">
  <div style="display: flex; justify-content: center;">
    ${body}
  </div>
</body>
</html>`;

      const blob = new Blob([html], { type: 'text/html' });
      this._url = window.URL.createObjectURL(blob) + '#toolbar=0&navpanes=0&scrollbar=0';

      if (!!this.iframe) {
        (this.iframe.nativeElement as HTMLIFrameElement).contentWindow.location.replace(this._url);
      }
    }

    return !!body;
  }
}
