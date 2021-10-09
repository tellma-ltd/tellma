// tslint:disable:member-ordering
import { Component, OnInit, OnDestroy, ViewChild, TemplateRef } from '@angular/core';
import { WorkspaceService, MasterDetailsStore } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, collectionsWithEndpoint, Control, getChoices, hasControlOptions, metadata, simpleControls } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import {
  PrintingTemplateForSave,
  PrintingTemplate,
  PrintingTemplateParameterForSave,
  PrintingTemplateRoleForSave,
  metadata_PrintingTemplate
} from '~/app/data/entities/printing-template';
import { NgControl } from '@angular/forms';
import { validationErrors, highlightInvalid, areServerErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { onCodeTextareaKeydown } from '~/app/data/util';
import { PrintingTemplateForClient, PrintingTemplateParameterForClient } from '~/app/data/dto/definitions-for-client';
import { PrintingTemplates } from '../print/print.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 't-printing-templates-details',
  templateUrl: './printing-templates-details.component.html',
  styles: []
})
export class PrintingTemplatesDetailsComponent extends DetailsBaseComponent implements OnInit, OnDestroy {

  private localState = new MasterDetailsStore();  // Used in popup mode

  private _sections: { [key: string]: boolean } = {
    Title: false,
    Usage: true,
    Deployment: false
  };

  public expand = 'Parameters,Roles.Role,ReportDefinition';
  public collapseEditor = false;
  public collapseMetadata = false;

  constructor(private workspace: WorkspaceService, private translate: TranslateService, private modalService: NgbModal) {
    super();
  }

  create = () => {
    const result: PrintingTemplateForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsDeployed = false;
    result.SupportsPrimaryLanguage = true;
    result.SupportsSecondaryLanguage = !!this.workspace.currentTenant.settings.SecondaryLanguageId;
    result.SupportsTernaryLanguage = !!this.workspace.currentTenant.settings.TernaryLanguageId;

    result.Usage = 'FromSearchAndDetails';
    result.Collection = 'Document';
    result.Body = defaultBody;

    result.Parameters = [];
    result.Roles = [];

    return result;
  }

