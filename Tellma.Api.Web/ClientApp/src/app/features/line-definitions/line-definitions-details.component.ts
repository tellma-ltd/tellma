// tslint:disable:member-ordering
import { Component, TemplateRef, ViewChild } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import {
  ChoicePropDescriptor, collectionsWithEndpoint, Control, getChoices, hasControlOptions, simpleControls
} from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { LineDefinitionForSave, LineDefinition, metadata_LineDefinition } from '~/app/data/entities/line-definition';
import { DefinitionVisibility, visibilityPropDescriptor } from '~/app/data/entities/base/definition-common';
import { entryColumnNames, DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { LineDefinitionColumn } from '~/app/data/entities/line-definition-column';
import { LineDefinitionEntry, LineDefinitionEntryForSave } from '~/app/data/entities/line-definition-entry';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { RuleType, ruleTypes, WorkflowSignature, PredicateType, predicateTypes } from '~/app/data/entities/workflow-signature';
import { PositiveLineState } from '~/app/data/entities/line';
import { LineDefinitionStateReason } from '~/app/data/entities/line-definition-state-reason';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';
import { isSpecified, onCodeTextareaKeydown } from '~/app/data/util';
import { LineDefinitionGenerateParameter } from '~/app/data/entities/line-definition-generate-parameter';

@Component({
  selector: 't-line-definitions-details',
  templateUrl: './line-definitions-details.component.html',
  styles: []
})
export class LineDefinitionsDetailsComponent extends DetailsBaseComponent {

  @ViewChild('lineDefinitionEntryModal', { static: true })
  lineDefinitionEntryModal: TemplateRef<any>;

  @ViewChild('controlOptions', { static: true })
  controlOptions: TemplateRef<any>;

  // private lineDefinitionsApi = this.api.lineDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = `Columns,Entries.ParentAccountType,Entries.EntryType,Entries.RelationDefinitions.RelationDefinition,
Entries.ResourceDefinitions.ResourceDefinition,Entries.NotedRelationDefinitions.NotedRelationDefinition,GenerateParameters,
Workflows.Signatures.Role,Workflows.Signatures.User,Workflows.Signatures.ProxyRole,StateReasons`;

  create = () => {
    const result: LineDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

    result.AllowSelectiveSigning = false;
    result.ViewDefaultsToForm = false;
    result.BarcodeBeepsEnabled = true;
    result.Columns = [];
    result.Entries = [];
    result.GenerateParameters = [];
    result.StateReasons = [];
    result.Workflows = [];

    return result;
  }

  clone: (item: LineDefinition) => LineDefinition = (item: LineDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as LineDefinition;
      clone.Id = null;

      if (!!clone.Columns) {
        clone.Columns.forEach(e => {
          e.Id = null;
        });
      }

      if (!!clone.Entries) {
        clone.Entries.forEach(e => {
          e.Id = null;
          e.RelationDefinitions.forEach(x => {
            x.Id = null;
          });

          e.ResourceDefinitions.forEach(x => {
            x.Id = null;
          });

          e.NotedRelationDefinitions.forEach(x => {
            x.Id = null;
          });
        });
      }

      if (!!clone.GenerateParameters) {
        clone.GenerateParameters.forEach(e => {
          e.Id = null;
        });
      }

      if (!!clone.StateReasons) {
        clone.StateReasons.forEach(e => {
          e.Id = null;
        });
      }

      if (!!clone.Workflows) {
        clone.Workflows.forEach(e => {
          e.Id = null;
          e.Signatures.forEach(x => {
            x.Id = null;
          });
        });
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    public modalService: NgbModal,
    private workspace: WorkspaceService,
    private api: ApiService,
    private translate: TranslateService) {
    super();

    // this.lineDefinitionsApi = this.api.lineDefinitionsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public isInactive: (model: LineDefinition) => string = (def: LineDefinition) =>
    !!def && def.Id === this.ws.definitions.ManualLinesDefinitionId ? 'Error_CannotModifySystemItem' : null

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  // Columns Grid

  public columnNameDisplay = (columnName: string) => {
    let prefix: string;
    switch (columnName) {
      case 'Memo':
        prefix = '';
        break;
      case 'PostingDate':
      case 'Boolean1':
      case 'Decimal1':
      case 'Text1':
        prefix = 'Line_';
        break;
      default:
        prefix = 'Entry_';
        break;
    }

    let key = prefix + columnName;
    if (key.endsWith('Id')) {
      key = key.substr(0, key.length - 2);
    }

    return this.translate.instant(key);
  }

  private _columnNameChoices: SelectorChoice[];

  public get columnNameChoices(): SelectorChoice[] {
    if (!this._columnNameChoices) {
      this._columnNameChoices = entryColumnNames.map(key => {
        return { value: key, name: () => this.columnNameDisplay(key) };
      });
    }

    return this._columnNameChoices;
  }

  public stateDisplay = (state: number) => {
    let key: string;
    if (state === 5) {
      key = 'Never';
    } else if (state >= 0) {
      key = 'Line_State_' + state;
    } else {
      key = 'Line_State_minus_' + Math.abs(state);
    }

    return this.translate.instant(key);
  }

  public isRequiredColumn(column: LineDefinitionColumn) {
    return column.ColumnName === 'CenterId' || column.ColumnName === 'CurrencyId';
  }

  public canFilter(column: LineDefinitionColumn) {
    return !!column.ColumnName && column.ColumnName.endsWith('Id');
  }

  public canInherit(column: LineDefinitionColumn) {
    // IMPORTANT: Keep in sync with LineDefinitionsController.cs
    switch (column.ColumnName) {
      case 'PostingDate':
      case 'Memo':
      case 'CurrencyId':
      case 'CenterId':
      case 'CustodianId':
      case 'RelationId':
      case 'ResourceId':
      case 'NotedRelationId':
      case 'Quantity':
      case 'UnitId':
      case 'Time1':
      case 'Duration':
      case 'DurationUnitId':
      case 'Time2':
      case 'ExternalReference':
      case 'ReferenceSourceId':
      case 'InternalReference':
        return true;
      default:
        return false;
    }
  }

  private _stateChoices: SelectorChoice[];
  public get stateChoices(): SelectorChoice[] {
    if (!this._stateChoices) {
      this._stateChoices = [0, 1, 2, 3, 4, 5].map(state => {
        return { value: state, name: () => this.stateDisplay(state) };
      });
    }

    return this._stateChoices;
  }

  private _toStateChoices: SelectorChoice[];
  public get toStateChoices(): SelectorChoice[] {
    if (!this._toStateChoices) {
      this._toStateChoices = [1, 2, 3, 4].map(state => {
        return { value: state, name: () => this.stateDisplay(state) };
      });
    }

    return this._toStateChoices;
  }

  private _negativeStateChoices: SelectorChoice[];
  public get negativeStateChoices(): SelectorChoice[] {
    if (!this._negativeStateChoices) {
      this._negativeStateChoices = [-1, -2, -3, -4].map(state => {
        return { value: state, name: () => this.stateDisplay(state) };
      });
    }

    return this._negativeStateChoices;
  }


  public directionDisplay = (direction: number) => {
    if (direction === 1) {
      return this.translate.instant('Entry_Direction_Debit');
    } else if (direction === -1) {
      return this.translate.instant('Entry_Direction_Credit');
    } else {
      return '';
    }
  }

  public ruleTypeDisplay = (ruleType: RuleType) => {
    return !!ruleType ? this.translate.instant('RuleType_' + ruleType) : '';
  }

  private _ruleTypeChoices: SelectorChoice[];
  public get ruleTypeChoices(): SelectorChoice[] {
    if (!this._ruleTypeChoices) {
      this._ruleTypeChoices = ruleTypes.map((t: RuleType) => {
        return {
          value: t,
          name: () => this.ruleTypeDisplay(t)
        };
      });
    }

    return this._ruleTypeChoices;
  }

  public predicateTypeDisplay = (predicateType: PredicateType) => {
    return !!predicateType ? this.translate.instant('PredicateType_' + predicateType) : ''; // this.translate.instant('Always');
  }

  private _predicateTypeChoices: SelectorChoice[];
  public get predicateTypeChoices(): SelectorChoice[] {
    if (!this._predicateTypeChoices) {
      this._predicateTypeChoices = predicateTypes.map((t: PredicateType) => {
        return {
          value: t,
          name: () => this.predicateTypeDisplay(t)
        };
      });

      // this._predicateTypeChoices.unshift({value : '', name: () => this.translate.instant('Always') });
    }

    return this._predicateTypeChoices;
  }

  public get functionalDisplay(): string {
    return this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName');
  }

  public get functionalDecimals(): number {
    return this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get functionalFormat(): string {
    const decimals = this.functionalDecimals;
    return `1.${decimals}-${decimals}`;
  }

  public visibilityDisplay = (visibility: DefinitionVisibility) => {
    const desc = visibilityPropDescriptor('', this.translate);
    return desc.format(visibility);
  }

  private _visibilityChoices: SelectorChoice[];
  public get visibilityChoices(): SelectorChoice[] {
    if (!this._visibilityChoices) {
      const desc = visibilityPropDescriptor('', this.translate);
      this._visibilityChoices = getChoices(desc);
    }

    return this._visibilityChoices;
  }

  // Control
  private _controlChoicesDefinitions: DefinitionsForClient;
  private _controlDisplayCache: { [key: string]: () => string };

  public controlDisplay = (control: Control) => {
    const ws = this.ws;
    const defs = ws.definitions;
    if (this._controlChoicesDefinitions !== defs) {
      this._controlChoicesDefinitions = defs;

      // display names
      this._controlDisplayCache = {};
      for (const choice of this.controlSimpleChoices()) {
        this._controlDisplayCache[choice.value] = choice.name;
      }

      for (const choice of this.controlEntityChoices()) {
        this._controlDisplayCache[choice.value] = choice.name;
      }
    }

    const displayFunc = this._controlDisplayCache[control];
    return !!displayFunc ? displayFunc() : '';
  }

  public controlSimpleChoices(): SelectorChoice[] {
    return simpleControls(this.translate);
  }

  public controlEntityChoices(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate, true);
  }

  public hasControlOptions(p: LineDefinitionGenerateParameter): boolean {
    return !!p && hasControlOptions(p.Control);
  }

  // Control Options
  public selectedParam: LineDefinitionGenerateParameter;
  public isEdit = false;
  public onControlOptions(p: LineDefinitionGenerateParameter, isEdit: boolean) {
    if (!p) {
      return;
    }

    this.selectedParam = p;
    this.isEdit = isEdit;
    this.modalService.open(this.controlOptions, { windowClass: 't-dark-theme t-wider-modal' });
  }


  public createEntry(): LineDefinitionEntryForSave {
    return {
      Id: 0,
      Direction: 1,
      RelationDefinitions: [],
      ResourceDefinitions: [],
      NotedRelationDefinitions: [],
    };
  }

  public createStateReason(): LineDefinitionStateReason {
    return {
      Id: 0,
      State: -2,
      IsActive: true
    };
  }

  public createColumn(): LineDefinitionColumn {
    return {
      Id: 0,
      EntryIndex: 0,
      InheritsFromHeader: 0,
      VisibleState: 0,
      RequiredState: 0,
      ReadOnlyState: 5,
    };
  }

  public entryToEdit: LineDefinitionEntryForSave;
  public entryIndex: number;
  public modalIsEdit: boolean;

  public onLineDefinitionEntryRelationDefinitions(entry: LineDefinitionEntryForSave, isEdit: boolean, index: number): void {
    this.activeEntryTab = 'relationDefinitions';
    this.onLineDefinitionEntryMore(entry, isEdit, index);
  }

  public onLineDefinitionEntryResourceDefinitions(entry: LineDefinitionEntryForSave, isEdit: boolean, index: number): void {
    this.activeEntryTab = 'resourceDefinitions';
    this.onLineDefinitionEntryMore(entry, isEdit, index);
  }

  public onLineDefinitionEntryNotedRelationDefinitions(entry: LineDefinitionEntryForSave, isEdit: boolean, index: number): void {
    this.activeEntryTab = 'notedRelationDefinitions';
    this.onLineDefinitionEntryMore(entry, isEdit, index);
  }

  public onLineDefinitionEntryMore(entry: LineDefinitionEntryForSave, isEdit: boolean, index: number): void {
    if (!!entry) {
      this.entryToEdit = entry;
      this.modalIsEdit = isEdit;
      this.entryIndex = index;
      this.modalService.open(this.lineDefinitionEntryModal, { windowClass: 't-dark-theme t-wider-modal' });
    }
  }

  // Grid management

  public rowDrop(event: CdkDragDrop<any[]>, collection: any[]) {
    moveItemInArray(collection, event.previousIndex, event.currentIndex);
  }

  public onDeleteRow(row: any, collection: any[]) {
    const index = collection.indexOf(row);
    if (index >= 0) {
      collection.splice(index, 1);
    }
  }

  public onInsertRow(collection: any[], create?: () => any) {
    const item = !!create ? create() : { Id: 0 };
    collection.push(item);
  }


  public column_rowDrop(event: CdkDragDrop<any[]>, model: LineDefinition) {
    this.rowDrop(event, model.Columns);

    this.choicesBarcodeColumnIndexChanged = true;

    // Adjust the barcode column index
    if (model.BarcodeColumnIndex === event.previousIndex) {
      model.BarcodeColumnIndex = event.currentIndex;
    } else if (event.previousIndex < model.BarcodeColumnIndex && event.currentIndex >= model.BarcodeColumnIndex) {
      model.BarcodeColumnIndex--;
    } else if (event.previousIndex > model.BarcodeColumnIndex && event.currentIndex <= model.BarcodeColumnIndex) {
      model.BarcodeColumnIndex++;
    }
  }

  public column_onDeleteRow(colIndex: number, model: LineDefinition) {
    const row = model.Columns[colIndex];
    this.onDeleteRow(row, model.Columns);

    this.choicesBarcodeColumnIndexChanged = true;

    // Adjust the barcode column index
    if (model.BarcodeColumnIndex === colIndex) {
      delete model.BarcodeColumnIndex;
    } else if (model.BarcodeColumnIndex > colIndex) {
      model.BarcodeColumnIndex--;
    }
  }

  public column_onInsertRow(model: LineDefinition) {
    this.onInsertRow(model.Columns, this.createColumn);

    this.choicesBarcodeColumnIndexChanged = true;
  }

  // Errors

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public weakEntityErrors(model: EntityForSave) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public showGeneralError(model: LineDefinition): boolean {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Code) ||
      areServerErrors(model.serverErrors.TitleSingular) ||
      areServerErrors(model.serverErrors.TitleSingular2) ||
      areServerErrors(model.serverErrors.TitleSingular3) ||
      areServerErrors(model.serverErrors.TitlePlural) ||
      areServerErrors(model.serverErrors.TitlePlural2) ||
      areServerErrors(model.serverErrors.TitlePlural3) ||
      areServerErrors(model.serverErrors.Description) ||
      areServerErrors(model.serverErrors.Description2) ||
      areServerErrors(model.serverErrors.Description3)
    );
  }

  public showColumnsError(model: LineDefinition): boolean {
    return !!model.Columns && model.Columns.some(e => this.weakEntityErrors(e));
  }

  public showEntriesError(model: LineDefinition): boolean {
    return !!model.Entries && model.Entries.some(e => this.weakEntityErrors(e) ||
      this.showRelationDefinitionsError(e) || this.showResourceDefinitionsError(e) || this.showNotedRelationDefinitionsError(e));
  }

  public showRelationDefinitionsError(entry: LineDefinitionEntry): boolean {
    return !!entry.RelationDefinitions && entry.RelationDefinitions.some(e => this.weakEntityErrors(e));
  }

  public showResourceDefinitionsError(entry: LineDefinitionEntry): boolean {
    return !!entry.ResourceDefinitions && entry.ResourceDefinitions.some(e => this.weakEntityErrors(e));
  }

  public showNotedRelationDefinitionsError(entry: LineDefinitionEntry): boolean {
    return !!entry.NotedRelationDefinitions && entry.NotedRelationDefinitions.some(e => this.weakEntityErrors(e));
  }

  public showPreprocessScriptError(model: LineDefinition): boolean {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.PreprocessScript));
  }

  public showValidateScriptError(model: LineDefinition): boolean {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.ValidateScript));
  }

  public showGenerateScriptError(model: LineDefinition): boolean {
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.GenerateLabel) ||
      areServerErrors(model.serverErrors.GenerateLabel2) ||
      areServerErrors(model.serverErrors.GenerateLabel3) ||
      areServerErrors(model.serverErrors.GenerateScript)
    )) ||
      (!!model.GenerateParameters && model.GenerateParameters.some(e => this.weakEntityErrors(e)));
  }


  public showGenerateParametersError(param: LineDefinitionEntry): boolean {
    return !!param.serverErrors && areServerErrors(param.serverErrors.ControlOptions);
  }

  public showWorkflowsError(model: LineDefinition): boolean {
    return !!model.Workflows && model.Workflows.some(w => {
      return !!w.Signatures && w.Signatures.some(e => this.weakEntityErrors(e));
    });
  }

  public showStateReasonsError(model: LineDefinition): boolean {
    return !!model.StateReasons && model.StateReasons.some(e => this.weakEntityErrors(e));
  }

  // Workflows

  public Signatures(model: LineDefinition, toState: PositiveLineState): WorkflowSignature[] {
    const workflow = model.Workflows.find(w => w.ToState === toState);
    return !!workflow ? workflow.Signatures : [];
  }

  public onInsertSignature(model: LineDefinition, toState: PositiveLineState): void {
    // If the workflow doesn't exist, add it
    let workflow = model.Workflows.find(w => w.ToState === toState);
    if (!workflow) {
      workflow = { ToState: toState, Signatures: [] };
      model.Workflows.push(workflow);
    }

    this.onInsertRow(workflow.Signatures);
  }

  public onDeleteSignature(model: LineDefinition, toState: PositiveLineState, signature: WorkflowSignature) {
    const workflow = model.Workflows.find(w => w.ToState === toState);
    this.onDeleteRow(signature, workflow.Signatures);

    // If the workflow is empty, remove it
    if (workflow.Signatures.length === 0) {
      this.onDeleteRow(workflow, model.Workflows);
    }
  }

  public signaturesCount(model: LineDefinition): number {
    // Counts the total number of signatures inside all workflows
    if (!model || !model.Workflows) {
      return 0;
    } else {
      let total = 0;
      for (const workflow of model.Workflows) {
        total += !!workflow.Signatures ? workflow.Signatures.length : 0;
      }
      return total;
    }
  }

  // Advanced

  public showAdvancedError(model: LineDefinition): boolean {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.BarcodeColumnIndex) ||
      areServerErrors(model.serverErrors.BarcodeProperty) ||
      areServerErrors(model.serverErrors.BarcodeExistingItemHandling) ||
      areServerErrors(model.serverErrors.BarcodeBeepsEnabled)
    );
  }

  // Tabs
  public get activeTab(): string {
    return this.ws.miscState.lineDefinition_activeTab || 'general';
  }

  public set activeTab(tab: string) {
    this.ws.miscState.lineDefinition_activeTab = tab;
  }

  public get activeEntryTab(): string {
    return this.ws.miscState.lineDefinition_activeEntryTab || 'relationDefinitions';
  }

  public set activeEntryTab(tab: string) {
    this.ws.miscState.lineDefinition_activeEntryTab = tab;
  }

  public onPreprocessScriptKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent, model: LineDefinition) {
    onCodeTextareaKeydown(elem, $event, v => model.PreprocessScript = v);
  }

  public onValidateScriptKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent, model: LineDefinition) {
    onCodeTextareaKeydown(elem, $event, v => model.ValidateScript = v);
  }

  public onGenerateScriptKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent, model: LineDefinition) {
    onCodeTextareaKeydown(elem, $event, v => model.GenerateScript = v);
  }

  // Barcode stuff

  public isBarcodeColumnIndexSpecified(model: LineDefinition) {
    return isSpecified(model.BarcodeColumnIndex);
  }

  public display_BarcodeColumnIndex(model: LineDefinition) {
    const index = model.BarcodeColumnIndex;
    if (isSpecified(index)) {
      const col = model.Columns[index];
      if (!!col) {
        return this.ws.getMultilingualValueImmediate(col, 'Label') || col.ColumnName;
      }
    }

    return '';
  }

  public choicesBarcodeColumnIndexChanged = true;
  public choicesBarcodeColumnIndexModel: LineDefinition;
  private choicesBarcodeColumnIndexResult: SelectorChoice[];
  public choices_BarcodeColumnIndex(model: LineDefinition): SelectorChoice[] {

    if (this.choicesBarcodeColumnIndexChanged || this.choicesBarcodeColumnIndexModel !== model) {
      this.choicesBarcodeColumnIndexChanged = false;
      this.choicesBarcodeColumnIndexModel = model;
      this.choicesBarcodeColumnIndexResult = model.Columns.map((col, index) => ({
        name: () => this.ws.getMultilingualValueImmediate(col, 'Label') || col.ColumnName,
        value: index
      }));
    }

    return this.choicesBarcodeColumnIndexResult;
  }

  display_BarcodeExistingItemHandling(model: LineDefinition) {
    const prop = metadata_LineDefinition(this.workspace, this.translate).properties.BarcodeExistingItemHandling as ChoicePropDescriptor;
    return prop.format(model.BarcodeExistingItemHandling);
  }
}
