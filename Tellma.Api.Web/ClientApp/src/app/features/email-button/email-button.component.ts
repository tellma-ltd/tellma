// tslint:disable:member-ordering
import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NgbModal, NgbModalRef, Placement } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { EmailTemplateForClient } from '~/app/data/dto/definitions-for-client';
import { IdResult } from '~/app/data/dto/id-result';
import { EmailCommandPreview, EmailCommandVersions, EmailPreview } from '~/app/data/dto/email-command-preview';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-email-button',
  templateUrl: './email-button.component.html',
  styles: [
  ]
})
export class EmailButtonComponent implements OnInit {

  @Input()
  emailCommandPreview: (template: EmailTemplateForClient) => Observable<EmailCommandPreview>;

  @Input()
  emailPreview: (template: EmailTemplateForClient, index: number, version?: string) => Observable<EmailPreview>;

  @Input()
  sendEmail: (template: EmailTemplateForClient, versions: EmailCommandVersions) => Observable<IdResult>;

  @Input()
  emailTemplates: EmailTemplateForClient[];

  @ViewChild('errorModal', { static: true })
  errorModal: TemplateRef<any>;

  @ViewChild('successModal', { static: true })
  successModal: TemplateRef<any>;

  public modalErrorMessage: string;

  constructor(
    private workspace: WorkspaceService,
    private modalService: NgbModal,
    private translate: TranslateService) { }

  ngOnInit(): void {
  }

  public get actionsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  get tenantId(): number {
    return this.workspace.ws.tenantId;
  }

  ////////////////////// Send Email

  @ViewChild('emailPreviewerModal', { static: true })
  emailPreviewerModal: TemplateRef<any>;

  get showSendEmail(): boolean {
    const templates = this.emailTemplates;
    return !!templates && templates.length > 0;
  }

  get canSendEmail(): boolean {
    return true;
  }

  public isEmailCommandLoading = false;
  public emailCommandError: () => string;

  public emailTemplate: EmailTemplateForClient;
  public preview: () => Observable<EmailCommandPreview>;
  public previewE: (index: number, version?: string) => Observable<EmailCommandPreview>;
  public emailCommand: EmailCommandPreview;
  public emailVersions: EmailCommandVersions;

  public onSendEmailModal(template: EmailTemplateForClient) {
    this.emailTemplate = template;
    this.preview = () => this.emailCommandPreview(template).pipe(tap(cmd => {
      this.emailCommand = cmd;

      // Fix versions
      this.emailVersions = { Version: cmd.Version, Emails: [] };
      if (!!cmd.Emails) {
        for (let i = 0; i < cmd.Emails.length; i++) {
          const email = cmd.Emails[i];
          if (!!email.Version) {
            this.emailVersions.Emails.push({ Index: i, Version: email.Version });
          }
        }
      }
    }));
    this.previewE = (i, v) => this.emailPreview(template, i, v).pipe(tap(email => {
      this.emailVersions.Emails.push({ Index: i, Version: email.Version });
    }));

    delete this.emailCommand;
    delete this.emailVersions;

    this.modalService.open(this.emailPreviewerModal, { windowClass: 't-master-modal t-notification-modal' });
  }

  public get total() {
    return !!this.emailCommand ? this.emailCommand.Emails.length : 0;
  }

  public get canConfirmSendEmail(): boolean {
    return this.hasPermissionToSendEmail() && !!this.emailCommand && !this.isEmailCommandLoading;
  }

  public onConfirmSend(modal: NgbModalRef) {

    const template = this.emailTemplate;

    this.sendEmail(template, this.emailVersions).subscribe(
      (idResult: IdResult) => {
        modal.close();
        this.commandId = idResult.Id;
        this.modalService.open(this.successModal);
      },
      (friendlyError) => {
        this.modalErrorMessage = friendlyError.error;
        this.modalService.open(this.errorModal);
      });
  }

  public commandId: number;

  public sendEmailTooltip() {
    return this.hasPermissionToSendEmail() ? undefined : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public get dropdownPlacement() {
    return this.workspace.ws.isRtl ? 'top-right' : 'top-left';
  }

  private _emailCommandPreviewTemplate: EmailTemplateForClient;
  private _emailCommandPreview: () => Observable<EmailCommandPreview>;

  public emailCommandPreviewFactory: (t: EmailTemplateForClient) => () => Observable<EmailCommandPreview> =
    (t: EmailTemplateForClient) => {
      if (!t) {
        delete this._emailCommandPreviewTemplate;
        delete this._emailCommandPreview;
      } else if (this._emailCommandPreviewTemplate !== t) {
        this._emailCommandPreviewTemplate = t;
        this._emailCommandPreview = () => this.emailCommandPreview(t);
      }

      return this._emailCommandPreview;
    }

  public display(template: EmailTemplateForClient) {
    return this.workspace.currentTenant.getMultilingualValueImmediate(template, 'Name');
  }

  public hasPermissionToSendEmail() {
    return this.workspace.currentTenant.canDo(`email-commands/${this.emailTemplate.EmailTemplateId}`, 'Send', null);
  }
}
