import { Component, ViewChild, TemplateRef } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import {
  ChoicePropDescriptor, getChoices, collections, metadata, entityDescriptorImpl, isNumeric, PropDescriptor
} from '~/app/data/entities/base/metadata';
import {
  ReportDefinitionForSave, metadata_ReportDefinition, ReportDefinition, ReportMeasureDefinition,
  ReportColumnDefinition, ReportRowDefinition, ReportDimensionDefinition, ReportSelectDefinition, ReportParameterDefinition
} from '~/app/data/entities/report-definition';
import { ActivatedRoute } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FilterTools } from '~/app/data/filter-expression';
import { NgControl } from '@angular/forms';
import { highlightInvalid, validationErrors, areServerErrors } from '~/app/shared/form-group/form-group.component';

export interface FieldInfo {
  path: string;
  desc: PropDescriptor;
  level: number;
  isExpanded?: boolean;
  childrenLoaded?: boolean;
  parent?: FieldInfo;
  match?: boolean;
}

@Component({
  selector: 'b-report-definitions-details',
  templateUrl: './report-definitions-details.component.html',
  styles: []
})
export class ReportDefinitionsDetailsComponent extends DetailsBaseComponent {

  private _allFields: FieldInfo[];
  private _currentCollection: string;
  private _currentDefinitionId: string;
  private _isEdit = false;
  private reportDefinitionsApi = this.api.reportDefinitionsApi(this.notifyDestruct$); // for intellisense
  private _sections: { [key: string]: boolean } = {
    Data: true,
    Filter: false,
    Chart: false,
    Title: false,
    MainMenu: false
  };

  public expand = '';
  public search: string;

  // Collapse or expand the 2 panes on the left
  public collapseFields = false;
  public collapseDefinition = false;

  public itemToEditHasChanged: false;
  public itemToEditNature: 'dimension' | 'measure' | 'select' | 'parameter';
  public itemToEdit: ReportRowDefinition | ReportColumnDefinition |
    ReportMeasureDefinition | ReportSelectDefinition | ReportParameterDefinition;

  @ViewChild('configureModal', { static: true })
  configureModal: TemplateRef<any>;

  /*
    The model can change in one of two ways:
    1 - Critical change: That requires the report-results.component to perform a full refresh of the screen (e.g. change of Type)
    2 - Non-critical change: That does not require the report-results.component to perform a full refresh (e.g. change of Title)
    Every time a critical change happens, we make a fresh mapping of the model
    into the modelForClient, to trigger report-results.component to refresh
  */

  /**
   * This contains a fresh mapping of the model since the last time a critical change was made
   */
  public modelForClient: ReportDefinitionForClient;

  /**
   * The last model that was copied into immutable model
   */
  private _currentModel: ReportDefinition;

  /**
   * Set to true when the model changes in a way that requires refreshing the report-results.component screen
   */
  private _currentModelModified = false;

  create = () => {
    const result = new ReportDefinitionForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Title = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Title2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Title3 = this.initialText;
    }

    result.State = 'Draft';
    // result.Collection = 'MeasurementUnit';
    result.Type = 'Summary';
    result.Rows = [];
    result.Columns = [];
    result.Measures = [];
    result.Select = [];
    result.Parameters = [];
    result.ShowColumnsTotal = true;
    result.ShowRowsTotal = true;

