import {
  Component, OnInit, Input, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy, OnChanges, SimpleChanges, Output, EventEmitter
} from '@angular/core';
import {
  WorkspaceService, ReportStatus, ReportStore, MultiSeries, SingleSeries, ReportArguments,
  PivotTable, DimensionCell, MeasureCell, LabelCell, MeasureInfo, DimensionInfo, ChartDimensionCell, DEFAULT_PAGE_SIZE
} from '~/app/data/workspace.service';
import { Subscription, Subject, Observable, of } from 'rxjs';
import { EntityDescriptor, metadata, entityDescriptorImpl, PropDescriptor, isText, isNumeric } from '~/app/data/entities/base/metadata';
import { TranslateService } from '@ngx-translate/core';
import { switchMap, tap, catchError, finalize } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { FilterTools, FilterExpression } from '~/app/data/filter-expression';
import { isSpecified, mergeEntitiesInWorkspace } from '~/app/data/util';
import {
  ReportDefinitionForClient, ReportDimensionDefinitionForClient,
  ReportMeasureDefinitionForClient, ReportSelectDefinitionForClient
} from '~/app/data/dto/definitions-for-client';
import { Router, Params } from '@angular/router';
import { displayEntity, displayValue } from '~/app/shared/auto-cell/auto-cell.component';
import { Entity } from '~/app/data/entities/base/entity';
import { EntitiesResponse, GetResponse } from '~/app/data/dto/get-response';
import { ReportOrderDirection, ChartType } from '~/app/data/entities/report-definition';

export enum ReportView {
  pivot = 'pivot',
  chart = 'chart'
}

/**
 * Hashes one dimension of an aggregate result for the pivot table
 */
interface PivotHash {
  cell: DimensionCell;
  values?: { [value: string]: PivotHash };
  children: DimensionCell[];
  undefined?: PivotHash;
}

