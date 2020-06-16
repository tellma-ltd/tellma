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
import { ContractDefinitionForSave, metadata_ContractDefinition, ContractDefinition } from '~/app/data/entities/contract-definition';
import { DefinitionVisibility } from '~/app/data/entities/base/definition-common';
import { ContractDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';

@Component({
  selector: 't-contract-definitions-details',
  templateUrl: './contract-definitions-details.component.html',
  styles: []
})
export class ContractDefinitionsDetailsComponent extends DetailsBaseComponent {

  // private contractDefinitionsApi = this.api.contractDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  create = () => {
    const result: ContractDefinitionForSave = {};
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

    result.AllowMultipleUsers = false;

    return result;
  }

  private allVisibilityProps(): string[] {
    const props = metadata_ContractDefinition(this.workspace, this.translate).properties;
    const result = [];
    for (const propName of Object.keys(props)) {
      if (propName.endsWith('Visibility')) {
        result.push(propName);
      }
    }

    return result;
  }

  clone: (item: ContractDefinition) => ContractDefinition = (item: ContractDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as ContractDefinition;
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

    // this.contractDefinitionsApi = this.api.contractDefinitionsApi(this.notifyDestruct$);
  }

  // get centerTypeChoices(): SelectorChoice[] {

  //   const descriptor = metadata_Center(this.workspace, this.translate)
  //     .properties.CenterType as ChoicePropDescriptor;

  //   return getChoices(descriptor);
  // }

  // public centerTypeLookup(value: string): string {
  //   const descriptor = metadata_Center(this.workspace, this.translate)
  //     .properties.CenterType as ChoicePropDescriptor;

  //   return descriptor.format(value);
  // }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public savePreprocessing(entity: ContractDefinition) {
    // Server validation on hidden collections will be confusing to the user
    if (entity.StartDateVisibility === 'None') {
      delete entity.StartDateLabel;
      delete entity.StartDateLabel2;
      delete entity.StartDateLabel3;
    }
  }

  public collapseDefinition = false;
  public onToggleDefinition(): void {
    this.collapseDefinition = !this.collapseDefinition;
    window.dispatchEvent(new Event('resize')); // So the chart would resize
  }

  private _isEdit = false;
  public watchIsEdit(isEdit: boolean): boolean {
    // this is a hack to trigger window resize when isEdit changes
    if (this._isEdit !== isEdit) {
      this._isEdit = isEdit;
    }

    return true;
  }

  public isInactive: (model: ContractDefinition) => string = (_: ContractDefinition) => null;

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

  public sectionErrors(section: string, model: ContractDefinition) {
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
        areServerErrors(model.serverErrors.AgentVisibility) ||
        areServerErrors(model.serverErrors.CurrencyVisibility) ||
        areServerErrors(model.serverErrors.TaxIdentificationNumberVisibility) ||
        areServerErrors(model.serverErrors.ImageVisibility) ||
        areServerErrors(model.serverErrors.StartDateVisibility) ||
        areServerErrors(model.serverErrors.StartDateLabel) ||
        areServerErrors(model.serverErrors.StartDateLabel2) ||
        areServerErrors(model.serverErrors.StartDateLabel3) ||
        areServerErrors(model.serverErrors.JobVisibility) ||
        areServerErrors(model.serverErrors.BankAccountNumberVisibility)
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

  public onDefinitionChange(model: ContractDefinition, prop?: string) {
    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  private _currentModel: ContractDefinition;
  private _currentModelModified = false;
  private _getForClientResult: ContractDefinitionForClient;

  public getForClient(model: ContractDefinition): ContractDefinitionForClient {
    if (!model) {
      return null;
    }

    if (this._currentModel !== model || this._currentModelModified) {
      this._currentModelModified = false;
      this._currentModel = model;

      // The mapping is trivial since the two data structures are identical
      this._getForClientResult = { ...model } as ContractDefinitionForClient;

      // In definitions for client, a null visibility becomes undefined
      for (const propName of this.allVisibilityProps()) {
        const value = this._getForClientResult[propName] as DefinitionVisibility;
        if (value === 'None') {
          delete this._getForClientResult[propName];
        }
      }
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

  public isVisible(visibility: DefinitionVisibility) {
    return visibility === 'Optional' || visibility === 'Required';
  }

  // Menu stuff

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_ContractDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_ContractDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }


  public onIconClick(model: ContractDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }
}
