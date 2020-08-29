// tslint:disable:member-ordering
import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ResourceDefinitionForSave, metadata_ResourceDefinition, ResourceDefinition } from '~/app/data/entities/resource-definition';
import { DefinitionVisibility, DefinitionCardinality } from '~/app/data/entities/base/definition-common';
import { ResourceDefinitionForClient, DefinitionForClient, DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';

@Component({
  selector: 't-resource-definitions-details',
  templateUrl: './resource-definitions-details.component.html',
  styles: []
})
export class ResourceDefinitionsDetailsComponent extends DetailsBaseComponent {

  private resourceDefinitionsApi = this.api.resourceDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = 'DefaultUnit,DefaultUnitMassUnit';

  create = () => {
    const result: ResourceDefinitionForSave = {};
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

    result.UnitCardinality = 'None';

    return result;
  }

  private allVisibilityProps(): string[] {
    const props = metadata_ResourceDefinition(this.workspace, this.translate).properties;
    const result = [];
    for (const propName of Object.keys(props)) {
      if (propName.endsWith('Visibility')) {
        result.push(propName);
      }
    }

    return result;
  }

  clone: (item: ResourceDefinition) => ResourceDefinition = (item: ResourceDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as ResourceDefinition;
      clone.Id = null;

      // if (!!clone.Rows) {
      //   clone.Rows.forEach(e => {
      //     e.Id = null;
      //   });
      // }
      // if (!!clone.Columns) {
      //   clone.Columns.forEach(e => {
      //     e.Id = null;
      //   });
      // }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.resourceDefinitionsApi = this.api.resourceDefinitionsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public savePreprocessing = (entity: ResourceDefinition) => {
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

  public isInactive: (model: ResourceDefinition) => string = (_: ResourceDefinition) => null;

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  private _sections: { [key: string]: boolean } = {
    Title: true,
    Fields: false,
    MainMenu: false
  };

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public sectionErrors(section: string, model: ResourceDefinition) {
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
        areServerErrors(model.serverErrors.ToDateTillLabel) ||
        areServerErrors(model.serverErrors.ToDateTillLabel2) ||
        areServerErrors(model.serverErrors.ToDateTillLabel3) ||
        areServerErrors(model.serverErrors.ToDateTillVisibility) ||
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

        // Resource Only
        areServerErrors(model.serverErrors.IdentifierLabel) ||
        areServerErrors(model.serverErrors.IdentifierLabel2) ||
        areServerErrors(model.serverErrors.IdentifierLabel3) ||
        areServerErrors(model.serverErrors.IdentifierVisibility) ||
        areServerErrors(model.serverErrors.ReorderLevelVisibility) ||
        areServerErrors(model.serverErrors.EconomicOrderQuantityVisibility) ||
        areServerErrors(model.serverErrors.UnitCardinality) ||
        areServerErrors(model.serverErrors.DefaultUnitId) ||
        areServerErrors(model.serverErrors.UnitMassVisibility) ||
        areServerErrors(model.serverErrors.DefaultUnitMassUnitId) ||
        areServerErrors(model.serverErrors.MonetaryValueVisibility) ||
        false
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

  public onDefinitionChange(model: ResourceDefinition, prop?: string) {
    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  private _currentModel: ResourceDefinition;
  private _currentModelModified = false;
  private _getForClientResult: ResourceDefinitionForClient;

  public getForClient(model: ResourceDefinition): ResourceDefinitionForClient {
    if (!model) {
      return null;
    }

    if (this._currentModel !== model || this._currentModelModified) {
      this._currentModelModified = false;
      this._currentModel = model;

      // The mapping is trivial since the two data structures are identical
      this._getForClientResult = { ...model } as ResourceDefinitionForClient;

      // In definitions for client, a None visibility becomes undefined
      for (const propName of this.allVisibilityProps()) {
        const value = this._getForClientResult[propName] as DefinitionVisibility;
        if (value === 'None') {
          delete this._getForClientResult[propName];
        }
      }

      // In definitions for client, a None cardinality becomes undefined
      if (this._getForClientResult.UnitCardinality === 'None') {
        delete this._getForClientResult.UnitCardinality;
      }
    }

    return this._getForClientResult;
  }

  private _resourceDefinitionTypeChoices: SelectorChoice[];
  public get resourceDefinitionTypeChoices(): SelectorChoice[] {
    if (!this._resourceDefinitionTypeChoices) {
      const propDesc = metadata_ResourceDefinition(this.workspace, this.translate)
        .properties.ResourceDefinitionType as ChoicePropDescriptor;

      this._resourceDefinitionTypeChoices = getChoices(propDesc);
    }

    return this._resourceDefinitionTypeChoices;
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

  private _lookupDefinitionChoicesDef: DefinitionsForClient;
  private _lookupDefinitionChoices: SelectorChoice[];
  public get lookupDefinitionChoices(): SelectorChoice[] {
    if (this._lookupDefinitionChoicesDef !== this.ws.definitions) {
      this._lookupDefinitionChoicesDef = this.ws.definitions;
      this._lookupDefinitionChoices = [];
      const lookups = this.ws.definitions.Lookups;
      for (const key of Object.keys(lookups)) {
        const id = +key;
        const lookupDef = lookups[id];
        this._lookupDefinitionChoices.push({
          value: id,
          name: () => this.ws.getMultilingualValueImmediate(lookupDef, 'TitleSingular')
        });
      }
    }

    return this._lookupDefinitionChoices;
  }

  private _relationDefinitionChoicesDef: DefinitionsForClient;
  private _relationDefinitionChoices: SelectorChoice[];
  public get relationDefinitionChoices(): SelectorChoice[] {
    if (this._relationDefinitionChoicesDef !== this.ws.definitions) {
      this._relationDefinitionChoicesDef = this.ws.definitions;
      this._relationDefinitionChoices = [];
      const relations = this.ws.definitions.Relations;
      for (const key of Object.keys(relations)) {
        const id = +key;
        const relationDef = relations[id];
        this._relationDefinitionChoices.push({
          value: id,
          name: () => this.ws.getMultilingualValueImmediate(relationDef, 'TitleSingular')
        });
      }
    }

    return this._relationDefinitionChoices;
  }

  public isVisible(visibility: DefinitionVisibility) {
    return visibility === 'Optional' || visibility === 'Required';
  }

  public isCardinalityVisible(cardinality: DefinitionCardinality) {
    return cardinality === 'Single' || cardinality === 'Multiple';
  }

  // Menu stuff

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_ResourceDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_ResourceDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }


  public onIconClick(model: ResourceDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }

  // State Management
  public onMakeHidden = (model: ResourceDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Hidden') {
      this.resourceDefinitionsApi.updateState([model.Id], { state: 'Hidden', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeVisible = (model: ResourceDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Visible') {
      this.resourceDefinitionsApi.updateState([model.Id], { state: 'Visible', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onMakeArchived = (model: ResourceDefinition): void => {
    if (!!model && !!model.Id && model.State !== 'Archived') {
      this.resourceDefinitionsApi.updateState([model.Id], { state: 'Archived', returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showMakeHidden = (model: ResourceDefinition) => !!model && model.State !== 'Hidden';
  public showMakeVisible = (model: ResourceDefinition) => !!model && model.State !== 'Visible';
  public showMakeArchived = (model: ResourceDefinition) => !!model && model.State !== 'Archived';

  public hasStatePermission = (model: ResourceDefinition) => this.ws.canDo('resource-definitions', 'State', model.Id);

  public stateTooltip = (model: ResourceDefinition) => this.hasStatePermission(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