@Component({
  selector: 'b-report-results',
  templateUrl: './report-results.component.html',
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportResultsComponent implements OnInit, OnChanges, OnDestroy {

  @Input()
  state: ReportStore; // immutable

  @Input()
  definition: ReportDefinitionForClient; // immutable

  @Input()
  arguments: ReportArguments = {}; // immutable

  @Input()
  view: ReportView;

  @Input()
  refresh: Observable<void>; // Must be set only once

  @Input()
  mode: 'screen' | 'preview' | 'dashboard' = 'screen';

  // @Output()
  // skipChange: EventEmitter<number> = new EventEmitter<number>(); // In screen mode this informs the parent component to update the URL

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense

  // Stuff that should go in a service
  private _currentRows: (DimensionCell | LabelCell | MeasureCell)[][];
  private _currentColumns: (DimensionCell | LabelCell)[][];

  /**
   * This is a copy of the state pivot table rows, but with the
   * collapsed rows removed, to support smooth virtual scrolling
   */
  private _modifiedRows: (DimensionCell | LabelCell | MeasureCell)[][];

  /**
   * Computed for the blank upper left corner
   */
  private _rowSpan: number;

  /**
   * Computed for the blank upper left corner, 0 when it's hidden
   */
  private _colSpan: number;

  /**
   * The maximum level among column dimensions that is visible
   */
  private _maxVisibleLevel: number;

  // NGX-Charts options
  animations = true;
  showXAxis = true;
  showYAxis = true;
  showXAxisLabel = true;
  showYAxisLabel = true;
  colorful = {
    domain: [
      '#17A2B8', '#1490A3', '#128091',
      '#0D606D', '#10707F', '#0B505B',
      '#073036', '#094049', '#042024',
      // '#80E0EF', '#C9F2F8', '#49D3E9',
      // '#1BC0DA', '#25CBE4', '#19B0C8',
    ]
  };
  monochromatic = { domain: ['#17a2b8'] };
  heat = { domain: ['#96D5DF', '#17a2b8', '#052429'] }; // different shades of the same color for heat map

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService,
    private api: ApiService, private cdr: ChangeDetectorRef, private router: Router) {
  }

  ngOnInit() {

    this._subscriptions = new Subscription();
    this._subscriptions.add(this.workspace.stateChanged$.subscribe({
      next: () => this.cdr.markForCheck()
    }));

    this._subscriptions.add(this.notifyFetch$.pipe(
      switchMap(() => this.doFetch())
    ).subscribe());

    // Hook the refresh event
    if (!!this.refresh) {
      this._subscriptions.add(this.refresh.pipe(
        tap(_ => this.state.reportStatus === ReportStatus.loaded))
        .subscribe(() => this.fetch()));
    }

    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);

    // Set to default
    this.view = this.view || (!!this.definition.Chart && !!this.definition.DefaultsToChart ? ReportView.chart : ReportView.pivot);

    this.state = this.state || new ReportStore(); // if no state is provided

    // Here we do the usual pattern of checking whether the state in the
    // singleton service is still the same as that supplied by the url
    // parameters in which case we do not have to fetch the data again
    const s = this.state;
    const hasChanges = this.applyChanges();
    if (s.reportStatus !== ReportStatus.loaded || hasChanges) {
      try {

        s.realColumns = this.computeRealDimensions(this.definition.Columns);
        s.realRows = this.computeRealDimensions(this.definition.Rows);
        s.uniqueDimensions = this.computeUniqueDimensions(s.realColumns.concat(s.realRows));
        s.measures = this.computeMeasureInfos(this.definition.Measures);
        s.singleNumericMeasure = this.state.measures.find(m => isNumeric(m.desc));

        s.disableFetch = false;
        this.fetch();
      } catch (ex) {
        s.disableFetch = true;
        s.reportStatus = ReportStatus.error;
        s.errorMessage = ex;
        this.cdr.markForCheck();
      }
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    // Using our famous pattern
    const screenDefProperties = [changes.state, changes.definition, changes.mode];
    const screenDefChanges = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());
    if (screenDefChanges) {

      this.ngOnDestroy();
      this.ngOnInit();
    } else {

      // Changes that require a mere refresh
      const screenRefreshProperties = [changes.arguments];
      const screenRefreshChanges = screenRefreshProperties.some(prop => !!prop && !prop.isFirstChange());
      // Refresh data whenever the arguments change
      if (screenRefreshChanges && this.applyChanges()) {
        this.state.skip = 0; // when arguments change reset to first page
        this.fetch();
      }
    }
  }

  /**
   * If there are changes that require a refresh it applies
   * them and returns true, otherwise returns false
   */
  private applyChanges(): boolean {
    const s = this.state;
    let hasChanged = false;

    if (this.definition !== s.definition) {
      s.definition = this.definition;
      hasChanged = true;
    }

    const urlArgs = this.arguments;
    const wsArgs = this.state.arguments;
    for (const key of Object.keys(urlArgs)) {
      if (wsArgs[key] !== urlArgs[key]) {
        wsArgs[key] = urlArgs[key];
        hasChanged = true;
      }
    }

    return hasChanged;
  }

  ngOnDestroy() {
    this.notifyDestruct$.next();
    this._subscriptions.unsubscribe();
  }

  get entityDescriptor(): EntityDescriptor {
    if (!this.definition) {
      return null;
    }

    const coll = this.definition.Collection;
    const definitionId = this.definition.DefinitionId;
    return !!coll ? metadata[coll](this.workspace.current, this.translate, definitionId) : null;
  }

  get apiEndpoint(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.apiEndpoint : null;
  }

  public fetch(): void {
    this.notifyFetch$.next();
  }

  doFetch(): Observable<void> {

    let s = this.state;
    if (s.disableFetch) {
      // bad definition
      return of(null);
    }

    s.reportStatus = ReportStatus.loading;

    // FILTER
    let filter: string;
    try {
      filter = this.computeFilter();
    } catch (ex) {
      s.reportStatus = ReportStatus.error;
      s.errorMessage = ex;
      return of(null);
    }

    // This will show the spinner
    this.cdr.markForCheck();

    let obs$: Observable<EntitiesResponse<Entity>>;
    if (this.showDetails) {
      const top = this.definition.Top || DEFAULT_PAGE_SIZE;
      const skip = !!this.definition.Top ? 0 : s.skip;
      const select = this.computeSelect();
      const orderby = this.definition.OrderBy;

      if (!select) {
        s.reportStatus = ReportStatus.information;
        s.information = () => this.translate.instant('DragSelect');

        return of(null);
      }

      obs$ = this.crud.get({
        top,
        skip,
        orderby,
        select,
        filter
      }).pipe(
        tap((response: GetResponse) => {
          s = this.state;
          s.top = response.Top;
          s.skip = response.Skip;
          s.total = response.TotalCount;
        })
      );
    } else { // Show Summary or Chart
      // SELECT
      const select = this.computeAggregateSelect();
      if (!select) {
        s.reportStatus = ReportStatus.information;
        s.information = () => this.translate.instant('DragDimensionsOrMeasures');

        return of(null);
      }

      // TOP
      const top = this.definition.Top;

      obs$ = this.crud.getAggregate({
        top,
        select,
        filter
      });
    }

    return obs$.pipe(
      tap((response: EntitiesResponse) => {
        s = this.state; // get the source
        s.reportStatus = ReportStatus.loaded;
        s.filter = filter;
        s.result = response.Result;
        if (!!response.RelatedEntities && Object.keys(response.RelatedEntities).length > 0) {
          // Merge the entities and Notify everyone
          mergeEntitiesInWorkspace(response.RelatedEntities, this.workspace);
          this.workspace.notifyStateChanged();
        }
      }),
      catchError((friendlyError) => {
        s = this.state; // get the source
        s.reportStatus = ReportStatus.error;
        s.errorMessage = friendlyError.error;
        return of(null);
      }),
      finalize(() => this.cdr.markForCheck())
    );
  }

  private computeMeasureInfos(measures: ReportMeasureDefinitionForClient[]): MeasureInfo[] {

    if (!measures) {
      return [];
    }

    return measures.map(measureDef => {
      const key = `${measureDef.Aggregation}(${measureDef.Path.split('/').map(e => e.trim()).join('/')})`;
      const aggregation = measureDef.Aggregation;
      const steps = measureDef.Path.split('/').map(e => e.trim());
      const prop = steps.pop();
      const collection = this.definition.Collection;
      const definitionId = this.definition.DefinitionId;
      const ws = this.workspace.current;
      const trx = this.translate;
      const entityDesc = entityDescriptorImpl(steps, collection, definitionId, ws, trx);
      const propDesc = entityDesc.properties[prop];
      if (!propDesc) {
        throw new Error(`The path '${measureDef.Path}' is not valid, the terminal property ${prop} was not found`);
      }

      let desc: PropDescriptor;
      switch (aggregation) {
        case 'count':
          desc = { control: 'number', label: propDesc.label, maxDecimalPlaces: 0, minDecimalPlaces: 0, alignment: 'right' };
          break;
        case 'max':
        case 'min':
          desc = propDesc;
          break;
        case 'sum':
          if (propDesc.control !== 'number') {
            console.error(`Use of sum aggregation on a non-numeric property ${prop}`);
          } else {
            desc = propDesc;
          }
          break;
        case 'avg':
          if (propDesc.control !== 'number') {
            console.error(`Use of avg aggregation on a non-numeric property ${prop}`);
          } else {
            desc = { ...propDesc };
            desc.minDecimalPlaces = Math.max(desc.minDecimalPlaces, 2);
            desc.maxDecimalPlaces = Math.max(desc.maxDecimalPlaces, 2);
          }
          break;
      }

      const label = () => !!measureDef.Label ? this.workspace.current.getMultilingualValueImmediate(measureDef, 'Label') :
        this.translate.instant('DefaultAggregationMeasure', {
          aggregation: this.translate.instant('ReportDefinition_Aggregation_' + aggregation),
          measure: desc.label()
        });

      return { key, desc, aggregation, label };
    });
  }

  private computeRealDimensions(dims: ReportDimensionDefinitionForClient[]): DimensionInfo[] {

    dims = dims || [];
    return dims.map(dim => {
      // Normalized the path
      const path = dim.Path.split('/').map((e: string) => e.trim()).join('/');

      // Get the PropDescriptor describing the target property of the path
      const collection = this.definition.Collection;
      const definitionId = this.definition.DefinitionId;
      const ws = this.workspace.current;
      const trx = this.translate;
      const steps = path.split('/');
      const prop = steps[steps.length - 1];
      const parentEntityDesc = entityDescriptorImpl(steps.slice(0, -1), collection, definitionId, ws, trx);
      const propDesc = parentEntityDesc.properties[prop];
      if (!propDesc) {
        throw new Error(`Property ${prop} does not exist on collection: '${collection}', definition: '${definitionId || ''}'.`);
      }

      // If this is a nav property, get the EntityDescriptor describing the target entity as well
      let entityDesc: EntityDescriptor;
      if (propDesc.control === 'navigation') {
        entityDesc = entityDescriptorImpl(steps, collection, definitionId, ws, trx);
      }

      // Create the dimension info
      const result: DimensionInfo = {
        key: path,
        path,
        propDesc,
        autoExpand: dim.AutoExpand,
        label: () => !!dim.Label ? this.workspace.current.getMultilingualValueImmediate(dim, 'Label') : propDesc.label()
      };

      // This is a nav property, add a few extra things to allow for
      // efficient extraction of mock navigation entities from the server results
      if (!!entityDesc) {
        const pathSlash = !!path ? path + '/' : '';

        result.entityDesc = entityDesc;
        result.idKey = `${pathSlash}Id`;
        result.selectKeys = entityDesc.select.map(s => ({ path: `${pathSlash}${s}`, prop: s }));
      }

      return result;
    });
  }

  /**
   * Used for charts, calculates the unique set of dimensions across the combined rows and charts
   */
  private computeUniqueDimensions(dims: DimensionInfo[]): DimensionInfo[] {
    const tracker = {};
    const result: DimensionInfo[] = [];
    for (const dim of dims) {
      if (!tracker[dim.key]) {
        tracker[dim.key] = dim.key;
        result.push(dim);
      }
    }

    return result;
  }

  private computeSelect(): string {
    const select: ReportSelectDefinitionForClient[] = this.select;
    if (!select || select.length === 0) {
      return '';
    }

    const resultPaths: { [path: string]: boolean } = {};
    const baseEntityDescriptor = this.entityDescriptor;

    // (1) append the current entity type default properties (usually 'Name', 'Name2' and 'Name3')
    baseEntityDescriptor.select.forEach(e => resultPaths[e] = true);

    // (2) append the definitoinId if any, it must always be loaded
    if (!!baseEntityDescriptor.definitionIds) {
      resultPaths.DefinitionId = true;
    }

    // (3) replace every path that terminates with a nav property (e.g. 'Unit' => 'Unit/Name,Unit/Name2,Unit/Name3')
    select.map(col => col.Path).forEach(path => {

      if (!path) {
        return;
      }

      const steps = path.split('/').map(e => e.trim());
      path = steps.join('/'); // to trim extra spaces

      try {
        const currentDesc = entityDescriptorImpl(steps, this.collection,
          this.definitionId, this.workspace.current, this.translate);

        currentDesc.select.forEach(descSelect => resultPaths[`${path}/${descSelect}`] = true);
      } catch {
        resultPaths[path] = true;
      }
    });

    return Object.keys(resultPaths).join(',');
  }

  private computeAggregateSelect(): string {

    if (!this.definition) {
      return '';
    }

    const collection = this.definition.Collection;
    const definitionId = this.definition.DefinitionId;
    const cols = this.definition.Columns || [];
    const rows = this.definition.Rows || [];
    const atomsTracker: { [path: string]: true } = {};
    const atoms: string[] = [];

    function addAtom(path: string, orderDir?: ReportOrderDirection) {
      if (!atomsTracker[path]) {
        atomsTracker[path] = true;
        atoms.push(`${path} ${orderDir || ''}`.trim());
      }
    }

    // (1) Add dimensions (special handling for nav props)
    cols.concat(rows).forEach(dimensionDef => {
      const path = dimensionDef.Path.trim().split('/').map(e => e.trim());
      const property = path[path.length - 1]; // last element
      const stringPath = path.join('/');
      const orderDir = dimensionDef.OrderDirection;

      // get the description of the entity hosting the property
      const currentDesc = entityDescriptorImpl(path.slice(0, -1), collection,
        definitionId, this.workspace.current, this.translate);

      const propDesc = currentDesc.properties[property];
      if (!!propDesc && propDesc.control === 'navigation') {
        // For nav properties, select the Id + the display properties
        addAtom(`${stringPath}/Id`);

        // This is to ensure that ordering of select columns is done in the correct order
        currentDesc.orderby.forEach(o => {
          const descSelect = currentDesc.select.find(s => s === o);
          if (!!descSelect) {
            const descSelectPath = `${stringPath}/${descSelect}`.trim();
            addAtom(descSelectPath, orderDir);
          }
        });

        currentDesc.select.forEach(descSelect => {
          const descSelectPath = `${stringPath}/${descSelect}`.trim();
          addAtom(descSelectPath, orderDir);
        });
      } else {

        // For non-nav properties, simply add the path as is
        addAtom(stringPath, orderDir);
      }
    });

    // (2) Add measures (nav props not allowed)
    const measures = this.definition.Measures || [];
    measures.forEach(measureDef => {
      // extract the relevant values from definition
      const path = measureDef.Path.split('/').map(e => e.trim()).join('/');
      const orderDir = measureDef.OrderDirection;
      const aggregation = measureDef.Aggregation;

      // construct the atom
      let selectAtom = path;
      selectAtom = !!aggregation ? `${aggregation}(${selectAtom})` : selectAtom;

      // add it to the dictionary
      addAtom(selectAtom, orderDir);

      if (aggregation === 'avg') {
        // in order to aggregate averages, we will need the count too
        addAtom(`count(${path})`);
      }
    });

    // Return result
    const select = atoms.join(',');
    return select;
  }

  private computeFilter(): string {

    let exp: FilterExpression = FilterTools.parse(this.definition.Filter);
    if (!exp) {
      return null;
    }

    const lowerCaseArgs: ReportArguments = {};
    for (const arg of Object.keys(this.arguments)) {
      lowerCaseArgs[arg.toLowerCase()] = this.arguments[arg];
    }

    const lowerCaseDefs: { [key: string]: boolean } = {};
    if (this.definition.Parameters) {
      for (const paramDef of this.definition.Parameters.filter(p => !!p.Key)) {
        lowerCaseDefs[paramDef.Key.toLowerCase()] = paramDef.IsRequired;
      }
    }

    exp = this.applyArguments(exp, lowerCaseArgs, lowerCaseDefs);

    return FilterTools.stringify(exp);
  }

  public applyArguments(
    exp: FilterExpression, lowerCaseArgs: ReportArguments,
    lowerCaseDefs: { [key: string]: boolean }): FilterExpression {

    switch (exp.type) {
      case 'conjunction':
      case 'disjunction':
        const left = this.applyArguments(exp.left, lowerCaseArgs, lowerCaseDefs);
        const right = this.applyArguments(exp.right, lowerCaseArgs, lowerCaseDefs);
        if (!!left && !!right) {
          exp.left = left;
          exp.right = right;
          return exp;
        } else {
          return !!left ? left : !!right ? right : null;
        }

      case 'negation':
        const inner = this.applyArguments(exp.inner, lowerCaseArgs, lowerCaseDefs);
        if (!!inner) {
          exp.inner = inner;
          return exp;
        } else {
          return null;
        }

      case 'atom':
        // If atom has a parameter that is not in arguments or definitions, remove it
        if (exp.value.startsWith('@')) {
          const keyLower = exp.value.substr(1).toLowerCase(); // case insensitive
          const value = lowerCaseArgs[keyLower];
          const isRequired = !!lowerCaseDefs[keyLower];

          if (isSpecified(value)) {
            const entityDesc = entityDescriptorImpl(
              exp.path,
              this.definition.Collection,
              this.definition.DefinitionId,
              this.workspace.current,
              this.translate);

            const propDesc = entityDesc.properties[exp.property];
            if (isText(propDesc)) {
              exp.value = `'${value.replace('\'', '\'\'')}'`;
            } else {
              exp.value = value + '';
            }
            return exp;

          } else if (isRequired) {
            // value is undefined or null but is required
            throw new Error(`Required parameter ${exp.value} was not set`);
          } else {
            // prune this atom out of the filter
            return null;
          }
        } else {
          // no placeholder (@XYZ) in this atom, all good
          return exp;
        }
    }
  }

  private computeFilterAtoms(dimension: DimensionCell | ChartDimensionCell): string[] {
    const atoms: string[] = [];
    let currentDimension = dimension;
    while (!!currentDimension) {
      let path = currentDimension.path;
      let propDesc = currentDimension.propDesc;

      if (!!propDesc) {
        // (1) Adjust path and propDesc in the case of a nav property
        if (propDesc.control === 'navigation') {
          // Update path
          const fkName = propDesc.foreignKeyName;
          const steps = path.split('/').slice(0, -1);
          path = steps.concat([fkName]).join('/');

          // Update propDesc
          const entityDesc = currentDimension.entityDesc;
          propDesc = entityDesc.properties.Id;
          if (!propDesc) {
            // Developer mistake
            throw new Error(`Entity descriptor for ${entityDesc.titlePlural()} is missing an Id descriptor.`);
          }
        }

        // (2) Calculate the filter atom and add it
        const valueId = currentDimension.valueId;
        if (!isSpecified(valueId)) {
          atoms.push(`${path} eq null`);
        } else if (isText(propDesc)) {
          atoms.push(`${path} eq '${valueId.replace('\'', '\'\'')}'`);
        } else {
          atoms.push(`${path} eq ${valueId + ''}`);
        }
      }

      currentDimension = currentDimension.parent;
    }

    return atoms;
  }

  // UI Bindings

  public get showDetails(): boolean {
    return !!this.definition && this.definition.Type === 'Details';
  }

  public get showSummary(): boolean {
    return !!this.definition && this.definition.Type === 'Summary'
      && this.view === ReportView.pivot;
  }

  public get showChart(): boolean {
    return !!this.definition && this.definition.Type === 'Summary'
      && this.view === ReportView.chart && !!this.state.singleNumericMeasure;
  }

  public get showSpecifyNumericMeasure(): boolean {
    return !!this.definition && this.definition.Type === 'Summary'
      && this.view === ReportView.chart && !this.state.singleNumericMeasure;
  }

  public get showResults(): boolean {
    return this.state.reportStatus === ReportStatus.loaded && !this.showNoItemsFound;
  }

  public get showErrorMessage(): boolean {
    return this.state.reportStatus === ReportStatus.error;
  }

  public get showInformation(): boolean {
    return this.state.reportStatus === ReportStatus.information;
  }

  public get showSpinner(): boolean {
    return this.state.reportStatus === ReportStatus.loading;
  }

  public get showNoItemsFound(): boolean {
    const s = this.state;
    return s.reportStatus === ReportStatus.loaded &&
      (!s.result || s.result.length === 0);
  }

  public get errorMessage(): string {
    return this.state.errorMessage;
  }

  public information(): string {
    return this.state.information();
  }

  public get chart(): ChartType {
    return !!this.definition ? this.definition.Chart : null;
  }

  //////// SUMMARY - PIVOT

  private extractValueAndValueId(g: Entity, dimension: DimensionInfo): { value: any, valueId: any } {
    let value: any;
    let valueId: any;
    if (!!dimension.entityDesc) {
      // For navigation properties, the value is an object emulating the real entity, and the id is its Id
      valueId = g[dimension.idKey];
      if (isSpecified(valueId)) {
        value = { Id: valueId };
        dimension.selectKeys.forEach(key => value[key.prop] = g[key.path]);
      }
    } else {
      // For simple properties (non-nav) both the id and the value are set to the value of that property
      valueId = g[dimension.key];
      value = valueId;
    }

    return { value, valueId };
  }

  private extractValueId(g: Entity, dimension: DimensionInfo): any {
    if (!!dimension.entityDesc) {
      // For navigation properties, the value is an object emulating the real entity, and the id is its Id
      return g[dimension.idKey];
    } else {
      // For simple properties (non-nav) both the id and the value are set to the value of that property
      return g[dimension.key];
    }
  }

  public get pivot(): PivotTable {
    // The various parts of the pivot table bind to what is returned by this property
    const s = this.state;
    if (s.currentResultForPivot !== s.result) {
      s.currentResultForPivot = s.result;

      // Helper function that builds hashes mapping every combination of dimensions to a dimension index
      const dimensionHash = (dimensions: DimensionInfo[]): { hash: PivotHash, cells: DimensionCell[] } => {

        const rootHash: PivotHash = { cell: null, children: [] }; // cell == null is the root

        if (dimensions.length > 0) {
          for (const g of s.result) {
            let currentHash = rootHash;
            let level = 0;
            const lastDimension = dimensions[dimensions.length - 1];
            for (const dimension of dimensions) {
              const { value, valueId } = this.extractValueAndValueId(g, dimension);
              let targetHash: PivotHash;

              // Either go down the values or the undefined route
              if (isSpecified(valueId)) {
                if (!currentHash.values) {
                  currentHash.values = {};
                }

                targetHash = currentHash.values[valueId];

              } else {
                targetHash = currentHash.undefined;
              }

              // If the target hash is null, create it and add it
              if (!targetHash) {
                targetHash = {
                  cell: {
                    type: 'dimension',
                    path: dimension.path,
                    value,
                    valueId,
                    propDesc: dimension.propDesc,
                    entityDesc: dimension.entityDesc,
                    isExpanded: dimension !== lastDimension && dimension.autoExpand,
                    hasChildren: dimension !== lastDimension,
                    level,
                    index: 0, // computed below
                    parent: currentHash.cell,
                    isTotal: false
                  },
                  children: []
                };

                currentHash.children.push(targetHash.cell);

                if (isSpecified(valueId)) {
                  currentHash.values[valueId] = targetHash;
                } else {
                  currentHash.undefined = targetHash;
                }
              }

              level++;
              currentHash = targetHash;
            }
          }
        }

        // This recursive function adds the dimension cells
        // in order, parents always before children
        const cells: DimensionCell[] = [];
        let index = 0;
        function addHash(hash: PivotHash) {
          for (const cell of hash.children) {
            cell.index = index++;
            cells.push(cell);
            const childHash = isSpecified(cell.valueId) ? hash.values[cell.valueId] : hash.undefined;
            addHash(childHash);
          }
        }

        addHash(rootHash);

        return { hash: rootHash, cells };
      };

      /////////// Calculate the top half of the report (the "columnHeaders")
      const realColumns = s.realColumns;
      const columnsResult = dimensionHash(realColumns);
      const columnCells = columnsResult.cells;
      const columnHeaders: (DimensionCell | LabelCell)[][] = [];

      for (const cell of columnCells) {
        if (!columnHeaders[cell.level]) {
          columnHeaders[cell.level] = [cell];
        } else {
          columnHeaders[cell.level].push(cell);
        }
      }

      // Prepare the bottom half of the pivot table
      const realRows = s.realRows;
      const rowResult = dimensionHash(realRows);
      const rowCells: DimensionCell[] = rowResult.cells;

      const columnGrandTotalIndex = columnCells.length;
      const rowGrandTotalIndex = rowCells.length;

      // Whether to show the column or row totals
      const showColumnTotals: boolean = s.measures.length > 0 &&
        (this.definition.ShowColumnsTotal || realColumns.length === 0);
      const showRowTotals: boolean = s.measures.length > 0 &&
        (this.definition.ShowRowsTotal || realRows.length === 0 && s.measures.length > 0);

      if (showColumnTotals) {
        if (realColumns.length > 0) {
          const columnsGrandTotalLabel: LabelCell = {
            type: 'label',
            label: () => this.translate.instant('GrandTotal'),
            level: 0,
            parent: null,
            isTotal: realColumns.length > 0
          };
          columnHeaders[0].push(columnsGrandTotalLabel);
        }
      }

      /////////// Calculate the bottom half of the report (the "rows")
      const rows: (DimensionCell | MeasureCell | LabelCell)[][] = [];
      const rowProps = realRows.length === 0 ? -1 : 0; // TODO: number of property columns for the rows that will be displayed
      for (let r = 0; r < rowCells.length; r++) {
        const rowCell = rowCells[r];
        const row: (DimensionCell | LabelCell | MeasureCell)[] = [rowCell];
        for (let c = 0; c < columnCells.length; c++) {
          row[c + rowProps + 1] = {
            type: 'measure',
            parent: columnCells[c],
            values: [],
            counts: [],
          };
        }

        // Add the column grand total if required
        if (showColumnTotals) {
          row[columnGrandTotalIndex + rowProps + 1] = {
            type: 'measure',
            parent: null,
            values: [],
            counts: [],
            isTotal: realColumns.length > 0
          };
        }

        rows[r] = row;
      }

      if (showRowTotals) {
        const rowsGrandTotalLabel: LabelCell = {
          type: 'label',
          label: () => this.translate.instant('GrandTotal'),
          level: 0,
          parent: null,
          isTotal: realRows.length > 0
        };

        const totalRow: (DimensionCell | LabelCell | MeasureCell)[] = realRows.length > 0 ? [rowsGrandTotalLabel] : [];

        for (let c = 0; c < columnCells.length; c++) {
          totalRow[c + rowProps + 1] = {
            type: 'measure',
            parent: columnCells[c] || null,
            values: [],
            counts: [],
            isTotal: rowsGrandTotalLabel.isTotal,
          };
        }

        // Add the grand-grand total if required
        if (showColumnTotals) {
          totalRow[columnGrandTotalIndex + rowProps + 1] = {
            type: 'measure',
            parent: null,
            values: [],
            counts: [],
            isTotal: rowsGrandTotalLabel.isTotal || realColumns.length > 0
          };
        }

        rows.push(totalRow);
      }

      // This is only useful if there is an AVG measure
      const countKeys = this.definition.Measures.map(m =>
        `count(${m.Path.split('/').map(e => e.trim()).join('/')})`);

      const columnsHash = columnsResult.hash;
      const rowHash = rowResult.hash;

      // Add nulls at the end if needed to represent the grand totals
      const realColumnsAndGrandTotal = showColumnTotals ? realColumns.concat([null]) : realColumns;
      const realRowsAndGrandTotal = showRowTotals ? realRows.concat([null]) : realRows;

      function normalize(value: any) {
        return isSpecified(value) ? value : null;
      }

      // Add the data points
      for (const g of s.result) {
        let currentColHash = columnsHash;
        for (const col of realColumnsAndGrandTotal) {
          let colIndex: number;
          if (!col) {
            // Grand total
            colIndex = columnGrandTotalIndex;
          } else {

            const colValueId = this.extractValueId(g, col);
            if (isSpecified(colValueId)) {
              currentColHash = currentColHash.values[colValueId];
            } else {
              currentColHash = currentColHash.undefined;
            }

            colIndex = currentColHash.cell.index;
          }

          let currentRowHash = rowHash;
          for (const row of realRowsAndGrandTotal) {
            let rowIndex: number;
            if (!row) {
              // Grand total
              rowIndex = rowGrandTotalIndex;
            } else {
              const rowValueId = this.extractValueId(g, row);
              if (isSpecified(rowValueId)) {
                currentRowHash = currentRowHash.values[rowValueId];
              } else {
                currentRowHash = currentRowHash.undefined;
              }

              rowIndex = currentRowHash.cell.index;
            }

            s.measures.forEach((m, index) => {
              const measureCell = rows[rowIndex][colIndex + rowProps + 1] as MeasureCell;
              const value = normalize(g[m.key]);
              const total = normalize(measureCell.values[index]);

              switch (m.aggregation) {
                case 'sum':
                case 'count':
                  measureCell.values[index] = total === null && value === null ? null : total + value;
                  break;
                case 'max':
                  measureCell.values[index] = total === null ? value : value === null ? total : total < value ? value : total;
                  break;
                case 'min':
                  measureCell.values[index] = total === null ? value : value === null ? total : total < value ? total : value;
                  break;
                case 'avg':
                  const countKey = countKeys[index];
                  const valueCount = (g[countKey] || 0) as number;
                  const totalCount = measureCell.counts[index] || 0;
                  measureCell.counts[index] = valueCount + totalCount;
                  measureCell.values[index] = total === null ? value : value === null ? total :
                    ((total * totalCount) + (value * valueCount)) / (totalCount + valueCount);
                  break;
              }
            });
          }
        }
      }

      // Finally... set the pivot
      s.pivot = {
        columnHeaders,
        rows
      };
    }

    return s.pivot;
  }

  public onExpandRow(d: DimensionCell): void {
    d.isExpanded = !d.isExpanded;
    this.recomputeVisibleRows();
  }

  public onExpandColumn(d: DimensionCell) {
    d.isExpanded = !d.isExpanded;
    this.recomputeColumnSpans();
  }

  private recomputeVisibleRows() {
    // This method makes a new copy of the rows WITHOUT the invisible
    // rows this is to support efficient virtual scrolling
    const rows = this.state.pivot.rows;

    // with a single sweep, recompute which columns are visible
    rows.forEach(row => {
      const cell = row[0];
      if (cell.type === 'dimension') {
        cell.isVisible = !cell.parent || (cell.parent.isExpanded && cell.parent.isVisible);
      }
    });

    // with another single sweep filter out invisible columns
    this._modifiedRows = rows.filter(e => e.length === 0 || e[0].type !== 'dimension' || (e[0] as DimensionCell).isVisible);
  }

  private recomputeColumnSpans() {
    const s = this.state;
    const cols = s.pivot.columnHeaders;
    const measureCount = s.measures.length;

    // Calculte the visibility of every cell and set colSpan to 0
    cols.forEach(row => row.forEach(cell => {
      cell.colSpan = 0;
      cell.isVisible = !cell.parent || (cell.parent.isVisible && cell.parent.isExpanded);
    }));

    // Calculate the colSpan of every cell's parent
    for (let i = cols.length - 1; i >= 0; i--) {
      for (let j = cols[i].length - 1; j >= 0; j--) {
        const cell = cols[i][j];
        cell.colSpan = cell.colSpan || measureCount || 1; // measureCount might be 0
        if (!!cell.parent && !!cell.parent.isExpanded) {
          cell.parent.colSpan += cell.colSpan;
        }
      }
    }

    // Calculate the maximum visible level
    this._maxVisibleLevel = -1;
    cols.forEach(row => row.forEach(cell => {
      if (cell.isVisible && cell.level > this._maxVisibleLevel) {
        this._maxVisibleLevel = cell.level;
      }
    }));

    // Calculate the rowSpan of every cell
    cols.forEach(row => row.forEach(cell => {
      cell.rowSpan = cell.type === 'label' || !cell.isExpanded ? this._maxVisibleLevel + 1 - cell.level : 1;
    }));

    // Calculate the spans of the top left corner cell
    this._colSpan = s.realRows.length === 0 ? 0 : 1; // TODO: if there is one row dimension and no column dimensions
    this._rowSpan = this._maxVisibleLevel + 1 + (this.showMeasureLabels ? 1 : 0);
  }

  public hasChildren(d: DimensionCell): boolean {
    return d.hasChildren;
  }

  public flipArrow(node: DimensionCell): string {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl && !node.isExpanded ? 'horizontal' : null;
  }

  public rotateArrow(node: DimensionCell): number {
    return node.isExpanded ? 90 : 0;
  }

  public isMeasureVisible(cell: MeasureCell) {
    return !cell.parent || cell.parent.isVisible && !cell.parent.isExpanded;
  }

  public trackByRow(_: number, row: any[]) {
    return row[0];
  }

  public trackByObject(_: number, obj: any) {
    return obj;
  }

  public paddingLeft(node: DimensionCell): string {
    return this.workspace.ws.isRtl ? '0' : (node.level * 30) + 'px';
  }

  public paddingRight(node: DimensionCell): string {
    return this.workspace.ws.isRtl ? (node.level * 30) + 'px' : '0';
  }

  public showHeaderRow(index: number) {
    return index <= this._maxVisibleLevel; // 0 in order to show the blank cell
  }

  private checkColumnFreshness() {
    if (this._currentColumns !== this.pivot.columnHeaders) {
      this._currentColumns = this.pivot.columnHeaders;
      this.recomputeColumnSpans();
    }
  }

  public get columnHeaders(): (DimensionCell | LabelCell)[][] {
    this.checkColumnFreshness();
    return this.state.pivot.columnHeaders;
  }

  public get rows(): (DimensionCell | LabelCell | MeasureCell)[][] {
    if (this._currentRows !== this.pivot.rows) {
      this._currentRows = this.pivot.rows;
      this.recomputeVisibleRows();
    }

    return this._modifiedRows;
  }

  public get showRowDimensionLabel(): boolean {
    // true when a descriptive label of the rows is displayed in the upper left corner instead of blank
    return false; // TODO
  }

  public get showMeasureLabels(): boolean {
    // The labels for the measures are hidden when there is only a
    // single measure unless when there are no column dimensions
    return !!this.rows && !!this.rows.length && this.measures.length !== 0 &&
      (this.measures.length > 1 || this.state.realColumns.length === 0);
  }

  public get rowSpan(): number {
    this.checkColumnFreshness();
    return this._rowSpan;
  }

  public get colSpan(): number {
    this.checkColumnFreshness();
    return this._colSpan;
  }

  public get measures(): MeasureInfo[] {
    return this.state.measures;
  }

  public onMeasureClick(cell: MeasureCell, rowIndex: number) {
    const col: DimensionCell = cell.parent;
    const row: DimensionCell = this.rows[rowIndex][0].type === 'dimension' ? this.rows[rowIndex][0] as DimensionCell : null;
    const atoms = this.computeFilterAtoms(col).concat(this.computeFilterAtoms(row));
    const filter = !!atoms.length ? atoms.reduce((f, atom) => f + ' and ' + atom) : null;
    this.drilldown(filter);
  }

  private drilldown(cellFilter: string) {
    const screenUrl = this.entityDescriptor.screenUrl;
    if (!!screenUrl) {
      const tenantId = this.workspace.ws.tenantId;
      const screenUrlSegments = screenUrl.split('/');

      // Prepare the filter
      const filter = this.state.filter;
      const combinedFilter =
        !!filter && !!cellFilter ? `${cellFilter} and (${filter})` :
          !!cellFilter ? cellFilter : !!filter ? filter : null;

      const params: Params = {
        inactive: true
      };

      if (!!combinedFilter) {
        params.filter = combinedFilter;
      }

      this.router.navigate(['app', tenantId + '', ...screenUrlSegments, params]);
    } else {
      const def = this.definition;
      console.error(`no screen URL is defined for collection: '${def.Collection}', definitionId '${def.DefinitionId}'`);
    }
  }

  public isDefined(cell: DimensionCell) {
    // IF this is false, a muted italic "(undefined)" is displayed instead
    return isSpecified(cell.valueId);
  }

  public get addRowExpanders() {
    return this.state.realRows.length > 1;
  }

  /////////////////// SUMMARY - CHARTS

  public get point(): string {
    // Point charts bind to this property
    const s = this.state;
    if (s.currentResultForPoint !== s.result) {
      s.currentResultForPoint = s.result;
      const measure = s.singleNumericMeasure;

      if (!s.result || !measure || s.result.length < 1) {
        s.point = null;
      } else if (s.uniqueDimensions.length === 0) {
        s.point = displayValue(s.result[0][measure.key], measure.desc, this.translate);
      } else {
        s.point = null;
      }
    }

    return s.point;
  }

  public get single(): SingleSeries {
    // Single series charts bind to this property
    const s = this.state;
    if (s.currentResultForSingle !== s.result || s.currentLangForSingle !== this.translate.currentLang) {
      s.currentResultForSingle = s.result;
      s.currentLangForSingle = this.translate.currentLang;
      const measure = s.singleNumericMeasure;

      if (!s.result || !measure) {
        s.single = null;
      } else if (s.uniqueDimensions.length === 1) {
        try {
          const dim = s.uniqueDimensions[0];
          const path = dim.path;
          const { propDesc, entityDesc } = dim;

          s.single = s.result.map(g => {
            const { value, valueId } = this.extractValueAndValueId(g, dim);
            const display = !isSpecified(valueId) ? this.translate.instant('Undefined') :
              !!entityDesc ? displayEntity(value, entityDesc) :
                displayValue(value, propDesc, this.translate);

            return {
              name: new ChartDimensionCell(display, path, valueId, propDesc, entityDesc),
              value: g[measure.key]
            };
          });
        } catch (ex) {
          s.reportStatus = ReportStatus.error;
          s.errorMessage = ex.message;
        }
      } else {
        s.single = null;
      }
    }

    return s.single;
  }

  public get multi(): MultiSeries {
    // Multi-series charts bind to this property
    const s = this.state;
    if (s.currentResultForMulti !== s.result || s.currentLangForMulti !== this.translate.currentLang) {
      s.currentResultForMulti = s.result;
      s.currentLangForMulti = this.translate.currentLang;
      const measure = s.singleNumericMeasure;

      if (!s.result || !measure) {
        s.multi = null;
      } else if (s.uniqueDimensions.length === 1) {
        // When the number of dimensions is just one, make the single-series pretend it's a multi-series
        const single = this.single;
        const label = this.firstDimensionLabel;
        const singletonDimension = new ChartDimensionCell(label, '', label, null, null);
        s.multi = [{
          name: singletonDimension,
          series: single
        }];
      } else if (s.uniqueDimensions.length === 2) {
        const dim = s.uniqueDimensions[0];
        const path = dim.path;
        const { propDesc, entityDesc } = dim;

        const dim2 = s.uniqueDimensions[1];
        const path2 = dim2.path;
        const { propDesc: propDesc2, entityDesc: entityDesc2 } = dim2;

        const valueToChildCollectionMap: { [id: string]: { cell: ChartDimensionCell, value: number }[] } = {};
        let undefinedChildCollectionMap: { cell: ChartDimensionCell, value: number }[];

        const rootCollection: ChartDimensionCell[] = [];

        for (const g of s.result) {
          const { value, valueId } = this.extractValueAndValueId(g, dim);
          let childCollection: { cell: ChartDimensionCell, value: number }[];
          if (isSpecified(valueId)) {
            childCollection = valueToChildCollectionMap[valueId];
          } else {
            childCollection = undefinedChildCollectionMap;
          }

          if (!childCollection) {
            const display = !isSpecified(valueId) ? this.translate.instant('Undefined') :
              !!entityDesc ? displayEntity(value, entityDesc) :
                displayValue(value, propDesc, this.translate);
            const dimensionCell = new ChartDimensionCell(display, path, valueId, propDesc, entityDesc);

            rootCollection.push(dimensionCell);
            childCollection = [];

            if (isSpecified(valueId)) {
              valueToChildCollectionMap[valueId] = childCollection;
            } else {
              undefinedChildCollectionMap = childCollection;
            }
          }

          const { value: value2, valueId: valueId2 } = this.extractValueAndValueId(g, dim2);
          const display2 = !isSpecified(valueId2) ? this.translate.instant('Undefined') :
            !!entityDesc2 ? displayEntity(value2, entityDesc2) :
              displayValue(value2, propDesc2, this.translate);

          childCollection.push({
            cell: new ChartDimensionCell(display2, path2, valueId2, propDesc2, entityDesc2),
            value: g[measure.key]
          });
        }

        s.multi = rootCollection.map(cell => {
          const childCollection = isSpecified(cell.valueId) ?
            valueToChildCollectionMap[cell.valueId] : undefinedChildCollectionMap;

          // Map children to their parents here
          childCollection.forEach(child => child.cell.parent = cell);

          return {
            name: cell,
            series: childCollection.map(cellValue => ({ name: cellValue.cell, value: cellValue.value }))
          };
        });
      } else {
        s.multi = null;
      }
    }

    return s.multi;
  }

  public formatChartDimension(d: ChartDimensionCell) {
    return d.display; // The chart labels
  }

  public get firstDimensionLabel() {
    const dimension = this.state.uniqueDimensions[0];
    return !!dimension ? dimension.label() : '';
  }

  public get secondDimensionLabel() {
    const dimension = this.state.uniqueDimensions[1];
    return !!dimension ? dimension.label() : '';
  }

  public get measureLabel() {
    const measure = this.state.singleNumericMeasure;
    return !!measure ? measure.label() : '';
  }

  public onChartSelect(point: ChartDimensionCell): void {
    if (!!point) {
      const atoms = this.computeFilterAtoms(point);
      const filter = !!atoms.length ? atoms.reduce((f, atom) => f + ' and ' + atom) : null;
      this.drilldown(filter);
    }
  }

  public onPointSelect(): void {
    this.drilldown(null);
  }

  public get numberOfDimensions(): number {
    return this.state.uniqueDimensions.length;
  }

  public get supportsPoint() {
    return this.numberOfDimensions === 0;
  }

  public get supportsSingle() {
    return this.numberOfDimensions === 1;
  }

  public get supportsMulti() {
    return this.numberOfDimensions === 2;
  }


  /////////////////// Details

  public get collection(): string {
    return !!this.definition ? this.definition.Collection : null;
  }

  public get definitionId(): string {
    return !!this.definition ? this.definition.DefinitionId : null;
  }

  public get select(): ReportSelectDefinitionForClient[] {
    return !!this.definition ? this.definition.Select : null;
  }

  public get entities(): Entity[] {
    return this.state.result;
  }

  public selectLabel(s: ReportSelectDefinitionForClient) {
    return this.workspace.current.getMultilingualValueImmediate(s, 'Label');
  }

  public onFlatSelect(entity: Entity): void {
    // tslint:disable-next-line:no-string-literal
    const id = entity['Id'];
    if (isSpecified(id)) {
      const screenUrl = this.entityDescriptor.screenUrl;
      if (!!screenUrl) {
        const tenantId = this.workspace.ws.tenantId;
        const screenUrlSegments = screenUrl.split('/');

        // Should we add the master parameters here?

        this.router.navigate(['app', tenantId + '', ...screenUrlSegments, id]);
      } else {
        const def = this.definition;
        console.error(`no screen URL is defined for collection: '${def.Collection}', definitionId '${def.DefinitionId}'`);
      }
    }
  }
}

