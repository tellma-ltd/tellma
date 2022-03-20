// tslint:disable:member-ordering
import { Component, Input, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NgbModal, NgbModalRef, Placement } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { MessageTemplateForClient } from '~/app/data/dto/definitions-for-client';
import { IdResult } from '~/app/data/dto/id-result';
import { MessageCommandPreview } from '~/app/data/dto/message-command-preview';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-message-button',
  templateUrl: './message-button.component.html',
  styles: [
  ]
})
export class MessageButtonComponent implements OnInit {

  @Input()
  messageCommandPreview: (template: MessageTemplateForClient) => Observable<MessageCommandPreview>;

  @Input()
  sendMessage: (template: MessageTemplateForClient, version: string) => Observable<IdResult>;

  @Input()
  messageTemplates: MessageTemplateForClient[];

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

  ////////////////////// Send Message

  @ViewChild('messagePreviewerModal', { static: true })
  messagePreviewerModal: TemplateRef<any>;

  get showSendMessage(): boolean {
    const templates = this.messageTemplates;
    return !!templates && templates.length > 0;
  }

  get canSendMessage(): boolean {
    return true;
  }

  public isMessageCommandLoading = false;
  public messageCommandError: () => string;

  public messageTemplate: MessageTemplateForClient;
  public preview: () => Observable<MessageCommandPreview>;
  public messageCommand: MessageCommandPreview;
  // private messageVersion: string;

  public onSendMessageModal(template: MessageTemplateForClient) {
    this.messageTemplate = template;
    this.preview = () => this.messageCommandPreview(template).pipe(tap(cmd => this.messageCommand = cmd));
    delete this.messageCommand;

    this.modalService.open(this.messagePreviewerModal, { windowClass: 't-master-modal t-notification-modal' });
  }

  public get total() {
    return !!this.messageCommand ? this.messageCommand.Messages.length : 0;
  }

  public get canConfirmSendMessage(): boolean {
    return this.hasPermissionToSendMessage() && !!this.messageCommand && !this.isMessageCommandLoading;
  }

  public onConfirmSend(modal: NgbModalRef) {

    const template = this.messageTemplate;

    this.sendMessage(template, this.messageCommand.Version).subscribe(
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

  public sendMessageTooltip() {
    return this.hasPermissionToSendMessage() ? undefined : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public get dropdownPlacement() {
    return this.workspace.ws.isRtl ? 'top-right' : 'top-left';
  }

  private _messageCommandPreviewTemplate: MessageTemplateForClient;
  private _messageCommandPreview: () => Observable<MessageCommandPreview>;

  public messageCommandPreviewFactory: (t: MessageTemplateForClient) => () => Observable<MessageCommandPreview> =
    (t: MessageTemplateForClient) => {
      if (!t) {
        delete this._messageCommandPreviewTemplate;
        delete this._messageCommandPreview;
      } else if (this._messageCommandPreviewTemplate !== t) {
        this._messageCommandPreviewTemplate = t;
        this._messageCommandPreview = () => this.messageCommandPreview(t);
      }

      return this._messageCommandPreview;
    }

  public display(template: MessageTemplateForClient) {
    return this.workspace.currentTenant.getMultilingualValueImmediate(template, 'Name');
  }

  public hasPermissionToSendMessage() {
    return this.workspace.currentTenant.canDo(`message-commands/${this.messageTemplate.MessageTemplateId}`, 'Send', null);
  }
}
