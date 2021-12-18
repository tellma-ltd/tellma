// tslint:disable:member-ordering
import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgbModal, NgbModalRef, Placement } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Observable, of, Subscription } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { EmailCommandPreview, EmailCommandVersions, EmailPreview } from '~/app/data/dto/email-command-preview';
import { Cardinality, NotificationUsage } from '~/app/data/entities/notification-template';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-email-button',
  templateUrl: './email-button.component.html',
  styles: [
  ]
})
export class EmailButtonComponent implements OnInit {

  @Input()
  emailCommandPreview: (template: EmailTemplate) => Observable<EmailCommandPreview>;

  @Input()
  emailPreview: (template: EmailTemplate, index: number, version?: string) => Observable<EmailPreview>;

  @Input()
  sendEmail: (template: EmailTemplate, versions?: EmailCommandVersions) => Observable<void>;

  @Input()
  emailTemplates: EmailTemplate[];

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  @ViewChild('successModal', { static: true })
  successModal: TemplateRef<any>;

  public modalSuccessMessage: string;
  public modalErrorMessage: string;

  constructor(
    private workspace: WorkspaceService,
    public modalService: NgbModal,
    private translate: TranslateService,
    private router: Router) { }

  ngOnInit(): void {
  }

  public get actionsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  private displayErrorMessage(errorMessage: string): void {
    this.modalErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  public displaySuccessMessage(message: string) {
    this.modalSuccessMessage = message;
    this.modalService.open(this.successModal);
  }

  get tenantId(): number {
    return this.workspace.ws.tenantId;
  }

  ////////////////////// Send Email

  @ViewChild('emailListModal', { static: true })
  emailListModal: TemplateRef<any>;

  @ViewChild('emailModal', { static: true })
  emailModal: TemplateRef<any>;

  // private _emailTemplatesDefinitions: DefinitionsForClient;
  // private _emailTemplatesCollection: string;
  // private _emailTemplatesDefinitionId: number;
  // private _emailTemplatesResult: EmailTemplate[];

  // public get emailTemplates(): EmailTemplate[] {
  //   if (!this.workspace.isApp) { // Emails are not supported in admin atm
  //     return [];
  //   }

  //   const ws = this.workspace.currentTenant;
  //   const collection = this.collection;
  //   const defId = this.definitionId;
  //   if (this._emailTemplatesDefinitions !== ws.definitions ||
  //     this._emailTemplatesCollection !== collection ||
  //     this._emailTemplatesDefinitionId !== defId) {

  //     this._emailTemplatesDefinitions = ws.definitions;
  //     this._emailTemplatesCollection = collection;
  //     this._emailTemplatesDefinitionId = defId;

  //     const result: EmailTemplate[] = [];

  //     const def = ws.definitions;
  //     const templates = Object.values(def.NotificationTemplates || {})
  //       .filter(e => e.Collection === collection && e.DefinitionId === defId && e.Usage === 'FromSearchAndDetails');

  //     for (const template of templates) {
  //       result.push({
  //         name: () => ws.getMultilingualValueImmediate(template, 'Name'),
  //         templateId: template.NotificationTemplateId,
  //         cardinality: template.Cardinality
  //       });
  //     }

  //     this._emailTemplatesResult = result;
  //   }

  //   return this._emailTemplatesResult;
  // }

  get showSendEmail(): boolean {
    const templates = this.emailTemplates;
    return !!templates && templates.length > 0;
  }

  get canSendEmail(): boolean {
    return true;
  }

  public isEmailCommandLoading = false;
  public emailCommandError: () => string;

  public emailCommand: EmailCommandPreview;
  public emailTemplate: EmailTemplate;
  private emailVersions: EmailCommandVersions;

  public onSendEmailModal(template: EmailTemplate) {
    this.emailTemplate = template;
    this.emailVersions = { Emails: [] };

    let sub: Subscription;
    const clear = () => {
      this.isEmailCommandLoading = false;
      this.isEmailLoading = false;
      this.emailCommandError = null;
      this.emailError = null;
      this.emailCommand = null;
      this.email = null;
      if (!!sub) {
        sub.unsubscribe();
      }
    };

    clear(); // Clear everything
    this.isEmailCommandLoading = true;

    sub = this.emailCommandPreview(template).pipe(
      tap((cmd: EmailCommandPreview) => {
        const email = cmd.Emails[0];
        this.emailCommand = cmd;
        this.email = email;
        this.emailVersions.Version = cmd.Version;
        if (!!email && !!email.Version) {
          this.emailVersions.Emails.push({ Index: 0, Version: email.Version });
        }
      }),
      catchError(friendlyError => {
        this.emailCommandError = () => friendlyError.error;
        return of();
      }),
      finalize(() => {
        this.isEmailCommandLoading = false;
      })
    ).subscribe();

    // IF there are unsaved changes, prompt the user asking if they would like them discarded
    const modal = template.cardinality === 'Bulk' ?
      this.modalService.open(this.emailListModal, { windowClass: 't-master-modal' }) :
      this.modalService.open(this.emailModal, { windowClass: 't-email-modal' });

    modal.result.then(clear, clear);
  }

  public isEmailLoading = false;
  public emailError: () => string;
  public email: EmailPreview;

  public onPreviewEmail(email: EmailPreview) {

    let sub: Subscription;
    const clear = () => {
      this.isEmailLoading = false;
      this.emailError = null;
      this.email = null;
      if (!!sub) {
        sub.unsubscribe();
      }
    };

    if (!!email.Version) {
      clear();
      this.email = email;

    } else {
      const template = this.emailTemplate;
      const emailCommand = this.emailCommand;
      const index = emailCommand.Emails.indexOf(email);
      const version = emailCommand.Version;

      if (index < 0) {
        console.error('Bug: Could not find email in the list.');
        return;
      }

      clear();
      this.isEmailLoading = true;

      sub = this.emailPreview(template, index, version).pipe(
        tap((serverEmail: EmailPreview) => {
          this.email = serverEmail;
          emailCommand.Emails[index] = serverEmail;
          emailCommand.Emails = emailCommand.Emails.slice(); // To trigger list refresh

          this.emailVersions.Emails.push({ Index: index, Version: serverEmail.Version });
        }),
        catchError(friendlyError => {
          this.emailError = () => friendlyError.error;
          return of();
        }),
        finalize(() => {
          this.isEmailLoading = false;
        })
      ).subscribe();
    }

    // IF there are unsaved changes, prompt the user asking if they would like them discarded
    const modal = this.modalService.open(this.emailModal, { windowClass: 't-email-modal' });
    modal.result.then(clear, clear);
  }

  public get isSingleEmail(): boolean {
    return !!this.emailTemplate && this.emailTemplate.cardinality === 'Single';
  }

  public get canConfirmSendEmail(): boolean {
    return this.hasPermissionToSendEmail() && !!this.emailCommand && !this.isEmailCommandLoading && !this.isEmailLoading;
  }

  public onConfirmSendEmail(modal: NgbModalRef) {

    const template = this.emailTemplate;

    this.sendEmail(template, this.emailVersions).subscribe(
      () => {
        modal.close();
        const key = template.cardinality === 'Bulk' ? 'SendEmailsSuccessMessage' : 'SendEmailSuccessMessage';
        const msg = this.translate.instant(key);
        this.displaySuccessMessage(msg);
      },
      (friendlyError) => {
        this.displayErrorMessage(friendlyError.error);
      });
  }

  public hasPermissionToSendEmail() {
    return !!this.emailTemplate && this.emailTemplate.canSend();
  }

  public sendEmailTooltip() {
    return this.hasPermissionToSendEmail() ? undefined : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public get dropdownPlacement() {
    return this.workspace.ws.isRtl ? 'top-right' : 'top-left';
  }
}

export interface EmailTemplate {
  name: () => string;
  templateId: number;
  usage: NotificationUsage;
  cardinality: Cardinality;
  canSend: () => boolean;
}
