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
import { LookupDefinitionForSave, metadata_LookupDefinition, LookupDefinition } from '~/app/data/entities/lookup-definition';
import { LookupDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';
import { LookupDefinitionReportDefinition } from '~/app/data/entities/lookup-definition-report-definition';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';

@Component({
  selector: 't-lookup-definitions-details',
  templateUrl: './lookup-definitions-details.component.html',
  styles: []
})
export class LookupDefinitionsDetailsComponent extends DetailsBaseComponent {

  @ViewChild('reportDefinitionModal', { static: true })
  reportDefinitionModal: TemplateRef<any>;

  private lookupDefinitionsApi = this.api.lookupDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = 'ReportDefinitions/ReportDefinition';

  create = () => {
    const result: LookupDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

    result.ReportDefinitions = [];

    return result;
  }

  private allVisibilityProps(): string[] {
    const props = metadata_LookupDefinition(this.workspace, this.translate).properties;
    const result = [];
    for (const propName of Object.keys(props)) {
      if (propName.endsWith('Visibility')) {
        result.push(propName);
      }
    }

    return result;
  }

  clone: (item: LookupDefinition) => LookupDefinition = (item: LookupDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as LookupDefinition;
      clone.Id = null;

      if (!!clone.ReportDefinitions) {
        clone.ReportDefinitions.forEach(e => {
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

    this.lookupDefinitionsApi = this.api.lookupDefinitionsApi(this.notifyDestruct$);
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

  public isInactive: (model: LookupDefinition) => string = (_: LookupDefinition) => null;

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  private _sections: { [key: string]: boolean } = {
    Title: true,
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

  public sectionErrors(section: string, model: LookupDefinition) {
    if (section === 'Title') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.Code) ||
        areServerErrors(model.serverErrors.TitleSingular) ||
        areServerErrors(model.serverErrors.TitleSingular2) ||
        areServerErrors(model.serverErrors.TitleSingular3) ||
        areServerErrors(model.serverErrors.TitlePlural) ||
        areServerErrors(model.serverErrors.TitlePlural2) ||
        areServerErrors(model.serverErrors.TitlePlural3)
      ));
    } else if (section === 'Reports') {
      return !!model.ReportDefinitions &&
        model.ReportDefinitions.some(e => this.weakEntityErrors(e));
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

  public onDefinitionChange(model: LookupDefinition, prop?: string) {
    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  private _currentModel: LookupDefinition;
  private _currentModelModified = false;
  private _getForClientResult: LookupDefinitionForClient;

  public getForClient(model: LookupDefinition): LookupDefinitionForClient {
    if (!model) {
      return null;
    }

    if (this._currentModel !== model || this._currentModelModified) {
      this._currentModelModified = false;
      this._currentModel = model;

      // The mapping is trivial since the two data structures are identical
      this._getForClientResult = { ...model } as LookupDefinitionForClient;
    }

    return this._getForClientResult;
  }

  // Menu stuff

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_LookupDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_LookupDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }


  public onIconClick(model: LookupDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }

  // State Management
  public onMakeHidden = (model: LookupDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Hidden') {
      this.lookupDefinitionsApi.updateState([model.Id], { state: 'Hidden', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeVisible = (model: LookupDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Visible') {
      this.lookupDefinitionsApi.updateState([model.Id], { state: 'Visible', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeArchived = (model: LookupDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Archived') {
      this.lookupDefinitionsApi.updateState([model.Id], { state: 'Archived', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showMakeHidden = (model: LookupDefinition) => !!model && model.State !== 'Hidden';
  public showMakeVisible = (model: LookupDefinition) => !!model && model.State !== 'Visible';
  public showMakeArchived = (model: LookupDefinition) => !!model && model.State !== 'Archived';

  public hasStatePermission = (model: LookupDefinition) => this.ws.canDo('lookup-definitions', 'State', model.Id);

  public stateTooltip = (model: LookupDefinition) => this.hasStatePermission(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  // Report Definitions

  public itemToEditHasChanged = false;
  public reportDefinitionToEdit: LookupDefinitionReportDefinition;

  public onItemToEditChange() {
    this.itemToEditHasChanged = true;
  }

  public onCreateReportDefinition(model: LookupDefinition) {
    const itemToEdit: LookupDefinitionReportDefinition = {};
    this.reportDefinitionToEdit = itemToEdit; // Create new
    this.modalService.open(this.reportDefinitionModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply) {
        model.ReportDefinitions.push(itemToEdit);
      }
    }, (_: any) => { });
  }

  public onConfigureReportDefinition(index: number, model: LookupDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.ReportDefinitions[index] } as LookupDefinitionReportDefinition;
    this.reportDefinitionToEdit = itemToEdit;
    this.modalService.open(this.reportDefinitionModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply && this.itemToEditHasChanged) {
        model.ReportDefinitions[index] = itemToEdit;
      }
    }, (_: any) => { });
  }

  public onDeleteReportDefinition(index: number, model: LookupDefinition) {
    model.ReportDefinitions.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public rowDrop(event: CdkDragDrop<any[]>, collection: any[]) {
    moveItemInArray(collection, event.previousIndex, event.currentIndex);
  }
}
