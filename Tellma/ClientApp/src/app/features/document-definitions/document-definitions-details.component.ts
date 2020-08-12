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

  public isInactive: (model: DocumentDefinition) => string = (_: DocumentDefinition) => null;

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  private _sections: { [key: string]: boolean } = {
    Title: false,
    Fields: true,
    MainMenu: false
  };

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public sectionErrors(section: string, model: DocumentDefinition) {
    if (section === 'Title') {
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
    } else if (section === 'Fields') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.MemoVisibility) ||
        areServerErrors(model.serverErrors.IsOriginalDocument) ||
        areServerErrors(model.serverErrors.DocumentType) ||
        areServerErrors(model.serverErrors.Prefix) ||
        areServerErrors(model.serverErrors.CodeWidth) ||
        areServerErrors(model.serverErrors.MemoVisibility) ||
        areServerErrors(model.serverErrors.ClearanceVisibility)
      ));
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

      result.CreditResourceDefinitionIds = [];
      result.DebitResourceDefinitionIds = [];
      result.CreditCustodyDefinitionIds = [];
      result.DebitCustodyDefinitionIds = [];
      result.NotedRelationDefinitionIds = [];

      // The rest looks identical to the C# code
      const documentLineDefinitions = result.LineDefinitions
        .map(e => defs.Lines[e.LineDefinitionId])
        .filter(e => !!e && !!e.Columns);

      for (const lineDef of documentLineDefinitions) {
        for (const colDef of lineDef.Columns.filter(c => !!c.InheritsFromHeader)) {
          // Memo
          if (colDef.ColumnName === 'Memo') {
            result.MemoIsCommonVisibility = true;
            if (!result.MemoLabel) {
              result.MemoLabel = colDef.Label;
              result.MemoLabel2 = colDef.Label2;
              result.MemoLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.MemoRequiredState ?? 5)) {
              result.MemoRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.MemoReadOnlyState ?? 5)) {
              result.MemoReadOnlyState = colDef.ReadOnlyState;
            }

            // Posting Date
          } else if (colDef.ColumnName === 'PostingDate') {
            result.PostingDateVisibility = true;
            if (!result.PostingDateLabel) {
              result.PostingDateLabel = colDef.Label;
              result.PostingDateLabel2 = colDef.Label2;
              result.PostingDateLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.PostingDateRequiredState ?? 5)) {
              result.PostingDateRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.PostingDateReadOnlyState ?? 5)) {
              result.PostingDateReadOnlyState = colDef.ReadOnlyState;
            }

            // Relations
          } else if (colDef.EntryIndex < lineDef.Entries.length) {
            const entryDef = lineDef.Entries[colDef.EntryIndex];

            // DebitResource
            if (colDef.ColumnName === 'ResourceId' && entryDef.Direction === 1) {
              result.DebitResourceVisibility = true;
              if (!result.DebitResourceLabel) {
                result.DebitResourceLabel = result.DebitResourceLabel || colDef.Label;
                result.DebitResourceLabel2 = result.DebitResourceLabel2 || colDef.Label2;
                result.DebitResourceLabel3 = result.DebitResourceLabel3 || colDef.Label3;

                result.DebitResourceDefinitionIds = entryDef.ResourceDefinitionIds;
              }

              if (colDef.RequiredState < (result.DebitResourceRequiredState ?? 5)) {
                result.DebitResourceRequiredState = colDef.RequiredState;
              }

              if (colDef.ReadOnlyState < (result.DebitResourceReadOnlyState ?? 5)) {
                result.DebitResourceReadOnlyState = colDef.ReadOnlyState;
              }
            }

            // CreditResource
            if (colDef.ColumnName === 'ResourceId' && entryDef.Direction === 1) {
              result.CreditResourceVisibility = true;
              if (!result.CreditResourceLabel) {
                result.CreditResourceLabel = result.CreditResourceLabel || colDef.Label;
                result.CreditResourceLabel2 = result.CreditResourceLabel2 || colDef.Label2;
                result.CreditResourceLabel3 = result.CreditResourceLabel3 || colDef.Label3;

                result.CreditResourceDefinitionIds = entryDef.ResourceDefinitionIds;
              }

              if (colDef.RequiredState < (result.CreditResourceRequiredState ?? 5)) {
                result.CreditResourceRequiredState = colDef.RequiredState;
              }

              if (colDef.ReadOnlyState < (result.CreditResourceReadOnlyState ?? 5)) {
                result.CreditResourceReadOnlyState = colDef.ReadOnlyState;
              }
            }

            // DebitCustody
            if (colDef.ColumnName === 'CustodyId' && entryDef.Direction === 1) {
              result.DebitCustodyVisibility = true;
              if (!result.DebitCustodyLabel) {
                result.DebitCustodyLabel = result.DebitCustodyLabel || colDef.Label;
                result.DebitCustodyLabel2 = result.DebitCustodyLabel2 || colDef.Label2;
                result.DebitCustodyLabel3 = result.DebitCustodyLabel3 || colDef.Label3;

                result.DebitCustodyDefinitionIds = entryDef.CustodyDefinitionIds;
              }

              if (colDef.RequiredState < (result.DebitCustodyRequiredState ?? 5)) {
                result.DebitCustodyRequiredState = colDef.RequiredState;
              }

              if (colDef.ReadOnlyState < (result.DebitCustodyReadOnlyState ?? 5)) {
                result.DebitCustodyReadOnlyState = colDef.ReadOnlyState;
              }
            }

            // CreditCustody
            if (colDef.ColumnName === 'CustodyId' && entryDef.Direction === 1) {
              result.CreditCustodyVisibility = true;
              if (!result.CreditCustodyLabel) {
                result.CreditCustodyLabel = result.CreditCustodyLabel || colDef.Label;
                result.CreditCustodyLabel2 = result.CreditCustodyLabel2 || colDef.Label2;
                result.CreditCustodyLabel3 = result.CreditCustodyLabel3 || colDef.Label3;

                result.CreditCustodyDefinitionIds = entryDef.CustodyDefinitionIds;
              }

              if (colDef.RequiredState < (result.CreditCustodyRequiredState ?? 5)) {
                result.CreditCustodyRequiredState = colDef.RequiredState;
              }

              if (colDef.ReadOnlyState < (result.CreditCustodyReadOnlyState ?? 5)) {
                result.CreditCustodyReadOnlyState = colDef.ReadOnlyState;
              }
            }

            // NotedRelation
            if (colDef.ColumnName === 'NotedRelationId') {
              result.NotedRelationVisibility = true;
              if (!result.NotedRelationLabel) {
                result.NotedRelationLabel = colDef.Label;
                result.NotedRelationLabel2 = colDef.Label2;
                result.NotedRelationLabel3 = colDef.Label3;

                result.NotedRelationDefinitionIds = entryDef.NotedRelationDefinitionIds;
              }

              if (colDef.RequiredState < (result.NotedRelationRequiredState ?? 5)) {
                result.NotedRelationRequiredState = colDef.RequiredState;
              }

              if (colDef.ReadOnlyState < (result.NotedRelationReadOnlyState ?? 5)) {
                result.NotedRelationReadOnlyState = colDef.ReadOnlyState;
              }
            }
          }

          // Center
          if (colDef.ColumnName === 'CenterId') {
            result.CenterVisibility = true;
            if (!(result.CenterLabel)) {
              result.CenterLabel = colDef.Label;
              result.CenterLabel2 = colDef.Label2;
              result.CenterLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.CenterRequiredState ?? 5)) {
              result.CenterRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.CenterReadOnlyState ?? 5)) {
              result.CenterReadOnlyState = colDef.ReadOnlyState;
            }
          }

          // Time1
          if (colDef.ColumnName === 'Time1') {
            result.Time1Visibility = true;
            if (!(result.Time1Label)) {
              result.Time1Label = colDef.Label;
              result.Time1Label2 = colDef.Label2;
              result.Time1Label3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.Time1RequiredState ?? 5)) {
              result.Time1RequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.Time1ReadOnlyState ?? 5)) {
              result.Time1ReadOnlyState = colDef.ReadOnlyState;
            }
          }

          // Time2
          if (colDef.ColumnName === 'Time2') {
            result.Time2Visibility = true;
            if (!(result.Time2Label)) {
              result.Time2Label = colDef.Label;
              result.Time2Label2 = colDef.Label2;
              result.Time2Label3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.Time2RequiredState ?? 5)) {
              result.Time2RequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.Time2ReadOnlyState ?? 5)) {
              result.Time2ReadOnlyState = colDef.ReadOnlyState;
            }
          }

          // Quantity
          if (colDef.ColumnName === 'Quantity') {
            result.QuantityVisibility = true;
            if (!(result.QuantityLabel)) {
              result.QuantityLabel = colDef.Label;
              result.QuantityLabel2 = colDef.Label2;
              result.QuantityLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.QuantityRequiredState ?? 5)) {
              result.QuantityRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.QuantityReadOnlyState ?? 5)) {
              result.QuantityReadOnlyState = colDef.ReadOnlyState;
            }
          }

          // Unit
          if (colDef.ColumnName === 'UnitId') {
            result.UnitVisibility = true;
            if (!(result.UnitLabel)) {
              result.UnitLabel = colDef.Label;
              result.UnitLabel2 = colDef.Label2;
              result.UnitLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.UnitRequiredState ?? 5)) {
              result.UnitRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.UnitReadOnlyState ?? 5)) {
              result.UnitReadOnlyState = colDef.ReadOnlyState;
            }
          }

          // Currency
          if (colDef.ColumnName === 'CurrencyId') {
            result.CurrencyVisibility = true;
            if (!(result.CurrencyLabel)) {
              result.CurrencyLabel = colDef.Label;
              result.CurrencyLabel2 = colDef.Label2;
              result.CurrencyLabel3 = colDef.Label3;
            }
            if (colDef.RequiredState < (result.CurrencyRequiredState ?? 5)) {
              result.CurrencyRequiredState = colDef.RequiredState;
            }

            if (colDef.ReadOnlyState < (result.CurrencyReadOnlyState ?? 5)) {
              result.CurrencyReadOnlyState = colDef.ReadOnlyState;
            }
          }
        }
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

  // Line Definitions

  public lineDefinitionToEdit: DocumentDefinitionLineDefinition;
  public itemToEditHasChanged = false;

  public onItemToEditChange() {
    this.itemToEditHasChanged = true;
  }

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
