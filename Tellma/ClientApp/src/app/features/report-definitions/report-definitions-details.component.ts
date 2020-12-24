// tslint:disable:member-ordering
import { Component, ViewChild, TemplateRef } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import {
  ChoicePropDescriptor, getChoices, collectionsWithEndpoint, metadata, entityDescriptorImpl,
  isNumeric, PropDescriptor, ParameterDescriptor, EntityDescriptor, hasControlOptions, Control, Collection
} from '~/app/data/entities/base/metadata';
import {
  ReportDefinitionForSave, metadata_ReportDefinition, ReportDefinition, ReportMeasureDefinition,
  ReportColumnDefinition, ReportRowDefinition, ReportDimensionDefinition, ReportSelectDefinition,
  ReportParameterDefinition
} from '~/app/data/entities/report-definition';
import { ActivatedRoute } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FilterTools, modifiers } from '~/app/data/filter-expression';
import { NgControl } from '@angular/forms';
import { highlightInvalid, validationErrors, areServerErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { computePropDesc } from '~/app/data/util';

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
  selector: 't-report-definitions-details',
  templateUrl: './report-definitions-details.component.html',
  styles: []
})
export class ReportDefinitionsDetailsComponent extends DetailsBaseComponent {

  private _allFields: FieldInfo[];
  private _currentCollection: string;
  private _currentDefinitionId: number;
  private _isEdit = false;
  private _sections: { [key: string]: boolean } = {
    Data: true,
    Filter: false,
    Chart: false,
    Title: false,
    MainMenu: false
  };

  public expand = 'Parameters,Select,Rows,Columns,Measures';
  public search: string;

  // Collapse or expand the 2 panes on the left
  public collapseFields = false;
  public collapseDefinition = false;

