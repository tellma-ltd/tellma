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
import { LookupDefinitionForSave, metadata_LookupDefinition, LookupDefinition } from '~/app/data/entities/lookup-definition';
import { DefinitionVisibility } from '~/app/data/entities/base/definition-common';
import { LookupDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { NgControl } from '@angular/forms';

@Component({
  selector: 't-lookup-definitions-details',
  templateUrl: './lookup-definitions-details.component.html',
  styles: []
})
export class LookupDefinitionsDetailsComponent extends DetailsBaseComponent {

  // private lookupDefinitionsApi = this.api.lookupDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  create = () => {
    const result: LookupDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.TitleSingular = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.TitleSingular2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.TitleSingular3 = this.initialText;
    }

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

    // this.lookupDefinitionsApi = this.api.lookupDefinitionsApi(this.notifyDestruct$);
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
}