    return result;
  }

  clone: (item: ReportDefinition) => ReportDefinition = (item: ReportDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as ReportDefinition;
      clone.Id = null;

      if (!!clone.Select) {
        clone.Select.forEach(e => {
          e.Id = null;
        });
      }
      if (!!clone.Parameters) {
        clone.Parameters.forEach(e => {
          e.Id = null;
        });
      }
      if (!!clone.Rows) {
        clone.Rows.forEach(e => {
          e.Id = null;
        });
      }
      if (!!clone.Columns) {
        clone.Columns.forEach(e => {
          e.Id = null;
        });
      }
      if (!!clone.Measures) {
        clone.Measures.forEach(e => {
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
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private route: ActivatedRoute, private modalService: NgbModal) {
    super();

    this.reportDefinitionsApi = this.api.reportDefinitionsApi(this.notifyDestruct$);
  }

  public decimalPlacesLookup(value: any): string {
    const descriptor = metadata_ReportDefinition(this.ws, this.translate, null).properties.E as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public get ws() {
    return this.workspace.current;
  }

  // public onActivate = (model: ReportDefinition): void => {
  //   if (!!model && !!model.Id) {
  //     this.reportDefinitionsApi.activate([model.Id], { returnEntities: true }).pipe(
  //       tap(res => addToWorkspace(res, this.workspace))
  //     ).subscribe({ error: this.details.handleActionError });
  //   }
  // }

  // public onDeactivate = (model: ReportDefinition): void => {
  //   if (!!model && !!model.Id) {
  //     this.reportDefinitionsApi.deactivate([model.Id], { returnEntities: true }).pipe(
  //       tap(res => addToWorkspace(res, this.workspace))
  //     ).subscribe({ error: this.details.handleActionError });
  //   }
  // }

  // public showActivate = (model: ReportDefinition) => !!model && !model.IsActive;
  // public showDeactivate = (model: ReportDefinition) => !!model && model.IsActive;

  // public canActivateDeactivateItem = (model: ReportDefinition) => this.ws.canDo('report-definitions', 'IsActive', model.Id);

  // public activateDeactivateTooltip = (model: ReportDefinition) => this.canActivateDeactivateItem(model) ? '' :
  //   this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get isNew(): boolean {
    return (this.isScreenMode && this.route.snapshot.paramMap.get('id') === 'new') || (this.isPopupMode && this.idString === 'new');
  }

  public isInactive: (model: ReportDefinition) => string = (model: ReportDefinition) => !!model && model.State === 'Archived' ?
    'Error_CannotModifyInactiveItemPleaseActivate' : null

  public onDefinitionChange(model: ReportDefinition, prop?: string) {

    if (!this.showDefinitionIdSelector(model)) {
      model.DefinitionId = null;
    }

    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  public onChartChange(chart: any, model: ReportDefinition) {
    if (!model.Chart && !!chart) {
      model.DefaultsToChart = true;
    }

    if (!!model.Chart && !chart) {
      model.DefaultsToChart = false;
    }

    model.Chart = chart;
    this.onDefinitionChange(model);
  }

  public onFilterChange(model: ReportDefinition) {
    // Here we synchronize the parameter list with the filter placeholders
    try {
      if (!model.Filter) {
        model.Parameters = [];
      } else {
        // (1) parse the filter to get the list of placeholder atoms
        const exp = FilterTools.parse(model.Filter);
        const placeholderAtoms = FilterTools.placeholderAtoms(exp);

        // (2) Use a tracker to accumulate all the placeholders in a case-insensitive fashion
        const phTracker: { [key: string]: string } = {};
        for (const atom of placeholderAtoms) {
          const key = atom.value.substr(1);
          const keyLower = key.toLowerCase();
          phTracker[keyLower] = key;
        }

        // (3) Remove parameters without a matching placeholder
        const parameters = model.Parameters.filter(p => !!phTracker[p.Key.toLowerCase()]);

        // (4) Create a tracker for existing parameters
        const paTracker: { [key: string]: ReportParameterDefinition } = {};
        parameters.forEach(pa => paTracker[pa.Key.toLowerCase()] = pa);

        // (5) Add new parameters for new placeholders
        const placeholders = Object.keys(phTracker).map(k => phTracker[k]);
        for (const placeholder of placeholders) {
          const placeholderLower = placeholder.toLowerCase();
          let parameterDef: ReportParameterDefinition = paTracker[placeholderLower];
          if (!parameterDef) {
            parameterDef = {
              Id: 0,
              Key: placeholder,
              IsRequired: false,
              ReportDefinitionId: model.Id,
            };

            paTracker[placeholderLower] = parameterDef;
            parameters.push(parameterDef);
          } else {
            parameterDef.Key = placeholder;
          }
        }

        model.Parameters = parameters;
      }
    } catch { }

    this.onDefinitionChange(model);
  }

  public getForClient(model: ReportDefinition): ReportDefinitionForClient {
    if (!model) {
      return null;
    }

    if (this._currentModel !== model || this._currentModelModified) {
      this._currentModelModified = false;
      this._currentModel = model;

      // The mapping is trivial since the two data structures are identical
      this.modelForClient = { ...model };
    }

    return this.modelForClient;
  }

  public get allCollections(): SelectorChoice[] {
    return collections(this.ws, this.translate);
  }

  public showDefinitionIdSelector(model: ReportDefinition): boolean {
    return !!model && !!model.Collection && !!metadata[model.Collection](this.ws, this.translate, null).definitionIds;
  }

  public allDefinitionIds(model: ReportDefinition): SelectorChoice[] {
    if (!!model && !!model.Collection) {
      const func = metadata[model.Collection];
      const desc = func(this.ws, this.translate, null);
      if (!!desc.definitionIds && !desc.definitionIdsArray) {
        desc.definitionIdsArray = desc.definitionIds
          .map(defId => ({ value: defId, name: func(this.ws, this.translate, defId).titlePlural }));
      }

      return desc.definitionIdsArray;
    } else {
      return null;
    }
  }

  public get allCharts(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.ws, this.translate, null).properties.Chart as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.ws, this.translate, null).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.ws, this.translate, null).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public getParameters(model: ReportDefinition): ReportParameterDefinition[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }
  public getColumns(model: ReportDefinition): ReportColumnDefinition[] {
    model.Columns = model.Columns || [];
    return model.Columns;
  }
  public getRows(model: ReportDefinition): ReportRowDefinition[] {
    model.Rows = model.Rows || [];
    return model.Rows;
  }
  public getMeasures(model: ReportDefinition): ReportMeasureDefinition[] {
    model.Measures = model.Measures || [];
    return model.Measures;
  }
  public getSelect(model: ReportDefinition): ReportSelectDefinition[] {
    model.Select = model.Select || [];
    return model.Select;
  }

  public drop(event: CdkDragDrop<any[]>, model: ReportDefinition) {

    // The four collections
    const allFields = this.allFields(model);
    const rows = model.Rows;
    const columns = model.Columns;
    const measures = model.Measures;
    const select = model.Select;

    // The source and destination collection
    const source = event.previousContainer.data;
    const sourceIndex = event.previousIndex;
    const destination = event.container.data;
    const destinationIndex = event.currentIndex;

    // Trigger a refresh if this is true at the end
    let modelHasChanged = false;

    if (source === allFields && destination === allFields) {
      // Do nothing
    } else if (source === destination && destination !== allFields) {
      // Reorder within array
      if (sourceIndex !== destinationIndex) {
        moveItemInArray(destination, sourceIndex, destinationIndex);
        modelHasChanged = true;
      }
    } else if (source === allFields && (destination === rows || destination === columns)) {
      // Create a new measure
      const fieldInfo = source[sourceIndex] as FieldInfo;
      const dimension: ReportColumnDefinition | ReportRowDefinition = {
        Id: 0,
        ReportDefinitionId: model.Id,
        Path: fieldInfo.path,
        AutoExpand: true
      };
      destination.splice(destinationIndex, 0, dimension);
      modelHasChanged = true;
    } else if (source === allFields && destination === measures) {
      // Create a new dimension
      const fieldInfo = source[sourceIndex] as FieldInfo;
      const dimension: ReportMeasureDefinition = {
        Id: 0,
        ReportDefinitionId: model.Id,
        Path: fieldInfo.path,
        Aggregation: isNumeric(fieldInfo.desc) && fieldInfo.path !== 'Id' ? 'sum' : 'count' // Default
      };
      destination.splice(destinationIndex, 0, dimension);
      modelHasChanged = true;
    } else if (source === allFields && destination === select) {
      // Create a new dimension
      const fieldInfo = source[sourceIndex] as FieldInfo;
      const column: ReportSelectDefinition = {
        Id: 0,
        ReportDefinitionId: model.Id,
        Path: fieldInfo.path,
      };
      destination.splice(destinationIndex, 0, column);
      modelHasChanged = true;

    } else if (source !== allFields && destination === allFields) {
      // Delete dimension/measure from source
      source.splice(sourceIndex, 1);
      modelHasChanged = true;
    } else if (source === measures && (destination === rows || destination === columns)) {
      // Add AutoExpand = true
      const measure = source.splice(sourceIndex, 1)[0] as ReportMeasureDefinition;
      const dimension: ReportDimensionDefinition = { ...measure, AutoExpand: true };
      destination.splice(destinationIndex, 0, dimension);
      modelHasChanged = true;
    } else if ((source === rows || source === columns) && destination === measures) {
      // add default Aggregation
      const dimension = source.splice(sourceIndex, 1)[0] as ReportDimensionDefinition;
      // tslint:disable-next-line:no-string-literal
      const measure: ReportMeasureDefinition = { ...dimension, Aggregation: dimension['Aggregation'] };
      if (!measure.Aggregation) {
        try {
          const steps = measure.Path.split('/');
          const prop = steps.pop();
          const desc = entityDescriptorImpl(steps, model.Collection, model.DefinitionId, this.ws, this.translate);
          const propDesc = desc.properties[prop];
          if (isNumeric(propDesc) && measure.Path !== 'Id') {
            measure.Aggregation = 'sum';
          } else {
            measure.Aggregation = 'count';
          }
        } catch {
          measure.Aggregation = 'sum';
        }
      }

      destination.splice(destinationIndex, 0, measure);
      modelHasChanged = true;
    } else if ((source === rows && destination === columns) || (source === columns && destination === rows)) {
      // Copy from rows to columns or vice a versa
      transferArrayItem(source, destination, sourceIndex, destinationIndex);
      modelHasChanged = true;
    } else {
      console.error('Unhandled case');
    }

    if (modelHasChanged) {
      this.onDefinitionChange(model);
    }
  }

  public onDeleteRow(index: number, model: ReportDefinition) {
    model.Rows.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onDeleteColumn(index: number, model: ReportDefinition) {
    model.Columns.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onDeleteMeasure(index: number, model: ReportDefinition) {
    model.Measures.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onDeleteSelect(index: number, model: ReportDefinition) {
    model.Select.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onConfigureRow(index: number, model: ReportDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.Rows[index] } as ReportRowDefinition;
    this.itemToEdit = itemToEdit;
    this.itemToEditNature = 'dimension';

    this.modalService.open(this.configureModal, { windowClass: 'b-dark-theme' }).result.then(() => {
      if (this.itemToEditHasChanged) {
        model.Rows[index] = itemToEdit;
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onConfigureColumn(index: number, model: ReportDefinition): void {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.Columns[index] } as ReportColumnDefinition;
    this.itemToEdit = itemToEdit;
    this.itemToEditNature = 'dimension';

    this.modalService.open(this.configureModal, { windowClass: 'b-dark-theme' }).result.then(
      () => {
        if (this.itemToEditHasChanged) {
          model.Columns[index] = itemToEdit;
          this.onDefinitionChange(model);
        }
      }, (_: any) => { }
    );
  }

  public onConfigureMeasure(index: number, model: ReportDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.Measures[index] } as ReportMeasureDefinition;
    this.itemToEdit = itemToEdit;
    this.itemToEditNature = 'measure';

    this.modalService.open(this.configureModal, { windowClass: 'b-dark-theme' }).result.then(() => {
      if (this.itemToEditHasChanged) {
        model.Measures[index] = itemToEdit;
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onConfigureSelect(index: number, model: ReportDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.Select[index] } as ReportSelectDefinition;
    this.itemToEdit = itemToEdit;
    this.itemToEditNature = 'select';

    this.modalService.open(this.configureModal, { windowClass: 'b-dark-theme' }).result.then(() => {
      if (this.itemToEditHasChanged) {
        model.Select[index] = itemToEdit;
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onConfigureParameter(index: number, model: ReportDefinition) {
    this.itemToEditHasChanged = false;
    const itemToEdit = { ...model.Parameters[index] } as ReportParameterDefinition;
    this.itemToEdit = itemToEdit;
    this.itemToEditNature = 'parameter';

    this.modalService.open(this.configureModal, { windowClass: 'b-dark-theme' }).result.then(() => {
      if (this.itemToEditHasChanged) {
        model.Parameters[index] = itemToEdit;
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public get canApply(): boolean {
    return this.itemToEditNature === 'parameter' || !!(this.itemToEdit as { Path: string }).Path;
  }

  // Tree of all fields
  public allFields(model: ReportDefinition): FieldInfo[] {
    if (!this._allFields || this._currentCollection !== model.Collection || this._currentDefinitionId !== model.DefinitionId) {
      this._currentCollection = model.Collection;
      this._currentDefinitionId = model.DefinitionId;
      this._allFields = this.getChildren(model.Collection, model.DefinitionId);
      this.onSearchChanged(model);
    }
    return this._allFields;
  }

  public hasChildren(info: FieldInfo): boolean {
    return info.desc.control === 'navigation';
  }

  public showTreeNode(node: FieldInfo): boolean {
    const parent = node.parent;
    return (!this.search || node.match) && (!parent || (parent.isExpanded && this.showTreeNode(parent)));
  }

  private getChildren(collection: string, definitionId: string, parent?: FieldInfo): FieldInfo[] {
    if (!collection) {
      return [];
    }

    const entityDesc = metadata[collection](this.ws, this.translate, definitionId);
    const level = !!parent ? parent.level + 1 : 0;
    const parentPath = !!parent ? `${parent.path}/` : '';
    return Object.keys(entityDesc.properties).map(prop => ({
      path: `${parentPath}${prop}`,
      desc: entityDesc.properties[prop],
      level,
      parent
    }));
  }

  public onExpand(node: FieldInfo, index: number, model: ReportDefinition): void {
    if (node.desc.control === 'navigation') {
      node.isExpanded = !node.isExpanded;
      if (node.isExpanded && !node.childrenLoaded) {
        const collection = node.desc.collection || node.desc.type;
        const definitionId = node.desc.definition;
        const children = this.getChildren(collection, definitionId, node);
        const allFields = this.allFields(model);
        allFields.splice(index + 1, 0, ...children);
        node.childrenLoaded = true;
        this.onSearchChanged(model);
      }
    }
  }

  public paddingLeft(node: FieldInfo): string {
    return this.workspace.ws.isRtl ? '0' : (node.level * 20) + 'px';
  }

  public paddingRight(node: FieldInfo): string {
    return this.workspace.ws.isRtl ? (node.level * 20) + 'px' : '0';
  }

  public flipNode(node: FieldInfo): string {
    // this is to flip the UI icons in RTL
    return this.flipIcon(node.isExpanded);
  }

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateNode(node: FieldInfo): number {
    return this.rotateIcon(node.isExpanded);
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  onSearchChanged(model: ReportDefinition) {
    const allFields = this.allFields(model);
    if (!!this.search) {
      const search = this.search.toLowerCase();
      // Clear all matches
      allFields.forEach(f => f.match = false);

      // If a field matches, flag it together with all its ancestors
      allFields.forEach(fieldInfo => {
        const label = fieldInfo.desc.label();
        if (!!label && label.toLowerCase().indexOf(search) >= 0) {
          let currentAncestor: FieldInfo = fieldInfo;
          while (!!currentAncestor && !currentAncestor.match) {
            currentAncestor.match = true;
            currentAncestor = currentAncestor.parent;
          }
        }
      });
    }
  }

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public onIconClick(model: ReportDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }

  public onToggleFields(): void {
    this.collapseFields = !this.collapseFields;
    window.dispatchEvent(new Event('resize')); // So the chart would resize
  }

  public onToggleDefinition(): void {
    this.collapseDefinition = !this.collapseDefinition;
    window.dispatchEvent(new Event('resize')); // So the chart would resize
  }

  public watchIsEdit(isEdit: boolean): boolean {
    // this is a hack to trigger window resize when isEdit changes
    if (this._isEdit !== isEdit) {
      this._isEdit = isEdit;
      window.dispatchEvent(new Event('resize')); // So the chart would resize
    }

    return true;
  }

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public titleSectionErrors(model: ReportDefinition) {
    if (!model.serverErrors) {
      return false;
    }

    return areServerErrors(model.serverErrors.Id) ||
      areServerErrors(model.serverErrors.Id) ||
      areServerErrors(model.serverErrors.Title) ||
      areServerErrors(model.serverErrors.Description) ||
      areServerErrors(model.serverErrors.Title2) ||
      areServerErrors(model.serverErrors.Description2) ||
      areServerErrors(model.serverErrors.Title3) ||
      areServerErrors(model.serverErrors.Description3);
  }
}
