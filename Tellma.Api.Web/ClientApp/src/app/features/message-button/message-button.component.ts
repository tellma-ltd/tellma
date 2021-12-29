// tslint:disable:member-ordering
import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgbModal, NgbModalRef, Placement } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Observable, of, Subscription } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { MessageCommandPreview, MessagePreview } from '~/app/data/dto/message-command-preview';
import { Cardinality, NotificationUsage } from '~/app/data/entities/notification-template';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-message-button',
  templateUrl: './message-button.component.html',
  styles: [
  ]
})
export class MessageButtonComponent implements OnInit {

  @Input()
  messageCommandPreview: (template: MessageTemplate) => Observable<MessageCommandPreview>;

  @Input()
  sendMessage: (template: MessageTemplate, version: string) => Observable<void>;

  @Input()
  messageTemplates: MessageTemplate[];

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  @ViewChild('successModal', { static: true })
  successModal: TemplateRef<any>;

  public modalSuccessMessage: string;
  public modalErrorMessage: string;

  constructor(
    private workspace: WorkspaceService,
    public modalService: NgbModal,
    private translate: TranslateService) { }

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

  ////////////////////// Send Message

  @ViewChild('messageListModal', { static: true })
  messageListModal: TemplateRef<any>;

  get showSendMessage(): boolean {
    const templates = this.messageTemplates;
    return !!templates && templates.length > 0;
  }

  get canSendMessage(): boolean {
    return true;
  }

  public isMessageCommandLoading = false;
  public messageCommandError: () => string;

  public messageTemplate: MessageTemplate;
  public preview: () => Observable<MessageCommandPreview>;
  public messageCommand: MessageCommandPreview;
  // private messageVersion: string;

  public onSendMessageModal(template: MessageTemplate) {
    this.messageTemplate = template;
    this.preview = () => this.messageCommandPreview(template).pipe(tap(cmd => this.messageCommand = cmd));
    delete this.messageCommand;

    this.modalService.open(this.messageListModal, { windowClass: 't-master-modal t-notification-modal' });
  }

  public get total() {
    return !!this.messageCommand ? this.messageCommand.Messages.length : 0;
  }

  public get canConfirmSendMessage(): boolean {
    return this.hasPermissionToSendMessage() && !!this.messageCommand && !this.isMessageCommandLoading;
  }

  public onConfirmSendMessage(modal: NgbModalRef) {

    const template = this.messageTemplate;

    this.sendMessage(template, this.messageCommand.Version).subscribe(
      () => {
        modal.close();
        const key = 'SendMessageSuccessMessage';
        const msg = this.translate.instant(key);
        this.displaySuccessMessage(msg);
      },
      (friendlyError) => {
        this.displayErrorMessage(friendlyError.error);
      });
  }

  public hasPermissionToSendMessage() {
    return !!this.messageTemplate && this.messageTemplate.canSend();
  }

  public sendMessageTooltip() {
    return this.hasPermissionToSendMessage() ? undefined : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public get dropdownPlacement() {
    return this.workspace.ws.isRtl ? 'top-right' : 'top-left';
  }

  private _messageCommandPreviewTemplate: MessageTemplate;
  private _messageCommandPreview: () => Observable<MessageCommandPreview>;

  public messageCommandPreviewFactory: (t: MessageTemplate) => () => Observable<MessageCommandPreview> =
    (t: MessageTemplate) => {
      if (!t) {
        delete this._messageCommandPreviewTemplate;
        delete this._messageCommandPreview;
      } else if (this._messageCommandPreviewTemplate !== t) {
        this._messageCommandPreviewTemplate = t;
        this._messageCommandPreview = () => this.messageCommandPreview(t);
      }

      return this._messageCommandPreview;
    }
}

export interface MessageTemplate {
  name: () => string;
  templateId: number;
  usage: NotificationUsage;
  cardinality: Cardinality;
  canSend: () => boolean;
}
