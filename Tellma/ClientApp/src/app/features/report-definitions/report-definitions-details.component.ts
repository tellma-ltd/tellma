// tslint:disable:member-ordering
import { Component, ViewChild, TemplateRef } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import {
  ChoicePropDescriptor, getChoices, collectionsWithEndpoint, metadata,
  isNumeric, PropDescriptor, hasControlOptions,
  NavigationPropDescriptor, Control, simpleControls
} from '~/app/data/entities/base/metadata';
import {
  ReportDefinitionForSave, metadata_ReportDefinition, ReportDefinition, ReportDefinitionMeasure,
  ReportDefinitionColumn, ReportDefinitionRow, ReportDefinitionSelect, ReportDefinitionParameter,
  ReportDefinitionDimensionAttribute
} from '~/app/data/entities/report-definition';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { DefinitionsForClient, ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { NgControl } from '@angular/forms';
import { highlightInvalid, validationErrors, areServerErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { DeBracket, Queryex, QueryexBase, QueryexColumnAccess, QueryexFunction } from '~/app/data/queryex';
import { QueryexUtil } from '~/app/data/queryex-util';

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
    Title: false,
    Data: true,
    Filter: false,
    Drilldown: false,
    Chart: false,
    MainMenu: false
  };

  public expand = 'Parameters,Select,Rows.Attributes,Columns.Attributes,Measures';
  public search: string;

  // Collapse or expand the 2 panes on the left
  public collapseFields = false;
  public collapseDefinition = false;

  public modelRef: ReportDefinition;

  @ViewChild('dimensionConfigModal', { static: true })
  dimensionConfigModal: TemplateRef<any>;

  @ViewChild('measureConfigModal', { static: true })
  measureConfigModal: TemplateRef<any>;

  @ViewChild('selectConfigModal', { static: true })
  selectConfigModal: TemplateRef<any>;

  @ViewChild('paramConfigModal', { static: true })
  paramConfigModal: TemplateRef<any>;

  @ViewChild('totalLabelsModal', { static: true })
  totalLabelsModal: TemplateRef<any>;

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
    result.Collection = 'DetailsEntry';
    result.Type = 'Summary';
    result.Rows = [];
    result.Columns = [];
    result.Measures = [];
    result.Select = [];
    result.Parameters = [];
    result.ShowColumnsTotal = true;
    result.ShowRowsTotal = true;
    result.IsCustomDrilldown = false;

    return result;
  }

  clone: (item: ReportDefinition) => ReportDefinition = (item: ReportDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as ReportDefinition;
      clone.Id = null;
      if (!!clone.Rows) {
        clone.Rows.forEach(e => {
          e.Id = null;
          if (!!e.Attributes) {
            e.Attributes.forEach(a => a.Id = null);
          }
        });
      }
      if (!!clone.Columns) {
        clone.Columns.forEach(e => {
          e.Id = null;
          if (!!e.Attributes) {
            e.Attributes.forEach(a => a.Id = null);
          }
        });
      }

      if (!!clone.Measures) {
        clone.Measures.forEach(e => e.Id = null);
      }

      if (!!clone.Select) {
        clone.Select.forEach(e => e.Id = null);
      }

      if (!!clone.Parameters) {
        clone.Parameters.forEach(e => e.Id = null);
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
    private modalService: NgbModal) {
    super();
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

  public watchIsEdit(isEdit: boolean): boolean {
    // this is a hack to trigger window resize when isEdit changes
    if (this._isEdit !== isEdit) {
      this._isEdit = isEdit;
      window.dispatchEvent(new Event('resize')); // So the chart would resize
    }

    return true;
  }

  /////////////////// Collection & DefinitionId v2.0

  public onCollectionChange(model: ReportDefinition) {

    this.validateModel(model);
    this.synchronizeParameters(model);
    this.onDefinitionChange(model);
  }

  public onDefinitionIdChange(model: ReportDefinition) {
    this.validateModel(model);
    this.synchronizeParameters(model);
    this.onDefinitionChange(model);
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

  ///////////////////// Charts v2.0

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

  ///////////////////// Fields, Drag n Drop v2.0

  public drop(event: CdkDragDrop<any[]>, model: ReportDefinition) {

    // The four collections
    const allFields = this.allFields(model);
    const rows = model.Rows;
    const columns = model.Columns;
    const measures = model.Measures;
    const selects = model.Select;

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
      const dimension: ReportDefinitionColumn | ReportDefinitionRow = {
        Id: 0,
        KeyExpression: fieldInfo.path,
        Localize: true,
        AutoExpandLevel: 1,
        ShowAsTree: true,
        ShowEmptyMembers: false,
        Attributes: [],
      };
      destination.splice(destinationIndex, 0, dimension);
      modelHasChanged = true;
    } else if (source === allFields && destination === measures) {
      // Create a new dimension
      const fieldInfo = source[sourceIndex] as FieldInfo;
      const aggregation = isNumeric(fieldInfo.desc) && fieldInfo.path !== 'Id' ? 'Sum' : 'Count';
      const measure: ReportDefinitionMeasure = {
        Id: 0,
        Expression: `${aggregation}(${fieldInfo.path})`,
      };
      destination.splice(destinationIndex, 0, measure);
      modelHasChanged = true;
    } else if (source === allFields && destination === selects) {
      // Create a new dimension
      const fieldInfo = source[sourceIndex] as FieldInfo;
      const select: ReportDefinitionSelect = {
        Id: 0,
        Expression: fieldInfo.path,
        Localize: true
      };
      destination.splice(destinationIndex, 0, select);
      modelHasChanged = true;
    } else if (source !== allFields && destination === allFields) {
      // Delete dimension/measure from source
      source.splice(sourceIndex, 1);
      modelHasChanged = true;
    } else if (source === measures && (destination === rows || destination === columns)) {
      // Get the measure from source
      const measure = source.splice(sourceIndex, 1)[0] as ReportDefinitionMeasure;

      // If there is a root aggregation function we try to peel it off and get the operand
      let measureExpression: string;
      try {
        const exp = Queryex.parseSingle(measure.Expression);
        if (!!exp && exp instanceof QueryexFunction && exp.isAggregation && exp.arguments.length === 1) {
          measureExpression = DeBracket(exp.arguments[0].toString());
        }
      } catch { }

      const dimension: ReportDefinitionRow | ReportDefinitionColumn = { ...measure };

      dimension.Id = 0;
      dimension.KeyExpression = measureExpression || measure.Expression;
      if (dimension.Localize === undefined) {
        dimension.Localize = true;
      }
      if (dimension.AutoExpandLevel === undefined) {
        dimension.AutoExpandLevel = 1;
      }
      if (dimension.ShowAsTree === undefined) {
        dimension.ShowAsTree = true;
      }
      if (dimension.Attributes === undefined) {
        dimension.Attributes = [];
      }

      destination.splice(destinationIndex, 0, dimension);
      modelHasChanged = true;
    } else if ((source === rows || source === columns) && destination === measures) {
      // add default Aggregation
      const dimension = source.splice(sourceIndex, 1)[0] as ReportDefinitionRow | ReportDefinitionColumn;

      // Here we attempt to guess a suitable aggregation function
      let aggregation: string;
      try {
        const exp = Queryex.parseSingle(dimension.KeyExpression);
        if (exp.aggregations().length > 0) {
          aggregation = null;
        } else if (exp instanceof QueryexColumnAccess && exp.path.length === 0 && exp.property === 'Id') {
          aggregation = 'Count';
        } else {
          const userOverrides = {};
          const autoOverrides = {}; // Rows and Columns should not contain prameters anyways
          const desc = QueryexUtil.nativeDesc(exp, userOverrides, autoOverrides,
            model.Collection, model.DefinitionId, this.workspace, this.translate);
          if (isNumeric(desc)) {
            aggregation = 'Sum';
          }
        }
      } catch { }

      // Create the measure
      const measure: ReportDefinitionMeasure = { ...dimension };
      if (!!aggregation) {
        measure.Expression = `${aggregation}(${dimension.KeyExpression})`;
      } else {
        measure.Expression = dimension.KeyExpression;
      }

      destination.splice(destinationIndex, 0, measure);
      modelHasChanged = true;
    } else if ((source === rows && destination === columns) || (source === columns && destination === rows)) {
      // Copy from rows to columns or vice a versa
      transferArrayItem(source, destination, sourceIndex, destinationIndex);
      modelHasChanged = true;
    } else {
      console.error('Unhandled drop case.');
    }

    if (modelHasChanged) {
      this.validateModel(model);
      this.synchronizeParameters(model);
      this.onDefinitionChange(model);
    }
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
    const parentPath = !!parent ? `${parent.path}.` : '';
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

  ////////////////////// Errors v2.0

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public weakEntityErrors(model: ReportDefinitionMeasure | ReportDefinitionSelect | ReportDefinitionParameter) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public dimensionErrors(model: ReportDefinitionRow | ReportDefinitionColumn) {
    return this.weakEntityErrors(model) ||
      (!!model.Attributes && model.Attributes.some(e => this.weakEntityErrors(e)));
  }

  public customizeLabelsErrors(model: ReportDefinition) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.ColumnsTotalLabel) ||
      areServerErrors(model.serverErrors.ColumnsTotalLabel2) ||
      areServerErrors(model.serverErrors.ColumnsTotalLabel3) ||
      areServerErrors(model.serverErrors.RowsTotalLabel) ||
      areServerErrors(model.serverErrors.RowsTotalLabel2) ||
      areServerErrors(model.serverErrors.RowsTotalLabel3)
    );
  }

  public dataSectionErrors(model: ReportDefinition) {
    const isDetails = model.Type === 'Details';
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.Type) ||
      areServerErrors(model.serverErrors.ShowColumnsTotal) ||
      areServerErrors(model.serverErrors.ShowRowsTotal) ||
      this.customizeLabelsErrors(model) ||
      (isDetails && (
        areServerErrors(model.serverErrors.OrderBy)) ||
        areServerErrors(model.serverErrors.Top))
    )) ||
      (!!model.Rows && model.Rows.some(e => this.dimensionErrors(e))) ||
      (!!model.Columns && model.Columns.some(e => this.dimensionErrors(e))) ||
      (!!model.Measures && model.Measures.some(e => this.weakEntityErrors(e))) ||
      (isDetails && !!model.Select && model.Select.some(e => this.weakEntityErrors(e)));
  }

  public filterSectionErrors(model: ReportDefinition) {
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.Filter) ||
      areServerErrors(model.serverErrors.Having))) ||
      (!!model.Parameters && model.Parameters.some(e => this.weakEntityErrors(e)));
  }

  public drilldownSectionErrors(model: ReportDefinition) {
    // This only appears in Summary
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.OrderBy) ||
      areServerErrors(model.serverErrors.IsCustomDrilldown)
    )) ||
      (!!model.Select && model.Select.some(e => this.weakEntityErrors(e)));
  }

  public chartSectionErrors(model: ReportDefinition) {
    return !!model.serverErrors && (
      areServerErrors(model.serverErrors.Chart) ||
      areServerErrors(model.serverErrors.ChartOptions) ||
      areServerErrors(model.serverErrors.DefaultsToChart));
  }

  public titleSectionErrors(model: ReportDefinition) {
    return !!model.serverErrors && (areServerErrors(model.serverErrors.Id) ||
      areServerErrors(model.serverErrors.Code) ||
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

  public savePreprocessing(model: ReportDefinition) {
    // Server validation on hidden collections will be confusing to the user
    if (model.Type === 'Details') {
      model.Rows = [];
      model.Columns = [];
      model.Measures = [];
      delete model.Having;
    }

    if (model.Type === 'Summary') {
      delete model.Top;
      if (!model.IsCustomDrilldown) {
        model.Select = [];
        delete model.OrderBy;
      }
    }

    if (model.Columns.length === 0) {
      model.ShowColumnsTotal = true;
    }

    if (model.Columns.length === 0 || !model.ShowColumnsTotal) {
      delete model.ColumnsTotalLabel;
      delete model.ColumnsTotalLabel2;
      delete model.ColumnsTotalLabel3;
    }

    if (model.Rows.length === 0) {
      model.ShowRowsTotal = true;
    }

    if (model.Rows.length === 0 || !model.ShowRowsTotal) {
      delete model.RowsTotalLabel;
      delete model.RowsTotalLabel2;
      delete model.RowsTotalLabel3;
    }
  }

  ///////////////////// Dimensions v2.0

  dimToEdit: ReportDefinitionRow | ReportDefinitionColumn;
  dimToEditHasChanged = false;

  // Caching
  _dimKeyExpression: string;
  _dimKeyExpressionDesc: PropDescriptor;

  private dimKeyExpressionDesc(keyExpression: string, model: ReportDefinitionForSave) {
    if (this._dimKeyExpression !== keyExpression) {
      this._dimKeyExpression = keyExpression;
      this._dimKeyExpressionDesc = null;

      const userOverrides = {};
      const autoOverrides = {}; // Should be fine, dimensions are not allowed to have parameters

      try {
        // Prepare the expression
        let exp: QueryexBase;
        const expTemp = Queryex.parseSingle(keyExpression);
        if (!!expTemp) {
          const aggregations = expTemp.aggregations();
          if (aggregations.length === 0) {
            exp = expTemp;
          }
        }

        // Prepare the descriptor
        if (!!exp) {
          const wss = this.workspace;
          const trx = this.translate;
          const keyDesc = QueryexUtil.nativeDesc(exp, userOverrides, autoOverrides, model.Collection, model.DefinitionId, wss, trx);
          switch (keyDesc.datatype) {
            case 'boolean':
            case 'hierarchyid':
            case 'geography':
              return;
          }

          this._dimKeyExpressionDesc = keyDesc;
        }

      } catch (e) { }
    }

    return this._dimKeyExpressionDesc;
  }

  private isDisplayDimensionDesc(desc: PropDescriptor): desc is NavigationPropDescriptor {

    return !!desc && desc.datatype === 'entity';
  }

  private isDisplayDimension(keyExpression: string, model: ReportDefinitionForSave): boolean {
    const desc = this.dimKeyExpressionDesc(keyExpression, model);
    return this.isDisplayDimensionDesc(desc);
  }

  public showDisplayExpression(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn, model: ReportDefinitionForSave): boolean {
    return this.isDisplayDimension(dimToEdit.KeyExpression, model);
  }

  public showDimensionAttributes(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn, model: ReportDefinitionForSave): boolean {
    return this.isDisplayDimension(dimToEdit.KeyExpression, model);
  }

  public showShowAsTree(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn, model: ReportDefinitionForSave): boolean {
    const desc = this.dimKeyExpressionDesc(dimToEdit.KeyExpression, model);
    return !!desc && desc.datatype === 'entity' &&
      !!metadata[desc.control](this.workspace, this.translate, desc.definitionId).properties.Parent;
  }

  public showShowEmptyMembers(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn, model: ReportDefinitionForSave): boolean {
    const desc = this.dimKeyExpressionDesc(dimToEdit.KeyExpression, model);
    return !!desc && (desc.control === 'choice' || desc.datatype === 'bit');
  }

  public validateDimension(dimension: ReportDefinitionRow | ReportDefinitionColumn, model: ReportDefinitionForSave): void {
    if (!dimension) {
      return;
    }

    dimension.serverErrors = {};
    for (const attribute of dimension.Attributes) {
      attribute.serverErrors = {};
    }

    //////////////// Key Expression Validation

    function addKeyExpressionError(err: string) {
      dimension.serverErrors.KeyExpression = dimension.serverErrors.KeyExpression || [];
      dimension.serverErrors.KeyExpression.push(err);
    }

    try {
      // Prepare the expression
      const exp = Queryex.parseSingle(dimension.KeyExpression);
      if (!!exp) {
        const aggregations = exp.aggregations();
        const parameters = exp.parameters();
        if (aggregations.length > 0) {
          addKeyExpressionError(`Expression cannot contain aggregation functions like '${aggregations[0].name}'.`);
        } else if (parameters.length > 0) {
          addKeyExpressionError(`Expression cannot contain parameters like '${parameters[0]}'.`);
        }
      } else {
        addKeyExpressionError(`Expression cannot be empty.`);
      }
    } catch (e) {
      addKeyExpressionError(e.message);
    }

    //////////////// Display Expression Validation

    function addDisplayExpressionError(err: string) {
      dimension.serverErrors.DisplayExpression = dimension.serverErrors.DisplayExpression || [];
      dimension.serverErrors.DisplayExpression.push(err);
    }

    if (this.showDisplayExpression(dimension, model)) {
      try {
        // Prepare the expression
        const exp = Queryex.parseSingle(dimension.DisplayExpression);
        if (!!exp) {
          const aggregations = exp.aggregations();
          const parameters = exp.parameters();
          if (aggregations.length > 0) {
            addDisplayExpressionError(`Expression cannot contain aggregation functions like '${aggregations[0].name}'.`);
          } else if (parameters.length > 0) {
            addDisplayExpressionError(`Expression cannot contain parameters like '${parameters[0]}'.`);
          }
        }
      } catch (e) {
        addDisplayExpressionError(e.message);
      }

      //////////////// Attributes' Expression Validation
      if (this.showDimensionAttributes(dimension, model)) {
        for (const attribute of dimension.Attributes) {

          function addAttributeExpressionError(err: string) {
            attribute.serverErrors.Expression = attribute.serverErrors.Expression || [];
            attribute.serverErrors.Expression.push(err);
          }

          try {
            // Prepare the expression
            const exp = Queryex.parseSingle(attribute.Expression);
            if (!!exp) {
              const aggregations = exp.aggregations();
              const parameters = exp.parameters();
              if (aggregations.length > 0) {
                addAttributeExpressionError(`Expression cannot contain aggregation functions like '${aggregations[0].name}'.`);
              } else if (parameters.length > 0) {
                addAttributeExpressionError(`Expression cannot contain parameters like '${parameters[0]}'.`);
              }
            } else {
              addAttributeExpressionError(`Expression cannot be empty.`);
            }
          } catch (e) {
            addAttributeExpressionError(e.message);
          }
        }
      }
    }
  }

  public onSetDimOrderDirection(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn, setIndex: number) {
    // This makes sure there is only one order direction per dimension
    if (setIndex >= 0) {
      dimToEdit.OrderDirection = null;
    }

    for (let i = 0; i < dimToEdit.Attributes.length; i++) {
      if (i !== setIndex) {
        dimToEdit.Attributes[i].OrderDirection = null;
      }
    }

    this.dimToEditHasChanged = true;
  }

  public getColumns(model: ReportDefinition): ReportDefinitionColumn[] {
    model.Columns = model.Columns || [];
    return model.Columns;
  }

  public getRows(model: ReportDefinition): ReportDefinitionRow[] {
    model.Rows = model.Rows || [];
    return model.Rows;
  }

  private onConfigureDimension(index: number, coll: (ReportDefinitionRow | ReportDefinitionColumn)[], model: ReportDefinition) {
    this.dimToEditHasChanged = false;
    const dimToEdit = JSON.parse(JSON.stringify(coll[index])) as ReportDefinitionRow | ReportDefinitionColumn;
    this.dimToEdit = dimToEdit;
    this.modelRef = model;

    this.modalService.open(this.dimensionConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.dimToEditHasChanged) {
        coll[index] = dimToEdit;

        if (!this.showDisplayExpression(dimToEdit, model)) {
          delete dimToEdit.DisplayExpression;
        }

        if (!this.showDimensionAttributes(dimToEdit, model)) {
          dimToEdit.Attributes = [];
        }

        // Dimensions have no parameters
        this.validateModel(model);
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onConfigureRow(index: number, model: ReportDefinition) {
    this.onConfigureDimension(index, model.Rows, model);
  }

  public onDeleteRow(index: number, model: ReportDefinition) {
    model.Rows.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onConfigureColumn(index: number, model: ReportDefinition): void {
    this.onConfigureDimension(index, model.Columns, model);
  }

  public onDeleteColumn(index: number, model: ReportDefinition) {
    model.Columns.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onDeleteAttribute(index: number, dimToEdit: ReportDefinitionRow | ReportDefinitionColumn) {
    dimToEdit.Attributes.splice(index, 1);
    this.dimToEditHasChanged = true;
  }

  public onInsertAttribute(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn) {
    const att: ReportDefinitionDimensionAttribute = { Localize: true };
    dimToEdit.Attributes.push(att);
    this.dimToEditHasChanged = true;
  }

  public rowDrop(event: CdkDragDrop<any[]>, collection: any[]) {
    moveItemInArray(collection, event.previousIndex, event.currentIndex);
  }

  public canApplyDimension(dimToEdit: ReportDefinitionRow | ReportDefinitionColumn): boolean {
    return !!dimToEdit.KeyExpression;
  }

  ///////////////////// Measures v2.0

  measureToEdit: ReportDefinitionMeasure;
  measureToEditHasChanged = false;
  measureShowAdvancedOptions = false;

  public getMeasures(model: ReportDefinition): ReportDefinitionMeasure[] {
    model.Measures = model.Measures || [];
    return model.Measures;
  }

  public onConfigureMeasure(index: number, model: ReportDefinition) {
    this.measureToEditHasChanged = false;
    const measureToEdit = { ...model.Measures[index] } as ReportDefinitionMeasure;
    this.measureToEdit = measureToEdit;
    this.modelRef = model;

    this.measureShowAdvancedOptions = !!measureToEdit.Control ||
      !!measureToEdit.DangerWhen || !!measureToEdit.WarningWhen || !!measureToEdit.SuccessWhen;

    this.modalService.open(this.measureConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.measureToEditHasChanged) {
        model.Measures[index] = measureToEdit;

        this.validateModel(model);
        this.synchronizeParameters(model);
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public validateMeasure(measure: ReportDefinitionMeasure, _: ReportDefinitionForSave): void {

    if (!measure) {
      return;
    }

    measure.serverErrors = {};
    function addExpressionError(err: string) {
      measure.serverErrors.Expression = measure.serverErrors.Expression || [];
      measure.serverErrors.Expression.push(err);
    }

    let mainExp: QueryexBase;
    try {
      ////////////////// Expression Validation

      mainExp = Queryex.parseSingle(measure.Expression);
      if (!!mainExp) {
        const unaggregated = mainExp.unaggregatedColumnAccesses();
        if (unaggregated.length > 0) {
          addExpressionError(`Expression cannot contain unaggregated column accesses like '${unaggregated[0]}'.`);
        }
      } else {
        addExpressionError(`Expression cannot be empty.`);
      }
    } catch (e) {
      addExpressionError(e.message);
    }

    ////////////////// Highlight Expression Validation (Success, Warning and Danger)
    if (!!mainExp) {
      function validateHighlightExpression(expString: string, prop: string): void {

        function addHighlightExpressionError(err: string) {
          measure.serverErrors[prop] = measure.serverErrors[prop] || [];
          measure.serverErrors[prop].push(err);
        }

        try {
          const exp = Queryex.parseSingle(expString, { placeholderReplacement: mainExp });
          if (!!exp) {
            const unaggregated = exp.unaggregatedColumnAccesses();
            if (unaggregated.length > 0) {
              addHighlightExpressionError(`Expression cannot contain unaggregated column accesses like '${unaggregated[0]}'.`);
            }
          }
        } catch (e) {
          addHighlightExpressionError(e.message);
        }
      }

      validateHighlightExpression(measure.SuccessWhen, 'SuccessWhen');
      validateHighlightExpression(measure.WarningWhen, 'WarningWhen');
      validateHighlightExpression(measure.DangerWhen, 'DangerWhen');
    }
  }

  public onDeleteMeasure(index: number, model: ReportDefinition) {
    model.Measures.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onToggleMeasureAdvancedOptions() {
    this.measureShowAdvancedOptions = !this.measureShowAdvancedOptions;
  }

  public canApplyMeasure(measure: ReportDefinitionMeasure): boolean {
    return !!measure.Expression;
  }

  ///////////////////// Select v2.0

  selectToEdit: ReportDefinitionSelect;
  selectToEditHasChanged = false;

  public getSelect(model: ReportDefinition): ReportDefinitionSelect[] {
    model.Select = model.Select || [];
    return model.Select;
  }

  public onDeleteSelect(index: number, model: ReportDefinition) {
    model.Select.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onConfigureSelect(index: number, model: ReportDefinition) {
    this.selectToEditHasChanged = false;
    const selectToEdit = { ...model.Select[index] } as ReportDefinitionSelect;
    this.selectToEdit = selectToEdit;
    this.modelRef = model;

    this.modalService.open(this.selectConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.selectToEditHasChanged) {
        model.Select[index] = selectToEdit;

        this.validateModel(model);
        this.synchronizeParameters(model);
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public validateSelect(select: ReportDefinitionSelect, model: ReportDefinitionForSave): void {

    if (!select) {
      return;
    }

    select.serverErrors = {};
    function addExpressionError(err: string) {
      select.serverErrors.Expression = select.serverErrors.Expression || [];
      select.serverErrors.Expression.push(err);
    }

    try {
      ////////////////// Expression Validation

      const exp = Queryex.parseSingle(select.Expression);
      if (!!exp) {
        const aggregations = exp.aggregations();
        if (aggregations.length > 0) {
          addExpressionError(`Expression cannot contain aggregation functions like '${aggregations[0].name}'.`);
        }
      } else {
        addExpressionError(`Expression cannot be empty.`);
      }
    } catch (e) {
      addExpressionError(e.message);
    }

    // TODO Control Validation
  }

  public canApplySelect(select: ReportDefinitionSelect): boolean {
    return !!select.Expression;
  }

  /////////////////// Parameters v2.0

  paramToEdit: ReportDefinitionParameter;
  paramToEditHasChanged = false;

  public getParameters(model: ReportDefinition): ReportDefinitionParameter[] {
    model.Parameters = model.Parameters || [];
    return model.Parameters;
  }

  public onConfigureParameter(index: number, model: ReportDefinition) {
    this.paramToEditHasChanged = false;
    const itemToEdit = { ...model.Parameters[index] } as ReportDefinitionParameter;
    this.paramToEdit = itemToEdit;
    this.modelRef = model;

    this.modalService.open(this.paramConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.paramToEditHasChanged) {
        if (!this.showParamDefaultExpression(itemToEdit)) {
          delete itemToEdit.DefaultExpression;
        }

        model.Parameters[index] = itemToEdit;

        // Configuring the parameter does not affect the parameters
        this.validateModel(model);
        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public validateParam(param: ReportDefinitionParameter, _: ReportDefinitionForSave): void {
    if (!param) {
      return;
    }

    param.serverErrors = {};
    function addDefaultExpressionError(err: string) {
      param.serverErrors.DefaultExpression = param.serverErrors.DefaultExpression || [];
      param.serverErrors.DefaultExpression.push(err);
    }

    ////////////////// Default Expression Validation
    let exp: QueryexBase;
    try {
      // Prepare the expression
      exp = Queryex.parseSingle(param.DefaultExpression);
      if (!!exp) {
        const aggregations = exp.aggregations();
        const columnAccesses = exp.columnAccesses();
        const parameters = exp.parameters();
        if (aggregations.length > 0) {
          addDefaultExpressionError(`Expression cannot contain aggregation functions like '${aggregations[0].name}'.`);
        } else if (columnAccesses.length > 0) {
          addDefaultExpressionError(`Expression cannot contain column access literals like '${columnAccesses[0]}'.`);
        } else if (parameters.length > 0) {
          addDefaultExpressionError(`Expression cannot contain parameters like '${parameters[0]}'.`);
        }
      }
    } catch (e) {
      addDefaultExpressionError(e.message);
    }
  }

  public showParamDefaultExpression(param: ReportDefinitionParameter): boolean {
    return param.Visibility === 'Optional';
  }

  public canApplyParam(_: ReportDefinitionParameter): boolean {
    return true;
  }

  ///////////////////// Total Labels v2.0

  totalLabelsToEdit: {
    ColumnsTotalLabel?: string,
    ColumnsTotalLabel2?: string,
    ColumnsTotalLabel3?: string,
    RowsTotalLabel?: string,
    RowsTotalLabel2?: string,
    RowsTotalLabel3?: string,
    serverErrors?: { [key: string]: string[] }
  };
  totalLabelsToEditHasChanged = false;


  public onConfigureTotalLabels(model: ReportDefinition) {
    this.totalLabelsToEditHasChanged = false;
    const totalLabelsToEdit = {
      ColumnsTotalLabel: model.ColumnsTotalLabel,
      ColumnsTotalLabel2: model.ColumnsTotalLabel2,
      ColumnsTotalLabel3: model.ColumnsTotalLabel3,
      RowsTotalLabel: model.RowsTotalLabel,
      RowsTotalLabel2: model.RowsTotalLabel2,
      RowsTotalLabel3: model.RowsTotalLabel3,
      serverErrors: model.serverErrors,
    };

    this.totalLabelsToEdit = totalLabelsToEdit;
    this.modelRef = model;

    this.modalService.open(this.totalLabelsModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.totalLabelsToEditHasChanged) {
        model.ColumnsTotalLabel = totalLabelsToEdit.ColumnsTotalLabel;
        model.ColumnsTotalLabel2 = totalLabelsToEdit.ColumnsTotalLabel2;
        model.ColumnsTotalLabel3 = totalLabelsToEdit.ColumnsTotalLabel3;
        model.RowsTotalLabel = totalLabelsToEdit.RowsTotalLabel;
        model.RowsTotalLabel2 = totalLabelsToEdit.RowsTotalLabel2;
        model.RowsTotalLabel3 = totalLabelsToEdit.RowsTotalLabel3;

        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  ///////////////////// Filter v2.0

  public onFilterChanged(model: ReportDefinition) {
    this.validateModel(model);
    this.synchronizeParameters(model);
    this.onDefinitionChange(model);
  }

  ///////////////////// Having v.20

  public onHavingChanged(model: ReportDefinition) {
    this.validateModel(model);
    this.synchronizeParameters(model);
    this.onDefinitionChange(model);
  }

  ///////////////////// Model Validation v2.0

  public validateModel(model: ReportDefinition) {

    if (!model) {
      return;
    }

    model.serverErrors = {};

    // Filter Validation
    if (!!model.Filter) {

      function addFilterError(err: string) {
        model.serverErrors.Filter = model.serverErrors.Filter || [];
        model.serverErrors.Filter.push(err);
      }

      try {
        const exp = Queryex.parseSingle(model.Filter);
        if (!!exp) {
          const aggregations = exp.aggregations();
          if (aggregations.length > 0) {
            addFilterError(`Expression cannot contain aggregation functions like '${aggregations[0].name}'.`);
          }
        }
      } catch (e) {
        addFilterError(e.message);
      }
    }

    // Having Validation
    if (!!model.Having) {

      function addHavingError(err: string) {
        model.serverErrors.Having = model.serverErrors.Having || [];
        model.serverErrors.Having.push(err);
      }

      try {
        const exp = Queryex.parseSingle(model.Having);
        if (!!exp) {
          const unaggregated = exp.unaggregatedColumnAccesses();
          if (unaggregated.length > 0) {
            addHavingError(`Expression cannot contain unaggregated column accesses like '${unaggregated[0]}'.`);
          }
        }
      } catch (e) {
        addHavingError(e.message);
      }
    }

    for (const col of model.Columns) {
      this.validateDimension(col, model);
    }

    for (const row of model.Rows) {
      this.validateDimension(row, model);
    }

    for (const measure of model.Measures) {
      this.validateMeasure(measure, model);
    }

    for (const select of model.Select) {
      this.validateSelect(select, model);
    }

    for (const param of model.Parameters) {
      this.validateParam(param, model);
    }
  }

  public synchronizeParameters(model: ReportDefinition) {
    try {
      const forClient = model as ReportDefinitionForClient;
      const paramInfos = QueryexUtil.getParameterDescriptors(forClient, this.workspace, this.translate);
      const keysLower = Object.keys(paramInfos);

      if (keysLower.length === 0) {
        model.Parameters = []; // Optimization
      } else {

        // (1) Remove parameters without a matching placeholder (case insensitive)
        const parameters = model.Parameters.filter(p => !!paramInfos[p.Key.toLowerCase()]);

        // (2) Create a tracker for existing model parameters
        const modelTracker: { [key: string]: ReportDefinitionParameter } = {};
        parameters.forEach(pa => modelTracker[pa.Key.toLowerCase()] = pa);

        // (3) Add new model parameters for new expression parameters
        for (const keyLower of keysLower) {
          const paramInfo = paramInfos[keyLower];
          let parameter: ReportDefinitionParameter = modelTracker[keyLower];
          if (!parameter) {
            parameter = {
              Id: 0,
              Visibility: paramInfo.isRequiredUsage ? 'Required' : 'Optional',
            };

            modelTracker[keyLower] = parameter;
            parameters.push(parameter);
          }

          parameter.Key = paramInfo.key;
        }

        model.Parameters = parameters;
      }
    } catch (e) {
      console.error(e.message);
    }
  }

  ////////////////// Control v2.0

  private _controlChoicesDefinitions: DefinitionsForClient;
  private _controlDisplayCache: { [key: string]: () => string };

  public controlDisplay = (control: Control) => {
    const ws = this.ws;
    const defs = ws.definitions;
    if (this._controlChoicesDefinitions !== defs) {
      this._controlChoicesDefinitions = defs;

      // display names
      this._controlDisplayCache = {};
      for (const choice of this.controlSimpleChoices()) {
        this._controlDisplayCache[choice.value] = choice.name;
      }

      for (const choice of this.controlEntityChoices()) {
        this._controlDisplayCache[choice.value] = choice.name;
      }
    }

    const displayFunc = this._controlDisplayCache[control];
    return !!displayFunc ? displayFunc() : '';
  }

  public controlSimpleChoices(): SelectorChoice[] {
    return simpleControls(this.translate);
  }

  public controlEntityChoices(): SelectorChoice[] {
    return collectionsWithEndpoint(this.workspace, this.translate, true);
  }

  public showOptions(control: Control) {
    return !!control && hasControlOptions(control);
  }
}