  clone: (item: PrintingTemplate) => PrintingTemplate = (item: PrintingTemplate) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as PrintingTemplate;
      delete clone.Id;
      if (!!clone.Parameters) {
        clone.Parameters.forEach(e => delete e.Id);
      }
      if (!!clone.Roles) {
        clone.Roles.forEach(e => delete e.Id);
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
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
    const key = 'printing-templates';
    if (!this.workspace.current.mdState[key]) {
      this.workspace.current.mdState[key] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[key];
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // UI Binding

  public isInactive: (model: PrintingTemplate) => string = (_: PrintingTemplate) => null;

  public onTemplateChange() {
    this._templateHasChanged = true;
  }

  public onPreviewChange() {
    this._previewHasChanged = true;
  }

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

  public titleSectionErrors(model: PrintingTemplate) {
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

  public usageSectionErrors(model: PrintingTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Usage) ||
      areServerErrors(model.serverErrors.Collection) ||
      areServerErrors(model.serverErrors.DefinitionId) ||
      areServerErrors(model.serverErrors.DownloadName) ||
      areServerErrors(model.serverErrors.SupportsPrimaryLanguage) ||
      areServerErrors(model.serverErrors.SupportsSecondaryLanguage) ||
      areServerErrors(model.serverErrors.SupportsTernaryLanguage)
    ) ||
      (!!model.Parameters && model.Parameters.some(e => this.weakEntityErrors(e)));
  }

  public mainMenuSectionErrors(model: PrintingTemplate) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.MainMenuSection) ||
      areServerErrors(model.serverErrors.MainMenuIcon) ||
      areServerErrors(model.serverErrors.MainMenuSortKey)) ||
      (!!model.Roles && model.Roles.some(e => this.weakEntityErrors(e)));
  }


  public weakEntityErrors(model: PrintingTemplateParameterForSave | PrintingTemplateRoleForSave) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public metadataPaneErrors(model: PrintingTemplate) {
    return this.titleSectionErrors(model) || this.usageSectionErrors(model) || this.weakEntityErrors(model);
  }

  public onIconClick(model: PrintingTemplateForSave, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
  }

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_PrintingTemplate(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_PrintingTemplate(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public templateSectionErrors(model: PrintingTemplate) {
    return !!model.serverErrors && areServerErrors(model.serverErrors.Body);
  }

  public get allCollections(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate);
  }

  public showDefinitionIdSelector(model: PrintingTemplateForSave): boolean {
    return !!model && !!model.Collection && !!metadata[model.Collection](this.workspace, this.translate, null).definitionIds;
  }

  public allDefinitionIds(model: PrintingTemplateForSave): SelectorChoice[] {
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

  public showCollectionAndDefinition(model: PrintingTemplateForSave) {
    return model.Usage === 'FromDetails' || model.Usage === 'FromSearchAndDetails';
  }

  public showReportDefinition(model: PrintingTemplateForSave) {
    return model.Usage === 'FromReport';
  }

  private _templateHasChanged = true;
  private _template: PrintingTemplateForClient;

  public template(model: PrintingTemplateForSave): PrintingTemplateForClient {
    if (!this._template || this._templateHasChanged) {
      this._templateHasChanged = false;
      this._template = {
        Name: model.Name,
        Name2: model.Name2,
        Name3: model.Name3,
        SupportsPrimaryLanguage: model.SupportsPrimaryLanguage,
        SupportsSecondaryLanguage: model.SupportsSecondaryLanguage,
        SupportsTernaryLanguage: model.SupportsTernaryLanguage,
        Usage: model.Usage,
        Collection: model.Collection,
        DefinitionId: model.DefinitionId,
        Parameters: model.Parameters.map(e => ({ ...e } as PrintingTemplateParameterForClient))
      };
    }

    return this._template;
  }

  private _previewHasChanged = true;
  private _preview: PrintingTemplates;
  private _bodyForPreview: string;
  private _contextForPreview: string;
  private _downloadNameForPreview: string;
  public preview(model: PrintingTemplateForSave): PrintingTemplates {
    if (!model) {
      return null;
    }

    if (this._bodyForPreview !== model.Body ||
      this._contextForPreview !== model.Context ||
      this._downloadNameForPreview !== model.DownloadName ||
      this._previewHasChanged) {
      this._bodyForPreview = model.Body;
      this._contextForPreview = model.Context;
      this._downloadNameForPreview = model.DownloadName;
      this._previewHasChanged = false;

      this._preview = {
        context: model.Context,
        downloadName: model.DownloadName,
        body: model.Body
      };
    }

    return this._preview;
  }

  public onKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent, model: PrintingTemplate) {
    onCodeTextareaKeydown(elem, $event, v => model.Body = v);
  }

  public showIsDeployed(model: PrintingTemplateForSave): boolean {
    return model.Usage !== 'Standalone';
  }

  public showMainMenuSection(model: PrintingTemplateForSave): boolean {
    return model.Usage === 'Standalone';
  }

  //////////////// Parameters

  public showParameters(model: PrintingTemplateForSave) {
    return !!model && model.Usage === 'Standalone';
  }

  public getParameters(model: PrintingTemplateForSave): PrintingTemplateParameterForSave[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }

  @ViewChild('paramConfigModal', { static: true })
  paramConfigModal: TemplateRef<any>;

  paramToEdit: PrintingTemplateParameterForSave;
  paramToEditHasChanged = false;
  modelRef: PrintingTemplateForSave;

  public onConfigureParameter(index: number, model: PrintingTemplateForSave) {
    this.paramToEditHasChanged = false;
    const itemToEdit = { ...model.Parameters[index] } as PrintingTemplateParameterForSave;
    this.paramToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.paramConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.paramToEditHasChanged) {

        model.Parameters[index] = itemToEdit;
        this.onTemplateChange();

        // this.validateModel(model);
        // this.synchronizeParameters(model);
        // this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onCreateParameter(model: PrintingTemplate) {
    this.onConfigureParameter(model.Parameters.length, model);
  }

  public onDeleteParameter(index: number, model: PrintingTemplate) {
    model.Parameters.splice(index, 1);
    this.onTemplateChange();
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

  public canApplyParam(p: PrintingTemplateParameterForSave): boolean {
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
      this.onTemplateChange();
    }
  }

  public savePreprocessing = (model: PrintingTemplateForSave) => {
    // Server validation on hidden collections will be confusing to the user
    if (!this.showParameters(model)) {
      model.Parameters = [];
    }

    if (!this.showMainMenuSection(model)) {
      model.Roles = [];
    }
  }

  ///////////////////// Roles

  public onDeleteRole(model: PrintingTemplate, index: number) {
    if (index >= 0) {
      model.Roles.splice(index, 1);
    }
  }

  public onInsertRole(model: PrintingTemplate) {
    const item = { Id: 0 };
    model.Roles.push(item);
  }
}

