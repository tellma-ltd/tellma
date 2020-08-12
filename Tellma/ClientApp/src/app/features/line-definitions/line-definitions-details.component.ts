// tslint:disable:member-ordering
import { Component, TemplateRef, ViewChild } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { LineDefinitionForSave, metadata_LineDefinition, LineDefinition } from '~/app/data/entities/line-definition';
import { DefinitionVisibility, visibilityPropDescriptor } from '~/app/data/entities/base/definition-common';
import { LineDefinitionForClient, entryColumnNames, DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { LineDefinitionColumn } from '~/app/data/entities/line-definition-column';
import { LineDefinitionEntry, LineDefinitionEntryForSave } from '~/app/data/entities/line-definition-entry';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { WorkflowForSave } from '~/app/data/entities/workflow';
import { RuleType, ruleTypes, WorkflowSignature, PredicateType, predicateTypes } from '~/app/data/entities/workflow-signature';
import { PositiveLineState } from '~/app/data/entities/line';
import { LineDefinitionStateReason } from '~/app/data/entities/line-definition-state-reason';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';

@Component({
  selector: 't-line-definitions-details',
  templateUrl: './line-definitions-details.component.html',
  styles: []
})
export class LineDefinitionsDetailsComponent extends DetailsBaseComponent {

  @ViewChild('lineDefinitionEntryModal', { static: true })
  lineDefinitionEntryModal: TemplateRef<any>;

  // private lineDefinitionsApi = this.api.lineDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = `Columns,Entries/AccountType,Entries/EntryType,Entries/CustodyDefinitions/CustodyDefinition,
Entries/ResourceDefinitions/ResourceDefinition,Entries/NotedRelationDefinitions/NotedRelationDefinition,GenerateParameters,
Workflows/Signatures/Role,Workflows/Signatures/User,Workflows/Signatures/ProxyRole,StateReasons`;

  create = () => {
    const result: LineDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

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
          e.CustodyDefinitions.forEach(x => {
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

  public isInactive: (model: LineDefinition) => string = (_: LineDefinition) => null;

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
      case 'TemplateLineId':
      case 'Multiplier':
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


  public directionDisplay = (direction: 1 | -1) => {
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

  private _dataTypeDisplayCache: { [key: string]: () => string };
  public dataTypeDisplay = (datatype: string) => {
    const _ = this.dataTypeChoices; // This will populate the cac
    const displayFunc = this._dataTypeDisplayCache[datatype];
    return !!displayFunc ? displayFunc() : '';
  }

  private _dataTypeChoicesDefinitions: DefinitionsForClient;
  private _dataTypeChoices: SelectorChoice[];
  public get dataTypeChoices(): SelectorChoice[] {
    const ws = this.ws;
    const defs = ws.definitions;
    if (this._dataTypeChoicesDefinitions !== defs) {
      this._dataTypeChoicesDefinitions = defs;

      const resourceDefinitions = Object.keys(defs.Resources).map(defId => {
        const def = defs.Resources[defId];
        return {
          value: 'Resource/' + defId,
          name: () => this.translate.instant('Resource') + ' - ' + ws.getMultilingualValueImmediate(def, 'TitleSingular')
        };
      });
      const custodyDefinitions = Object.keys(defs.Custodies).map(defId => {
        const def = defs.Custodies[defId];
        return {
          value: 'Custody/' + defId,
          name: () => this.translate.instant('Custody') + ' - ' + ws.getMultilingualValueImmediate(def, 'TitleSingular')
        };
      });
      const relationDefinitions = Object.keys(defs.Relations).map(defId => {
        const def = defs.Relations[defId];
        return {
          value: 'Relation/' + defId,
          name: () => this.translate.instant('Relation') + ' - ' + ws.getMultilingualValueImmediate(def, 'TitleSingular')
        };
      });

      this._dataTypeChoices = [
        { value: 'Date', name: () => this.translate.instant('DateTime') },
        { value: 'Decimal', name: () => this.translate.instant('Decimal') },
        { value: 'String', name: () => this.translate.instant('String') },
        { value: 'Center', name: () => this.translate.instant('Center') },
        { value: 'Currency', name: () => this.translate.instant('Currency') },
        { value: 'Resource', name: () => this.translate.instant('Resource') },
        ...resourceDefinitions,
        { value: 'Custody', name: () => this.translate.instant('Custody') },
        ...custodyDefinitions,
        { value: 'Relation', name: () => this.translate.instant('Relation') },
        ...relationDefinitions,
      ];

      this._dataTypeDisplayCache = { };
      for (const choice of this._dataTypeChoices) {
        this._dataTypeDisplayCache[choice.value] = choice.name;
      }
    }

    return this._dataTypeChoices;
  }

  public createEntry(): LineDefinitionEntryForSave {
    return {
      Direction: 1,
      CustodyDefinitions: [],
      ResourceDefinitions: [],
      NotedRelationDefinitions: [],
    };
  }

  public createStateReason(): LineDefinitionStateReason {
    return {
      State: -2,
      IsActive: true
    };
  }

  public createColumn(): LineDefinitionColumn {
    return {
      EntryIndex: 0,
      InheritsFromHeader: false,
      VisibleState: 0,
      RequiredState: 0,
      ReadOnlyState: 5,
    };
  }

  public entryToEdit: LineDefinitionEntryForSave;
  public entryIndex: number;
  public modalIsEdit: boolean;

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
    const item = !!create ? create() : {};
    collection.push(item);
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
    return !!model.Entries && model.Entries.some(e => this.weakEntityErrors(e) || this.showMoreError(e));
  }

  public showCustodyDefinitionsError(entry: LineDefinitionEntry): boolean {
    return !!entry.CustodyDefinitions && entry.CustodyDefinitions.some(e => this.weakEntityErrors(e));
  }

  public showResourceDefinitionsError(entry: LineDefinitionEntry): boolean {
    return !!entry.ResourceDefinitions && entry.ResourceDefinitions.some(e => this.weakEntityErrors(e));
  }

  public showNotedRelationDefinitionsError(entry: LineDefinitionEntry): boolean {
    return !!entry.NotedRelationDefinitions && entry.NotedRelationDefinitions.some(e => this.weakEntityErrors(e));
  }

  public showMoreError(entry: LineDefinitionEntry): boolean {
    return this.showCustodyDefinitionsError(entry) ||
      this.showResourceDefinitionsError(entry) ||
      this.showNotedRelationDefinitionsError(entry);
  }

  public showPreprocessScriptError(model: LineDefinition): boolean {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Script));
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

  // Tabs
  public get activeTab(): string {
    return this.details.state.detailsState.activeTab;
  }

  public set activeTab(tab: string) {
    this.details.state.detailsState.activeTab = tab;
  }

  public get activeEntryTab(): string {
    return this.details.state.detailsState.activeEntryTab;
  }

  public set activeEntryTab(tab: string) {
    this.details.state.detailsState.activeEntryTab = tab;
  }

}
