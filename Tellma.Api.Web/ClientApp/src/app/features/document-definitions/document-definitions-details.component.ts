// tslint:disable:member-ordering
import { Component, ViewChild, TemplateRef } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { DocumentDefinitionForSave, metadata_DocumentDefinition, DocumentDefinition } from '~/app/data/entities/document-definition';
import { DocumentDefinitionForClient, DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';
import { moveItemInArray, CdkDragDrop } from '@angular/cdk/drag-drop';
import { DocumentDefinitionLineDefinition } from '~/app/data/entities/document-definition-line-definition';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 't-document-definitions-details',
  templateUrl: './document-definitions-details.component.html',
  styles: []
})
export class DocumentDefinitionsDetailsComponent extends DetailsBaseComponent {

  @ViewChild('lineDefinitionModal', { static: true })
  lineDefinitionModal: TemplateRef<any>;

  private documentDefinitionsApi = this.api.documentDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = 'LineDefinitions.LineDefinition';

  create = () => {
    const result: DocumentDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

    result.PostingDateVisibility = 'Required';
    result.CenterVisibility = 'Required';
    result.ClearanceVisibility = 'None';
    result.MemoVisibility = 'Optional';
    result.IsOriginalDocument = false;
    result.HasAttachments = true;
    result.HasBookkeeping = true;
    result.CodeWidth = 4;
    result.DocumentType = 2;
    result.LineDefinitions = [];

    return result;
  }