// tslint:disable:no-trailing-whitespace
const defaultBody = `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>{{ 'Document' }}</title>
    <style>
        /* Printing CSS */
        {{ *define $PageSize as 'A4' }} /* https://mzl.la/3d8twxF */
        {{ *define $Orientation as 'Portrait' }} /* 'Portrait', 'Landscape' */
        {{ *define $Margins as '0.5in' }} /* The page margins */
        @media screen {
            body { background-color: #F9F9F9; }
            .page {
                margin-left: auto;
                margin-right: auto;
                margin-top: 1rem;
                margin-bottom: 1rem;
                border: 1px solid lightgrey;
                background-color: white;
                box-shadow: rgba(60, 64, 67, 0.15) 0px 1px 3px 1px;
                box-sizing: border-box;
                width: {{ PreviewWidth($PageSize, $Orientation) }};
                min-height: {{ PreviewHeight($PageSize, $Orientation) }};
                padding: {{ $Margins }};
            }
        }
        @page {
          margin: {{ $Margins }};
          size: {{ $PageSize }} {{ $Orientation }};
        }
        .page { break-after: page; }
        /* End Printing CSS */        
        * {
            font-family: sans-serif;
            box-sizing: border-box;
        }        
        body { margin: 0; }        
        body.rtl { direction: rtl; } 
        
    </style>
</head>
<body class="{{ IF($IsRtl, 'rtl', '') }}">
    <div class="page">
        <!-- HTML Template Here -->
        
    </div>
</body>
</html>`;


const defaultBodyOld = `<!DOCTYPE html>
<html lang="{{ $Lang }}">
<head>
    <meta charset="UTF-8">
    <title>{{ 'Document' }}</title>
    <style>

        /* Printing CSS: Remove if not for printing */
        {{ *define $PageSize as 'A4' }} /* https://mzl.la/3d8twxF */
        {{ *define $Orientation as 'Portrait' }} /* 'Portrait', 'Landscape' */
        {{ *define $Margins as '0.5in' }} /* The page margins */
        @media screen {
            body {
                background-color: #F9F9F9;
            }
            .page {
                margin-left: auto;
                margin-right: auto;
                margin-top: 1rem;
                margin-bottom: 1rem;
                border: 1px solid lightgrey;
                background-color: white;
                box-shadow: rgba(60, 64, 67, 0.15) 0px 1px 3px 1px;
                box-sizing: border-box;
                width: {{ PreviewWidth($PageSize, $Orientation) }};
                min-height: {{ PreviewHeight($PageSize, $Orientation) }};
                padding: {{ $Margins }};
            }
        }
        @page {
            margin: {{ $Margins }};
            size: {{ $PageSize }} {{ $Orientation }};
        }
        /* End Printing CSS */
        
        * {
            font-family: sans-serif;
        }
        
        body {
            margin: 0;
        }
        
        /* More CSS Here */
    
    </style>
</head>
<body class="{{ IF($IsRtl, 'rtl', '') }}">
    <div class="page">
        <!-- HTML Template Here -->
        
    </div>
</body>
</html>`;
