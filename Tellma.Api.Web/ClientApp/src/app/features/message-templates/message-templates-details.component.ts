// tslint:disable:member-ordering
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NgControl } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { merge, Observable, of, Subject, Subscription, timer } from 'rxjs';
import { catchError, debounceTime, finalize, switchMap, tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { TemplateParameterForClient } from '~/app/data/dto/definitions-for-client';
import { MessageCommandPreview, MessagePreview } from '~/app/data/dto/message-command-preview';
import { PrintArguments, PrintEntitiesArguments, PrintEntityByIdArguments } from '~/app/data/dto/print-arguments';
import {
  ChoicePropDescriptor, Collection, collectionsWithEndpoint, Control,
  getChoices, hasControlOptions, metadata, PropVisualDescriptor, simpleControls
} from '~/app/data/entities/base/metadata';
import {
  MessageTemplate, MessageTemplateForSave, MessageTemplateParameterForSave,
  MessageTemplateSubscriberForSave, metadata_MessageTemplate
} from '~/app/data/entities/message-template';
import { descFromControlOptions, isSpecified, updateOn } from '~/app/data/util';
import { PrintStore, WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { SelectorChoice } from '~/app/shared/selector/selector.component';

@Component({
  selector: 't-message-templates-details',
  templateUrl: './message-templates-details.component.html',
  styles: [
  ]
})
export class MessageTemplatesDetailsComponent extends DetailsBaseComponent implements OnInit, OnDestroy {

  private messageApi = this.api.messageTemplatesApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parameters,Subscribers.User';

  constructor(
    private workspace: WorkspaceService,
    private translate: TranslateService,
    private modalService: NgbModal,
    private api: ApiService) {
    super();
  }

  create = () => {
    const result: MessageTemplateForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsDeployed = false;
    result.Trigger = 'Manual';
    result.Cardinality = 'Single';
    result.Usage = 'FromSearchAndDetails';
    result.Collection = 'Document';
    result.PreventRenotify = false;

    result.Parameters = [];
    result.Subscribers = [];

    return result;
  }

  clone: (item: MessageTemplate) => MessageTemplate = (item: MessageTemplate) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as MessageTemplate;
      delete clone.Id;
      if (!!clone.Parameters) {
        clone.Parameters.forEach(e => delete e.Id);
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

  public savePreprocessing = (model: MessageTemplateForSave) => {
    // Server validation on hidden collections will be confusing to the user

    if (!this.showParameters(model)) {
      model.Parameters = [];
    }
    if (!this.showSubscribers(model)) {
      model.Subscribers = [];
    }
  }

  ngOnInit(): void {
    this.messageApi = this.api.messageTemplatesApi(this.notifyDestruct$);

    // Hook the delayed signals
    const delayedSignals = this.notifyDelayedFetch$.pipe(
      debounceTime(300),
    );
    this._subscriptions.add(delayedSignals.subscribe(_ => this.refresh$.next()));
  }

  // State

  private localState = new PrintStore();  // Used in popup mode
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

  public isInactive: (model: MessageTemplate) => string = (_: MessageTemplate) => null;

  // Sections

  public collapseMetadata = false;

  public onToggleMetadata(): void {
    this.collapseMetadata = !this.collapseMetadata;
  }

  private _sections: { [key: string]: boolean } = {
    Title: false,
    Behavior: true,
    Content: true,
    Deploy: false,
  };

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  // Errors

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public titleSectionErrors(model: MessageTemplate) {
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

  public behaviorSectionErrors(model: MessageTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Trigger) ||
      areServerErrors(model.serverErrors.Cardinality) ||
      areServerErrors(model.serverErrors.ListExpression) ||
      areServerErrors(model.serverErrors.Schedule) ||
      areServerErrors(model.serverErrors.ConditionExpression) ||
      areServerErrors(model.serverErrors.PreventRenotify) ||
      areServerErrors(model.serverErrors.Usage) ||
      areServerErrors(model.serverErrors.Collection) ||
      areServerErrors(model.serverErrors.DefinitionId)
    ) ||
      (!!model.Parameters && model.Parameters.some(e => this.weakEntityErrors(e))) ||
      (!!model.Subscribers && model.Subscribers.some(e => this.weakEntityErrors(e)));
  }

  public contentSectionErrors(model: MessageTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.PhoneNumber) ||
      areServerErrors(model.serverErrors.Content)
    );
  }

  public deploySectionErrors(model: MessageTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.IsDeployed) ||
      areServerErrors(model.serverErrors.MainMenuSection) ||
      areServerErrors(model.serverErrors.MainMenuIcon) ||
      areServerErrors(model.serverErrors.MainMenuSortKey)
    );
  }

  public weakEntityErrors(model: MessageTemplateParameterForSave | MessageTemplateSubscriberForSave) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public metadataPaneErrors(model: MessageTemplate) {
    return this.titleSectionErrors(model) || this.behaviorSectionErrors(model) ||
      this.contentSectionErrors(model) || this.deploySectionErrors(model);
  }

  // Fields

  public showUsageFields(model: MessageTemplateForSave) {
    return model.Trigger === 'Manual';
  }

  public showListExpression(model: MessageTemplateForSave) {
    return model.Cardinality === 'Multiple';
  }

  public showSchedule(model: MessageTemplateForSave) {
    return model.Trigger === 'Automatic';
  }

  public showConditionExpression(model: MessageTemplateForSave) {
    return false; // model.Trigger === 'Automatic';
  }

  public showPreventRenotify(model: MessageTemplateForSave) {
    return false; // model.Trigger === 'Automatic';
  }

  public showVersion(model: MessageTemplateForSave) {
    return this.showPreventRenotify(model) && model.PreventRenotify;
  }

  private _allCollections: SelectorChoice[];
  public get allCollections(): SelectorChoice[] {
    if (!this._allCollections) {
      this._allCollections = collectionsWithEndpoint(this.workspace, this.translate)
        .filter(e => e.value === 'Document' || e.value === 'Agent');
    }
    return this._allCollections;
  }

  public showDefinitionIdSelector(model: MessageTemplateForSave): boolean {
    return !!model && !!model.Collection && !!metadata[model.Collection](this.workspace, this.translate, null).definitionIds;
  }

  public allDefinitionIds(model: MessageTemplateForSave): SelectorChoice[] {
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

  public showCollectionAndDefinition(model: MessageTemplateForSave) {
    return model.Usage === 'FromDetails' || model.Usage === 'FromSearchAndDetails';
  }

  //////////////// Subscribers

  public showSubscribers(model: MessageTemplateForSave) {
    return model.Cardinality === 'Single';
  }

  public onDeleteSubscriber(model: MessageTemplate, index: number) {
    if (index >= 0) {
      model.Subscribers.splice(index, 1);
      this.onMetadataChange(model);
    }
  }

  public onInsertSubscriber(model: MessageTemplate) {
    const item = { Id: 0 };
    model.Subscribers.push(item);
  }

  //////////////// Parameters

  public drop(event: CdkDragDrop<any[]>, model: MessageTemplateForSave) {

    // The source and destination collection
    const source = event.previousContainer.data;
    const sourceIndex = event.previousIndex;
    const destination = event.container.data;
    const destinationIndex = event.currentIndex;

    if (source === destination && sourceIndex !== destinationIndex) {
      // Reorder within array
      moveItemInArray(destination, sourceIndex, destinationIndex);
      // this.onMetadataChange(model);
    }
  }

  @ViewChild('paramConfigModal', { static: true })
  paramConfigModal: TemplateRef<any>;

  modelRef: MessageTemplateForSave;

  public showParameters(model: MessageTemplateForSave) {
    return this.showUsageFields(model) && model.Usage === 'Standalone';
  }

  public getParameters(model: MessageTemplateForSave): MessageTemplateParameterForSave[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }

  paramToEdit: MessageTemplateParameterForSave;
  paramToEditHasChanged = false;

  public onConfigureParameter(index: number, model: MessageTemplateForSave) {
    this.paramToEditHasChanged = false;
    const itemToEdit = { ...model.Parameters[index] } as MessageTemplateParameterForSave;
    this.paramToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.paramConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.paramToEditHasChanged) {

        model.Parameters[index] = itemToEdit;
        this.onMetadataChange(model);
      }
    }, (_: any) => { });
  }

  public onCreateParameter(model: MessageTemplate) {
    this.onConfigureParameter(model.Parameters.length, model);
  }

  public onDeleteParameter(index: number, model: MessageTemplate) {
    model.Parameters.splice(index, 1);
    this.onMetadataChange(model);
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

  public canApplyParam(): boolean {
    const p = this.paramToEdit;
    return !!p.Key && !!p.Label && !!p.Control;
  }

  // Preview

  private template: MessageTemplateForSave;

  public onTemplateChange(template: MessageTemplateForSave): void {
    this.template = template;
    this.delayedFetch();
  }

  public onMetadataChange(template: MessageTemplateForSave): void {
    this.template = template;
    this.fetch();
  }

  public message: () => string;

  private fetch(): void {
    this.refresh$.next();
  }

  private notifyDelayedFetch$ = new Subject<void>();
  private delayedFetch(): void {
    this.notifyDelayedFetch$.next();
  }

  public refresh$ = new Subject<void>();

  public preview: () => Observable<MessageCommandPreview> = () => {

    const template = this.template;

    delete this.message;

    let base$: Observable<MessageCommandPreview>;
    if (template.Trigger === 'Manual') {
      template.Usage = template.Usage || 'FromSearchAndDetails';

      if (template.Usage === 'FromSearchAndDetails') {
        template.Collection = template.Collection || 'Document';

        const args: PrintEntitiesArguments = {
          filter: this.filter,
          orderby: this.orderby,
          top: this.top,
          skip: this.skip
        };

        base$ = this.messageApi.messageCommandPreviewEntities(template, args, this.arguments);
      } else if (template.Usage === 'FromDetails') {
        template.Collection = template.Collection || 'Document';

        const entityId = this.id;
        if (!entityId) {
          this.message = () => this.translate.instant('FillRequiredFields');
          return of();
        }

        const args: PrintEntityByIdArguments = {};

        base$ = this.messageApi.messageCommandPreviewEntity(entityId, template, args, this.arguments);
      } else if (template.Usage === 'Standalone') {
        if (this.areRequiredParamsMissing()) {
          this.message = () => this.translate.instant('FillRequiredFields');
          return of();
        }
        const args: PrintArguments = {};
        base$ = this.messageApi.messageCommandPreview(template, args, this.arguments);
      } else {
        const args: PrintArguments = {};
        base$ = this.messageApi.messageCommandPreview(template, args, this.arguments);
      }
    } else if (template.Trigger === 'Automatic') {
      const args: PrintArguments = {};
      base$ = this.messageApi.messageCommandPreview(template, args, this.arguments);
    }

    return base$;
  }

  // Preview Parameters

  public areRequiredParamsMissing = () => {
    return !!this.template.Parameters && this.template.Parameters
      .some(p => p.IsRequired && !isSpecified(this.arguments[p.Key]));
  }

  public watch(model: MessageTemplateForSave) {
    if (this.template !== model && !!model) {
      this.template = model;
      this.fetch();
    }

    return true;
  }

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

  public updateOn(p: TemplateParameterForClient): 'change' | 'blur' {
    const desc = this.paramterDescriptor(p);
    return updateOn(desc);
  }

  public paramterDescriptor(p: TemplateParameterForClient): PropVisualDescriptor {
    return p.desc || (p.desc = descFromControlOptions(this.ws, p.Control, p.ControlOptions));
  }

  public label(p: TemplateParameterForClient): string {
    return this.ws.localize(p.Label, p.Label2, p.Label3);
  }

  private _collection: Collection = null;
  private _definitionId: number = null;
  private _detailsPickerDesc: PropVisualDescriptor;
  public get detailsPickerDesc(): PropVisualDescriptor {
    const template = this.template;
    const collection = !!template ? template.Collection : null;
    const definitionId = !!template ? template.DefinitionId : null;

    if (this._collection !== collection || this._definitionId !== definitionId) {
      this._collection = collection;
      this._definitionId = definitionId;

      if (!!collection) {
        let options: string = null;
        if (!!definitionId) {
          options = JSON.stringify({ definitionId });
        }
        this._detailsPickerDesc = descFromControlOptions(this.ws, collection, options);
      } else {
        this._detailsPickerDesc = null;
      }
    }

    return this._detailsPickerDesc;
  }

  public get detailsPickerLabel(): string {
    const template = this.template;
    if (!!template && !!template.Collection) {
      const descFunc = metadata[template.Collection];
      const desc = descFunc(this.workspace, this.translate, template.DefinitionId);
      return desc.titleSingular();
    }

    return ''; // Should not reach here in theory
  }

  public get showParametersSection(): boolean {
    return this.showMasterAndDetailsParams || this.showDetailsParams || this.showCustomParameters;
  }

  public get showMasterAndDetailsParams() {
    const template = this.template;
    return !!template && template.Usage === 'FromSearchAndDetails' && !!template.Collection;
  }

  public get showDetailsParams() {
    const template = this.template;
    return !!template && template.Usage === 'FromDetails' && !!template.Collection;
  }

  public get showCustomParameters(): boolean {
    return this.template.Usage === 'Standalone' && !!this.template.Parameters && this.template.Parameters.length > 0;
  }

  // Main Menu

  public showMainMenuFields(model: MessageTemplateForSave) {
    return this.showUsageFields(model) && model.Usage === 'Standalone' && model.IsDeployed;
  }

  public onIconClick(model: MessageTemplateForSave, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
  }

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_MessageTemplate(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_MessageTemplate(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }
}