  clone: (item: DocumentDefinition) => DocumentDefinition = (item: DocumentDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as DocumentDefinition;
      delete clone.Id;

      if (!!clone.LineDefinitions) {
        clone.LineDefinitions.forEach(e => {
          delete e.Id;
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
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService, private modalService: NgbModal) {
    super();

    this.documentDefinitionsApi = this.api.documentDefinitionsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public collapseDefinition = false;
  public onToggleDefinition(): void {
    this.collapseDefinition = !this.collapseDefinition;
  }

  private _isEdit = false;
  public watchIsEdit(isEdit: boolean): boolean {
    // this is a hack to trigger window resize when isEdit changes
    if (this._isEdit !== isEdit) {
      this._isEdit = isEdit;
    }

    return true;
  }

  public isInactive: (model: DocumentDefinition) => string = (def: DocumentDefinition) => null;

  public isSystem = (def: DocumentDefinition) => !!def && def.Id === this.ws.definitions.ManualJournalVouchersDefinitionId;

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  private _sections: { [key: string]: boolean } = {
    General: false,
    Definition: true,
    MainMenu: false
  };

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public weakEntityErrors(model: EntityForSave) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public sectionErrors(section: string, model: DocumentDefinition) {
    if (section === 'General') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.Code) ||
        areServerErrors(model.serverErrors.Description) ||
        areServerErrors(model.serverErrors.Description2) ||
        areServerErrors(model.serverErrors.Description3) ||
        areServerErrors(model.serverErrors.TitleSingular) ||
        areServerErrors(model.serverErrors.TitleSingular2) ||
        areServerErrors(model.serverErrors.TitleSingular3) ||
        areServerErrors(model.serverErrors.TitlePlural) ||
        areServerErrors(model.serverErrors.TitlePlural2) ||
        areServerErrors(model.serverErrors.TitlePlural3)
      ));
    } else if (section === 'Definition') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.DocumentType) ||
        areServerErrors(model.serverErrors.LineDefinitions) ||
        areServerErrors(model.serverErrors.ClearanceVisibility) ||
        areServerErrors(model.serverErrors.PostingDateVisibility) ||
        areServerErrors(model.serverErrors.CenterVisibility) ||
        areServerErrors(model.serverErrors.MemoVisibility) ||
        areServerErrors(model.serverErrors.Prefix) ||
        areServerErrors(model.serverErrors.CodeWidth) ||
        areServerErrors(model.serverErrors.IsOriginalDocument) ||
        areServerErrors(model.serverErrors.HasBookkeeping) ||
        areServerErrors(model.serverErrors.HasAttachments)
      )) ||
        (!!model.LineDefinitions && model.LineDefinitions.some(e => this.weakEntityErrors(e)));
    } else if (section === 'MainMenu') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.MainMenuIcon) ||
        areServerErrors(model.serverErrors.MainMenuSection) ||
        areServerErrors(model.serverErrors.MainMenuSortKey)
      ));
    }

    return false;
  }

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public serverErrors(obj: EntityForSave, prop: string): string[] {
    if (!obj || !obj.serverErrors) {
      return null;
    }

    return obj.serverErrors[prop];
  }

  public onDefinitionChange(model: DocumentDefinition, prop?: string) {
    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  private _currentModel: DocumentDefinition;
  private _currentModelModified = false;
  private _getForClientDefinitions: DefinitionsForClient;
  private _getForClientResult: DocumentDefinitionForClient;

  public getForClient(model: DocumentDefinition): DocumentDefinitionForClient {
    if (!model) {
      return null;
    }

    const defs = this.ws.definitions;

    if (this._currentModel !== model || this._currentModelModified || defs !== this._getForClientDefinitions) {
      this._currentModelModified = false;
      this._currentModel = model;
      this._getForClientDefinitions = defs;

      // IMPORTANT: Keep in sync with DefinitionsController.cs
      const result: DocumentDefinitionForClient = {
        ...model,
      };

      if (!result.CodeWidth) {
        result.CodeWidth = 4;
      }

      if (result.PostingDateVisibility === 'None') {
        delete result.PostingDateVisibility;
      }

      if (result.CenterVisibility === 'None') {
        delete result.CenterVisibility;
      }

      if (result.MemoVisibility === 'None') {
        delete result.MemoVisibility;
      }

      if (result.ClearanceVisibility === 'None') {
        delete result.ClearanceVisibility;
      }

      // TODO: Workflow
      result.CanReachState1 = false;
      result.CanReachState2 = false;
      result.CanReachState3 = false;
      result.HasWorkflow = false;

      result.AgentDefinitionIds = [];
      result.ResourceDefinitionIds = [];
      result.NotedAgentDefinitionIds = [];

      // The rest looks identical to the C# code
      const documentLineDefinitions = result.LineDefinitions
        .map(e => defs.Lines[e.LineDefinitionId])
        .filter(e => !!e && !!e.Columns);

      // Hash tables to accumulate some values
      let agentDefIds: { [id: number]: true } = {};
      let resourceDefIds: { [id: number]: true } = {};
      let notedAgentDefIds: { [id: number]: true } = {};

      let agentFilters: { [filter: string]: true } = {};
      let resourceFilters: { [filter: string]: true } = {};
      let notedAgentFilters: { [filter: string]: true } = {};

      let currencyFilters: { [filter: string]: true } = {};
      let centerFilters: { [filter: string]: true } = {};
      centerFilters[`CenterType eq 'BusinessUnit'`] = true;
      let unitFilters: { [filter: string]: true } = {};
      let durationUnitFilters: { [filter: string]: true } = {};
      let referenceSourceFilters: { [filter: string]: true } = {};

      for (const lineDef of documentLineDefinitions) {
        for (const colDef of lineDef.Columns.filter(c => c.InheritsFromHeader === 2)) {
          if (colDef.ColumnName === 'PostingDate') {
            result.PostingDateIsCommonVisibility = true;
            result.PostingDateVisibility = result.PostingDateVisibility || 'Optional';
            if (!result.PostingDateLabel) {
              result.PostingDateLabel = colDef.Label;
              result.PostingDateLabel2 = colDef.Label2;
              result.PostingDateLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.PostingDateRequiredState ?? 0)) {
              result.PostingDateRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.PostingDateReadOnlyState ?? 0)) {
              result.PostingDateReadOnlyState = colDef.ReadOnlyState;
            }

          } else if (colDef.ColumnName === 'CenterId') {
            result.CenterIsCommonVisibility = true;
            result.CenterVisibility = result.CenterVisibility || 'Optional';
            if (!(result.CenterLabel)) {
              result.CenterLabel = colDef.Label;
              result.CenterLabel2 = colDef.Label2;
              result.CenterLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.CenterRequiredState ?? 0)) {
              result.CenterRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.CenterReadOnlyState ?? 0)) {
              result.CenterReadOnlyState = colDef.ReadOnlyState;
            }

            // Accumulate all the filter atoms in the hash set
            if (!colDef.Filter) {
              centerFilters = null; // It means no filters will be added
            } else if (centerFilters != null) {
              centerFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'Memo') {
            result.MemoIsCommonVisibility = true;
            result.MemoVisibility = result.MemoVisibility || 'Optional';
            if (!result.MemoLabel) {
              result.MemoLabel = colDef.Label;
              result.MemoLabel2 = colDef.Label2;
              result.MemoLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.MemoRequiredState ?? 0)) {
              result.MemoRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.MemoReadOnlyState ?? 0)) {
              result.MemoReadOnlyState = colDef.ReadOnlyState;
            }

          } else if (colDef.ColumnName === 'CurrencyId') {
            result.CurrencyVisibility = true;
            if (!(result.CurrencyLabel)) {
              result.CurrencyLabel = colDef.Label;
              result.CurrencyLabel2 = colDef.Label2;
              result.CurrencyLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.CurrencyRequiredState ?? 0)) {
              result.CurrencyRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.CurrencyReadOnlyState ?? 0)) {
              result.CurrencyReadOnlyState = colDef.ReadOnlyState;
            }

            // Accumulate all the filter atoms in the hash set
            if (!colDef.Filter) {
              currencyFilters = null; // It means no filters will be added
            } else if (currencyFilters != null) {
              currencyFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'AgentId') {

            result.AgentVisibility = true;
            if (!result.AgentLabel) {
              result.AgentLabel = colDef.Label;
              result.AgentLabel2 = colDef.Label2;
              result.AgentLabel3 = colDef.Label3;
            }

            if (colDef.RequiredState > (result.AgentRequiredState ?? 0)) {
              result.AgentRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.AgentReadOnlyState ?? 0)) {
              result.AgentReadOnlyState = colDef.ReadOnlyState;
            }

            if (colDef.EntryIndex < lineDef.Entries.length) {
              const entryDef = lineDef.Entries[colDef.EntryIndex];
              if (!entryDef.AgentDefinitionIds || entryDef.AgentDefinitionIds.length === 0) {
                agentDefIds = null; // Means no definitionIds will be added
              } else if (!!agentDefIds) {
                for (const defId of entryDef.AgentDefinitionIds) {
                  agentDefIds[defId] = true;
                }
              }
            }

            if (!colDef.Filter) {
              agentFilters = null;
            } else if (!!agentFilters) {
              agentFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'ResourceId') {

            result.ResourceVisibility = true;
            if (!result.ResourceLabel) {
              result.ResourceLabel = colDef.Label;
              result.ResourceLabel2 = colDef.Label2;
              result.ResourceLabel3 = colDef.Label3;
            }

            if (colDef.RequiredState > (result.ResourceRequiredState ?? 0)) {
              result.ResourceRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.ResourceReadOnlyState ?? 0)) {
              result.ResourceReadOnlyState = colDef.ReadOnlyState;
            }

            if (colDef.EntryIndex < lineDef.Entries.length) {
              const entryDef = lineDef.Entries[colDef.EntryIndex];
              if (!entryDef.ResourceDefinitionIds || entryDef.ResourceDefinitionIds.length === 0) {
                resourceDefIds = null; // Means no definitionIds will be added
              } else if (!!resourceDefIds) {
                for (const defId of entryDef.ResourceDefinitionIds) {
                  resourceDefIds[defId] = true;
                }
              }
            }

            if (!colDef.Filter) {
              resourceFilters = null;
            } else if (!!resourceFilters) {
              resourceFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'NotedAgentId') {

            result.NotedAgentVisibility = true;
            if (!result.NotedAgentLabel) {
              result.NotedAgentLabel = colDef.Label;
              result.NotedAgentLabel2 = colDef.Label2;
              result.NotedAgentLabel3 = colDef.Label3;
            }

            if (colDef.RequiredState > (result.NotedAgentRequiredState ?? 0)) {
              result.NotedAgentRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.NotedAgentReadOnlyState ?? 0)) {
              result.NotedAgentReadOnlyState = colDef.ReadOnlyState;
            }

            if (colDef.EntryIndex < lineDef.Entries.length) {
              const entryDef = lineDef.Entries[colDef.EntryIndex];
              if (!entryDef.NotedAgentDefinitionIds || entryDef.NotedAgentDefinitionIds.length === 0) {
                notedAgentDefIds = null; // Means no definitionIds will be added
              } else if (!!notedAgentDefIds) {
                for (const defId of entryDef.NotedAgentDefinitionIds) {
                  notedAgentDefIds[defId] = true;
                }
              }
            }

            if (!colDef.Filter) {
              notedAgentFilters = null;
            } else if (!!notedAgentFilters) {
              notedAgentFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'Quantity') {
            result.QuantityVisibility = true;
            if (!result.QuantityLabel) {
              result.QuantityLabel = colDef.Label;
              result.QuantityLabel2 = colDef.Label2;
              result.QuantityLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.QuantityRequiredState ?? 0)) {
              result.QuantityRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.QuantityReadOnlyState ?? 0)) {
              result.QuantityReadOnlyState = colDef.ReadOnlyState;
            }


          } else if (colDef.ColumnName === 'UnitId') {

            result.UnitVisibility = true;
            if (!result.UnitLabel) {
              result.UnitLabel = colDef.Label;
              result.UnitLabel2 = colDef.Label2;
              result.UnitLabel3 = colDef.Label3;
            }

            if (colDef.RequiredState > (result.UnitRequiredState ?? 0)) {
              result.UnitRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.UnitReadOnlyState ?? 0)) {
              result.UnitReadOnlyState = colDef.ReadOnlyState;
            }

            if (!colDef.Filter) {
              unitFilters = null;
            } else if (!!unitFilters) {
              unitFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'Time1') {
            result.Time1Visibility = true;
            if (!result.Time1Label) {
              result.Time1Label = colDef.Label;
              result.Time1Label2 = colDef.Label2;
              result.Time1Label3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.Time1RequiredState ?? 0)) {
              result.Time1RequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.Time1ReadOnlyState ?? 0)) {
              result.Time1ReadOnlyState = colDef.ReadOnlyState;
            }
          } else if (colDef.ColumnName === 'Duration') {
            result.DurationVisibility = true;
            if (!result.DurationLabel) {
              result.DurationLabel = colDef.Label;
              result.DurationLabel2 = colDef.Label2;
              result.DurationLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.DurationRequiredState ?? 0)) {
              result.DurationRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.DurationReadOnlyState ?? 0)) {
              result.DurationReadOnlyState = colDef.ReadOnlyState;
            }

          } else if (colDef.ColumnName === 'DurationUnitId') {

            result.DurationUnitVisibility = true;
            if (!result.DurationUnitLabel) {
              result.DurationUnitLabel = colDef.Label;
              result.DurationUnitLabel2 = colDef.Label2;
              result.DurationUnitLabel3 = colDef.Label3;
            }

            if (colDef.RequiredState > (result.DurationUnitRequiredState ?? 0)) {
              result.DurationUnitRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.DurationUnitReadOnlyState ?? 0)) {
              result.DurationUnitReadOnlyState = colDef.ReadOnlyState;
            }

            if (!colDef.Filter) {
              durationUnitFilters = null;
            } else if (!!durationUnitFilters) {
              durationUnitFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'Time2') {
            result.Time2Visibility = true;
            if (!result.Time2Label) {
              result.Time2Label = colDef.Label;
              result.Time2Label2 = colDef.Label2;
              result.Time2Label3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.Time2RequiredState ?? 0)) {
              result.Time2RequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.Time2ReadOnlyState ?? 0)) {
              result.Time2ReadOnlyState = colDef.ReadOnlyState;
            }

          } else if (colDef.ColumnName === 'ExternalReference') {
            result.ExternalReferenceVisibility = true;
            if (!result.ExternalReferenceLabel) {
              result.ExternalReferenceLabel = colDef.Label;
              result.ExternalReferenceLabel2 = colDef.Label2;
              result.ExternalReferenceLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.ExternalReferenceRequiredState ?? 0)) {
              result.ExternalReferenceRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.ExternalReferenceReadOnlyState ?? 0)) {
              result.ExternalReferenceReadOnlyState = colDef.ReadOnlyState;
            }

          } else if (colDef.ColumnName === 'ReferenceSourceId') {
            result.ReferenceSourceVisibility = true;
            if (!result.ReferenceSourceLabel) {
              result.ReferenceSourceLabel = colDef.Label;
              result.ReferenceSourceLabel2 = colDef.Label2;
              result.ReferenceSourceLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.ReferenceSourceRequiredState ?? 0)) {
              result.ReferenceSourceRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.ReferenceSourceReadOnlyState ?? 0)) {
              result.ReferenceSourceReadOnlyState = colDef.ReadOnlyState;
            }

            // Accumulate all the filter atoms in the hash set
            if (!colDef.Filter) {
              referenceSourceFilters = null; // It means no filters will be added
            } else if (referenceSourceFilters != null) {
              referenceSourceFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'InternalReference') {
            result.InternalReferenceVisibility = true;
            if (!result.InternalReferenceLabel) {
              result.InternalReferenceLabel = colDef.Label;
              result.InternalReferenceLabel2 = colDef.Label2;
              result.InternalReferenceLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.InternalReferenceRequiredState ?? 0)) {
              result.InternalReferenceRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.InternalReferenceReadOnlyState ?? 0)) {
              result.InternalReferenceReadOnlyState = colDef.ReadOnlyState;
            }
          }
        }
      }
      // Calculate the definitionIds and filters
      result.AgentDefinitionIds = Object.keys(agentDefIds ?? {}).map(e => +e);
      result.ResourceDefinitionIds = Object.keys(resourceDefIds ?? {}).map(e => +e);
      result.NotedAgentDefinitionIds = Object.keys(notedAgentDefIds ?? {}).map(e => +e);

      result.AgentFilter = disjunction(agentFilters);
      result.ResourceFilter = disjunction(resourceFilters);
      result.NotedAgentFilter = disjunction(notedAgentFilters);
      result.CenterFilter = disjunction(centerFilters);
      result.CurrencyFilter = disjunction(currencyFilters);
      result.UnitFilter = disjunction(unitFilters);
      result.DurationUnitFilter = disjunction(durationUnitFilters);
      result.ReferenceSourceFilter = disjunction(referenceSourceFilters);

      // JV has some hard coded values:
      if (model.Code === 'ManualJournalVoucher') {
          // PostingDate
          result.PostingDateVisibility = 'Required';
          result.PostingDateIsCommonVisibility = false;
          result.PostingDateLabel = null;
          result.PostingDateLabel2 = null;
          result.PostingDateLabel3 = null;

          // Center
          // result.CenterVisibility = 'Required';
          result.CenterIsCommonVisibility = false;
          result.CenterLabel = null;
          result.CenterLabel2 = null;
          result.CenterLabel3 = null;

          // Memo
          result.MemoVisibility = 'Optional';
          result.MemoIsCommonVisibility = false;
          result.MemoLabel = null;
          result.MemoLabel2 = null;
          result.MemoLabel3 = null;

          result.CurrencyVisibility = false;

          result.AgentVisibility = false;
          result.ResourceVisibility = false;
          result.NotedAgentVisibility = false;

          result.QuantityVisibility = false;
          result.UnitVisibility = false;
          result.Time1Visibility = false;
          result.DurationVisibility = false;
          result.DurationUnitVisibility = false;
          result.Time2Visibility = false;

          result.InternalReferenceVisibility = false;
          result.ReferenceSourceVisibility = false;
          result.ExternalReferenceVisibility = false;
      }

      this._getForClientResult = result;
    }

    return this._getForClientResult;
  }

  private _visibilityChoices: SelectorChoice[];
  public get visibilityChoices(): SelectorChoice[] {
    if (!this._visibilityChoices) {
      this._visibilityChoices = [
        { value: 'None', name: () => this.translate.instant('Visibility_None') },
        { value: 'Optional', name: () => this.translate.instant('Visibility_Optional') },
        { value: 'Required', name: () => this.translate.instant('Visibility_Required') }
      ];
    }

    return this._visibilityChoices;
  }

  // Document Type

  public get documentTypeChoices(): SelectorChoice[] {
    const desc = metadata_DocumentDefinition(this.workspace, this.translate).properties.DocumentType as ChoicePropDescriptor;
    return getChoices(desc);
  }

  // Menu stuff

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_DocumentDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_DocumentDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }


  public onIconClick(model: DocumentDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }

  // State Management
  public onMakeHidden = (model: DocumentDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Hidden') {
      this.documentDefinitionsApi.updateState([model.Id], { state: 'Hidden', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeVisible = (model: DocumentDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Visible') {
      this.documentDefinitionsApi.updateState([model.Id], { state: 'Visible', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeArchived = (model: DocumentDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Archived') {
      this.documentDefinitionsApi.updateState([model.Id], { state: 'Archived', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showMakeHidden = (model: DocumentDefinition) => !!model && model.State !== 'Hidden';
  public showMakeVisible = (model: DocumentDefinition) => !!model && model.State !== 'Visible';
  public showMakeArchived = (model: DocumentDefinition) => !!model && model.State !== 'Archived';

  public hasStatePermission = (model: DocumentDefinition) => this.ws.canDo('document-definitions', 'State', model.Id);

  public stateTooltip = (model: DocumentDefinition) => this.hasStatePermission(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  // Grid management

  public rowDrop(event: CdkDragDrop<any[]>, collection: any[]) {
    moveItemInArray(collection, event.previousIndex, event.currentIndex);
  }

  public onInsertRow(collection: any[], create?: () => any) {
    const item = !!create ? create() : {};
    collection.push(item);
  }

  public itemToEditHasChanged = false;

  public onItemToEditChange() {
    this.itemToEditHasChanged = true;
  }

  // Line Definitions

  public lineDefinitionToEdit: DocumentDefinitionLineDefinition;

  public onCreateLineDefinition(model: DocumentDefinition) {
    const itemToEdit: DocumentDefinitionLineDefinition = { IsVisibleByDefault: true };
    this.lineDefinitionToEdit = itemToEdit; // Create new
    this.modalService.open(this.lineDefinitionModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply) {
        model.LineDefinitions.push(itemToEdit);
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onConfigureLineDefinition(index: number, model: DocumentDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.LineDefinitions[index] } as DocumentDefinitionLineDefinition;
    this.lineDefinitionToEdit = itemToEdit;
    this.modalService.open(this.lineDefinitionModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply && this.itemToEditHasChanged) {
        model.LineDefinitions[index] = itemToEdit;
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onDeleteLineDefinition(index: number, model: DocumentDefinition) {
    model.LineDefinitions.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public get canApplyLineDefinition(): boolean {
    return !!this.lineDefinitionToEdit && !!this.lineDefinitionToEdit.LineDefinitionId;
  }
}

function disjunction(filtersHash: { [key: string]: true }): string {
  if (!!filtersHash) {
    const filters = Object.keys(filtersHash);
    if (filters.length === 1) {
      return filters[0];
    } else if (filters.length > 1) {
      return filters.map(e => `(${e})`)?.reduce((e1, e2) => `${e1} or ${e2}`);
    }
  }

  return null;
}