  public modelRef: ReportDefinition;
  public itemToEditHasChanged = false;
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
    const result: ReportDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Title = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Title2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Title3 = this.initialText;
    }

    result.ShowInMainMenu = false;
    // result.Collection = 'Unit';
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
    private workspace: WorkspaceService, private translate: TranslateService,
    private route: ActivatedRoute, private modalService: NgbModal) {
    super();
  }

  public decimalPlacesLookup(value: any): string {
    const descriptor = metadata_ReportDefinition(this.workspace, this.translate).properties.E as ChoicePropDescriptor;
    return descriptor.format(value);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public isInactive: (model: ReportDefinition) => string = (_: ReportDefinition) => null;

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

  private entityDescriptor(model: ReportDefinition): EntityDescriptor {
    return !!model.Collection ? metadata[model.Collection](this.workspace, this.translate, model.DefinitionId) : null;
  }

  private synchronizeFilter(model: ReportDefinition) {
    // Here we synchronize the parameter list with the filter placeholders and the built in parameter descriptors
    try {
      // (1) Get the parameters from the custom filter
      const placeholderAtoms = FilterTools.placeholderAtoms(FilterTools.parse(model.Filter));
      const customParamsKeys = placeholderAtoms.map(atom => atom.value.substr(1));

      // (2) Get the built-in parameter descriptors
      const desc = this.entityDescriptor(model);
      const builtInParamsDescriptors = !!desc ? desc.parameters || [] : [];

      if (customParamsKeys.length === 0 && builtInParamsDescriptors.length === 0) {
        // Optimization
        model.Parameters = [];
      } else {

        // (3) Use a tracker to accumulate all the keys in a case-insensitive fashion
        const paramTracker: { [key: string]: string } = {};
        for (const key of customParamsKeys) {
          const keyLower = key.toLowerCase();
          paramTracker[keyLower] = key;
        }
        for (const param of builtInParamsDescriptors) {
          const key = param.key;
          const keyLower = key.toLowerCase();
          paramTracker[keyLower] = key;
        }

        // (4) Remove parameters without a matching placeholder
        const parameters = model.Parameters.filter(p => !!paramTracker[p.Key.toLowerCase()]);

        // (5) Create a tracker for existing model parameters
        const modelTracker: { [key: string]: ReportParameterDefinition } = {};
        parameters.forEach(pa => modelTracker[pa.Key.toLowerCase()] = pa);

        // (5) Add new parameters for new placeholders
        const keys = Object.keys(paramTracker).map(k => paramTracker[k]);
        for (const key of keys) {
          const keyLower = key.toLowerCase();
          let parameterDef: ReportParameterDefinition = modelTracker[keyLower];
          const builtInMatch = builtInParamsDescriptors.find(e => e.key === key);
          if (!parameterDef) {
            parameterDef = {
              Id: 0,
              Key: key,
              Visibility: !!builtInMatch && builtInMatch.isRequired ? 'Required' :
                !!builtInMatch && !builtInMatch.isRequired ? 'None' : 'Optional',
              Control: this.getParamPropDescriptor(model, key).control
            };

            modelTracker[keyLower] = parameterDef;
            parameters.push(parameterDef);
          } else {
            parameterDef.Key = key;
            parameterDef.Control = this.getParamPropDescriptor(model, key).control;
          }
        }

        model.Parameters = parameters;
      }
    } catch { } // Errors will be reported by the report preview
  }

  public onCollectionChange(model: ReportDefinition) {

    this.synchronizeFilter(model);
    this.onDefinitionChange(model);
  }

  public onFilterChange(model: ReportDefinition) {

    this.synchronizeFilter(model);
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
      this.modelForClient = { ...model } as ReportDefinitionForClient;
    }

    return this.modelForClient;
  }

  public get allCollections(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate);
  }

  public showDefinitionIdSelector(model: ReportDefinition): boolean {
    return !!model && !!model.Collection && !!metadata[model.Collection](this.workspace, this.translate, null).definitionIds;
  }

  public allDefinitionIds(model: ReportDefinition): SelectorChoice[] {
    if (!!model && !!model.Collection) {
      const func = metadata[model.Collection];
      const desc = func(this.workspace, this.translate, null);
      if (!!desc.definitionIds && !desc.definitionIdsArray) {
        desc.definitionIdsArray = desc.definitionIds
          .map(defId => ({ value: defId, name: func(this.workspace, this.translate, defId).titlePlural }));
      }

      return desc.definitionIdsArray;
    } else {
      return null;
    }
  }

  public get allCharts(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.workspace, this.translate).properties.Chart as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public getParameters(model: ReportDefinition): ReportParameterDefinition[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }

  public getParameterDescriptor(key: string, model?: ReportDefinition): ParameterDescriptor {
    model = model || this.modelRef;
    const entityDesc = metadata[model.Collection](this.workspace, this.translate, model.DefinitionId);
    const result = !!entityDesc.parameters ? entityDesc.parameters.find(e => e.key.toLowerCase() === key.toLowerCase()) : null;
    return result;
  }

  public showNone(key: string): boolean {
    // Visibility option 'None' is only available when the parameter is built-in
    const desc = this.getParameterDescriptor(key);
    return !!desc;
  }

  public showOptional(key: string): boolean {
    // Visibility option 'Optional' is available when the parameter is either regular OR built-in but not required
    const desc = this.getParameterDescriptor(key);
    return !desc || !desc.isRequired;
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
          const desc = entityDescriptorImpl(steps, model.Collection, model.DefinitionId, this.workspace, this.translate);
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
    this.modelRef = model;

    this.modalService.open(this.configureModal, { windowClass: 't-dark-theme' }).result.then(() => {
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
    this.modelRef = model;

    this.modalService.open(this.configureModal, { windowClass: 't-dark-theme' }).result.then(
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
    this.modelRef = model;

    this.modalService.open(this.configureModal, { windowClass: 't-dark-theme' }).result.then(() => {
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
    this.modelRef = model;

    this.modalService.open(this.configureModal, { windowClass: 't-dark-theme' }).result.then(() => {
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
    this.modelRef = model;

    this.modalService.open(this.configureModal, { windowClass: 't-dark-theme' }).result.then(() => {
      if (this.itemToEditHasChanged) {
        model.Parameters[index] = itemToEdit;
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  private _getParamPropDescriptorCollection: Collection;
  private _getParamPropDescriptorDefinitionId: number;
  private _getParamPropDescriptorFilter: string;
  private _getParamPropDescriptorResults: { [key: string]: PropDescriptor } = {};

  public getParamPropDescriptor(model: ReportDefinition, key: string): PropDescriptor {
    if (this._getParamPropDescriptorCollection !== model.Collection ||
      this._getParamPropDescriptorDefinitionId !== model.DefinitionId ||
      this._getParamPropDescriptorFilter !== model.Filter) {
      this._getParamPropDescriptorCollection = model.Collection;
      this._getParamPropDescriptorDefinitionId = model.DefinitionId;
      this._getParamPropDescriptorFilter = model.Filter;

      this._getParamPropDescriptorResults = {};
      if (!!model.Collection) {

        // Custom params
        const placeholderAtoms = FilterTools.placeholderAtoms(FilterTools.parse(model.Filter));
        for (const atom of placeholderAtoms) {
          const keyLower = atom.value.substr(1).toLowerCase();
          this._getParamPropDescriptorResults[keyLower] = computePropDesc(
            this.workspace, this.translate, model.Collection, model.DefinitionId, atom.path, atom.property, atom.modifier);
        }

        // Built-in params
        const desc = this.entityDescriptor(model);
        const builtInParamsDescriptors = !!desc ? desc.parameters || [] : [];
        for (const param of builtInParamsDescriptors) {
          this._getParamPropDescriptorResults[param.key.toLowerCase()] = param.desc;
        }
      }
    }

    return this._getParamPropDescriptorResults[key.toLowerCase()];
  }

  public showOptions(p: ReportParameterDefinition, model: ReportDefinition, key: string) {
    let control = p.Control; // This overrides the default
    if (!control) {
      const desc = this.getParamPropDescriptor(model, key);
      control = !!desc ? desc.control : null;
    }

    return hasControlOptions(control);
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
    return info.desc.datatype === 'entity';
  }

  public showTreeNode(node: FieldInfo): boolean {
    const parent = node.parent;
    return (!this.search || node.match) && (!parent || (parent.isExpanded && this.showTreeNode(parent)));
  }

  private getChildren(collection: string, definitionId: number, parent?: FieldInfo): FieldInfo[] {
    if (!collection) {
      return [];
    }

    const entityDesc = metadata[collection](this.workspace, this.translate, definitionId);
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
    if (node.desc.datatype === 'entity') {
      node.isExpanded = !node.isExpanded;
      if (node.isExpanded && !node.childrenLoaded) {
        const collection = node.desc.control;
        const definitionId = node.desc.definitionId;
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

  public weakEntityErrors(model: ReportRowDefinition | ReportColumnDefinition |
    ReportMeasureDefinition | ReportSelectDefinition | ReportParameterDefinition) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public dataSectionErrors(model: ReportDefinition) {
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.Type) ||
      areServerErrors(model.serverErrors.ShowColumnsTotal) ||
      areServerErrors(model.serverErrors.ShowRowsTotal) ||
      areServerErrors(model.serverErrors.OrderBy) ||
      areServerErrors(model.serverErrors.Top)
    )) ||
      (!!model.Rows && model.Rows.some(e => this.weakEntityErrors(e))) ||
      (!!model.Columns && model.Columns.some(e => this.weakEntityErrors(e))) ||
      (!!model.Measures && model.Measures.some(e => this.weakEntityErrors(e))) ||
      (!!model.Select && model.Select.some(e => this.weakEntityErrors(e)));
  }

  public filterSectionErrors(model: ReportDefinition) {
    return (!!model.serverErrors && areServerErrors(model.serverErrors.Filter)) ||
      (!!model.Parameters && model.Parameters.some(e => this.weakEntityErrors(e)));
  }

  public chartSectionErrors(model: ReportDefinition) {
    return !!model.serverErrors && (areServerErrors(model.serverErrors.Chart) ||
      areServerErrors(model.serverErrors.DefaultsToChart));
  }

  public titleSectionErrors(model: ReportDefinition) {
    return !!model.serverErrors && (areServerErrors(model.serverErrors.Id) ||
      areServerErrors(model.serverErrors.Id) ||
      areServerErrors(model.serverErrors.Title) ||
      areServerErrors(model.serverErrors.Description) ||
      areServerErrors(model.serverErrors.Title2) ||
      areServerErrors(model.serverErrors.Description2) ||
      areServerErrors(model.serverErrors.Title3) ||
      areServerErrors(model.serverErrors.Description3));
  }

  public mainMenuSectionErrors(model: ReportDefinition) {
    return !!model.serverErrors && (areServerErrors(model.serverErrors.ShowInMainMenu) ||
      areServerErrors(model.serverErrors.MainMenuSection) ||
      areServerErrors(model.serverErrors.MainMenuIcon) ||
      areServerErrors(model.serverErrors.MainMenuSortKey));
  }

  public get modifiers(): string[] {
    return modifiers;
  }

  public isDate(path: string, model: ReportDefinitionForSave): boolean {
    // when this function returns true, the field for date functions becomes visible
    if (!path || !path.trim()) {
      return false;
    }

    try {
      const steps = path.split('/');
      const prop = steps.pop();
      const desc = entityDescriptorImpl(steps, model.Collection, model.DefinitionId, this.workspace, this.translate);
      const propDesc = desc.properties[prop];
      return propDesc.control === 'date' || propDesc.control === 'datetime';
    } catch {
      return false;
    }
  }

  public onPathChanged(itemToEdit: ReportRowDefinition | ReportColumnDefinition, model: ReportDefinitionForSave) {
    // This removes the modifier if the field isn't of type date
    if (!this.isDate(itemToEdit.Path, model)) {
      delete itemToEdit.Modifier;
    }
  }

  public savePreprocessing(entity: ReportDefinition) {
    // Server validation on hidden collections will be confusing to the user
    if (entity.Type === 'Details') {
      entity.Rows = [];
      entity.Columns = [];
      entity.Measures = [];
    }

    if (entity.Type === 'Summary') {
      entity.Select = [];
    }
  }
}
