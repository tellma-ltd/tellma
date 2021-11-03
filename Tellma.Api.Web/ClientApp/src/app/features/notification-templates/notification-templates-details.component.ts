// tslint:disable:member-ordering
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NgControl } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { merge, Observable, of, Subject, Subscription } from 'rxjs';
import { catchError, debounceTime, finalize, switchMap, tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { EmailCommandPreview, EmailPreview } from '~/app/data/dto/email-command-preview';
import { PrintEntitiesArguments } from '~/app/data/dto/print-arguments';
import { collectionsWithEndpoint, Control, hasControlOptions, metadata, simpleControls } from '~/app/data/entities/base/metadata';
import {
  NotificationTemplate,
  NotificationTemplateAttachmentForSave,
  NotificationTemplateForSave,
  NotificationTemplateParameterForSave,
  NotificationTemplateSubscriberForSave
} from '~/app/data/entities/notification-template';
import { onCodeTextareaKeydown } from '~/app/data/util';
import { MasterDetailsStore, PrintStore, WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-notification-templates-details',
  templateUrl: './notification-templates-details.component.html',
  styles: [
  ]
})
export class NotificationTemplatesDetailsComponent extends DetailsBaseComponent implements OnInit, OnDestroy {

  private notificationsApi = this.api.notificationTemplatesApi(this.notifyDestruct$); // for intellisense
  private localState = new PrintStore();  // Used in popup mode

  private _sections: { [key: string]: boolean } = {
    Title: false,
    Behavior: true
  };

  public expand = 'Parameters,Subscribers.User,Attachments.PrintingTemplate,ReportDefinition';
  public collapseEditor = false;
  public collapseMetadata = false;

  constructor(
    private workspace: WorkspaceService,
    private translate: TranslateService,
    private modalService: NgbModal,
    private api: ApiService) {
    super();
  }

  create = () => {
    const result: NotificationTemplateForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsDeployed = false;
    result.Channel = 'Email';
    result.Trigger = 'Automatic';
    result.Cardinality = 'Single';
    result.Usage = 'FromSearchAndDetails';
    result.Collection = 'Document';
    result.Body = defaultEmailBody;

    result.Parameters = [];
    result.Attachments = [];
    result.Subscribers = [];

    return result;
  }

  clone: (item: NotificationTemplate) => NotificationTemplate = (item: NotificationTemplate) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as NotificationTemplate;
      delete clone.Id;
      if (!!clone.Parameters) {
        clone.Parameters.forEach(e => delete e.Id);
      }
      if (!!clone.Attachments) {
        clone.Attachments.forEach(e => delete e.Id);
      }
      if (!!clone.Subscribers) {
        clone.Subscribers.forEach(e => delete e.Id);
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  public savePreprocessing = (model: NotificationTemplateForSave) => {
    // Server validation on hidden collections will be confusing to the user

    if (!this.showParameters(model)) {
      model.Parameters = [];
    }
    if (!this.showAttachments(model)) {
      model.Attachments = [];
    }
    if (!this.showSubscribers(model)) {
      model.Subscribers = [];
    }
  }

  ngOnInit(): void {
    this.notificationsApi = this.api.notificationTemplatesApi(this.notifyDestruct$);

    // Hook the fetch signals
    const templateSignals = this.notifyDelayedFetch$.pipe(
      debounceTime(300),
    );

    const otherSignals = this.notifyFetch$;
    const allSignals = merge(templateSignals, otherSignals);

    this._subscriptions.add(allSignals.pipe(
      switchMap(_ => this.doFetch())
    ).subscribe());
  }

  public get state(): PrintStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.isPopupMode) {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new PrintStore();
      }

