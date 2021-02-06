import {
  Component, OnInit, Input, ChangeDetectionStrategy, ChangeDetectorRef,
  OnDestroy, OnChanges, SimpleChanges, Output, EventEmitter, ViewChild, ElementRef
} from '@angular/core';
import {
  WorkspaceService, ReportStatus, ReportStore, MultiSeries, SingleSeries, ReportArguments,
  PivotTable, MeasureCell, LabelCell, ChartDimensionCell, DimensionCell, AncestorGroup, HighlightClass
} from '~/app/data/workspace.service';
import { Subscription, Subject, Observable, of } from 'rxjs';
import {
  EntityDescriptor,
  metadata,
  PropVisualDescriptor,
  NumberPropVisualDescriptor,
  PercentPropVisualDescriptor,
  ChoicePropVisualDescriptor,
  DataType,
} from '~/app/data/entities/base/metadata';
import { TranslateService } from '@ngx-translate/core';
import { switchMap, tap, catchError, finalize, skip as skipObservable } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { isSpecified, csvPackage, downloadBlob, FriendlyError } from '~/app/data/util';
import { ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { Router, Params, ActivatedRoute, ParamMap } from '@angular/router';
import { displayScalarValue } from '~/app/data/util';
import { ChartType } from '~/app/data/entities/report-definition';
import { DimensionInfo, MeasureInfo, ParameterInfo, QueryexUtil, SelectInfo, UniqueAggregationInfo } from '~/app/data/queryex-util';
import { DeBracket, Queryex, QueryexColumnAccess, QueryexDirection, QueryexFunction } from '~/app/data/queryex';
import { DynamicRow, GetAggregateResponse } from '~/app/data/dto/get-aggregate-response';
import { GetFactResponse } from '~/app/data/dto/get-fact-response';

export enum ReportView {
  pivot = 'pivot',
  chart = 'chart'
}

/**
 * The color palette for the charts, which is just different shades of teal
 */
const palette = [
  // '#17A2B8',
  '#1490A3',
  '#128091',
  '#10707F',
  '#0D606D',
  // '#0B505B',
  // '#094049',
  // '#073036',
  // '#042024',
  // '#073036',
  // '#094049',
  '#0B505B',
  '#0D606D',
  '#10707F',
  '#128091',
  // '#1490A3',

  // '#80E0EF', '#C9F2F8', '#49D3E9',
  // '#1BC0DA', '#25CBE4', '#19B0C8',
];

const monochromeIndex = 1; // If we had to choose one of the above colors we choose the one with this index

const success = '#28a745';
const warning = '#ffc107';
const danger = '#dc3545';

/**
 * Hashes one dimension of an aggregate result for the pivot table
 */
interface PivotHash {
  cell: DimensionCell;
  ancestors?: { [id: number]: DimensionCell }; // the ancestors of all the children
  values?: { [value: string]: PivotHash };
  undefined?: PivotHash;
}

@Component({
  selector: 't-report-results',
  templateUrl: './report-results.component.html',
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReportResultsComponent implements OnInit, OnChanges, OnDestroy {

  static DEFAULT_PAGE_SIZE = 60;
  static CACHE_BUSTER = 1;

  public maximumColumns = 130; // If the report has more columns than this it will display an error

  public _rowAttributePlacement = ['bottom', 'top', 'bottom-left', 'top-left'];
  public _rowAttributePlacementRtl = ['bottom', 'top', 'bottom-right', 'top-right'];

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
  export: Observable<string>; // Must be set only once

  @Input()
  mode: 'screen' | 'preview' | 'dashboard' | 'embedded' = 'screen';

  @Input()
  disableDrilldown = false; // In popup mode we want to disable drilldown

  @Output()
  public exportStarting = new EventEmitter<void>();

  @Output()
  public exportSuccess = new EventEmitter<void>();

  @Output()
  public exportError = new EventEmitter<FriendlyError>();

  @Output()
  public orderbyChange = new EventEmitter<void>();

  @ViewChild('flatHeader', { static: false })
  flatHeader: ElementRef<HTMLTableRowElement>;

  private _subscriptions: Subscription;
  private notifyFetch$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense

  private orderbyKey: string;
  private orderbyDir: QueryexDirection;

  /**
   * This is a copy of the state pivot table rows, but with the
   * collapsed rows removed, to support smooth virtual scrolling
   */
  private _currentRows: DimensionCell[];
  private _modifiedRows: DimensionCell[];

  // NGX-Charts options
  animations = false;
  showXAxis = true;
  showYAxis = true;
  showXAxisLabel = true;
  showYAxisLabel = true;
  colorful = { domain: palette };
  monochromatic = { domain: [palette[monochromeIndex]] };
  heat = { domain: ['#96D5DF', '#052429'] }; // different shades of the same color for heat map

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService,
    private api: ApiService, private cdr: ChangeDetectorRef, private router: Router,
    private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.state = this.state || new ReportStore(); // if no state is provided
    const s = this.state;

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

    if (!!this.export) {
      this._subscriptions.add(this.export.pipe(
        tap(fileName => this.onExport(fileName)))
        .subscribe());
    }

    if (this.mode === 'screen') {
      const extractOrderByFields = (params: ParamMap) => {
        const orderbyUrl = params.get('$orderby');
        try {
          const orderbyExp = Queryex.parse(orderbyUrl, { expectDirKeywords: true });
          this.orderbyKey = orderbyExp[0].toString();
          this.orderbyDir = orderbyExp[0].direction || 'asc';
        } catch {
          delete this.orderbyKey;
          delete this.orderbyDir;
        }
      };

      extractOrderByFields(this.route.snapshot.paramMap);
      this._subscriptions.add(this.route.paramMap.pipe(skipObservable(1)).subscribe((params: ParamMap) => {
        extractOrderByFields(params);
        if (this.applyChanges()) {
          this.fetch();
        }
      }));
    }

    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);
    this.view = this.view || (!!this.definition.Chart && !!this.definition.DefaultsToChart ? ReportView.chart : ReportView.pivot);

    // Here we do the usual pattern of checking whether the state in the
    // singleton service is still the same as that supplied by the url
    // parameters in which case we do not have to fetch the data again
    const hasChanges = this.applyChanges();
    if (s.reportStatus !== ReportStatus.loaded || hasChanges) {
      try {
        const { filter, having, columns, rows, measures, select, parameters } =
          QueryexUtil.getReportInfos(this.definition, this.workspace, this.translate);

        const parameterInfos: { [keyLower: string]: ParameterInfo } = {};
        parameters.forEach(p => parameterInfos[p.keyLower] = p);

        s.parameterInfos = parameterInfos;
        s.filterExp = filter;
        s.havingExp = having;
        s.columnInfos = columns;
        s.rowInfos = rows;
        s.measureInfos = measures;
        s.uniqueMeasureAggregations = this.computeUniqueMeasureAggregations(measures);
        s.selectInfos = select;
        s.dimensionInfos = s.columnInfos.concat(s.rowInfos);
        s.singleNumericMeasureIndex = this.state.measureInfos.findIndex(m => m.isNumeric);
        s.badDefinition = false;

        this.fetch(); // Query the server
      } catch (ex) {
        // Clear everything and show the error message
        s.parameterInfos = null;
        s.filterExp = null;
        s.havingExp = null;
        s.columnInfos = null;
        s.rowInfos = null;
        s.measureInfos = null;
        s.uniqueMeasureAggregations = null;
        s.selectInfos = null;
        s.dimensionInfos = null;
        s.singleNumericMeasureIndex = -1;

        s.badDefinition = true;
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

    if (this.isDetails && this.canOrderByFlat) {
      if (this.orderbyKey !== s.orderbyKey) {
        s.orderbyKey = this.orderbyKey;
        hasChanged = true;
      }
      if (this.orderbyDir !== s.orderbyDir) {
        s.orderbyDir = this.orderbyDir;
        hasChanged = true;
      }
    }

    const urlArgs = this.arguments;
    const wsArgs = this.state.arguments;
    for (const key of Object.keys(urlArgs).concat(Object.keys(wsArgs))) {
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
    if (!coll) {
      return null;
    }

    const metadataFn = metadata[coll];
    if (!metadataFn) {
      return null;
    }

    const definitionId = this.definition.DefinitionId;
    return metadataFn(this.workspace, this.translate, definitionId);
  }

  get apiEndpoint(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.apiEndpoint : null;
  }

  public fetch(): void {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {

    let s = this.state;
    if (s.badDefinition) {
      // bad definition
      return of(null);
    }

    s.reportStatus = ReportStatus.loading;

    // FILTER & HAVING
    let filter: string;
    let having: string;
    try {
      filter = this.computeFilter(s);
      having = this.computeHaving(s);
    } catch (e) {
      s.reportStatus = ReportStatus.error;
      s.errorMessage = e.message;
      return of(null);
    }

    // This will show the spinner
    this.cdr.markForCheck();

    let obs$: Observable<any>;
    if (this.showDetails) {
      const top = this.definition.Top || ReportResultsComponent.DEFAULT_PAGE_SIZE;
      const skip = !!this.definition.Top ? 0 : s.skip;

      // Prepare the select and orderby
      let select: string;
      let orderby: string;
      try {
        select = this.computeSelect(s);
        orderby = this.computeOrderBy(s);
        if (!select) {
          s.reportStatus = ReportStatus.information;
          s.information = () => this.translate.instant('DragSelect');

          return of(null);
        }
      } catch (e) {
        s.reportStatus = ReportStatus.error;
        s.errorMessage = e.message;
        return of(null);
      }

      // To prevent the jarring movemenet of headers when you refresh or orderby
      this.rememberFlatColumnWidths();

      obs$ = this.crud.getFact({
        top,
        skip,
        orderby,
        select,
        filter,
        countEntities: true
      }).pipe(
        tap((response: GetFactResponse) => {
          s.reportStatus = ReportStatus.loaded;
          s.top = response.Result.length;
          s.skip = skip;
          s.total = response.TotalCount;
          s.result = response.Result;

          this.forgetFlatColumnWidths();
        })
      );
    } else { // Show Summary or Chart
      // SELECT
      let select: string;
      try {
        select = this.computeAggregateSelect(s);
        if (!select) {
          s.reportStatus = ReportStatus.information;
          s.information = () => this.translate.instant('DragDimensionsOrMeasures');

          return of(null);
        }
      } catch (e) {
        s.reportStatus = ReportStatus.error;
        s.errorMessage = e.message;
        return of(null);
      }

      // TOP
      const top = this.definition.Top;
      obs$ = this.crud.getAggregate({
        top,
        select,
        filter,
        having
      }).pipe(
        tap((response: GetAggregateResponse) => {
          s.reportStatus = ReportStatus.loaded;
          s.result = response.Result;

          // Add the ancestors in fast-to-lookup data structures
          s.ancestorGroups = {};
          for (const da of response.DimensionAncestors || []) {
            const ancestors: { [id: number]: DynamicRow } = {};
            for (const g of da.Result) {
              const id = g[da.IdIndex - da.MinIndex];
              ancestors[id] = g;
            }

            s.ancestorGroups[da.IdIndex] = { minIndex: da.MinIndex, ancestors };
          }
        })
      );
    }

    return obs$.pipe(
      catchError((friendlyError) => {
        s = this.state; // get the source
        s.reportStatus = ReportStatus.error;
        s.errorMessage = friendlyError.error;
        return of(null);
      }),
      finalize(() => this.cdr.markForCheck())
    );
  }

  private computeUniqueMeasureAggregations(measures: MeasureInfo[]): UniqueAggregationInfo[] {
    const tracker: { [stringified: string]: number } = {};
    const uniqueAggregations: { exp: QueryexFunction }[] = [];

    function add(aggregation: QueryexFunction): number {
      const aggString = aggregation.toString();
      let uniqueIndex = tracker[aggString];
      if (uniqueIndex === undefined) {
        uniqueIndex = uniqueAggregations.length;
        tracker[aggString] = uniqueIndex;
        uniqueAggregations.push({ exp: aggregation });
      }

      return uniqueIndex;
    }

    // First gather all the aggregations
    const allAggregations: QueryexFunction[] = [];
    for (const measure of measures) {
      measure.exp.aggregationsInner(allAggregations);
      if (!!measure.success) {
        measure.success.aggregationsInner(allAggregations);
      }

      if (!!measure.warning) {
        measure.warning.aggregationsInner(allAggregations);
      }

      if (!!measure.danger) {
        measure.danger.aggregationsInner(allAggregations);
      }
    }

    // Then use the tracker to get a array of unique aggregations and tag the original aggregation with the unique index
    for (const aggregation of allAggregations) {
      // Average is split into Count and Sum
      if (aggregation.nameLower === 'avg') {
        const sum = aggregation.clone() as QueryexFunction;
        sum.setName('sum');
        aggregation.sumIndex = add(sum);

        const count = aggregation.clone() as QueryexFunction;
        count.setName('count');
        aggregation.countIndex = add(count);
      } else {
        aggregation.index = add(aggregation);
      }
    }

    return uniqueAggregations;
  }

  private computeFilter(s?: ReportStore): string {
    if (!this.definition) {
      return '';
    }

    s = s || this.state;
    return s.filterExp ? DeBracket(QueryexUtil.stringify(s.filterExp, this.arguments, s.parameterInfos)) : null;
  }

  private computeHaving(s?: ReportStore): string {
    if (!this.definition) {
      return '';
    }

    s = s || this.state;
    return s.havingExp ? DeBracket(QueryexUtil.stringify(s.havingExp, this.arguments, s.parameterInfos)) : null;
  }

  private computeOrderBy(s?: ReportStore): string {
    s = s || this.state;
    if (this.isDetails && this.canOrderByFlat && !!s.orderbyKey) {
      // Prepare the order by based on the selected column
      const info = s.selectInfos.find(e => e.expToString === s.orderbyKey);
      const orderDirection = this.orderDirection(info);
      if (!!orderDirection) {
        const orderbyBase = QueryexUtil.stringify(info.exp, this.arguments, s.parameterInfos);
        if (!!info.entityDesc) {
          return info.entityDesc.orderby().map(e => `${orderbyBase}.${e} ${orderDirection}`).join(',');
        } else {
          if (info.localize) {
            const ws = this.workspace.currentTenant;
            const lang = ws.isSecondaryLanguage ? 2 : ws.isTernaryLanguage ? 3 : 1;

            const orderbySecondary = QueryexUtil.stringify(info.exp, this.arguments, s.parameterInfos, lang);
            if (orderbyBase === orderbySecondary) {
              return orderbyBase;
            } else {
              return `${orderbySecondary} ${orderDirection},${orderbyBase} ${orderDirection}`;
            }
          } else {
            return `${orderbyBase} ${orderDirection}`;
          }
        }
      }
    }

    // Return default from report definition
    return this.definition.OrderBy;
  }

  private computeSelect(s?: ReportStore): string {

    if (!this.definition) {
      return '';
    }

    s = s || this.state;
    const selects = s.selectInfos;
    if (!selects || selects.length === 0) {
      return '';
    }

    const args = this.arguments;
    const infos = s.parameterInfos;

    const atomsTracker: { [atom: string]: number } = {};
    const atoms: string[] = [];

    function addAtom(atom: string): number {
      atom = DeBracket(atom);
      let index = atomsTracker[atom];
      if (index === undefined) {
        index = atoms.length;
        atomsTracker[atom] = index;
        atoms.push(atom);
      }

      return index;
    }

    // First we add a few hidden selects, that are needed for navigate-to-details
    const baseEntityDesc = this.entityDescriptor;
    if (!!baseEntityDesc.properties.Id) {
      s.idIndex = addAtom('Id');
    } else {
      s.idIndex = -1;
    }

    if (!baseEntityDesc.definitionId && !!baseEntityDesc.definitionIds) {
      s.defIdIndex = addAtom('DefinitionId');
    } else {
      s.defIdIndex = -1;
    }

    if (!!baseEntityDesc.navigateToDetailsSelect) {
      s.navigateToDetailsIndices = baseEntityDesc.navigateToDetailsSelect.map(select => addAtom(select));
    } else {
      delete s.navigateToDetailsIndices;
    }

    // Second we add the selects specified by the user
    for (const select of selects) {
      let indices: number[] = [];
      const { exp, localize, entityDesc } = select;
      if (!!entityDesc) {
        const attString = QueryexUtil.stringify(exp, args, infos);
        for (const selectProp of entityDesc.select) {
          indices.push(addAtom(attString + '.' + selectProp));
        }
      } else {
        for (const lang of localize ? [1, 2, 3] : [1]) {
          const attString = QueryexUtil.stringify(exp, args, infos, lang as 1 | 2 | 3);
          indices.push(addAtom(attString));
        }

        if (indices.every(i => i === indices[0])) {
          indices = [indices[0]];
        }
      }
      select.indices = indices;
    }

    // Return Result
    return atoms.join(',');
  }

  private computeAggregateSelect(s?: ReportStore): string {

    if (!this.definition) {
      return '';
    }

    s = s || this.state;

    const args = this.arguments;
    const infos = s.parameterInfos;

    const atomsTracker: { [atom: string]: number } = {};
    const atoms: string[] = [];

    function addAtom(atom: string): number {
      atom = DeBracket(atom);
      let index = atomsTracker[atom];
      if (index === undefined) {
        index = atoms.length;
        atomsTracker[atom] = index;
        atoms.push(atom);
      }

      return index;
    }

    const cols = s.columnInfos;
    const rows = s.rowInfos;

    // (1) Add dimensions (special handing for nav column accesses)
    for (const dimInfo of cols.concat(rows)) {
      const { keyExp, entityDesc, dispExp, localize } = dimInfo;

      const keyString = QueryexUtil.stringify(keyExp, args, infos);
      if (!!entityDesc) {
        dimInfo.keyIndex = addAtom(keyString + '.Id');

        if (!!dispExp) {
          let indices: number[] = [];
          for (const lang of localize ? [1, 2, 3] : [1]) {
            const dispString = QueryexUtil.stringify(dispExp, args, infos, lang as 1 | 2 | 3, keyString);
            indices.push(addAtom(dispString));
          }

          if (indices.every(i => i === indices[0])) {
            indices = [indices[0]];
          }
          dimInfo.indices = indices;
        } else {
          // Get the select from metadata
          dimInfo.indices = [];
          for (const selectProp of entityDesc.select) {
            dimInfo.indices.push(addAtom(keyString + '.' + selectProp));
          }
        }

        for (const attInfo of dimInfo.attributes) {
          attInfo.indices = [];
          const { exp: attExp, localize: attLocalize, entityDesc: attEntityDesc } = attInfo;
          if (!!attEntityDesc) {
            attInfo.indices = [];
            const attString = QueryexUtil.stringify(attExp, args, infos, 1, keyString);
            for (const selectProp of attEntityDesc.select) {
              attInfo.indices.push(addAtom(attString + '.' + selectProp));
            }
          } else {
            let indices: number[] = [];
            for (const lang of attLocalize ? [1, 2, 3] : [1]) {
              const attString = QueryexUtil.stringify(attExp, args, infos, lang as 1 | 2 | 3, keyString);
              indices.push(addAtom(attString));
            }

            if (indices.every(i => i === indices[0])) {
              indices = [indices[0]];
            }
            attInfo.indices = indices;
          }
        }

        if (dimInfo.showAsTree) {
          // This is the clue for the server to include all the ancestors of keyExp
          dimInfo.parentKeyIndex = addAtom(keyString + '.ParentId');
        }
      } else {
        dimInfo.keyIndex = addAtom(keyString);
      }
    }

    // (2) Add the measures
    for (const aggregationInfo of s.uniqueMeasureAggregations) {
      const exp = aggregationInfo.exp;
      const aggString = QueryexUtil.stringify(exp, args, infos);
      aggregationInfo.index = addAtom(aggString);
    }

    // Return result
    return atoms.join(',');
  }

  public onExport(fileName?: string): void {
    // This function exports the pivot table to a CSV file, the way it would look fully expanded
    try {
      const s = this.state;
      if (s.reportStatus === ReportStatus.loaded) {

        if (this.isDetails) {
          const data: string[][] = [];

          const createAndDownload = (flat: any[][]) => {
            const selectInfos = s.selectInfos;

            // Add the header
            {
              const dataRow: string[] = [];
              data.push(dataRow);
              for (const info of selectInfos) {
                dataRow.push(info.label());
              }
            }

            // Add the rows
            for (const row of flat) {
              const dataRow: string[] = [];
              data.push(dataRow);

              for (let i = 0; i < selectInfos.length; i++) {
                const info = selectInfos[i];
                const display = this.displayValue(row[i], info.desc, info.entityDesc);
                dataRow.push(display);
              }
            }

            this.downloadData(data, fileName);
            this.exportSuccess.emit();
          };

          if (!!this.definition.Top) {
            // All the data we need is already loaded, download immediately
            createAndDownload(this.flat);
          } else {
            // Flat report with paging => query the server to get all the data

            // Query parameters
            let filter: string;
            let select: string;
            let orderby: string;
            try {
              filter = this.computeFilter(s);
              select = this.computeSelect();
              orderby = this.computeOrderBy();
            } catch (ex) {
              this.exportError.emit(ex);
              return;
            }

            if (!select) {
              const msg = this.translate.instant('DragSelect');
              this.exportError.emit(msg);
              return;
            }

            this.exportStarting.emit();
            this.crud.getFact({
              top: 2147483647,
              skip: 0,
              orderby,
              select,
              filter
            }).pipe(
              tap((response: GetFactResponse) => {
                const flat = this.flatInner(response.Result, s);
                createAndDownload(flat);
                this.exportSuccess.emit();
              }),
              catchError((err: FriendlyError) => {
                this.exportError.emit(err.error);
                return of();
              })
            ).subscribe();
          }

        } else if (this.isSummary) {
          const data: string[][] = [];

          // Summary report => no need to query the server
          const { columnHeaders, columns, rows, colSpan, columnsGrandTotalLabel, rowsGrandTotalLabel, rowsGrandTotalMeasures } = s.pivot;
          const realMeasures = s.measureInfos;

          // Grab some useful metadata
          const singleRowDimension = this.singleRowDimension;
          const singleRowDimensionAttributes = singleRowDimension ? singleRowDimension.attributes : [];

          // column Headers
          const rowSpanned: { [colIndex: number]: true } = {};
          let isFirstRow = true;
          for (const headerRow of columnHeaders) {
            const dataRow = [];
            data.push(dataRow);

            // Blank Cell
            for (let i = 0; i < colSpan; i++) {
              dataRow.push(null);
            }

            // Attributes #1
            if (singleRowDimension) {
              if (isFirstRow) {
                for (const att of singleRowDimensionAttributes) {
                  dataRow.push(att.label());
                }
              } else {
                for (const _ of singleRowDimensionAttributes) {
                  dataRow.push(null);
                }
              }
            }

            // Column Headers
            for (const colCell of headerRow) {
              // (1) If the column is occupied by a cell from an upper row with a rowSpan, account for it
              while (rowSpanned[dataRow.length]) {
                dataRow.push(null);
              }

              // (2) If this cell has a row span, flag the index in the rowSpanned so subsequent rows account for it
              if (colCell.rowSpan > 1) {
                rowSpanned[dataRow.length] = true;
              }

              // (3) Add the cell's display
              let display: string;
              if (this.isDefined(colCell)) {
                display = this.displayValue(colCell.value, colCell.info.desc, colCell.info.entityDesc);
              } else {
                display = this.translate.instant('Undefined');
              }
              dataRow.push(display);

              // (4) Add colSpan if any
              for (let i = 1; i < colCell.expandedColSpan; i++) {
                dataRow.push(null);
              }
            }

            // Columns Grand Total
            if (isFirstRow && columnsGrandTotalLabel) {
              dataRow.push(columnsGrandTotalLabel.label());
            }

            isFirstRow = false;
          }

          // Measure labels
          if (this.showMeasureLabelsRow) {
            const dataRow: string[] = [];
            data.push(dataRow);

            // Blank cell in upper left corner
            for (let i = 0; i < colSpan; i++) {
              dataRow.push(null);
            }

            // Attributes #2
            if (singleRowDimension) {
              if (isFirstRow) {
                for (const att of singleRowDimensionAttributes) {
                  dataRow.push(att.label());
                }
              } else {
                for (const _ of singleRowDimensionAttributes) {
                  dataRow.push(null);
                }
              }
            }

            // Measure Labels
            for (const cell of columns) {
              if (cell.immediateChildren.length === 0) {
                for (const measure of realMeasures) {
                  dataRow.push(measure.label());
                }
              }
            }

            // Columns Grand Totals Measure Labels
            if (this.showColumnsGrandTotalsMeasureLabels) {
              for (const measure of realMeasures) {
                dataRow.push(measure.label());
              }
            }
          }

          // Rows
          for (const rowCell of rows) {
            const dataRow: string[] = [];
            data.push(dataRow);

            // Add the row label
            {
              let display = '';
              for (let i = 0; i < rowCell.level; i++) {
                display += '            ';
              }
              if (this.isDefined(rowCell)) {
                display += this.displayValue(rowCell.value, rowCell.info.desc, rowCell.info.entityDesc);
              } else {
                display += this.translate.instant('Undefined');
              }

              dataRow.push(display);
            }

            // Add the attributes
            if (singleRowDimension) {
              for (const att of rowCell.attributes) {
                const display = this.displayValue(att.value, att.info.desc, att.info.entityDesc);
                dataRow.push(display);
              }
            }

            // Add the measures
            for (const cell of rowCell.measures) {
              if (!cell.column || cell.column.immediateChildren.length === 0) {
                for (let i = 0; i < realMeasures.length; i++) {
                  const display = displayScalarValue(cell.values[i], realMeasures[i].desc, this.workspace, this.translate);
                  dataRow.push(display);
                }

                if (realMeasures.length === 0) {
                  dataRow.push(null); // empty cell
                }
              }
            }
          }

          // Rows Grand Total
          if (rowsGrandTotalMeasures) {
            const dataRow: string[] = [];
            data.push(dataRow);

            // Rows Grand Total label
            if (rowsGrandTotalLabel) {
              dataRow.push(rowsGrandTotalLabel.label());
            }

            // Empty attributes cells
            if (singleRowDimension) {
              for (const _ of singleRowDimensionAttributes) {
                dataRow.push(null);
              }
            }

            // Rows Grand Total measures
            for (const cell of rowsGrandTotalMeasures) {
              if (!cell.column || cell.column.immediateChildren.length === 0) {
                for (let i = 0; i < realMeasures.length; i++) {
                  const display = displayScalarValue(cell.values[i], realMeasures[i].desc, this.workspace, this.translate);
                  dataRow.push(display);
                }
              }
            }
          }

          this.downloadData(data, fileName);
          this.exportSuccess.emit();
        } else {
          // Nothing to download
          return;
        }
      }
    } catch (err) {
      this.exportError.emit(err);
    }
  }

  private downloadData(data: string[][], fileName: string) {

    // Calculate maxCols;
    let maxCols = 0;
    for (const row of data) {
      if (row.length > maxCols) {
        maxCols = row.length;
      }
    }

    // If there are labels at the end, this pads all the rows underneath it
    for (const dataRow of data) {
      while (dataRow.length < maxCols) {
        dataRow.push(null);
      }
    }

    // Download
    const csvBlob = csvPackage(data);
    downloadBlob(csvBlob, fileName || this.translate.instant('Report') + '.csv');
  }

  // UI Bindings

  public get isDetails(): boolean {
    return !!this.definition && this.definition.Type === 'Details';
  }

  public get isSummary(): boolean {
    return !!this.definition && this.definition.Type === 'Summary';
  }

  public get showDetails(): boolean {
    return this.isDetails;
  }

  public get showSummary(): boolean {
    return this.isSummary && this.view === ReportView.pivot;
  }

  public get showChart(): boolean {
    return this.isSummary && this.view === ReportView.chart && this.state.singleNumericMeasureIndex !== -1;
  }

  public get showSpecifyNumericMeasure(): boolean {
    return this.isSummary && this.view === ReportView.chart && this.state.singleNumericMeasureIndex === -1;
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

  private extractValue(g: DynamicRow, dimension: DimensionInfo, offset = 0): any {
    if (!!dimension.entityDesc) {
      // The value is the select array (either from localized display expression or from the entity desc select)
      return dimension.indices.map(i => g[i - offset]);
    } else {
      return g[dimension.keyIndex - offset];
    }
  }

  public get pivot(): PivotTable {
    // The various parts of the pivot table bind to what is returned by this property
    const s = this.state;
    const def = this.definition;
    const ws = this.workspace.currentTenant;

    if (s.currentResultForPivot !== s.result) {
      s.currentResultForPivot = s.result;

      // Some booleans related to column and row totals
      const realColumns = s.columnInfos;
      const showColumnsTotalsLabel = realColumns.length > 0;
      const showColumnsTotals: boolean = s.measureInfos.length > 0 &&
        (def.ShowColumnsTotal || realColumns.length === 0);

      const realRows = s.rowInfos;
      const showRowsTotalsLabel = realRows.length > 0;
      const showRowsTotals: boolean = s.measureInfos.length > 0 &&
        (def.ShowRowsTotal || realRows.length === 0);

      // Helper function that builds hashes mapping every combination of dimensions to a dimension index
      const dimensionHash = (dimensionInfos: DimensionInfo[], columns?: DimensionCell[]): { hash: PivotHash, roots: DimensionCell[] } => {

        const rootHash: PivotHash = { cell: null }; // cell == null is the root
        const roots: DimensionCell[] = [];

        const addEmptyMeasures = (cell: DimensionCell) => {
          if (columns) {
            // When columns are supplied it means these are rows, so we fill the measures array
            cell.measures = [];
            const howMany = showColumnsTotals ? columns.length + 1 : columns.length;
            for (let i = 0; i < howMany; i++) {
              const column = columns[i];
              cell.measures.push({
                aggValues: [],
                values: [],
                classes: [],
                column, // undefined means columns grand total
                row: cell,
                isTotal: showColumnsTotalsLabel && !column
              });
            }
          }
        };

        // Links the parent (and all its ancestors) to the previous hash and returns it
        const addAncestor = (
          parentId: number,
          dimInfo: DimensionInfo,
          prevHash: PivotHash,
          ancestorGroup: AncestorGroup): DimensionCell => {
          let parent: DimensionCell;
          if (!parentId) {
            parent = prevHash.cell;
          } else {
            parent = prevHash.ancestors[parentId];
            if (!parent) {
              const gAncestor = ancestorGroup.ancestors[parentId]; // if this is undefined, the server didn't do its job right
              const valueParent = this.extractValue(gAncestor, dimInfo, ancestorGroup.minIndex);
              const grandParentId = gAncestor[dimInfo.parentKeyIndex - ancestorGroup.minIndex];
              const grandParent = addAncestor(grandParentId, dimInfo, prevHash, ancestorGroup);
              const prevHashLevel = prevHash.cell ? prevHash.cell.level : -1;
              const level = grandParent ? grandParent.level + 1 : 0;
              parent = { // This one is an ancestor, must flag it
                info: dimInfo,
                value: valueParent,
                valueId: parentId,
                isExpanded: dimInfo.autoExpandLevel > (level - prevHashLevel) - 1, // adjusted in flatten
                level: grandParent ? grandParent.level + 1 : 0,
                index: 0, // computed later in flatten()
                immediateParent: grandParent,
                parent: prevHash.cell,
                immediateChildren: [],
                attributes: []
              };

              for (const info of dimInfo.attributes) {
                // Add the attribute value
                let attValue: any;
                if (info.entityDesc || info.indices.length > 1) {
                  attValue = info.indices.map(i => gAncestor[i - ancestorGroup.minIndex]);
                } else {
                  const i = info.indices[0];
                  attValue = gAncestor[i - ancestorGroup.minIndex];
                }
                parent.attributes.push({ value: attValue, info });

                // Set the sortValue
                if (info.isOrdered) {
                  parent.sortValue = this.sortValue(attValue, info.desc, info.entityDesc);
                }
              }

              if (dimInfo.isOrdered) {
                parent.sortValue = this.sortValue(parent.value, dimInfo.desc, dimInfo.entityDesc);
              }

              // Add the cell to its parent
              (grandParent ? grandParent.immediateChildren : roots).push(parent);

              addEmptyMeasures(parent); // Adds measures array if 'columns' are supplied
              prevHash.ancestors[parentId] = parent; // For future re-use
            }

            parent.isAncestor = true; // For drilldown
          }

          return parent;
        };

        if (dimensionInfos.length > 0) {
          for (const g of s.result) {
            let prevHash = rootHash;
            for (const dimInfo of dimensionInfos) {
              const valueId = g[dimInfo.keyIndex];
              const value = this.extractValue(g, dimInfo); // Could be array
              let currentHash: PivotHash;

              // Either go down the values or the undefined route
              if (isSpecified(valueId)) {
                if (!prevHash.values) {
                  prevHash.values = {};
                }

                currentHash = prevHash.values[valueId];
              } else {
                currentHash = prevHash.undefined;
              }

              // This section determines the parent of the cell we're about to create
              if (!currentHash) {
                let parent: DimensionCell;
                if (dimInfo.showAsTree) {

                  // If it's a tree dimension, make sure to initialize the ancestors dictionary on the previous hash
                  if (!prevHash.ancestors) {
                    prevHash.ancestors = {};
                  }

                  const ancestorGroup = s.ancestorGroups[dimInfo.keyIndex];
                  if (ancestorGroup.ancestors[valueId]) {
                    // The cell itself appears elswhere as an ancestor => we add it underneath itself
                    // And add its parent-self in the ancestors dictionary. This way parents always are
                    // the aggregation of their children and the children are homogenous (no currencies between centers)
                    parent = addAncestor(valueId, dimInfo, prevHash, ancestorGroup);
                  } else {
                    // The cell is not an ancestor of another cell, no need to add it underneath itself
                    const parentId = g[dimInfo.parentKeyIndex];
                    parent = addAncestor(parentId, dimInfo, prevHash, ancestorGroup);
                  }
                } else {
                  // Flat dimension, the parent is the cell from the previous dimension
                  parent = prevHash.cell;
                }

                const level = parent ? parent.level + 1 : 0;
                const prevHashLevel = prevHash.cell ? prevHash.cell.level : -1;
                const cell: DimensionCell = {
                  info: dimInfo,
                  value,
                  valueId,
                  isExpanded: dimInfo.autoExpandLevel > (level - prevHashLevel) - 1, // adjusted in flatten
                  level,
                  index: 0, // Computed later in flatten()
                  immediateParent: parent,
                  parent: prevHash.cell,
                  immediateChildren: [],
                  attributes: []
                };

                for (const info of dimInfo.attributes) {
                  // Add the attribute value
                  let attValue: any;
                  if (info.entityDesc || info.indices.length > 1) {
                    attValue = info.indices.map(i => g[i]);
                  } else {
                    const i = info.indices[0];
                    attValue = g[i];
                  }
                  cell.attributes.push({ value: attValue, info });

                  // Set the sortValue
                  if (info.isOrdered) {
                    cell.sortValue = this.sortValue(attValue, info.desc, info.entityDesc);
                  }
                }

                if (dimInfo.isOrdered) {
                  cell.sortValue = this.sortValue(cell.value, dimInfo.desc, dimInfo.entityDesc);
                }

                // Add the cell to its parent
                (parent ? parent.immediateChildren : roots).push(cell);

                // Adds measures array if 'columns' are supplied
                addEmptyMeasures(cell);

                // Set the current hash
                currentHash = { cell };

                // Set the current hash in the correct category inside previous hash
                if (isSpecified(valueId)) {
                  prevHash.values[valueId] = currentHash;
                } else {
                  prevHash.undefined = currentHash;
                }
              }

              // For the next iteration
              prevHash = currentHash;
            }
          }
        }

        return { hash: rootHash, roots };
      };

      // This recursive function adds the dimension cells
      // in order, parents always before children
      function flatten(cells: DimensionCell[], array: DimensionCell[], index = 0): number {

        // Do the sorting here
        const orderDir = cells[0].info.orderDir;
        switch (orderDir) {
          case 'asc':
            cells.sort((a, b) => a.sortValue > b.sortValue ? 1 : a.sortValue < b.sortValue ? -1 : 0);
            break;
          case 'desc':
            cells.sort((a, b) => a.sortValue < b.sortValue ? 1 : a.sortValue > b.sortValue ? -1 : 0);
            break;
        }

        // Then the flattening
        for (const cell of cells) {
          cell.index = index++;
          array.push(cell);
          if (!!cell.immediateChildren && cell.immediateChildren.length > 0) {
            index = flatten(cell.immediateChildren, array, index);
          } else {
            cell.isExpanded = false;
          }
        }

        return index;
      }

      /////////// Calculate the top half of the report (the "columnHeaders")
      const { hash: columnsHash, roots: columnRoots } = dimensionHash(realColumns);

      // Prepare a flat and sorted list of column cells (parents before children)
      const columnCells: DimensionCell[] = [];
      if (columnRoots.length > 0) {
        flatten(columnRoots, columnCells); // fills columnCells
      }

      // Add them in a 2-D columnHeaders array, each according to its level
      const columnHeaders: DimensionCell[][] = [];
      for (const cell of columnCells) {
        if (!columnHeaders[cell.level]) {
          columnHeaders[cell.level] = [cell];
        } else {
          columnHeaders[cell.level].push(cell);
        }
      }

      let columnsGrandTotalLabel: LabelCell;
      if (showColumnsTotals && showColumnsTotalsLabel) {
        columnsGrandTotalLabel = {
          label: !!def.ColumnsTotalLabel ?
            () => ws.localize(def.ColumnsTotalLabel, def.ColumnsTotalLabel2, def.ColumnsTotalLabel3) :
            () => this.translate.instant('GrandTotal'),
          isTotal: showColumnsTotalsLabel
        };
      }

      /////////// Calculate the bottom half of the report (the "rows")
      const { hash: rowsHash, roots: rowsRoots } = dimensionHash(realRows, columnCells);

      let rowsGrandTotalLabel: LabelCell;
      let rowsGrandTotalMeasures: MeasureCell[];
      if (showRowsTotals) {
        // Add the label
        if (showRowsTotalsLabel) {
          // The rows totals label is only visible when there is at least one row dimension
          rowsGrandTotalLabel = {
            label: !!def.RowsTotalLabel ?
              () => ws.localize(def.RowsTotalLabel, def.RowsTotalLabel2, def.RowsTotalLabel3) :
              () => this.translate.instant('GrandTotal'),
            isTotal: showRowsTotalsLabel
          };
        }

        // Add the row total measures
        rowsGrandTotalMeasures = [];
        const howManyMeasures = showColumnsTotals ? columnCells.length + 1 : columnCells.length;
        for (let i = 0; i < howManyMeasures; i++) {
          rowsGrandTotalMeasures.push({
            aggValues: [],
            values: [],
            classes: [],
            column: columnCells[i], // null means grand total
            row: null,
            isTotal: showRowsTotalsLabel
          });
        }
      }

      function normalize(value: any) {
        return isSpecified(value) ? value : null;
      }

      // Add the data points and their aggregations
      if (s.uniqueMeasureAggregations.length > 0) {
        for (const g of s.result) {
          let colIndex = columnCells.length; // Columns grand totals index
          let currentColHash = columnsHash;
          for (const col of realColumns) {
            const valueId = g[col.keyIndex];
            currentColHash = isSpecified(valueId) ?
              currentColHash.values[valueId] : currentColHash.undefined;

            colIndex = currentColHash.cell.index;
          }

          let measuresRow: MeasureCell[] = rowsGrandTotalMeasures; // Rows grand totals row
          let dimRow: DimensionCell;
          let currentRowHash = rowsHash;
          for (const row of realRows) {
            const valueId = g[row.keyIndex];
            currentRowHash = isSpecified(valueId) ?
              currentRowHash.values[valueId] :
              currentRowHash.undefined;

            dimRow = currentRowHash.cell as DimensionCell;
            dimRow.measures = dimRow.measures || [];
            measuresRow = dimRow.measures;
          }

          // Climb up the rows
          let currentRow = measuresRow;
          while (!!currentRow) {
            let currentColIndex = colIndex;
            let cell = currentRow[currentColIndex];
            // Climb up the columns
            while (!!cell) {
              s.uniqueMeasureAggregations.forEach((aggInfo, index) => {
                const { exp: aggFunction, index: selectIndex } = aggInfo;
                const value = normalize(g[selectIndex]);
                const total = normalize(cell.aggValues[index]);

                switch (aggFunction.nameLower) {
                  case 'sum':
                  case 'count':
                    cell.aggValues[index] = total === null && value === null ? null : total + value;
                    break;
                  case 'max':
                    cell.aggValues[index] = total === null ? value : value === null ? total : total < value ? value : total;
                    break;
                  case 'min':
                    cell.aggValues[index] = total === null ? value : value === null ? total : total < value ? total : value;
                    break;
                }
              });

              if (cell.column && cell.column.immediateParent) {
                currentColIndex = cell.column.immediateParent.index;
                cell = currentRow[currentColIndex];
              } else if (currentColIndex !== columnCells.length) {
                currentColIndex = columnCells.length; // Columns grand totals index
                cell = currentRow[currentColIndex];
              } else {
                break; // break the column-wise loop
              }
            }

            const leafCell = currentRow[colIndex];
            if (leafCell.row && leafCell.row.immediateParent) {
              currentRow = leafCell.row.immediateParent.measures;
            } else if (currentRow !== rowsGrandTotalMeasures) {
              currentRow = rowsGrandTotalMeasures; // Rows grand totals row
            } else {
              break; // break the row-wise loop
            }
          }
        }
      }

      if (s.measureInfos.length > 0) {

        // Evaluate cell values and classes from the aggregation values
        const setCellValues = (cell: MeasureCell, isAggregatedRow: boolean) => {
          s.measureInfos.forEach((m, index) => {
            const value = QueryexUtil.evaluateExp(m.exp, cell.aggValues, this.arguments, this.workspace);
            cell.values[index] = value;
            if (!!m.danger && QueryexUtil.evaluateExp(m.danger, cell.aggValues, this.arguments, this.workspace)) {
              cell.classes[index] = 't-danger';
            } else if (!!m.warning && QueryexUtil.evaluateExp(m.warning, cell.aggValues, this.arguments, this.workspace)) {
              cell.classes[index] = 't-warning';
            } else if (!!m.success && QueryexUtil.evaluateExp(m.success, cell.aggValues, this.arguments, this.workspace)) {
              cell.classes[index] = 't-success';
            } else {
              cell.classes[index] = null;
            }

            // Set the disableDrilldown for this cell
            if (!!s.havingExp) {
              // There is a having filter
              if (isAggregatedRow) {
                cell.disableDrilldown = true; // Has an aggregated row dimension -> disable drilldown
              } else {
                const inAggregatedColumn =
                  (!!cell.column && cell.column.immediateChildren.length > 0) || (!cell.column && s.columnInfos.length > 0);

                if (inAggregatedColumn) {
                  cell.disableDrilldown = true; // Has an aggregated column dimension -> disable drilldown
                }
              }
            }

            if (m.isOrdered && !!cell.row) {
              cell.row.sortValue = value; // Measures aren't multi-lingual
            }
          });
        };

        if (!!rowsGrandTotalMeasures) {
          const isAggregated = s.rowInfos.length > 0;
          for (const cell of rowsGrandTotalMeasures) {
            setCellValues(cell, isAggregated);
          }
        }

        // Recursive function that applies setCellValues to all the measure cells not in grand total
        function setAllCellValues(cells: DimensionCell[]) {
          for (const cell of cells) {
            const isAggregated = cell.immediateChildren.length > 0;
            for (const m of cell.measures) {
              setCellValues(m, isAggregated);
            }

            if (isAggregated) {
              setAllCellValues(cell.immediateChildren);
            }
          }
        }

        setAllCellValues(rowsRoots);
      }

      // Prepare a flat and sorted list of row dimensions (parents before children)
      const rowCells: DimensionCell[] = [];
      if (rowsRoots.length > 0) {
        flatten(rowsRoots, rowCells); // fills rows
      }

      // Finally... set the pivot
      s.pivot = {
        maxVisibleLevel: 0,
        rowSpan: 0,
        colSpan: 0,
        columnHeaders,
        columns: columnCells,
        rows: rowCells,
        rowsGrandTotalMeasures,
        columnsGrandTotalLabel,
        rowsGrandTotalLabel,
        columnsGrandTotalsIncluded: showColumnsTotals,
      };

      this.recomputeColumnSpans(s.pivot); // Sets maxVisibleLevel, rowSpan and colSpan for the pivot and every cell in columnHeader

      // In this final section we compute expanded colspan and rowspan
      // Those do not change when the user expands and collapses nodes

      columnHeaders.forEach(row => row.forEach(cell => {
        cell.expandedColSpan = 0;
      }));
      for (let i = columnHeaders.length - 1; i >= 0; i--) {
        for (let j = columnHeaders[i].length - 1; j >= 0; j--) {
          const cell = columnHeaders[i][j];
          cell.expandedColSpan = cell.expandedColSpan || s.measureInfos.length || 1; // measureCount might be 0
          if (!!cell.immediateParent) {
            cell.immediateParent.expandedColSpan += cell.expandedColSpan;
          }
        }
      }

      // Calculate the rowSpan of every cell
      columnHeaders.forEach(colHeaderRow => colHeaderRow.forEach(cell => {
        cell.expandedRowSpan = cell.immediateChildren.length === 0 ? columnHeaders.length - cell.level : 1;
      }));
    }

    return s.pivot;
  }

  private recomputeColumnSpans(pivot?: PivotTable) {
    const s = this.state;
    pivot = pivot || s.pivot;
    const cols = pivot.columnHeaders;
    const measureCount = s.measureInfos.length;

    // Calculte the visibility of every cell and set colSpan to 0
    cols.forEach(row => row.forEach(cell => {
      cell.colSpan = 0;
      cell.isVisible = !cell.immediateParent || (cell.immediateParent.isVisible && cell.immediateParent.isExpanded);
    }));

    // Calculate the colSpan of every cell's parent
    for (let i = cols.length - 1; i >= 0; i--) {
      for (let j = cols[i].length - 1; j >= 0; j--) {
        const cell = cols[i][j];
        cell.colSpan = cell.colSpan || measureCount || 1; // measureCount might be 0
        if (!!cell.immediateParent && !!cell.immediateParent.isExpanded) {
          cell.immediateParent.colSpan += cell.colSpan;
        }
      }
    }

    // Calculate the maximum visible level
    pivot.maxVisibleLevel = -1;
    cols.forEach(row => row.forEach(cell => {
      if (cell.isVisible && cell.level > pivot.maxVisibleLevel) {
        pivot.maxVisibleLevel = cell.level;
      }
    }));

    // Calculate the rowSpan of every cell
    cols.forEach(colHeaderRow => colHeaderRow.forEach(cell => {
      cell.rowSpan = !cell.isExpanded ? pivot.maxVisibleLevel + 1 - cell.level : 1;
    }));

    // Calculate the rowSpan of the grand columns total
    {
      const cell = pivot.columnsGrandTotalLabel;
      if (!!cell) {
        cell.colSpan = measureCount || 1;
        cell.rowSpan = pivot.maxVisibleLevel + 1;
      }
    }

    // Calculate the spans of the top left corner cell
    pivot.colSpan = s.rowInfos.length === 0 ? 0 : 1;
    pivot.rowSpan = pivot.maxVisibleLevel + 1 + (this.showMeasureLabelsRow ? 1 : 0);
  }

  public showHeaderRow(index: number) {
    return index <= this.pivot.maxVisibleLevel;
  }

  // End Attribute stuff

  public onExpandRow(d: DimensionCell): void {
    d.isExpanded = !d.isExpanded;
    this.recomputeVisibleRows();
  }

  public onExpandColumn(d: DimensionCell): void {
    d.isExpanded = !d.isExpanded;
    this.recomputeColumnSpans();
  }

  private recomputeVisibleRows() {
    // This method makes a new copy of the rows WITHOUT the invisible
    // rows this is to support efficient virtual scrolling
    const rows = this.state.pivot.rows;

    // with 2 sweeps, recompute which columns are visible
    rows.forEach(row => row.isVisible = !row.immediateParent || (row.immediateParent.isExpanded && row.immediateParent.isVisible));
    this._modifiedRows = rows.filter(e => e.isVisible);
  }

  public hasChildren(d: DimensionCell): boolean {
    return !!d.immediateChildren && d.immediateChildren.length > 0;
  }

  public flipArrow(node: DimensionCell): string {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl && !node.isExpanded ? 'horizontal' : null;
  }

  public get rowAttributePlacement() {
    return this.workspace.ws.isRtl ? this._rowAttributePlacementRtl : this._rowAttributePlacement;
  }

  public rotateArrow(node: DimensionCell): number {
    return node.isExpanded ? 90 : 0;
  }

  public isMeasureVisible(column: DimensionCell) {
    // The function accepts the column of that measure
    return !column || (column.isVisible && !column.isExpanded);
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

  public get columnHeaders(): DimensionCell[][] {
    // this.checkColumnFreshness();
    return this.pivot.columnHeaders;
  }

  public get columnCells(): DimensionCell[] {
    return this.pivot.columns;
  }

  public get tooManyColumns(): boolean {
    return this.pivot.columns.length * (this.measures.length || 1) > this.maximumColumns;
  }

  public get rows(): DimensionCell[] {
    const pivot = this.pivot;
    if (this._currentRows !== pivot.rows) {
      this._currentRows = pivot.rows;
      this.recomputeVisibleRows();
    }

    return this._modifiedRows;
  }

  public get showRowDimensionLabel(): boolean {
    // true when a descriptive label of the rows is displayed in the upper left corner instead of blank
    return false; // TODO
  }

  public get singleRowDimension(): DimensionInfo {
    // Returns the row dimension only if there is a single one
    const s = this.state;
    if (!!s.rowInfos && s.rowInfos.length === 1) {
      return s.rowInfos[0];
    }
  }

  public get showMeasureLabelsRow(): boolean {
    const s = this.state;

    // The labels for the measures are hidden when there is only a
    // single measure unless when there are no column dimensions
    if (s.measureInfos.length > 0 && (s.measureInfos.length > 1 || s.columnInfos.length === 0)) {
      return true;
    } else {
      // We also show the measures labels row anyways if there are single row dimension attributes and no column headers to show them
      const single = this.singleRowDimension;
      return !this.showHeaderRow(0) && !!single && single.attributes.length > 0;
    }
  }

  public get showColumnsGrandTotalsMeasureLabels(): boolean {
    // The labels for the measures are hidden when there is only a
    // single measure unless when there are no column dimensions
    return !!this.pivot.columnsGrandTotalsIncluded;
  }

  public get rowSpan(): number {
    return this.pivot.rowSpan;
  }

  public get colSpan(): number {
    return this.pivot.colSpan;
  }

  public get rowsGrandTotalMeasures(): MeasureCell[] {
    return this.pivot.rowsGrandTotalMeasures;
  }

  public get columnsGrandTotalLabel(): LabelCell {
    return this.pivot.columnsGrandTotalLabel;
  }

  public get rowsGrandTotalLabel(): LabelCell {
    return this.pivot.rowsGrandTotalLabel;
  }

  public get measures(): MeasureInfo[] {
    return this.state.measureInfos;
  }

  private computeFilterAtoms(dimension: DimensionCell | ChartDimensionCell): string[] {
    const atoms: string[] = [];
    let currentDimension = dimension;
    while (!!currentDimension) {
      // If the next node is an ancestor AND has the same dimension info, skip it, since it's part of the same tree
      // (1) Stringify this dimensions key expression
      const { keyExp, keyDesc, entityDesc } = currentDimension.info;

      let datatype: DataType;
      let keyString: string;

      if (currentDimension.isAncestor) {
        // For ancestors we have to specify the Id property itself otherwise descof will throw an error
        datatype = entityDesc.properties.Id.datatype;
        keyString = QueryexUtil.stringify(keyExp, undefined, undefined) + '.Id';
      } else if (keyDesc.datatype === 'entity') {
        // If it's a nav property update everything to match the associated FK (FK creates less joins than using Id)
        const columnAccess = keyExp.clone() as QueryexColumnAccess;
        columnAccess.property = keyDesc.foreignKeyName;

        datatype = entityDesc.properties.Id.datatype;
        keyString = QueryexUtil.stringify(columnAccess, undefined, undefined);
      } else {
        // For scalar values use everything as is
        datatype = keyDesc.datatype;
        keyString = QueryexUtil.stringify(keyExp, undefined, undefined);
      }

      // (2) Stringify the value
      const valueId = currentDimension.valueId;
      let valueString: string;
      if (!isSpecified(valueId)) {
        valueString = 'null';
      } else if (QueryexUtil.needsQuotes(datatype)) {
        valueString = valueId.replace('\'', '\'\'');
      } else {
        valueString = valueId + '';
      }

      // (3) Determine the the operator
      const op = currentDimension.isAncestor ? 'descof' : 'eq';

      // (4) Assemble the atom and add it
      atoms.push(`(${keyString} ${op} ${valueString})`);

      // For the next iteration
      currentDimension = currentDimension.parent;
    }

    return atoms;
  }

  public computeMeasureAtoms(measureIndex: number): string[] {
    if (measureIndex === -1) {
      return [];
    }
    const s = this.state;
    const info = s.measureInfos[measureIndex];
    const aggregations = info.exp.aggregations();
    const atomsTracker: { [key: string]: true } = {};
    for (const aggregation of aggregations) {
      if (aggregation.arguments.length < 2) {
        // One of the aggregations does not have a filter, don't add a filter atom
        return [];
      } else {
        const condition = aggregation.arguments[1];
        const conditionString = QueryexUtil.stringify(condition, this.arguments, s.parameterInfos);
        atomsTracker[conditionString] = true;
      }
    }

    const atoms = Object.keys(atomsTracker);
    if (atoms.length === 0) {
      return [];
    } else if (atoms.length === 1) {
      return atoms;
    } else {
      const reduced = atoms.reduce((f, atom) => f + ' or ' + atom);
      return [`(${reduced})`];
    }
  }

  public onMeasureClick(cell: MeasureCell, measureIndex: number): void {
    if (cell.disableDrilldown) {
      return;
    }

    const col: DimensionCell = cell.column;
    const row: DimensionCell = cell.row;
    const rowAtoms = this.computeFilterAtoms(row);
    const columnAtoms = this.computeFilterAtoms(col);
    const measureAtom = this.computeMeasureAtoms(measureIndex); // Returns a singleton or empty array

    const allAtoms = rowAtoms.concat(columnAtoms).concat(measureAtom);
    let filter: string;
    if (allAtoms.length === 0) {
      filter = null;
    } else if (allAtoms.length === 1) {
      filter = allAtoms[0];
    } else {
      const reduced = allAtoms.reduce((f, atom) => f + ' and ' + atom);
      filter = `(${reduced})`;
    }

    this.drilldown(filter);
  }

  private drilldown(cellFilter: string) {
    if (this.disableDrilldown) {
      return; // In popup mode, navigation behavior is strange
    }

    // Prepare the filter
    const s = this.state;
    const reportFilter = QueryexUtil.stringify(s.filterExp, this.arguments, s.parameterInfos);
    const combinedFilter =
      !!reportFilter && !!cellFilter ? `${reportFilter} and ${cellFilter}` :
        !!cellFilter ? DeBracket(cellFilter) :
          !!reportFilter ? DeBracket(reportFilter) : null;

    // Prepare the navigation parameters
    const params: Params = {};
    if (!!combinedFilter) {
      params.filter = combinedFilter;
    }

    // Grab the tenantId
    const tenantId = this.workspace.ws.tenantId;

    const def = this.definition;
    if (def.IsCustomDrilldown && !!def.Id && def.Select && def.Select.length > 0) {
      // Drilldown to the custom drilldown screen
      const reportDefId = this.definition.Id;
      params.cache_buster = ReportResultsComponent.CACHE_BUSTER++;
      this.router.navigate(['app', tenantId + '', 'drilldown', reportDefId + '', params]);

    } else if (!!this.entityDescriptor.masterScreenUrl) {
      // Drilldown to the default entity screen
      const screenUrlSegments = this.entityDescriptor.masterScreenUrl.split('/');
      params.inactive = true;
      this.router.navigate(['app', tenantId + '', ...screenUrlSegments, params]);

    } else {
      console.error(`No screen URL is defined for collection: '${def.Collection}', definitionId '${def.DefinitionId}'`);
    }
  }

  public isDefined(cell: DimensionCell) {
    // IF this is false, a muted italic "(undefined)" is displayed instead
    return isSpecified(cell.valueId);
  }

  public get addRowExpanders() {
    const infos = this.state.rowInfos;
    return infos.length > 1 || (infos.length > 0 && infos[0].showAsTree);
  }

  // Auto Cell

  public displayValue(value: any, desc: PropVisualDescriptor, entityDesc: EntityDescriptor) {
    if (Array.isArray(value)) {
      if (!!entityDesc) {
        return entityDesc.formatFromVals(value);
      } else {
        const localizedValue = this.workspace.currentTenant.localize(value[0], value[1], value[2]);
        return displayScalarValue(localizedValue, desc, this.workspace, this.translate);
      }
    } else {
      return displayScalarValue(value, desc, this.workspace, this.translate);
    }
  }

  private sortValue(value: any, _: PropVisualDescriptor, entityDesc: EntityDescriptor) {
    if (Array.isArray(value)) {
      if (!!entityDesc) {
        return entityDesc.formatFromVals(value);
      } else {
        return this.workspace.currentTenant.localize(value[0], value[1], value[2]);
      }
    } else {
      return value;
    }
  }

  public descAlignment(desc: NumberPropVisualDescriptor | PercentPropVisualDescriptor): 'right' {
    if (desc.isRightAligned) {
      return 'right';
    }
  }

  public hasColor(desc: ChoicePropVisualDescriptor) {
    return !!desc.color;
  }

  public stateColor(value: any, desc: ChoicePropVisualDescriptor) {
    if (desc.color) {
      return desc.color(value);
    }
  }

  /////////////////// SUMMARY - CHARTS

  public get point(): string {
    // Point charts bind to this property
    const pivot = this.pivot;
    const s = this.state;
    if (s.currentPivotForPoint !== pivot) {
      s.currentPivotForPoint = pivot;
      const measureIndex = s.singleNumericMeasureIndex;
      const measure = s.measureInfos[measureIndex];

      if (!measure || !pivot.rowsGrandTotalMeasures || pivot.rowsGrandTotalMeasures.length < 1) {
        s.point = null;
      } else if (s.dimensionInfos.length === 0) {
        const cell = pivot.rowsGrandTotalMeasures[0];
        const value = cell.values[measureIndex] || 0;
        s.point = displayScalarValue(value, measure.desc, this.workspace, this.translate);
        s.pointClass = cell.classes[measureIndex];
      } else {
        s.point = null; // Just to make the code look like multi()
        s.pointClass = null;
      }

      delete s.singleCellsHash;
    }

    return s.point;
  }

  public get pointClass(): HighlightClass {
    const _ = this.point; // To populate the class
    return this.state.pointClass;
  }

  private chartDimensionCellFromDimensionCell = (
    dimCell: DimensionCell,
    color: string,
    index: number,
    parent?: ChartDimensionCell): ChartDimensionCell => {
    const dimValueDisplay = !isSpecified(dimCell.valueId) ? this.translate.instant('Undefined') :
      this.displayValue(dimCell.value, dimCell.info.desc, dimCell.info.entityDesc);

    return new ChartDimensionCell(dimValueDisplay, dimCell.valueId, dimCell.info, index, color, parent);
  }

  public get single(): SingleSeries {
    // Single series charts bind to this property
    const pivot = this.pivot;
    const s = this.state;
    if (s.currentPivotForSingle !== pivot || s.currentLangForSingle !== this.translate.currentLang) {
      s.currentPivotForSingle = pivot;
      s.currentLangForSingle = this.translate.currentLang;
      const measureIndex = s.singleNumericMeasureIndex;
      const measure = s.measureInfos[measureIndex];

      if (!measure || !pivot) {
        s.single = null;
      } else if (s.dimensionInfos.length === 1) {
        try {
          const measureCells: MeasureCell[] = s.rowInfos.length > 0 ? pivot.rows.map(r => r.measures[0]) : pivot.rowsGrandTotalMeasures;

          let singleSum = 0;
          const single: SingleSeries = [];
          let index = 0;
          for (const measureCell of measureCells) {
            const dimCell = measureCell.row || measureCell.column;
            if (!dimCell || dimCell.isAncestor) {
              continue; // Totals and tree ancestors are not displayed in the chart
            }

            const color = this.colorFromClass(measureCell.classes[measureIndex]);

            // Get the measure value
            const measureValue = measureCell.values[measureIndex] || 0;
            singleSum += measureValue;
            single.push({
              name: this.chartDimensionCellFromDimensionCell(dimCell, color, index++),
              value: measureValue
            });
          }
          s.single = single;

          if (!!pivot.rowsGrandTotalMeasures && pivot.rowsGrandTotalMeasures.length > 0) {
            const grandGrandTotalCell = pivot.rowsGrandTotalMeasures[pivot.rowsGrandTotalMeasures.length - 1];

            // Get the real total
            const grandGrandTotal = grandGrandTotalCell.values[measureIndex] || 0;

            // Get the sum total
            s.totalEqualsSum = Math.abs(Math.round(grandGrandTotal * 1000000) - Math.round(singleSum * 1000000)) < 2;
          }

          delete s.singleCellsHash;

        } catch (ex) {
          s.reportStatus = ReportStatus.error;
          s.errorMessage = ex.message;
        }
      } else {
        s.single = null; // Just to make the code look like multi()
      }
    }

    return s.single;
  }

  public customColors = (cell: ChartDimensionCell) => {
    return cell.color || palette[cell.index % palette.length];
  }

  public alternativeCustomColors = (cellToString: string) => {
    // This function is only for single series charts that give you cell.toString() instead of cell:
    // - Pie
    // - Doughnut
    // - Tree map
    // - Number cards

    // Prepare the hash if it's not there
    const s = this.state;
    if (!s.singleCellsHash) {
      s.singleCellsHash = {};
      for (const point of this.single) {
        s.singleCellsHash[point.name.toString()] = point.name;
      }
    }
    const cell = s.singleCellsHash[cellToString];
    return this.customColors(cell);
  }

  private colorFromClass(c: HighlightClass): string {
    let color: string;
    switch (c) {
      case 't-success':
        color = success;
        break;
      case 't-warning':
        color = warning;
        break;
      case 't-danger':
        color = danger;
        break;
    }

    return color;
  }

  public get multi(): MultiSeries {
    // Multi-series charts bind to this property
    const pivot = this.pivot;
    const s = this.state;
    if (s.currentPivotForMulti !== pivot || s.currentLangForMulti !== this.translate.currentLang) {
      s.currentPivotForMulti = pivot;
      s.currentLangForMulti = this.translate.currentLang;
      const measureIndex = s.singleNumericMeasureIndex;
      const measure = s.measureInfos[measureIndex];

      if (!measure || !pivot) {
        s.multi = null;
      } else if (s.dimensionInfos.length === 1) {
        // When the number of dimensions is just one, make the single-series pretend it's a multi-series
        const single = this.single;
        const label = this.firstDimensionLabel;
        const singletonDimension = new ChartDimensionCell(label, label, null, monochromeIndex); // 2 is the default chart color
        s.multi = [{
          name: singletonDimension,
          series: single
        }];
      } else if (s.dimensionInfos.length === 2) {
        const dim1 = s.dimensionInfos[0];
        const dim2 = s.dimensionInfos[1];

        // const data: { dim1Cell: DimensionCell, values: { dim2Cell: DimensionCell, values: MeasureCell[] }[] }[] = [];

        const multi: MultiSeries = [];
        if (s.rowInfos.length === 2 || s.columnInfos.length === 2) {
          // Both dimensions are rows or both are columns
          // These two change when the dimensions are rows or columns, the rest of the Algorithm is the same
          let getMeasureFn: (dimCell: DimensionCell) => MeasureCell;
          let dimCells: DimensionCell[];
          if (s.rowInfos.length === 2) {
            dimCells = pivot.rows;
            getMeasureFn = (dimCell) => dimCell.measures[0];
          } else {
            dimCells = pivot.columns;
            getMeasureFn = (dimCell) => pivot.rowsGrandTotalMeasures[dimCell.index];
          }

          // The Algorithm
          let currentChartParent: ChartDimensionCell;
          let currentSeries: SingleSeries;
          let parentIndex = 0;
          let index: number;

          for (const dimCell of dimCells) {
            if (dimCell.isAncestor) {
              continue; // Tree ancestors are not included in charts
            }

            if (dimCell.info === dim1) {
              currentChartParent = this.chartDimensionCellFromDimensionCell(dimCell, undefined, parentIndex++);
              currentSeries = [];
              index = 0;

              multi.push({ name: currentChartParent, series: currentSeries });
            }

            if (dimCell.info === dim2) { // <- must be a child of currentParent
              const measureCell = getMeasureFn(dimCell);
              const color = this.colorFromClass(measureCell.classes[measureIndex]);
              const chartCell = this.chartDimensionCellFromDimensionCell(dimCell, color, index++, currentChartParent);
              const value = measureCell.values[measureIndex] || 0;

              currentSeries.push({ name: chartCell, value });
            }
          }
        } else {
          // One dimension is row, and one is column
          let parentIndex = 0;
          for (const columnCell of pivot.columns) { // Flip to rows
            if (columnCell.isAncestor) {
              continue; // Tree ancestors are not included in charts
            }

            const currentChartParent: ChartDimensionCell = this.chartDimensionCellFromDimensionCell(columnCell, undefined, parentIndex++);
            const currentSeries: SingleSeries = [];
            multi.push({ name: currentChartParent, series: currentSeries });

            let index = 0;
            for (const rowCell of pivot.rows) {
              if (rowCell.isAncestor) {
                continue; // Totals and ancestors are not included in charts
              }

              const measureCell = rowCell.measures[columnCell.index];
              const color = this.colorFromClass(measureCell.classes[measureIndex]);
              const chartCell = this.chartDimensionCellFromDimensionCell(rowCell, color, index++, currentChartParent);
              const value = measureCell.values[measureIndex] || 0;
              currentSeries.push({ name: chartCell, value });
            }
          }
        }

        s.multi = multi;
      } else {
        s.multi = null;
      }

      delete s.singleCellsHash;
    }

    return s.multi;
  }

  public formatChartDimension(d: ChartDimensionCell) {
    return d.display; // The chart labels
  }

  public formatAlternativeChartDimension(d: { data: { name: ChartDimensionCell } }) {
    // For some reason, some chart types pass this data structure instead
    return d.data.name.display;
  }

  public formatChartMeasureValue = (value: number, s?: ReportStore) => {
    s = s || this.state;
    const measure = s.measureInfos[s.singleNumericMeasureIndex];
    return displayScalarValue(value, measure.desc, this.workspace, this.translate);
  }

  public formatAlternativeChartMeasureValue = (d: { value: number }) => {
    // For some reason, some chart types pass this data structure instead
    return this.formatChartMeasureValue(d.value);
  }

  public formatStringChartMeasureValue = (value: string) => {
    // For some reason, some chart types pass a formatted string 3,241.2 instead of a number
    value = value.replace(',', '');
    return this.formatChartMeasureValue(+value);
  }

  public showGaugeTotal: () => boolean = () => {
    // The ngx-guage displays the sum of the values in its inner text
    // So if the total != sum we need to hide it the inner text
    return this.state.totalEqualsSum;
  }

  public get firstDimensionLabel() {
    const dimension = this.state.dimensionInfos[0];
    return !!dimension ? dimension.label() : '';
  }

  public get secondDimensionLabel() {
    const dimension = this.state.dimensionInfos[1];
    return !!dimension ? dimension.label() : '';
  }

  public get measureLabel() {
    const s = this.state;
    const measureIndex = s.singleNumericMeasureIndex;
    const measure = s.measureInfos[measureIndex];
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
    return this.state.dimensionInfos.length;
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

  public get selectInfos(): SelectInfo[] {
    return this.state.selectInfos;
  }

  public get flat(): any[][] {
    // Flat table headers will remain visible while it's loading, but it won't display any results
    // This way the user can order columns by clicking on headers without waiting for the results to come back
    if (!this.showResults) {
      return [];
    }

    // The details report table binds to this array of array
    const s = this.state;
    const result = s.result;
    if (s.currentResultForFlat !== result) {
      s.currentResultForFlat = result;
      s.flat = this.flatInner(result, s);
    }

    return s.flat;
  }

  private flatInner(result: DynamicRow[], s: ReportStore) {
    const flat: any[][] = [];

    if (!result) {
      return [];
    }

    for (const row of result) {
      const flatRow = [];
      for (const info of s.selectInfos) {
        if (info.entityDesc || info.indices.length > 1) {
          // If there are multiple indices add them as an array
          flatRow.push(info.indices.map(i => row[i]));
        } else {
          // If there is a single index, add the value directly, not as an array
          const index = info.indices[0];
          flatRow.push(row[index]);
        }
      }

      flat.push(flatRow);
    }

    return flat;
  }

  public isRightAligned(desc: PropVisualDescriptor) {
    switch (desc.control) {
      case 'number':
      case 'percent':
        return desc.isRightAligned;
      default:
        return false;
    }
  }

  public onFlatSelect(rowIndex: number): void {
    const s = this.state;
    const row = s.result[rowIndex];
    const desc = this.entityDescriptor;
    if (!!desc.navigateToDetailsFromVals) {
      const navToDetailsVals = s.navigateToDetailsIndices.map(i => row[i]);
      desc.navigateToDetailsFromVals(navToDetailsVals, this.router);

    } else if (!!desc.masterScreenUrl) {

      if (s.idIndex > -1) {
        const id = row[s.idIndex];
        const tenantId = this.workspace.ws.tenantId;
        const screenUrlSegments = desc.masterScreenUrl.split('/');

        // Prepare the commands
        const commands = ['app', tenantId + '', ...screenUrlSegments];
        if (s.defIdIndex > -1) {
          // A definitioned entity, but the report is generic
          const defId = row[s.defIdIndex];
          commands.push(defId);
        }
        commands.push(id);

        this.router.navigate(commands);
      } else {
        const def = this.definition;
        console.error(`no screen URL is defined for collection: '${def.Collection}', definitionId '${def.DefinitionId}'`);
      }
    }
  }

  public get canOrderByFlat(): boolean {
    return !this.definition.Top;
  }

  public onOrderByFlat(info: SelectInfo): void {
    console.log(this.flatHeader);
    if (!this.canOrderByFlat) {
      return;
    }

    if (info.exp.columnAccesses().length === 0) {
      return; // Cannot order by a constant column
    }

    // The purpose of this step is to add the orderby to the url in screen mode
    const s = this.state;

    // Toggle the direction of this column info
    const key = info.exp.toString();
    if (s.orderbyKey === key) {
      if (s.orderbyDir === 'asc') {
        s.orderbyDir = 'desc'; // Flip to desc
      } else {
        // Delete it
        delete s.orderbyKey;
        delete s.orderbyDir;
      }
    } else {
      // Add asc
      s.orderbyKey = key;
      s.orderbyDir = 'asc';
    }

    this.orderbyChange.emit(); // Tell the containing component so it updates the url if needed

    this.rememberFlatColumnWidths();
    this.fetch();
  }

  private orderDirection(info: SelectInfo): QueryexDirection {
    if (this.isOrdered(info)) {
      return this.state.orderbyDir;
    }
  }

  public isOrdered(info: SelectInfo): boolean {
    if (info) {
      const s = this.state;
      if (info.expToString === s.orderbyKey) {
        return !!s.orderbyDir;
      }
    }
  }

  public get isDescending(): boolean {
    return this.state.orderbyDir === 'desc';
  }

  private rememberFlatColumnWidths() {
    if (!this.flatHeader) {
      return;
    }

    const tr = this.flatHeader.nativeElement;
    if (!tr) {
      return;
    }

    // This hack prevents the jarring movement when the user clicks
    // to order by and the data rows go away for a second
    const infos = this.selectInfos;
    const leading = 1;
    const trailing = 1;
    for (let i = leading; i < tr.cells.length - trailing; i++) {
      const info = infos[i - leading];
      if (!!info) {
        info.width = tr.cells[i].offsetWidth + 'px';
      }
    }
  }

  private forgetFlatColumnWidths() {
    this.state.selectInfos.forEach(e => delete e.width);
  }
}