/*
  [Reporting Framework TODO]
  - Implement the backend API
  - Add the State commands to the UI
  - Implement dimension properties
  - Add flat report totals
  - Support tree dimensions
  - Implement dashboard framework
  - Localize report definition error messages
  - Protect against huge number of pivot table columns
  - Protect against huge number of data points for charts
  - Handle mismatching of aggregation function and data type before SQL
  - Handle mismatching of aggregation function in the UI
  - Allow Top in Summary reports
  - Support Date parts in paths
  - Support special date filter operators
  - Add copy to clipboard to the fields list
  - Show validation on the remaining fields
  - Add the remaining parameter controls, especially the details pickers
  - The gauge chart displays total sum even when the aggregation is avg, max or min
  - Special handling for Lookups where the definitionId is required
  - Handle the bug in charts with duplicate names
  - Rename Select to Columns

  [Dimension Properties Steps]
  - Add field to ReportDimensionDefinition  (C#)
  - Add field to ReportDimensionDefinition  (TS)
  - Add field to ReportDimensionDefinitionForClient
  - Add field in edit modal
  - Account for it in computeSelect
  - Add collection in DimensionInfo
  - Add collection of DimensionCell in DimensionCell
  - Add popover to all column and row cells
  - In case of one row and no columns, add the properties as columns
*/