      return this.localState;
    } else {

      // screen mode on the other hand use the global state
      return this.globalState;
    }
  }

  private get globalState(): PrintStore {
    const key = 'notification-templates';
    const rs = this.workspace.currentTenant.notificationState;
    if (!rs[key]) {
      rs[key] = new PrintStore();
    }

    return rs[key];
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // UI Binding

  public isInactive: (model: NotificationTemplate) => string = (_: NotificationTemplate) => null;

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public onToggleEditor(): void {
    this.collapseEditor = !this.collapseEditor;
  }

  public onToggleMetadata(): void {
    this.collapseMetadata = !this.collapseMetadata;
  }

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public titleSectionErrors(model: NotificationTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Name) ||
      areServerErrors(model.serverErrors.Name2) ||
      areServerErrors(model.serverErrors.Name3) ||
      areServerErrors(model.serverErrors.Code) ||
      areServerErrors(model.serverErrors.Caption) ||
      areServerErrors(model.serverErrors.Description) ||
      areServerErrors(model.serverErrors.Description2) ||
      areServerErrors(model.serverErrors.Description3)
    );
  }

  public behaviorSectionErrors(model: NotificationTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Channel) ||
      areServerErrors(model.serverErrors.Trigger) ||
      areServerErrors(model.serverErrors.Cardinality) ||
      areServerErrors(model.serverErrors.ListExpression) ||
      areServerErrors(model.serverErrors.Schedule) ||
      areServerErrors(model.serverErrors.ConditionExpression) ||
      areServerErrors(model.serverErrors.MaximumRenotify) ||

      areServerErrors(model.serverErrors.Usage) ||
      areServerErrors(model.serverErrors.Collection) ||
      areServerErrors(model.serverErrors.DefinitionId) ||
      areServerErrors(model.serverErrors.ReportDefinitionId) ||
      areServerErrors(model.serverErrors.Subject) ||
      areServerErrors(model.serverErrors.AddressExpression) ||
      areServerErrors(model.serverErrors.IsDeployed)
    ) ||
      (!!model.Parameters && model.Parameters.some(e => this.weakEntityErrors(e))) ||
      (!!model.Attachments && model.Attachments.some(e => this.weakEntityErrors(e))) ||
      (!!model.Subscribers && model.Subscribers.some(e => this.weakEntityErrors(e)));
  }

  public weakEntityErrors(model:
    NotificationTemplateParameterForSave | NotificationTemplateAttachmentForSave | NotificationTemplateSubscriberForSave) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public metadataPaneErrors(model: NotificationTemplate) {
    return this.titleSectionErrors(model) || this.behaviorSectionErrors(model);
  }

  public showUsageFields(model: NotificationTemplateForSave) {
    return model.Trigger === 'Manual';
  }

  public showListExpression(model: NotificationTemplateForSave) {
    return model.Cardinality === 'Bulk';
  }

  public showSchedule(model: NotificationTemplateForSave) {
    return model.Trigger === 'Automatic';
  }

  public showConditionExpression(model: NotificationTemplateForSave) {
    return model.Trigger === 'Automatic';
  }

  public showMaximumRenotify(model: NotificationTemplateForSave) {
    return model.Trigger === 'Automatic' && model.Cardinality === 'Single';
  }

  public showAddressExpression(model: NotificationTemplateForSave) {
    return model.Cardinality === 'Bulk';
  }

  public showEmailBodyEditor(model: NotificationTemplateForSave) {
    return model.Channel === 'Email';
  }

  public showSmsBodyEditor(model: NotificationTemplateForSave) {
    return model.Channel === 'Sms';
  }

  public onChannelChange(model: NotificationTemplateForSave) {
    if (model.Channel === 'Sms' && model.Body === defaultEmailBody) {
      model.Body = defaultSmsBody;
    }

    if (model.Channel === 'Email' && model.Body === defaultSmsBody) {
      model.Body = defaultEmailBody;
    }
  }

  public get allCollections(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate);
  }

  public showDefinitionIdSelector(model: NotificationTemplateForSave): boolean {
    return !!model && !!model.Collection && !!metadata[model.Collection](this.workspace, this.translate, null).definitionIds;
  }

  public allDefinitionIds(model: NotificationTemplateForSave): SelectorChoice[] {
    if (!!model && !!model.Collection) {
      const func = metadata[model.Collection];
      const desc = func(this.workspace, this.translate, null);
      if (!!desc.definitionIds && !desc.definitionIdsArray) {
        desc.definitionIdsArray = desc.definitionIds
          .map(defId => ({ value: defId, name: func(this.workspace, this.translate, defId).titlePlural }));
      }

      return desc.definitionIdsArray;
    } else {
      return null;
    }
  }

  public showCollectionAndDefinition(model: NotificationTemplateForSave) {
    return model.Usage === 'FromDetails' || model.Usage === 'FromSearchAndDetails';
  }

  public showReportDefinition(model: NotificationTemplateForSave) {
    return false; // model.Usage === 'FromReport';
  }

  public onKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent, model: NotificationTemplate) {
    onCodeTextareaKeydown(elem, $event, v => model.Body = v);
  }

  //////////////// Config Modal

  @ViewChild('configModal', { static: true })
  configModal: TemplateRef<any>;

  configType: 'Parameter' | 'Subscriber' | 'Attachment';
  modelRef: NotificationTemplateForSave;

  public get isParameter() { return this.configType === 'Parameter'; }
  public get isSubscriber() { return this.configType === 'Subscriber'; }
  public get isAttachment() { return this.configType === 'Attachment'; }

  public canApply() {
    if (this.isParameter) {
      return this.canApplyParam(this.paramToEdit);
    }
    if (this.isAttachment) {
      return this.canApplyAttachment(this.attToEdit);
    }
    if (this.isSubscriber) {
      return this.canApplySubscriber(this.subToEdit);
    }
  }

  public drop(event: CdkDragDrop<any[]>, model: NotificationTemplateForSave) {

    // The source and destination collection
    const source = event.previousContainer.data;
    const sourceIndex = event.previousIndex;
    const destination = event.container.data;
    const destinationIndex = event.currentIndex;

    if (source === destination && sourceIndex !== destinationIndex) {
      // Reorder within array
      moveItemInArray(destination, sourceIndex, destinationIndex);
      this.onTemplateChange(model);
    }
  }

  //////////////// Parameters

  public showParameters(model: NotificationTemplateForSave) {
    return model.Trigger === 'Manual';
  }

  public getParameters(model: NotificationTemplateForSave): NotificationTemplateParameterForSave[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }

  paramToEdit: NotificationTemplateParameterForSave;
  paramToEditHasChanged = false;

  public onConfigureParameter(index: number, model: NotificationTemplateForSave) {
    this.configType = 'Parameter';
    this.paramToEditHasChanged = false;
    const itemToEdit = { ...model.Parameters[index] } as NotificationTemplateParameterForSave;
    this.paramToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.configModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.paramToEditHasChanged) {

        model.Parameters[index] = itemToEdit;
        this.onTemplateChange(model);
      }
    }, (_: any) => { });
  }

  public onCreateParameter(model: NotificationTemplate) {
    this.onConfigureParameter(model.Parameters.length, model);
  }

  public onDeleteParameter(index: number, model: NotificationTemplate) {
    model.Parameters.splice(index, 1);
    this.onTemplateChange(model);
  }

  public controlSimpleChoices(): SelectorChoice[] {
    return simpleControls(this.translate);
  }

  public controlEntityChoices(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate, true);
  }

  public showOptions(control: Control) {
    return !!control && hasControlOptions(control);
  }

  public canApplyParam(p: NotificationTemplateParameterForSave): boolean {
    return !!p.Key && !!p.Label && !!p.Control;
  }

  //////////////// Subscribers

  public showSubscribers(model: NotificationTemplateForSave) {
    return model.Cardinality === 'Single';
  }

  public showUser() {
    const sub = this.subToEdit;
    return sub.AddressType === 'User';
  }

  public showEmail() {
    const model = this.modelRef;
    const sub = this.subToEdit;
    return model.Channel === 'Email' && sub.AddressType === 'Text';
  }

  public showPhone() {
    const model = this.modelRef;
    const sub = this.subToEdit;
    return model.Channel === 'Sms' && sub.AddressType === 'Text';
  }

  public getSubscribers(model: NotificationTemplateForSave): NotificationTemplateSubscriberForSave[] {
    model.Subscribers = model.Subscribers || [];
    return model.Subscribers;
  }

  subToEdit: NotificationTemplateSubscriberForSave;
  subToEditHasChanged = false;

  public onConfigureSubscriber(index: number, model: NotificationTemplateForSave) {
    this.configType = 'Subscriber';
    this.subToEditHasChanged = false;
    const itemToEdit = { ...model.Subscribers[index] } as NotificationTemplateSubscriberForSave;
    itemToEdit.AddressType = itemToEdit.AddressType || 'User';
    this.subToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.configModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.subToEditHasChanged) {

        model.Subscribers[index] = itemToEdit;
        this.onTemplateChange(model);
      }
    }, (_: any) => { });
  }

  public onCreateSubscriber(model: NotificationTemplate) {
    this.onConfigureSubscriber(model.Subscribers.length, model);
  }

  public onDeleteSubscriber(index: number, model: NotificationTemplate) {
    model.Subscribers.splice(index, 1);
    this.onTemplateChange(model);
  }

  public displaySubscriber(s: NotificationTemplateSubscriberForSave, model: NotificationTemplateForSave) {
    if (s.AddressType === 'User') {
      return this.ws.getMultilingualValue('User', s.UserId, 'Name');
    }

    if (s.AddressType === 'Text') {
      const smsEnabled = this.ws.settings.SmsEnabled;

      if (model.Channel === 'Email' || !smsEnabled) {
        return s.Email;
      }
      if (model.Channel === 'Sms' && smsEnabled) {
        return s.Phone;
      }
    }
  }

  public canApplySubscriber(s: NotificationTemplateSubscriberForSave): boolean {
    const model = this.modelRef;
    return !!s.AddressType &&
      (
        (s.AddressType === 'User' && !!s.UserId) ||
        (s.AddressType === 'Text' && (
          (model.Channel === 'Email' && !!s.Email) ||
          (model.Channel === 'Sms' && !!s.Phone)
        )
        )
      );
  }

  //////////////// Attachments

  public showAttachments(model: NotificationTemplateForSave) {
    return model.Channel === 'Email';
  }

  attToEdit: NotificationTemplateAttachmentForSave;
  attToEditHasChanged = false;

  public getAttachments(model: NotificationTemplateForSave): NotificationTemplateAttachmentForSave[] {
    model.Attachments = model.Attachments || [];
    return model.Attachments;
  }

  public onConfigureAttachment(index: number, model: NotificationTemplateForSave) {
    this.configType = 'Attachment';
    this.attToEditHasChanged = false;
    const itemToEdit = { ...model.Attachments[index] } as NotificationTemplateAttachmentForSave;
    this.attToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.configModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.attToEditHasChanged) {

        model.Attachments[index] = itemToEdit;
        this.onTemplateChange(model);
      }
    }, (_: any) => { });
  }

  public onCreateAttachment(model: NotificationTemplate) {
    this.onConfigureAttachment(model.Attachments.length, model);
  }

  public onDeleteAttachment(index: number, model: NotificationTemplate) {
    model.Attachments.splice(index, 1);
    this.onTemplateChange(model);
  }

  public displayAttachment(s: NotificationTemplateAttachmentForSave) {
    return this.ws.getMultilingualValue('PrintingTemplate', s.PrintingTemplateId, 'Name');
  }

  public canApplyAttachment(a: NotificationTemplateAttachmentForSave): boolean {
    return !!a.PrintingTemplateId;
  }

  /////////// Preview

  public get filter(): string {
    return this.state.filter;
  }

  public set filter(v: string) {
    this.state.filter = v;
  }

  public get orderby(): string {
    return this.state.orderby;
  }

  public set orderby(v: string) {
    this.state.orderby = v;
  }

  public get top(): number {
    return this.state.top;
  }

  public set top(v: number) {
    this.state.top = v;
  }

  public get skip(): number {
    return this.state.skip;
  }

  public set skip(v: number) {
    this.state.skip = v;
  }

  public get id(): number | string {
    return this.state.id;
  }

  public set id(v: number | string) {
    this.state.id = v;
  }

  public get arguments() {
    return this.state.arguments;
  }

  public onArgumentChange() {
    this.fetch();
  }


  private template: NotificationTemplateForSave;

  public onTemplateChange(template: NotificationTemplateForSave): void {
    this.template = template;
    this.delayedFetch();
  }

  public onPreviewChange(): void {

  }


  public isEmailCommandLoading = false;
  public emailCommandError: () => string;

  public emailCommand: EmailCommandPreview;
  public email: EmailPreview;

  public onPreviewEmail(email: EmailPreview) {
  }

  private notifyFetch$ = new Subject<void>();
  private notifyDelayedFetch$ = new Subject<void>();

  private fetch(): void {
    this.notifyFetch$.next();
  }

  private delayedFetch(): void {
    this.notifyDelayedFetch$.next();
  }

  private doFetch(): Observable<void> {

    const template = this.template;
    const entityId = this.id;

    this.isEmailCommandLoading = true;

    let base$: Observable<EmailCommandPreview>;
    if (template.Usage === 'FromSearchAndDetails') {


      const args: PrintEntitiesArguments = {
        filter: this.filter,
        orderby: this.orderby,
        top: this.top,
        skip: this.skip
      };

      base$ = this.notificationsApi.emailCommandPreviewEntities(template, args, this.arguments);
    } else if (template.Usage === 'FromDetails') {
      // TODO
    } else {
      // TODO
    }

    base$ = template.Usage === 'FromSearchAndDetails' ?
      this.notificationsApi.emailCommandPreviewEntities(template, { i: [entityId] }) :
      null; // this.crud.emailCommandPreviewEntity(this.entityId, template.templateId);

    return base$.pipe(
      tap(cmd => {
        const email = cmd.Emails[0];
        this.emailCommand = cmd;
        this.email = email;
      }),
      catchError(friendlyError => {
        this.emailCommandError = () => friendlyError.error;
        return of(null);
      }),
      finalize(() => {
        this.isEmailCommandLoading = false;
      })
    );
  }
}

const defaultSmsBody = '';

// tslint:disable:no-trailing-whitespace
const defaultEmailBody = `<table style="border-collapse: collapse;font-family: sans-serif;">
    <tr>
        <td>

        </td>
    </tr>    
</table>`;
