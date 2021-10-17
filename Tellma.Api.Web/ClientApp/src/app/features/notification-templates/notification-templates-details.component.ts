// tslint:disable:member-ordering
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { NgControl } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { collectionsWithEndpoint, Control, hasControlOptions, metadata, simpleControls } from '~/app/data/entities/base/metadata';
import {
  NotificationTemplate,
  NotificationTemplateAttachmentForSave,
  NotificationTemplateForSave,
  NotificationTemplateParameterForSave,
  NotificationTemplateSubscriberForSave
} from '~/app/data/entities/notification-template';
import { onCodeTextareaKeydown } from '~/app/data/util';
import { MasterDetailsStore, WorkspaceService } from '~/app/data/workspace.service';
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

  private localState = new MasterDetailsStore();  // Used in popup mode

  private _sections: { [key: string]: boolean } = {
    Title: false,
    Behavior: true
  };

  public expand = 'Parameters,Subscribers.User,Attachments.PrintingTemplate,ReportDefinition';
  public collapseEditor = false;
  public collapseMetadata = false;

  constructor(private workspace: WorkspaceService, private translate: TranslateService, private modalService: NgbModal) {
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
    result.Trigger = 'Manual';
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
  }

  public get state(): MasterDetailsStore {
    // important to always reference the source, and not keep a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.isPopupMode) {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new MasterDetailsStore();
      }

      return this.localState;
    } else {

      // screen mode on the other hand use the global state
      return this.globalState;
    }
  }

  private get globalState(): MasterDetailsStore {
    const key = 'notification-templates';
    if (!this.workspace.current.mdState[key]) {
      this.workspace.current.mdState[key] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[key];
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
      areServerErrors(model.serverErrors.Caption) ||
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

  public showAddressExpression(model: NotificationTemplateForSave) {
    return model.Cardinality === 'Bulk';
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

  //////////////// Parameters

  public showParameters(model: NotificationTemplateForSave) {
    return model.Trigger === 'Manual';
  }

  public getParameters(model: NotificationTemplateForSave): NotificationTemplateParameterForSave[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }

  @ViewChild('paramConfigModal', { static: true })
  paramConfigModal: TemplateRef<any>;

  paramToEdit: NotificationTemplateParameterForSave;
  paramToEditHasChanged = false;
  modelRef: NotificationTemplateForSave;

  public onConfigureParameter(index: number, model: NotificationTemplateForSave) {
    this.paramToEditHasChanged = false;
    const itemToEdit = { ...model.Parameters[index] } as NotificationTemplateParameterForSave;
    this.paramToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.paramConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.paramToEditHasChanged) {

        model.Parameters[index] = itemToEdit;
        // this.onTemplateChange();
      }
    }, (_: any) => { });
  }

  public onCreateParameter(model: NotificationTemplate) {
    this.onConfigureParameter(model.Parameters.length, model);
  }

  public onDeleteParameter(index: number, model: NotificationTemplate) {
    model.Parameters.splice(index, 1);
    // this.onTemplateChange();
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

  public dropParameter(event: CdkDragDrop<any[]>) {

    // The source and destination collection
    const sourceIndex = event.previousIndex;
    const destination = event.container.data;
    const destinationIndex = event.currentIndex;

    // Reorder within array
    if (sourceIndex !== destinationIndex) {
      moveItemInArray(destination, sourceIndex, destinationIndex);
      // this.onTemplateChange();
    }
  }

  // Attachments
  public showAttachments(model: NotificationTemplateForSave) {
    return model.Channel === 'Email';
  }

  // Subscribers
  public showSubscribers(model: NotificationTemplateForSave) {
    return model.Cardinality === 'Single';
  }
}

// tslint:disable:no-trailing-whitespace
const defaultEmailBody = `<table style="border-collapse: collapse;">
    <tr>
        <td>
            
        </td>
    </tr>    
</table>`;
