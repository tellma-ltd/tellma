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
import { DocumentDefinitionMarkupTemplate } from '~/app/data/entities/document-definition-markup-template';

@Component({
  selector: 't-document-definitions-details',
  templateUrl: './document-definitions-details.component.html',
  styles: []
})
export class DocumentDefinitionsDetailsComponent extends DetailsBaseComponent {

  @ViewChild('lineDefinitionModal', { static: true })
  lineDefinitionModal: TemplateRef<any>;

  @ViewChild('markupTemplateModal', { static: true })
  markupTemplateModal: TemplateRef<any>;

  private documentDefinitionsApi = this.api.documentDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = 'LineDefinitions/LineDefinition,MarkupTemplates/MarkupTemplate';

  create = () => {
    const result: DocumentDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

    result.ClearanceVisibility = 'None';
    result.MemoVisibility = 'Optional';
    result.IsOriginalDocument = false;
    result.CodeWidth = 4;
    result.DocumentType = 2;
    result.LineDefinitions = [];
    result.MarkupTemplates = [];

    return result;
  }

  clone: (item: DocumentDefinition) => DocumentDefinition = (item: DocumentDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as DocumentDefinition;
      clone.Id = null;

      if (!!clone.LineDefinitions) {
        clone.LineDefinitions.forEach(e => {
          e.Id = null;
        });
      }
      if (!!clone.MarkupTemplates) {
        clone.MarkupTemplates.forEach(e => {
          e.Id = null;
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
        areServerErrors(model.serverErrors.MemoVisibility) ||
        areServerErrors(model.serverErrors.IsOriginalDocument) ||
        areServerErrors(model.serverErrors.DocumentType) ||
        areServerErrors(model.serverErrors.Prefix) ||
        areServerErrors(model.serverErrors.CodeWidth) ||
        areServerErrors(model.serverErrors.MemoVisibility) ||
        areServerErrors(model.serverErrors.ClearanceVisibility) ||
        areServerErrors(model.serverErrors.LineDefinitions) ||
        areServerErrors(model.serverErrors.MarkupTemplates)
      )) ||
        (!!model.LineDefinitions && model.LineDefinitions.some(e => this.weakEntityErrors(e))) ||
        (!!model.MarkupTemplates && model.MarkupTemplates.some(e => this.weakEntityErrors(e)));
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

      // Not needed for preview
      result.MarkupTemplates = [];

      result.NotedRelationDefinitionIds = [];

      // The rest looks identical to the C# code
      const documentLineDefinitions = result.LineDefinitions
        .map(e => defs.Lines[e.LineDefinitionId])
        .filter(e => !!e && !!e.Columns);

      // Hash tables to accumulate some values
      let notedRelationDefIds: { [id: number]: true } = {};
      let notedRelationFilters: { [filter: string]: true } = {};
      let centerFilters: { [filter: string]: true } = {};
      let currencyFilters: { [filter: string]: true } = {};

      for (const lineDef of documentLineDefinitions) {
        for (const colDef of lineDef.Columns.filter(c => !!c.InheritsFromHeader)) {
          if (colDef.ColumnName === 'Memo') {
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

          } else if (colDef.ColumnName === 'PostingDate') {
            result.PostingDateVisibility = true;
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

            // Relations
          } else if (colDef.ColumnName === 'NotedRelationId') {

            result.NotedRelationVisibility = true;
            if (!result.NotedRelationLabel) {
              result.NotedRelationLabel = colDef.Label;
              result.NotedRelationLabel2 = colDef.Label2;
              result.NotedRelationLabel3 = colDef.Label3;
            }

            if (colDef.RequiredState > (result.NotedRelationRequiredState ?? 0)) {
              result.NotedRelationRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.NotedRelationReadOnlyState ?? 0)) {
              result.NotedRelationReadOnlyState = colDef.ReadOnlyState;
            }

            if (colDef.EntryIndex < lineDef.Entries.length) {
              const entryDef = lineDef.Entries[colDef.EntryIndex];
              if (!entryDef.NotedRelationDefinitionIds || entryDef.NotedRelationDefinitionIds.length === 0) {
                notedRelationDefIds = null; // Means no definitionIds will be added
              } else if (!!notedRelationDefIds) {
                for (const defId of entryDef.NotedRelationDefinitionIds) {
                  notedRelationDefIds[defId] = true;
                }
              }
            }

            if (!colDef.Filter) {
              notedRelationFilters = null;
            } else if (!!notedRelationFilters) {
              notedRelationFilters[colDef.Filter] = true;
            }

          } else if (colDef.ColumnName === 'CenterId') {
            result.CenterVisibility = true;
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

          } else if (colDef.ColumnName === 'AdditionalReference') {
            result.AdditionalReferenceVisibility = true;
            if (!result.AdditionalReferenceLabel) {
              result.AdditionalReferenceLabel = colDef.Label;
              result.AdditionalReferenceLabel2 = colDef.Label2;
              result.AdditionalReferenceLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState > (result.AdditionalReferenceRequiredState ?? 0)) {
              result.AdditionalReferenceRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState > (result.AdditionalReferenceReadOnlyState ?? 0)) {
              result.AdditionalReferenceReadOnlyState = colDef.ReadOnlyState;
            }
          }
        }
      }
      // Calculate the definitionIds and filters
      result.NotedRelationDefinitionIds = Object.keys(notedRelationDefIds ?? {}).map(e => +e);
      result.NotedRelationFilter = disjunction(notedRelationFilters);
      result.CenterFilter = disjunction(centerFilters);
      result.CurrencyFilter = disjunction(currencyFilters);

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

  // Markup Templates

  public markupTemplateToEdit: DocumentDefinitionMarkupTemplate;

  public onCreateMarkupTemplate(model: DocumentDefinition) {
    const itemToEdit: DocumentDefinitionMarkupTemplate = {};
    this.markupTemplateToEdit = itemToEdit; // Create new
    this.modalService.open(this.markupTemplateModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply) {
        model.MarkupTemplates.push(itemToEdit);
      }
    }, (_: any) => { });
  }

  public onConfigureMarkupTemplate(index: number, model: DocumentDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.MarkupTemplates[index] } as DocumentDefinitionMarkupTemplate;
    this.markupTemplateToEdit = itemToEdit;
    this.modalService.open(this.markupTemplateModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply && this.itemToEditHasChanged) {
        model.MarkupTemplates[index] = itemToEdit;
      }
    }, (_: any) => { });
  }

  public onDeleteMarkupTemplate(index: number, model: DocumentDefinition) {
    model.MarkupTemplates.splice(index, 1);
    this.onDefinitionChange(model);
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
