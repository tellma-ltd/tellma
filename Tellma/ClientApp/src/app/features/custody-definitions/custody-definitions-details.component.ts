// tslint:disable:member-ordering
import { Component, TemplateRef, ViewChild } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace, onCodeTextareaKeydown } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { CustodyDefinitionForSave, metadata_CustodyDefinition, CustodyDefinition } from '~/app/data/entities/custody-definition';
import { DefinitionVisibility } from '~/app/data/entities/base/definition-common';
import { CustodyDefinitionForClient, DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { CustodyDefinitionReportDefinition } from '~/app/data/entities/custody-definition-report-definition';

@Component({
  selector: 't-custody-definitions-details',
  templateUrl: './custody-definitions-details.component.html',
  styles: []
})
export class CustodyDefinitionsDetailsComponent extends DetailsBaseComponent {

  @ViewChild('reportDefinitionModal', { static: true })
  reportDefinitionModal: TemplateRef<any>;

  @ViewChild('scriptModal', { static: true })
  scriptModal: TemplateRef<any>;

  private custodyDefinitionsApi = this.api.custodyDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = `ReportDefinitions/ReportDefinition,
Lookup1Definition,Lookup2Definition,Lookup3Definition,Lookup4Definition,CustodianDefinition`;

  create = () => {
    const result: CustodyDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

    // Set all visibility properties to 'None' by default
    const none: DefinitionVisibility = 'None';
    for (const propName of this.allVisibilityProps()) {
      result[propName] = none;
    }

    result.ReportDefinitions = [];

    return result;
  }

  private allVisibilityProps(): string[] {
    const props = metadata_CustodyDefinition(this.workspace, this.translate).properties;
    const result = [];
    for (const propName of Object.keys(props)) {
      if (propName.endsWith('Visibility')) {
        result.push(propName);
      }
    }

    return result;
  }

  clone: (item: CustodyDefinition) => CustodyDefinition = (item: CustodyDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as CustodyDefinition;
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

    this.custodyDefinitionsApi = this.api.custodyDefinitionsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public savePreprocessing = (entity: CustodyDefinition) => {
    // Server validation on hidden properties will be confusing to the user
    for (const prop of this.allVisibilityProps()) {
      const value: DefinitionVisibility = entity[prop];
      if (value === 'None') {
        const woLabel = prop.substr(0, prop.length - 'Visibility'.length);
        delete entity[woLabel + 'Label'];
        delete entity[woLabel + 'Label2'];
        delete entity[woLabel + 'Label3'];
        delete entity[woLabel + 'DefinitionId'];
      }
    }
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

  public isInactive: (model: CustodyDefinition) => string = (_: CustodyDefinition) => null;

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  private _sections: { [key: string]: boolean } = {
    Title: true,
    Fields: false,
    Scripts: false,
    Reports: false,
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

  public sectionErrors(section: string, model: CustodyDefinition) {
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
    } else if (section === 'Fields') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.CurrencyVisibility) ||
        areServerErrors(model.serverErrors.CenterVisibility) ||
        areServerErrors(model.serverErrors.ImageVisibility) ||
        areServerErrors(model.serverErrors.DescriptionVisibility) ||
        areServerErrors(model.serverErrors.LocationVisibility) ||
        areServerErrors(model.serverErrors.FromDateLabel) ||
        areServerErrors(model.serverErrors.FromDateLabel2) ||
        areServerErrors(model.serverErrors.FromDateLabel3) ||
        areServerErrors(model.serverErrors.FromDateVisibility) ||
        areServerErrors(model.serverErrors.ToDateLabel) ||
        areServerErrors(model.serverErrors.ToDateLabel2) ||
        areServerErrors(model.serverErrors.ToDateLabel3) ||
        areServerErrors(model.serverErrors.ToDateVisibility) ||
        areServerErrors(model.serverErrors.Decimal1Label) ||
        areServerErrors(model.serverErrors.Decimal1Label2) ||
        areServerErrors(model.serverErrors.Decimal1Label3) ||
        areServerErrors(model.serverErrors.Decimal1Visibility) ||
        areServerErrors(model.serverErrors.Decimal2Label) ||
        areServerErrors(model.serverErrors.Decimal2Label2) ||
        areServerErrors(model.serverErrors.Decimal2Label3) ||
        areServerErrors(model.serverErrors.Decimal2Visibility) ||
        areServerErrors(model.serverErrors.Int1Label) ||
        areServerErrors(model.serverErrors.Int1Label2) ||
        areServerErrors(model.serverErrors.Int1Label3) ||
        areServerErrors(model.serverErrors.Int1Visibility) ||
        areServerErrors(model.serverErrors.Int2Label) ||
        areServerErrors(model.serverErrors.Int2Label2) ||
        areServerErrors(model.serverErrors.Int2Label3) ||
        areServerErrors(model.serverErrors.Int2Visibility) ||
        areServerErrors(model.serverErrors.Lookup1Label) ||
        areServerErrors(model.serverErrors.Lookup1Label2) ||
        areServerErrors(model.serverErrors.Lookup1Label3) ||
        areServerErrors(model.serverErrors.Lookup1Visibility) ||
        areServerErrors(model.serverErrors.Lookup1DefinitionId) ||
        areServerErrors(model.serverErrors.Lookup2Label) ||
        areServerErrors(model.serverErrors.Lookup2Label2) ||
        areServerErrors(model.serverErrors.Lookup2Label3) ||
        areServerErrors(model.serverErrors.Lookup2Visibility) ||
        areServerErrors(model.serverErrors.Lookup2DefinitionId) ||
        areServerErrors(model.serverErrors.Lookup3Label) ||
        areServerErrors(model.serverErrors.Lookup3Label2) ||
        areServerErrors(model.serverErrors.Lookup3Label3) ||
        areServerErrors(model.serverErrors.Lookup3Visibility) ||
        areServerErrors(model.serverErrors.Lookup3DefinitionId) ||
        areServerErrors(model.serverErrors.Lookup4Label) ||
        areServerErrors(model.serverErrors.Lookup4Label2) ||
        areServerErrors(model.serverErrors.Lookup4Label3) ||
        areServerErrors(model.serverErrors.Lookup4Visibility) ||
        areServerErrors(model.serverErrors.Lookup4DefinitionId) ||
        areServerErrors(model.serverErrors.Text1Label) ||
        areServerErrors(model.serverErrors.Text1Label2) ||
        areServerErrors(model.serverErrors.Text1Label3) ||
        areServerErrors(model.serverErrors.Text1Visibility) ||
        areServerErrors(model.serverErrors.Text2Label) ||
        areServerErrors(model.serverErrors.Text2Label2) ||
        areServerErrors(model.serverErrors.Text2Label3) ||
        areServerErrors(model.serverErrors.Text2Visibility) ||

        // Custody Only
        areServerErrors(model.serverErrors.CustodianVisibility) ||
        areServerErrors(model.serverErrors.CustodianDefinitionId) ||
        areServerErrors(model.serverErrors.ExternalReferenceLabel) ||
        areServerErrors(model.serverErrors.ExternalReferenceLabel2) ||
        areServerErrors(model.serverErrors.ExternalReferenceLabel3) ||
        areServerErrors(model.serverErrors.ExternalReferenceVisibility)
      ));
    } else if (section === 'Scripts') {
      return (!!model.serverErrors && (
        areServerErrors(model.serverErrors.PreprocessScript) ||
        areServerErrors(model.serverErrors.ValidateScript)
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

  public serverErrors(obj: EntityForSave, prop: string): string[] {
    if (!obj || !obj.serverErrors) {
      return null;
    }

    return obj.serverErrors[prop];
  }

  public onDefinitionChange(model: CustodyDefinition, prop?: string) {
    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  private _currentModel: CustodyDefinition;
  private _currentModelModified = false;
  private _getForClientResult: CustodyDefinitionForClient;

  public getForClient(model: CustodyDefinition): CustodyDefinitionForClient {
    if (!model) {
      return null;
    }

    if (this._currentModel !== model || this._currentModelModified) {
      this._currentModelModified = false;
      this._currentModel = model;

      // The mapping is trivial since the two data structures are identical
      this._getForClientResult = { ...model } as CustodyDefinitionForClient;

      // In definitions for client, a null visibility becomes undefined
      for (const propName of this.allVisibilityProps()) {
        const value = this._getForClientResult[propName] as DefinitionVisibility;
        if (value === 'None') {
          delete this._getForClientResult[propName];
        }
      }

      console.log(this._getForClientResult);
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

  private _cardinalityChoices: SelectorChoice[];
  public get cardinalityChoices(): SelectorChoice[] {
    if (!this._cardinalityChoices) {
      this._cardinalityChoices = [
        { value: 'None', name: () => this.translate.instant('Cardinality_None') },
        { value: 'Single', name: () => this.translate.instant('Cardinality_Single') },
        { value: 'Multiple', name: () => this.translate.instant('Cardinality_Multiple') }
      ];
    }

    return this._cardinalityChoices;
  }

  public isVisible(visibility: DefinitionVisibility) {
    return visibility === 'Optional' || visibility === 'Required';
  }

  // Menu stuff

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_CustodyDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_CustodyDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }


  public onIconClick(model: CustodyDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }

  // State Management
  public onMakeHidden = (model: CustodyDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Hidden') {
      this.custodyDefinitionsApi.updateState([model.Id], { state: 'Hidden', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeVisible = (model: CustodyDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Visible') {
      this.custodyDefinitionsApi.updateState([model.Id], { state: 'Visible', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeArchived = (model: CustodyDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Archived') {
      this.custodyDefinitionsApi.updateState([model.Id], { state: 'Archived', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showMakeHidden = (model: CustodyDefinition) => !!model && model.State !== 'Hidden';
  public showMakeVisible = (model: CustodyDefinition) => !!model && model.State !== 'Visible';
  public showMakeArchived = (model: CustodyDefinition) => !!model && model.State !== 'Archived';

  public hasStatePermission = (model: CustodyDefinition) => this.ws.canDo('custody-definitions', 'State', model.Id);

  public stateTooltip = (model: CustodyDefinition) => this.hasStatePermission(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  // Report Definitions

  public itemToEditHasChanged = false;
  public reportDefinitionToEdit: CustodyDefinitionReportDefinition;

  public onItemToEditChange() {
    this.itemToEditHasChanged = true;
  }

  public onCreateReportDefinition(model: CustodyDefinition) {
    const itemToEdit: CustodyDefinitionReportDefinition = {};
    this.reportDefinitionToEdit = itemToEdit; // Create new
    this.modalService.open(this.reportDefinitionModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply) {
        model.ReportDefinitions.push(itemToEdit);
      }
    }, (_: any) => { });
  }

  public onConfigureReportDefinition(index: number, model: CustodyDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.ReportDefinitions[index] } as CustodyDefinitionReportDefinition;
    this.reportDefinitionToEdit = itemToEdit;
    this.modalService.open(this.reportDefinitionModal, { windowClass: 't-dark-theme' }).result.then((apply: boolean) => {
      if (apply && this.itemToEditHasChanged) {
        model.ReportDefinitions[index] = itemToEdit;
      }
    }, (_: any) => { });
  }

  public onDeleteReportDefinition(index: number, model: CustodyDefinition) {
    model.ReportDefinitions.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public rowDrop(event: CdkDragDrop<any[]>, collection: any[]) {
    moveItemInArray(collection, event.previousIndex, event.currentIndex);
  }

  // Scripts

  public script: string;
  public scriptModalLabel: () => string;
  public onEditScript(scriptName: string, model: CustodyDefinition) {
    // Prep
    this.script = model[scriptName]; // Copy the script
    this.scriptModalLabel = () => this.translate.instant('Definition_' + scriptName);

    // Launch the modal
    this.modalService.open(this.scriptModal, { windowClass: 't-dark-theme t-details-modal' }).result.then((apply: boolean) => {
      if (apply) {
        model[scriptName] = this.script;
      }
    }, (_: any) => { });
  }

  public onScriptKeydown(elem: HTMLTextAreaElement, $event: KeyboardEvent) {
    onCodeTextareaKeydown(elem, $event, v => this.script = v);
  }
}
