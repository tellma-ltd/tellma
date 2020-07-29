import {
  Component, EventEmitter, Input, OnDestroy, OnInit, Output, TemplateRef,
  ViewChild, OnChanges, SimpleChanges, ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { ActivatedRoute, Params, Router, ParamMap } from '@angular/router';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { merge, Observable, of, Subject, Subscription } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap, tap, finalize, skip, map } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { GetResponse } from '~/app/data/dto/get-response';
import { EntitiesResponse } from '~/app/data/dto/entities-response';
import { addToWorkspace, downloadBlob, isSpecified, csvPackage } from '~/app/data/util';
import {
  MasterDetailsStore,
  MasterStatus,
  WorkspaceService,
  NodeInfo,
  MasterDisplayMode,
  TreeRefreshMode,
  DEFAULT_PAGE_SIZE as DEFAULT_PAGE_SIZE,
  MAXIMUM_COUNT
} from '~/app/data/workspace.service';
import { FlatTreeControl } from '@angular/cdk/tree';
import {
  metadata,
  EntityDescriptor,
  entityDescriptorImpl,
  NavigationPropDescriptor,
  PropDescriptor
} from '~/app/data/entities/base/metadata';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { StorageService } from '~/app/data/storage.service';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { formatNumber } from '@angular/common';
import { ImportResult } from '~/app/data/dto/import-result';
import { ImportMode, ImportArguments_Mode } from '~/app/data/dto/import-arguments';
import { Entity } from '~/app/data/entities/base/entity';
import { EntityWithKey } from '~/app/data/entities/base/entity-with-key';
import { displayValue, displayEntity } from '../auto-cell/auto-cell.component';
import { SelectorChoice } from '../selector/selector.component';

enum SearchView {
  tiles = 'tiles',
  table = 'table'
}

type ExportMode = 'WhatISee' | 'ForImport';

export interface MultiselectAction {
  template: TemplateRef<any>;
  action: (ids: (string | number)[]) => Observable<any>;
  canAction?: (ids: (string | number)[]) => boolean;
  actionTooltip?: (ids: (string | number)[]) => string;
  showAction?: (ids: (string | number)[]) => boolean;
}

@Component({
  selector: 't-master',
  templateUrl: './master.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MasterComponent implements OnInit, OnDestroy, OnChanges {

  @Input()
  masterCrumb: string;

  @Input()
  collection: string; // This is one of two properties that define the screen

  @Input()
  definitionId: number; // This is one of two properties that define the screen

  @Input()
  tileTemplate: TemplateRef<any>;

  @Input()
  tableSummaryColumnTemplate: TemplateRef<any>;

  @Input()
  tableSummaryHeaderTemplate: TemplateRef<any>;

  @Input()
  showCreateButton = true;

  @Input()
  showImportButton = true;

  @Input()
  showExportButton = true;

  @Input()
  showDeleteButton = true;

  @Input()
  allowMultiselect = true;

  @Input()
  enableTreeView = false;

  @Input()
  includeInactive = false;

  @Input()
  multiselectActions: MultiselectAction[] = [];

  @Input()
  selectDefault: string;

  @Input()
  selectForTiles: string;

  @Input()
  additionalSelect: string; // Loaded, but does not appear in the grid

  @Input()
  filterDefault: string;

  @Input()
  skipInput: number;

  @Input()
  theme: 'light' | 'dark' = 'light';

  @Input()
  filterDefinition: {
    [groupName: string]: {
      template: TemplateRef<any>,
      expression: string
    }[]
  } = {};

  @Input()
  additionalCommands: TemplateRef<any>[]; // TODO

  @Input() // popup: limits the tiles to only 2 per row, hides import, export and multiselect
  mode: 'popup' | 'screen' = 'screen';

  @Input()
  exportPageSize = 10000;

  @Input()
  exportFileName: string;

  @Output()
  choose = new EventEmitter<number | string>(); // Fired in popup mode to indicate choosing an item

  @Output()
  create = new EventEmitter<void>();

  @Output()
  cancel = new EventEmitter<void>();

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  @ViewChild('importModal', { static: true })
  public importModal: TemplateRef<any>;

  private localState = new MasterDetailsStore();  // Used in popup mode
  private searchChanged$ = new Subject<string>();
  private notifyFetch$ = new Subject();
  private notifyDownloadTemplate$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private _selectOld = 'null';
  private _tableColumnPaths: string[] = [];
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense
  private _subscriptions: Subscription;
  private _computeOrderByCache: { [path: string]: string } = {}; // need to be erased on screen startup
  private _computeOrderByLang: string = null;
  private _reverseOrderByCache: { [path: string]: string } = {}; // need to be erased on screen startup
  private _editingColumns = false;
  private _currentStringIds: string = null;
  private _parentIdsFromUserSettings: (string | number)[] = [];

  public searchView: SearchView;
  public checked = {};
  public exportSkip = 0;
  public showExportSpinner = false;
  public actionErrorMessage: string;
  public actionValidationErrors: { [id: string]: string[] } = {};
  public treeControl = new FlatTreeControl<NodeInfo>(node => node.level - 1, node => node.hasChildren);

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private router: Router, private cdr: ChangeDetectorRef,
    private route: ActivatedRoute, private translate: TranslateService, public modalService: NgbModal, private storage: StorageService,
    private customUserSettings: CustomUserSettingsService) {
  }

  ngOnInit() {

    // Use some RxJS magic to refresh the data as the user changes the parameters
    const searchBoxSignals = this.searchChanged$.pipe(
      debounceTime(20), // 175
      distinctUntilChanged(),
      tap(() => this.state.skip = 0),
      tap(() => this.exportSkip = 0),
      tap(() => this.urlStateChange())
    );

    const otherSignals = this.notifyFetch$;
    const allSignals = merge(searchBoxSignals, otherSignals);

    this._subscriptions = new Subscription();
    this._subscriptions.add(allSignals.pipe(
      switchMap(() => this.doFetch())
    ).subscribe());

    this._subscriptions.add(this.workspace.stateChanged$.subscribe({
      next: () => this.cdr.markForCheck()
    }));

    this._subscriptions.add(this.notifyDownloadTemplate$.pipe(
      switchMap(() => this.doDownloadTemplate())
    ).subscribe());

    // Reset the state of the master component state
    this.localState = new MasterDetailsStore();
    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);
    this.checked = {};
    this.exportSkip = 0;
    this.showExportSpinner = false;
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};
    this._selectOld = 'null';
    this._tableColumnPaths = [];
    this._computeOrderByCache = {}; // need to be erased on screen startup
    this._computeOrderByLang = null;
    this._reverseOrderByCache = {}; // need to be erased on screen startup
    this._editingColumns = false;
    this._currentStringIds = null;
    this._parentIdsFromUserSettings = null;

    // use the default
    this.searchView = (!!window && window.innerWidth >= 992) ? SearchView.table : SearchView.tiles;

    // default display mode
    let displayMode: MasterDisplayMode = this.enableTreeView ?
      MasterDisplayMode.tree : MasterDisplayMode.flat; // Default search view

    // const params = this.route.snapshot.paramMap;
    const handleFreshStateFromUrl = (params: ParamMap) => {

      // here we handle the URL parameters
      let hasChanged = false;
      const s = this.state;

      if (this.isPopupMode) {

        // select
        const select = this.selectFromUserSettings || this.selectDefault || '';
        if (select !== s.select) {
          s.select = select;
          hasChanged = true;
        }

        // filter
        const filter = this.filterDefault || null;
        if (filter !== s.customFilter) {
          s.customFilter = filter;
          hasChanged = true;
        }

        const inactive = this.includeInactive;
        if (inactive !== s.inactive) {
          s.inactive = inactive;
          hasChanged = true;
        }

      } else { // this is only in screen mode

        // view: tiles vs table
        const view = params.get('view');
        if (!!view && !!SearchView[view]) {
          this.searchView = SearchView[view];
        }

        // display mode (flat vs tree): has a precise default value
        displayMode = !!params.get('display') && !!MasterDisplayMode[params.get('display')] ?
          MasterDisplayMode[params.get('display')] : displayMode; // Default search view

        // select
        const urlSelect = params.get('select') || this.selectFromUserSettings || this.selectDefault || '';
        if (urlSelect !== s.select) {
          s.select = urlSelect;
          hasChanged = true;
        }

        // filter
        const urlFilter = params.get('filter') || null;
        if (urlFilter !== s.customFilter) {
          s.customFilter = urlFilter;
          hasChanged = true;
        }

        // search
        const urlSearch = params.get('search') || null;
        if (urlSearch !== s.search) {
          s.search = urlSearch;
          hasChanged = true;
        }

        // orderby
        const urlOrderby = params.get('orderby') || null;
        if (urlOrderby !== s.orderby) {
          s.orderby = urlOrderby;
          hasChanged = true;
        }

        // skip
        let urlSkip = +params.get('skip') || 0;
        urlSkip = urlSkip < 0 ? 0 : urlSkip;
        if (urlSkip !== s.skip) {
          s.skip = urlSkip;
          hasChanged = true;
        }

        // Inactive
        const urlInactiveString = params.get('inactive');
        const urlInactive = !!urlInactiveString ? (urlInactiveString.toString() === 'true') : this.includeInactive;
        if (urlInactive !== !!s.inactive) {
          s.inactive = urlInactive;
          hasChanged = true;
        }
      }

      // display mode: has a precise default value
      if (s.displayMode !== displayMode) {
        s.displayMode = displayMode;
        hasChanged = true;
      }

      // (hasChanged === true) means we navigated to this screen with different url params than last time
      // (masterStatus !== loaded) means we navigated to this master screen for the first time
      // (masterStatus !== loading) means the url state has NOT changed from within the screen
      // (s.mdLastKey != this.endpoint) means another master screen was opened before coming here
      // In either of the above cases, we need to refresh
      if (hasChanged || (s.masterStatus !== MasterStatus.loaded && s.masterStatus !== MasterStatus.loading) ||
        this.workspace.current.mdLastKey !== this.apiEndpoint) {
        this.workspace.current.mdLastKey = this.apiEndpoint;
        this.fetch();
      }
    };

    this._subscriptions.add(this.route.paramMap.pipe(skip(1)).subscribe(handleFreshStateFromUrl));
    handleFreshStateFromUrl(this.route.snapshot.paramMap);
  }

  ngOnDestroy() {
    if (this.state.masterStatus === MasterStatus.loading) {
      delete this.state.masterStatus; // So that coming back will trigger a refresh
    }

    // This cancels any asynchronous backend calls
    this.notifyDestruct$.next();
    this._subscriptions.unsubscribe();
    this.cancelAllTreeQueries();
  }

  ngOnChanges(changes: SimpleChanges) {

    // the combination of these two properties defines a whole new screen from the POV of the user
    // when either of these properties change it is equivalent to a screen closing and
    // and another screen opening even though Angular may reuse the same
    // component and never call ngOnDestroy and ngOnInit. So we call them
    // manually here if this is not the first time these properties are set
    // to simulate a screen closing and opening again
    const screenDefProperties = [changes.collection, changes.definitionId];
    const screenDefChanges = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());
    if (screenDefChanges) {

      this.ngOnDestroy();
      this.ngOnInit();
    }
  }

  private cancelAllTreeQueries(): void {
    if (!!this.state.treeNodes) {
      this.state.treeNodes.forEach(node => {

        // All table nodes that are loading or error, are reset to null so
        // they appear collapsed next time the screen is opened
        if (node.status !== MasterStatus.loaded) {
          node.status = null;
          node.isExpanded = false;
        }

        // cancel any pending queries
        if (!!node.notifyCancel$) {
          node.notifyCancel$.next();
        }
      });
    }
  }

  public fetch() {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {

    // any currently running queries that are loading children of nodes need to be canceled
    this.cancelAllTreeQueries();

    // Remove previous Ids from the store
    let s = this.state;
    s.flatIds = [];
    s.treeNodes = [];
    s.detailsId = null; // clear the cached details item
    this.checked = {}; // clear all selection
    this.actionValidationErrors = {}; // clear validation errors
    s.masterStatus = MasterStatus.loading;

    // This will show the spinner
    this.cdr.markForCheck();

    const isTree = this.isTreeMode;
    const isSearchOrFilter = this.searchOrFilter;

    // compute the parameters
    const select = this.computeSelect();
    let obs$: Observable<EntitiesResponse>;

    if (isTree && !isSearchOrFilter) {
      const filter = s.inactive || !this.showIncludeInactive ? null : `(${this.inactiveFilter}) or ActiveChildCount gt 0`;
      const parentIds: (string | number)[] = this.parentIdsFromUserSettings;

      obs$ = this.crud.getChildrenOf({
        i: parentIds,
        select,
        filter,
        roots: true
      }).pipe(
        tap((response: GetResponse) => {
          s = this.state; // get the source
          s.top = response.Result.length;
          s.skip = 0;
          s.total = s.top;
        }),
      );

    } else {
      const top = DEFAULT_PAGE_SIZE;
      const skipParam = s.skip;
      const orderby = isTree ? 'Node' : s.orderby;
      const search = s.search;
      const filter = this.computeFilter(s);

      // Retrieve the entities
      obs$ = this.crud.getFact({
        top,
        skip: skipParam,
        orderby,
        search,
        select,
        filter,
        countEntities: true
      }).pipe(
        tap((response: GetResponse) => {
          s = this.state; // get the source
          s.top = response.Top;
          s.skip = response.Skip;
          s.total = response.TotalCount;
        }),
      );
    }

    return obs$.pipe(
      tap((response: EntitiesResponse) => {
        s = this.state; // get the source
        s.masterStatus = MasterStatus.loaded;
        s.extras = response.Extras;
        s.collectionName = response.CollectionName;

        // add to the relevant collection depending on mode
        if (isTree) {
          const ids = addToWorkspace(response, this.workspace);
          const entityWs = this.workspace.current[response.CollectionName];
          s.updateTreeNodes(ids, entityWs, TreeRefreshMode.cleanSlate, this.searchOrFilter);
        } else {
          s.flatIds = addToWorkspace(response, this.workspace);
        }
      }),
      catchError((friendlyError) => {
        s = this.state; // get the source
        s.masterStatus = MasterStatus.error;
        s.errorMessage = friendlyError.error;
        return of(null);
      }),
      finalize(() => this.cdr.markForCheck())
    );
  }

  private fetchNodeChildren(parentNode: NodeInfo): void {

    if (!parentNode.notifyCancel$) {
      parentNode.notifyCancel$ = new Subject<void>();
    } else {
      parentNode.notifyCancel$.next(); // cancel previous call
    }

    const parentId = parentNode.id;

    // capture the state object and clear the details object
    let s = this.state;
    s.detailsId = null;

    // show rotator next to the expanded item
    parentNode.status = MasterStatus.loading;

    const filter = s.inactive || !this.showIncludeInactive ? null : `(${this.inactiveFilter}) or ActiveChildCount gt 0`;
    const select = this.computeSelect();
    const parentIds: (string | number)[] = [parentId];

    // This will show the spinner
    this.cdr.markForCheck();

    // Retrieve the entities
    const crud = this.api.crudFactory(this.apiEndpoint, parentNode.notifyCancel$);
    crud.getChildrenOf({
      filter,
      select,
      i: parentIds,
      roots: false
    }).pipe(
      tap((response: GetResponse) => {
        s = this.state; // get the source
        s.collectionName = response.CollectionName;

        // Hide the rotator in the parent
        parentNode.status = MasterStatus.loaded;
        const ids = addToWorkspace(response, this.workspace);
        const entityWs = this.workspace.current[response.CollectionName];
        s.updateTreeNodes(ids, entityWs, TreeRefreshMode.preserveExpansions);
        s.total = s.total + ids.length;
      }),
      catchError((friendlyError) => {
        s = this.state; // get the source
        parentNode.status = MasterStatus.error;
        this.displayErrorModal(friendlyError.error);
        return of(null);
      }),
      finalize(() => this.cdr.markForCheck())
    ).subscribe();
  }

  public get state(): MasterDetailsStore {
    // Important to always reference the source, and not take a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs
    if (this.mode === 'popup') {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new MasterDetailsStore();
      }

      return this.localState;
    } else { // this.mode === 'screen'

      // screens on the other hand use a global store
      const key = this.mdStateKey;
      if (!this.workspace.current.mdState[key]) {
        this.workspace.current.mdState[key] = new MasterDetailsStore();
      }

      return this.workspace.current.mdState[key];
    }
  }

  private get mdStateKey(): string {
    return this.apiEndpoint;
  }

  private get view(): string {
    return this.entityDescriptor.apiEndpoint;
  }

  private urlStateChange(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL
    if (this.isScreenMode) {
      const s = this.state;
      const params: Params = {
        view: this.searchView
      };

      if (!!s.displayMode) {
        params.display = s.displayMode;
      }

      if (!!s.skip) {
        params.skip = s.skip;
      }

      if (!!s.select) {
        params.select = s.select;
      }

      if (!!s.search) {
        params.search = s.search;
      }

      if (!!s.orderby) {
        params.orderby = s.orderby;
      }

      if (!!s.customFilter) {
        params.filter = s.customFilter;
      }

      if (isSpecified(s.inactive)) {
        params.inactive = s.inactive;
      }

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    }
  }

  // Calculated screen properties

  get entityDescriptor(): EntityDescriptor {
    const coll = this.collection;
    return !!coll ? metadata[coll](this.workspace, this.translate, this.definitionId) : null;
  }

  get apiEndpoint(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.apiEndpoint : null;
  }

  get titleSingular(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.titleSingular() : null;
  }

  private computeSelect(): string {
    const select = this.state.select || '';
    const resultPaths: { [path: string]: true } = {};
    const baseEntityDescriptor = this.entityDescriptor;

    // (0) add select for tiles
    if (this.selectForTiles) {
      this.selectForTiles.split(',').forEach(e => resultPaths[e] = true);
    }

    // (1) append the current entity type default properties (usually 'Name', 'Name2' and 'Name3')
    baseEntityDescriptor.select.forEach(e => resultPaths[e] = true);

    // (2) append the definitoinId if any, it must always be loaded
    if (!!baseEntityDescriptor.definitionIds) {
      resultPaths.DefinitionId = true;
    }

    // (3) Append addition select
    if (!!this.additionalSelect) {
      this.additionalSelect.split(',').forEach(e => resultPaths[e] = true);
    }

    // (4) Append select required for nav to details
    if (!!baseEntityDescriptor.navigateToDetailsSelect) {
      baseEntityDescriptor.navigateToDetailsSelect.forEach(e => resultPaths[e] = true);
    }

    // (5) replace every path that terminates with a nav property (e.g. 'Unit' => 'Unit/Name,Unit/Name2,Unit/Name3')
    select.split(',').forEach(path => {

      const steps = path.split('/').map(e => e.trim());
      path = steps.join('/'); // to trim extra spaces

      try {
        const currentDesc = entityDescriptorImpl(steps, this.collection,
          this.definitionId, this.workspace, this.translate);

        currentDesc.select.forEach(descSelect => resultPaths[`${path}/${descSelect}`] = true);
      } catch {
        resultPaths[path] = true;
      }
    });

    // (6) in tree mode we add tree stuff and ensure that Parent has the same selects as the children
    if (this.isTreeMode) {
      // tree stuff
      ['Level', 'ChildCount', 'ActiveChildCount', 'IsActive'].forEach(e => resultPaths[e] = true);

      // select from parents what you select from children
      const selectWithParents = Object.keys(resultPaths)
        .map(atom => atom.trim())
        .filter(atom => !!atom && !atom.startsWith('Parent/'))
        .map(atom => `Parent/${atom}`);

      selectWithParents.forEach(e => resultPaths[e] = true);
    }

    return Object.keys(resultPaths).join(',');
  }

  private computeSelectForExport(): string {
    const select = this.state.select || '';
    const resultPaths: { [path: string]: true } = {};
    const baseEntityDescriptor = this.entityDescriptor;

    baseEntityDescriptor.select.forEach(e => resultPaths[e] = true);

    // (3) replace every path that terminates with a nav property (e.g. 'Unit' => 'Unit/Name,Unit/Name2,Unit/Name3')
    select.split(',').forEach(path => {

      const steps = path.split('/').map(e => e.trim());
      path = steps.join('/'); // to trim extra spaces

      try {
        const currentDesc = entityDescriptorImpl(steps, this.collection,
          this.definitionId, this.workspace, this.translate);

        currentDesc.select.forEach(descSelect => resultPaths[`${path}/${descSelect}`] = true);
      } catch {
        resultPaths[path] = true;
      }
    });

    return Object.keys(resultPaths).join(',');
  }

  private get selectKey(): string {
    return `${this.collection + (!!this.definitionId ? '/' + this.definitionId : '')}/select`;
  }

  private get selectFromUserSettings(): string {
    return this.customUserSettings.getString(this.selectKey);
  }

  private saveSelectToUserSettings(select: string) {
    this.customUserSettings.save(this.selectKey, select);
  }

  private get parentIdsKey(): string {
    const prefix = this.workspace.isApp ? (this.workspace.ws.tenantId + '/') : '';
    const screen = this.collection + (!!this.definitionId ? '/' + this.definitionId : '');

    return `${prefix}${screen}/parent_ids`;
  }

  private get parentIdsFromUserSettings(): (string | number)[] {
    const stringIds = this.storage.getItem(this.parentIdsKey);
    if (stringIds !== this._currentStringIds) {
      this._currentStringIds = stringIds;
      let result = [];
      if (!!stringIds) {
        try {
          result = JSON.parse(stringIds);
        } catch { }
      }

      this._parentIdsFromUserSettings = result;
    }

    return this._parentIdsFromUserSettings;
  }

  private saveParentIdsToUserSettings(ids: (string | number)[]) {
    const stringIds = JSON.stringify(ids);
    this.storage.setItem(this.parentIdsKey, stringIds);
  }


  ////////////// UI Bindings below

  get errorMessage() {
    return this.state.errorMessage;
  }

  get flatIds() {
    return this.state.flatIds;
  }

  get orderBy() {
    return this.state.orderby;
  }

  onOrderBy(path: string, event: MouseEvent) {
    event.stopPropagation();
    path = this.computeOrderBy(path);
    if (!!path) {
      const s = this.state;
      if (!this.isOrderedBy(path)) {
        s.orderby = path;
      } else {
        if (!this.desc) {
          s.orderby = this.reverseOrderBy(path);
        } else {
          s.orderby = null;
        }
      }
      this.exportSkip = 0;
      s.skip = 0;
      this.fetch();
      this.urlStateChange();
    }
  }

  get desc() {
    return this.state.orderby && this.state.orderby.endsWith(' desc');
  }

  public isOrderedBy(path: string) {
    path = this.computeOrderBy(path);
    return this.state.orderby === path || this.state.orderby === this.reverseOrderBy(path);
  }

  private computeOrderBy(path: string): string {
    const currentLang = this.translate.currentLang;
    if (this._computeOrderByLang !== currentLang) {
      this._computeOrderByLang = currentLang;
      this._computeOrderByCache = {};
    }

    if (!path) {
      return null;
    }

    if (!this._computeOrderByCache[path]) {
      let result = path || '';
      if (!result) {
        return null;
      } else if (result === '(Description)') {
        result = this.entityDescriptor.orderby().join(',');
      } else {

        try {
          const entityDesc = entityDescriptorImpl(result.split('/'),
            this.collection, this.definitionId, this.workspace, this.translate);

          if (!!entityDesc) {
            result = entityDesc.orderby().map(e => `${result}/${e}`).join(',');
          }

        } catch { }
      }

      this._computeOrderByCache[path] = result;
    }

    return this._computeOrderByCache[path];
  }

  private reverseOrderBy(orderby: string): string {

    if (!orderby) {
      return null;
    }

    if (!this._reverseOrderByCache[orderby]) {
      this._reverseOrderByCache[orderby] = orderby.split(',').map(e => e + ' desc').join(',');
    }

    return this._reverseOrderByCache[orderby];
  }

  get from(): number {
    const s = this.state;
    return Math.min(s.skip + 1, this.total);
  }

  get to(): number {
    const s = this.state;
    if (s.masterStatus === MasterStatus.loaded) {
      // If the data is loaded, just count the data
      return Math.min(s.skip + s.masterIds.length, s.total);
    } else {
      // Otherwise display the selected count while the data is loading
      return Math.min(s.skip + DEFAULT_PAGE_SIZE, s.total);
    }
  }

  get total(): number {
    return this.state.total;
  }

  get totalDisplay(): string {
    const total = this.total;
    if (total >= MAXIMUM_COUNT) {
      return formatNumber(MAXIMUM_COUNT - 1, 'en-GB') + '+';
    } else {
      return total.toString();
    }
  }

  onFirstPage() {
    this.state.skip = 0;
    this.fetch();
    this.urlStateChange();
  }

  get canFirstPage(): boolean {
    return this.canPreviousPage;
  }

  onPreviousPage() {
    const s = this.state;
    s.skip = Math.max(s.skip - DEFAULT_PAGE_SIZE, 0);
    this.fetch();
    this.urlStateChange();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + DEFAULT_PAGE_SIZE;
    this.fetch();
    this.urlStateChange();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  get enableTree(): boolean {
    return this.enableTreeView;
  }

  get searchOrFilter(): boolean {
    return !!this.search || !!this.isAnyFilterCheckedOtherThanInactive;
  }

  get showPagingControls(): boolean {
    return this.isFlatMode || this.searchOrFilter;
  }

  get isTreeMode(): boolean {
    return this.state.isTreeMode;
  }

  get isFlatMode(): boolean {
    return !this.isTreeMode; // this.state.displayMode === MasterDisplayMode.flat;
  }

  get showTilesView(): boolean {
    return this.searchView === SearchView.tiles;
  }

  get showTableView(): boolean {
    return this.searchView === SearchView.table;
  }

  get showErrorMessage(): boolean {
    return this.state.masterStatus === MasterStatus.error;
  }

  get showSpinner(): boolean {
    return this.state.masterStatus === MasterStatus.loading;
  }

  get showNoItemsFound(): boolean {
    return this.state.masterStatus === MasterStatus.loaded &&
      (!this.state.masterIds || this.state.masterIds.length === 0);
  }

  get showImport(): boolean {
    return !this.isPopupMode && this.showImportButton && !this.missingDefinitionId;
  }

  get showExport(): boolean {
    return !this.isPopupMode && this.showExportButton;
  }

  get showExportForImport(): boolean {
    return this.showExport && this.showImport;
  }

  get showDataDropdown(): boolean {
    return this.showImport || this.showExport || this.showCollapseAll;
  }

  get isScreenMode(): boolean {
    return this.mode === 'screen';
  }

  get isPopupMode(): boolean {
    return this.mode === 'popup';
  }

  get isLight() {
    return this.theme === 'light';
  }

  get isDark() {
    return this.theme === 'dark';
  }

  onTreeMode() {
    const s = this.state;
    if (!s.isTreeMode) {
      s.displayMode = MasterDisplayMode.tree;

      // Remove any order by
      s.orderby = null;
      this.exportSkip = 0;
      s.skip = 0;

      this.fetch();
      this.urlStateChange();
    }
  }

  onFlatMode() {
    const s = this.state;
    if (s.isTreeMode) {
      s.displayMode = MasterDisplayMode.flat;
      this.fetch();
      this.urlStateChange();
    }
  }

  onTilesView() {
    this.searchView = SearchView.tiles;
    this.urlStateChange();
  }

  onTableView() {
    this.searchView = SearchView.table;
    this.urlStateChange();
  }

  onCreate() {
    if (!this.canCreate) {
      return;
    }

    if (this.isPopupMode) {
      this.create.emit();
    } else {
      this.router.navigate(['.', 'new'], { relativeTo: this.route });
    }
  }

  onRefresh() {
    // The if statement to deal with incessant button clickers (Users who hit refresh repeatedly)
    if (this.state.masterStatus !== MasterStatus.loading) {
      this.fetch();
    }
  }

  // tslint:disable:member-ordering
  public importErrorMessage: string;
  public importResult: ImportResult;
  public downloadingTemplate = false;

  private importKeyChoicesCollection: string;
  private importKeyChoicesDefinitionId: number;
  private importKeyChoicesResult: SelectorChoice[];

  public get importKeyChoices(): SelectorChoice[] {
    if (this.collection !== this.importKeyChoicesCollection ||
      this.definitionId !== this.importKeyChoicesDefinitionId) {
      this.importKeyChoicesCollection = this.collection;
      this.importKeyChoicesDefinitionId = this.definitionId;
      this.importKeyChoicesResult = [];

      const props = this.entityDescriptor.properties;

      // Collect the names of all foreign keys
      const fks: { [fkName: string]: true } = {};
      for (const propName of Object.keys(props)) {
        const prop = props[propName];
        if (prop.control === 'navigation') {
          fks[prop.foreignKeyName] = true;
        }
      }

      for (const propName of Object.keys(props)) {
        if (fks[propName]) {
          // FKs are not good candidates for user keys
          continue;
        }

        const prop = props[propName];
        if (prop.control === 'text' || (prop.control === 'number' && prop.maxDecimalPlaces === 0)) {
          this.importKeyChoicesResult.push({ value: propName, name: prop.label });
        }
      }
    }

    return this.importKeyChoicesResult;
  }

  private _importModeChoices: SelectorChoice[];
  get importModeChoices(): SelectorChoice[] {

    if (!this._importModeChoices) {
      this._importModeChoices = Object.keys(ImportArguments_Mode)
        .map(key => ({ name: () => this.translate.instant(ImportArguments_Mode[key]), value: key }));
    }

    return this._importModeChoices;
  }

  public importMode: ImportMode = 'Insert';
  public importKey: string;

  public get showImportKey(): boolean {
    return this.importMode === 'Update' || this.importMode === 'Merge';
  }

  onImport() {
    // Try to suggestion a user key
    if (!this.importKey) {
      const choices = this.importKeyChoices;
      if (choices.find(e => e.value === 'Code')) {
        this.importKey = 'Code';
      } else if (choices.find(e => e.value === 'Name')) {
        this.importKey = 'Name';
      } else if (choices.find(e => e.value === 'Label')) {
        this.importKey = 'Label';
      }
    }

    this.modalService.open(this.importModal, { windowClass: 't-import-modal' })
      .result.then(
        // Cleanup
        () => delete this.importErrorMessage,
        () => delete this.importErrorMessage,
      );
  }

  public get enableImportButton(): boolean {
    return !!this.importMode && (this.importMode === 'Insert' || !!this.importKey);
  }

  onSelectFileToImport(input: HTMLInputElement, modal: NgbModalRef) {
    if (!this.canImport) {
      return;
    }

    const files = input.files;
    if (files.length === 0) {
      return;
    }

    const file = files[0];
    input.value = '';

    // Clear any displayed errors
    this.importErrorMessage = null;
    this.importResult = null;

    this.crud.import({ mode: this.importMode, key: this.importKey }, file).subscribe(
      (importResult: ImportResult) => {

        // Close the modal if the import is successful
        modal.close();

        // refresh the data
        this.fetch();

        // // Show the result to the user
        // this.importResult = importResult;
      },
      (friendlyError: any) => {
        this.importErrorMessage = friendlyError.error;
      }
    );
  }

  public onDownloadTemplate() {
    this.notifyDownloadTemplate$.next();
  }

  public doDownloadTemplate() {
    this.downloadingTemplate = true;
    return this.crud.template({}).pipe(
      tap((template: Blob) => {
        this.downloadingTemplate = false;
        downloadBlob(template, this.masterCrumb + '.csv');
      }),
      catchError((friendlyError: any) => {
        this.downloadingTemplate = false;
        this.displayErrorModal(friendlyError.error);
        return of(null);
      }),
      finalize(() => {
        this.downloadingTemplate = false;
      })
    );
  }

  /**
   * Returns true when the screen is a generic master screen of otherwise definitioned entities
   * e.g. screen Resources or Relations showing entities of various DefinitionId
   */
  private get missingDefinitionId(): boolean {
    return !!this.entityDescriptor.definitionIds && !this.definitionId;
  }

  public onChoose(id: number | string, isEdit?: boolean) {
    if (this.isPopupMode) {
      this.choose.emit(id);
    } else {
      const customNav = this.entityDescriptor.navigateToDetails;
      if (!!customNav) {
        // If a custom choice handler is provided use that
        const entity = this.workspace.current[this.collection][id] as Entity;
        customNav(entity, this.router, this.mdStateKey);
      } else if (this.missingDefinitionId) {
        // If this screen is a generic master screen of definitioned entities do two things:
        // (1) Make sure the definition Id is in the target route
        // (2) Add state_key param to let the details screen use the same state object
        const definitionId = this.workspace.current[this.collection][id].DefinitionId;
        const extras = { state_key: this.mdStateKey };
        this.workspace.isEdit = isEdit;
        this.router.navigate(['.', definitionId, id, extras], { relativeTo: this.route });
      } else {
        this.workspace.isEdit = isEdit;
        this.router.navigate(['.', id], { relativeTo: this.route });
      }
    }
  }

  public get showCreate() {
    return this.showCreateButton && (this.isPopupMode || !this.missingDefinitionId);
  }

  public get canCreatePermissions(): boolean {
    return this.workspace.current.canCreate(this.view);
  }

  private get notArchived(): boolean {
    return !this.entityDescriptor.isArchived;
  }

  public get canCreate(): boolean {
    return this.canCreatePermissions && this.notArchived;
  }

  public get createTooltip(): string {
    return !this.canCreatePermissions ? this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions') :
      !this.notArchived ? this.translate.instant('Error_DefinitionIsArchived') : '';
  }

  public get canImportPermissions(): boolean {
    return this.workspace.current.canCreate(this.view);
  }

  public get canImport(): boolean {
    return this.canImportPermissions;
  }

  public get importTooltip(): string {
    return this.canImportPermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public get canExportPermissions(): boolean {
    return this.workspace.current.canRead(this.view);
  }

  public get canExport(): boolean {
    return this.canExportPermissions;
  }

  public get exportTooltip(): string {
    return this.canExportPermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public get showDelete() {
    return this.showDeleteButton;
  }

  public get showDeleteWithDescendants() {
    return this.showDelete && this.isTreeMode;
  }

  public get canDeletePermissions(): boolean {
    return this.workspace.current.canDo(this.view, 'Delete', null);
  }

  public get canDelete(): boolean {
    return this.canDeletePermissions;
  }

  public get deleteTooltip(): string {
    return this.canDeletePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public trackById(_: any, id: number | string) {
    return id;
  }

  public trackByNodeId(_: any, node: NodeInfo) {
    return node.id;
  }

  public colWidth(colPath: string) {
    // This returns an html percentage width based on the weights assigned to this column and all the other columns

    // Get the weight of this column
    const weight = colPath === '(Description)' ? 2 : 1;

    // Get the total weight of all columns
    const totalWeight = 2 + this.tableColumnPaths.length; // to account for the description column

    // if totalweight = 0 this method will never be called in the first place)
    return ((weight / totalWeight) * 100) + '%';
  }

  public get search(): string {
    return this.state.search;
  }

  public set search(val: string) {
    if (!val) {
      val = null;
    }

    const s = this.state;
    if (s.search !== val) {
      s.search = val;
    }
    this.searchChanged$.next(val);
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get errorPopoverPlacement() {
    return this.workspace.ws.isRtl ? 'left' : 'right';
  }

  public get filterDropdownPlacement(): string {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  public get tableColumnPaths(): string[] {
    const select = this.state.select || '';
    if (select !== this._selectOld) {
      this._selectOld = select;
      this._tableColumnPaths = select.split(',').map(e => e.trim()).filter(e => !!e);
    }
    return this._tableColumnPaths;
  }

  // Export-related stuff
  public get showExportPaging(): boolean {
    return this.maxTotalExport < this.total;
  }

  public get fromExport(): number {
    return Math.min(this.exportSkip + 1, this.totalExport);
  }

  public get toExport(): number {
    return Math.min(this.exportSkip + this.maxTotalExport, this.totalExport);
  }

  public get totalExport(): number {
    return this.total;
  }

  public get canPreviousPageExport() {
    return this.exportSkip > 0;
  }

  public get canNextPageExport() {
    return this.toExport < this.totalExport;
  }

  public onPerviousPageExport() {
    this.exportSkip = Math.max(this.exportSkip - this.exportPageSize, 0);
  }

  public onNextPageExport() {
    this.exportSkip = this.exportSkip + this.exportPageSize;
  }

  get maxTotalExport(): number {
    return this.exportPageSize;
  }

  public onExport(exportPagingModal: TemplateRef<any>, mode: ExportMode): void {
    this.exportMode = mode;
    if (this.showExportPaging) {
      // If the number of records is large, paging is required, open the paging modal
      this.modalService.open(exportPagingModal);
    } else {
      // If the number of records is small, and paging isn't required, export straight away
      this.onDoExport();
    }
  }

  /**
   * Exports all search results, if there are too many it will prompt the user to choose a page to export
   */
  public onExportForImport(exportPagingModal: TemplateRef<any>): void {

    if (this.showExportPaging) {
      // If the number of records is large, paging is required, open the paging modal
      this.modalService.open(exportPagingModal);
    } else {
      // If the number of records is small, and paging isn't required, export straight away
      this.onDoExport();
    }
  }

  private exportMode: ExportMode;
  public onDoExport(): void {
    if (!this.canExport) {
      return;
    }

    const from = this.fromExport;
    const to = this.toExport;
    const s = this.state;
    const filter = this.computeFilter(s);

    let obs$: Observable<Blob>;
    if (this.exportMode === 'ForImport') {
      obs$ = this.crud.export({
        top: this.exportPageSize,
        skip: this.exportSkip,
        orderby: s.orderby,
        search: s.search,
        filter
      });
    } else if (this.exportMode === 'WhatISee') {
      const colPaths = this.tableColumnPaths;
      obs$ = this.crud.getFact({
        top: this.exportPageSize,
        skip: this.exportSkip,
        orderby: s.orderby,
        search: s.search,
        filter,
        select: this.computeSelectForExport(),
        countEntities: false
      }).pipe(
        map(response => {
          const paths = colPaths.slice();
          paths.unshift(''); // For the description
          const data = composeEntities(response, paths, this.collection, this.definitionId, this.workspace, this.translate);
          return csvPackage(data);
        })
      );
    } else {
      // Future proofing
      console.error(`Unknown export mode ${this.exportMode}`);
    }

    this.showExportSpinner = true;
    obs$.pipe(
      tap((blob: Blob) => {
        this.showExportSpinner = false;
        const fileName = `${this.exportFileName || this.masterCrumb} ${from}-${to}.csv`;
        downloadBlob(blob, fileName);
      }),
      catchError(friendlyError => {
        this.showExportSpinner = false;
        this.displayErrorModal(friendlyError.error);
        return of();
      }),
      finalize(() => {
        this.showExportSpinner = false;
        this.cdr.markForCheck();
      })
    ).subscribe();
  }

  /**
   * Exports records that are multi-selected by the user
   */
  public onExportByIds(mode: ExportMode) {
    if (!this.canExport) {
      return;
    }

    // Grab the selected Ids
    const ids = this.checkedIds;
    if (!ids || ids.length === 0) {
      return;
    }

    let obs$: Observable<Blob>;
    if (mode === 'ForImport') {
      obs$ = this.crud.exportByIds(ids);
    } else if (mode === 'WhatISee') {
      const colPaths = this.tableColumnPaths;
      obs$ = this.crud.getByIds(ids, {
        select: this.computeSelectForExport(),
        i: ids
      }).pipe(
        map(response => {
          const paths = colPaths.slice();
          paths.unshift(''); // For the description
          const data = composeEntities(response, paths, this.collection, this.definitionId, this.workspace, this.translate);
          return csvPackage(data);
        })
      );
    } else {
      // Future proofing
      console.error(`Unknown export mode ${mode}`);
    }

    this.showExportSpinner = true;
    obs$.pipe(tap(
      (blob: Blob) => {
        this.showExportSpinner = false;
        const fileName = `${this.exportFileName || this.masterCrumb}.csv`;
        downloadBlob(blob, fileName);
      },
      (friendlyError: any) => {
        this.showExportSpinner = false;
        this.displayErrorModal(friendlyError.error);
      },
      () => {
        this.showExportSpinner = false;
      }
    ),
      finalize(() => this.cdr.markForCheck())
    ).subscribe();
  }

  // Multiselect-related stuff

  public get showCheckboxes(): boolean {
    return this.isScreenMode && !this.missingDefinitionId && (
      (!!this.multiselectActions && this.multiselectActions.length > 0) ||
      this.showDelete || this.showDeleteWithDescendants);
  }

  public get canCheckAll(): boolean {
    return this.displayedIds.length > 0;
  }

  public get areAllChecked(): boolean {
    return this.displayedIds.length > 0 && this.displayedIds.every(id => !!this.checked[id]);
  }

  public get areAnyChecked(): boolean {
    return this.displayedIds.some(id => !!this.checked[id]);
  }

  public get checkedCount(): number {
    return this.checkedIds.length;
  }

  public get checkedIds(): (number | string)[] {
    return this.displayedIds.filter(id => !!this.checked[id]);
  }

  public onCheckAll() {
    if (this.areAllChecked) {
      // Uncheck all
      this.checked = {};

    } else {
      // Check all
      this.displayedIds.forEach(id => this.checked[id] = true);
    }
  }

  public get displayedIds(): (string | number)[] {
    if (this.isTreeMode) {
      return this.treeNodes.filter(node => this.showTreeNode(node)).map(node => node.id);
    } else {
      return this.flatIds;
    }
  }

  public onCancelMultiselect() {
    this.checked = {};
    this.actionValidationErrors = {};
  }

  public canAction(action: MultiselectAction) {
    if (!!action.canAction) {
      return action.canAction(this.checkedIds);
    } else {
      // true by default
      return true;
    }
  }

  public showAction(action: MultiselectAction): boolean {

    if (!!action.showAction) {
      return action.showAction(this.checkedIds);
    } else {
      // true by default
      return true;
    }
  }

  public actionTooltip(action: MultiselectAction) {
    if (!!action.actionTooltip) {
      return action.actionTooltip(this.checkedIds);
    } else {
      // don't show a tooltip by default
      return '';
    }
  }

  public onAction(action: MultiselectAction) {
    // clear any previous errors
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};

    const ids = this.checkedIds;
    action.action(ids).pipe(tap(
      () => this.checked = {},
      (friendlyError: any) => {
        this.handleActionError(ids, friendlyError);
      }
    ),
      finalize(() => this.cdr.markForCheck())
    ).subscribe();
  }

  public onDelete() {
    const ids = this.checkedIds;
    this.onDeleteImpl(ids);
  }

  private onDeleteImpl(ids: (string | number)[]) {
    // clear any previous errors
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};

    this.crud.delete(ids).pipe(tap(
      () => this.onSuccessfulDelete(ids),
      (friendlyError: any) => {
        this.handleActionError(ids, friendlyError);
      }
    ),
      finalize(() => this.cdr.markForCheck())
    ).subscribe();
  }

  public onSuccessfulDelete = (ids: (string | number)[]) => {
    // Update the UI to reflect deletion of items
    this.state.delete(ids, this.workspace.current[this.state.collectionName]);
    this.checked = {};
    if (this.displayedIds.length === 0 && this.total > 0) {
      // auto refresh if the user deleted the entire page
      this.fetch();
    }
  }

  public onDeleteFromContextMenu(id: string | number, deleteModal: TemplateRef<any>) {
    if (this.canDelete) {
      // Check only that box
      this.checked = {};
      this.checked[id] = true;
      this.cdr.markForCheck();

      this.modalService.open(deleteModal);
    }
  }

  public onEditFromContextMenu(id: string | number) {
    if (this.canCreate) {
      this.onChoose(id, true);
    }
  }

  public onDeleteWithDescendants() {
    // clear any previous errors
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};

    const ids = this.checkedIds;
    this.crud.deleteWithDescendants(ids).pipe(tap(
      () => {
        // Update the UI to reflect deletion of items
        this.state.delete(ids, this.workspace.current[this.state.collectionName]);
        this.checked = {};
        if (this.displayedIds.length === 0 && this.total > 0) {
          // auto refresh if the user deleted the entire page
          this.fetch();
        }
      },
      (friendlyError: any) => {
        this.handleActionError(ids, friendlyError);
      }
    ),
      finalize(() => this.cdr.markForCheck())
    ).subscribe();
  }

  private handleActionError(ids: (string | number)[], friendlyError) {
    // This handles any errors caused by actions

    if (friendlyError.status === 422) {
      const keys = Object.keys(friendlyError.error);
      const tracker: { [id: string]: { [error: string]: true } } = {};
      keys.forEach(key => {
        // Validation error keys are expected to look like this '[33].XYZ'
        // The code below extracts the index and maps it back to the correct id
        const pieces = key.split(']'); // ['[33', '.XYZ']
        if (pieces.length > 1) {
          let firstPiece = pieces[0]; // '[33'
          if (firstPiece.startsWith('[')) {
            firstPiece = firstPiece.substring(1, firstPiece.length); // '33'
            const index = +firstPiece; // 33
            if (index < ids.length) {
              // Get the Id that corresponds to this index
              const id = ids[index];
              if (!this.actionValidationErrors[id]) {
                this.actionValidationErrors[id] = [];
              }

              if (!tracker[id]) {
                tracker[id] = {};
              }

              friendlyError.error[key].forEach((errorMessage: string) => {
                // action errors map ids to list of errors messages
                if (!tracker[id][errorMessage]) {
                  tracker[id][errorMessage] = true; // Don't add the same message more than once
                  this.actionValidationErrors[id].push(errorMessage);
                }
              });
            } else {
              // Developer mistake
              console.error('The key index returned was outside the range of the collection sent: ' + key);
            }
          } else {
            // Developer mistake
            console.error('One of the keys in the 422 response did not contain an opening square bracket [: ' + key);
          }
        } else {
          // Developer mistake
          console.error('One of the keys in the 422 response did not contain a closing square bracket ]: ' + key);
        }
      });

      this.displayErrorModal(this.translate.instant('ActionDidNotPassValidation'));
    } else {
      this.displayErrorModal(friendlyError.error);
    }
  }

  private displayErrorModal(errorMessage: string): void {
    this.actionErrorMessage = errorMessage;
    this.modalService.open(this.errorModal);
  }

  public showErrorHighlight(id: string | number): boolean {
    return this.actionValidationErrors[id] && this.actionValidationErrors[id].length > 0;
  }

  // filter-related stuff

  private computeFilter(s: MasterDetailsStore): string {
    const filterState = s.builtInFilterSelections;
    const disjunctions: string[] = [];
    const groupNames = Object.keys(this.filterDefinition);
    for (const groupName of groupNames) {
      const expressions: string[] = [];
      const groupState = filterState[groupName];
      if (!!groupState) {
        for (const expression in groupState) {
          if (groupState[expression]) {
            expressions.push(expression);
          }
        }
      }
      const disjunction = expressions.join(' or ');
      if (!!disjunction) {
        disjunctions.push(disjunction);
      }
    }

    // built-in filter created from the user's multi-selection of built-in expression
    let builtin = disjunctions.join(') and (');
    if (!!builtin) {
      builtin = `(${builtin})`;
    }

    // custom filter entered directly by the user
    const custom = this.state.customFilter;

    // AND the custom filter and the built-in filter together
    let filter = (!!custom && !!builtin) ? `(${custom}) and (${builtin})` :
      !!custom ? custom : !!builtin ? builtin : null;

    // Add inactive if specified
    if (!s.inactive && this.showIncludeInactive) {
      if (!!filter) {
        filter = `(${filter}) and (${this.inactiveFilter})`;
      } else {
        filter = this.inactiveFilter;
      }
    }

    return filter;
  }

  public get inactiveFilter(): string {
    const desc = this.entityDescriptor;
    return desc.inactiveFilter;
  }

  public get includeInactiveLabel(): string {
    const desc = this.entityDescriptor;
    return !!desc.includeInactveLabel ? desc.includeInactveLabel() : this.translate.instant('IncludeInactive');
  }

  public get showIncludeInactive(): boolean {
    return !!this.inactiveFilter; // If the label is not specified then hide the option
  }

  public onIncludeInactive(): void {
    const s = this.state;
    s.inactive = !s.inactive;
    this.fetch();
    this.urlStateChange();
  }

  public get isIncludeInactive(): boolean {
    return this.state.inactive;
  }

  public onFilterCheck(groupName: string, expression: string) {
    const filterGroups = this.state.builtInFilterSelections;
    if (!filterGroups[groupName]) {
      filterGroups[groupName] = {};
    }

    const group = filterGroups[groupName];
    group[expression] = !group[expression];
    this.state.skip = 0;
    this.exportSkip = 0;

    this.fetch();
    this.urlStateChange();
  }

  public isFilterChecked(groupName: string, expression: string): boolean {
    const s = this.state.builtInFilterSelections;
    return !!s[groupName] && !!s[groupName][expression];
  }

  public get isAnyFilterChecked(): boolean {
    // when this is true the UI shows the red circle
    // This code checks whether any expression in any group is checked, also if include inactive is checked
    return this.state.inactive || this.isAnyFilterCheckedOtherThanInactive;
  }

  public get isAnyFilterCheckedOtherThanInactive(): boolean {
    // when this is true, the way data is queried in tree view changes from paged to not paged
    return Object.keys(this.filterDefinition).some(groupName => {
      const group = this.filterDefinition[groupName];
      return group.some(e => this.isFilterChecked(groupName, e.expression));
    }) || !!this.state.customFilter;
  }

  public onClearFilter() {
    if (this.isAnyFilterChecked) {
      const s = this.state;
      s.inactive = false;
      s.builtInFilterSelections = {};
      s.customFilter = null;
      s.skip = 0;
      this.exportSkip = 0;
      this.fetch();
      this.urlStateChange();
    }
  }

  public get groupNames(): string[] {
    return Object.keys(this.filterDefinition);
  }

  public filterTemplates(groupName: string): {
    template: TemplateRef<any>,
    expression: string
  }[] {
    return this.filterDefinition[groupName];
  }

  // END filter related stuff

  public isRecentlyViewed(id: number | string) {
    return this.state.detailsId === id;
  }

  public onCancel() {
    this.cancel.emit();
  }

  public get customFilter(): string {
    return this.state.customFilter;
  }

  public set customFilter(v: string) {
    v = v || null;
    if (this.state.customFilter !== v) {
      this.state.customFilter = v;
      this.state.skip = 0;
      this.exportSkip = 0;
      this.fetch();
      this.urlStateChange();
    }
  }

  public get stateSelect(): string {
    return this.state.select;
  }

  public set stateSelect(v: string) {
    v = v || null;
    if (this.state.select !== v) {
      this.state.select = v;

      this.fetch();
      this.urlStateChange();
      this.saveSelectToUserSettings(v);
    }
  }

  public onDragLeave(e: CdkDragDrop<string[]>) {
    const paths = this.tableColumnPaths;
    const currIndex = this.workspace.ws.isRtl ? (paths.length - e.currentIndex - 1) : e.currentIndex;
    const prevIndex = e.previousIndex;
    if (prevIndex !== currIndex) {
      moveItemInArray(paths, prevIndex, currIndex);
      this.state.select = paths.join(',');

      this.urlStateChange();
      this.saveSelectToUserSettings(this.state.select);
    }
  }

  public get editingColumns(): boolean {
    return this._editingColumns;
  }

  public set editingColumns(v: boolean) {
    this._editingColumns = v;
  }

  public onEditColumns() {
    this.editingColumns = !this.editingColumns;
  }

  public onDeleteColumn(index: number) {
    const paths = this.tableColumnPaths;
    this.state.select = paths.filter((_: string, i: number) => index !== i).join(',');

    this.urlStateChange();
    this.saveSelectToUserSettings(this.state.select);
  }

  public entity(id: string | number) {
    return this.workspace.current.get(this.collection, id);
  }

  ////////////////// TREE STUFF

  public showTreeNode(node: NodeInfo) {
    const parent = node.parent;
    return !parent || (parent.isExpanded && this.showTreeNode(parent));
  }

  public get treeNodes(): NodeInfo[] {
    return this.state.treeNodes;
  }

  public onExpand(node: NodeInfo): void {
    node.isExpanded = !node.isExpanded;
    if (node.isExpanded && node.status !== MasterStatus.loaded &&
      node.status !== MasterStatus.loading && !this.searchOrFilter) {
      this.fetchNodeChildren(node);
    }

    // Calculate the nodes that are both visible and expanded
    const expandedIds = this.state.treeNodes.filter(n => n.isExpanded && this.showTreeNode(n)).map(n => n.id);
    this.saveParentIdsToUserSettings(expandedIds);
  }

  public paddingLeft(node: NodeInfo): string {
    return this.workspace.ws.isRtl ? '0' : (this.treeControl.getLevel(node) * 30) + 'px';
  }

  public paddingRight(node: NodeInfo): string {
    return this.workspace.ws.isRtl ? (this.treeControl.getLevel(node) * 30) + 'px' : '0';
  }

  public flipNode(node: NodeInfo): string {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl && !node.isExpanded ? 'horizontal' : null;
  }

  public rotateNode(node: NodeInfo): number {
    return node.isExpanded ? 90 : 0;
  }

  public showNodeSpinner(node: NodeInfo): boolean {
    return node.status === MasterStatus.loading;
  }

  public hasChildren(node: NodeInfo): boolean {
    return node.hasChildren;
  }

  public get canCollapseAll(): boolean {
    return true;
  }

  public get showCollapseAll(): boolean {
    return this.isTreeMode && !this.searchOrFilter;
  }

  public onCollapseAll(): void {
    const someNodesAreExpanded = this.treeNodes.some(e => e.isExpanded);
    if (someNodesAreExpanded) {
      this.treeNodes.forEach(e => e.isExpanded = false);
    }

    this.saveParentIdsToUserSettings([]);
  }

  public get disableContextMenu(): boolean {
    return this.isPopupMode || (!this.showDelete && !this.showCreate);
  }
}

function metadataFactory(collection: string) {
  const factory = metadata[collection]; // metadata factory for User
  if (!factory) {
    throw new Error(`The collection ${collection} does not exist`);
  }

  return factory;
}

function composeEntities(
  response: EntitiesResponse,
  colPaths: string[],
  collection: string,
  defId: number,
  ws: WorkspaceService,
  trx: TranslateService): string[][] {

  // This array will contain the final result
  const result: string[][] = [];

  // This is the base descriptor
  const baseDesc: EntityDescriptor = metadataFactory(collection)(ws, trx, defId);

  // Step 1: Prepare the headers and extractors
  const headers: string[] = []; // Simple array of header displays
  const extracts: ((e: Entity) => string)[] = []; // Array of functions, one for each column to get the string value

  for (const path of colPaths) {
    const pathArray = (path || '').split('/').map(e => e.trim()).filter(e => !!e);

    // This will contain the display steps of a single header. E.g. Item / Created By / Name
    const headerArray: string[] = [];
    const navProps: NavigationPropDescriptor[] = [];
    let finalPropDesc: PropDescriptor = null;

    // Loop over all steps except last one
    let isError = false;
    let currentDesc = baseDesc;

    for (let i = 0; i < pathArray.length; i++) {
      const step = pathArray[i];
      const prop = currentDesc.properties[step];
      if (!prop) {
        isError = true;
        break;
      } else {
        headerArray.push(prop.label());
        if (prop.control === 'navigation') {
          currentDesc = metadataFactory(prop.collection || prop.type)(ws, trx, prop.definition);
          navProps.push(prop);
        } else if (i !== pathArray.length - 1) {
          // Only navigation properties are allowed unless this is the last one
          isError = true;
        } else {
          finalPropDesc = prop;
        }
      }
    }

    // Prepare the entities in a dictionary for fast lookup by Id
    const relatedEntities: { [key: string]: { [id: string]: EntityWithKey } } = {};
    for (const coll of Object.keys(response.RelatedEntities)) {
      const entitiesOfTypeArray = response.RelatedEntities[coll];
      const entitiesOfType = (relatedEntities[coll] = {});
      for (const entity of entitiesOfTypeArray) {
        entitiesOfType[entity.Id] = entity;
      }
    }
    {
      // Don't forget the main collection (important for self referencing trees)
      const coll = response.CollectionName;
      const entitiesOfType = (relatedEntities[coll] = {});
      for (const entity of response.Result) {
        entitiesOfType[entity.Id] = entity;
      }
    }

    if (isError) {
      headers.push(`(${trx.instant('Error')})`);
      extracts.push(_ => `(${trx.instant('Error')})`);
    } else {
      headers.push(headerArray.join(' / ') || baseDesc.titleSingular() || trx.instant('DisplayName'));
      extracts.push(entity => {
        let i = 0;
        for (; i < navProps.length; i++) {
          const navProp = navProps[i];
          const propName = pathArray[i];

          if (entity.EntityMetadata[propName] === 2 || propName === 'Id') {

            const entitiesOfType = relatedEntities[navProp.collection || navProp.type];

            // Get the foreign key
            const fkValue = entity[navProp.foreignKeyName];
            if (!fkValue) {
              return ''; // The nav entity is null
            }

            // Get the nav entity
            entity = entitiesOfType[fkValue];
            if (!entity) {
              // Anomaly from Server
              console.error(`Property ${propName} loaded but null, even though FK ${navProp.foreignKeyName} is loaded`);
              return `(${trx.instant('Error')})`;
            }
          } else if (entity.EntityMetadata[propName] === 1) {
            // Masked because of user permissions
            return `*******`;
          } else {
            // Bug
            return `(${trx.instant('NotLoaded')})`;
          }
        }

        // Final step
        if (!!finalPropDesc) {
          const propName = pathArray[i];
          if (entity.EntityMetadata[propName] === 2 || propName === 'Id') {
            const val = entity[propName];
            return displayValue(val, finalPropDesc, trx);
          } else if (entity.EntityMetadata[propName] === 1) {
            // Masked because of user permissions
            return `*******`;
          } else {
            // Bug
            return `(${trx.instant('NotLoaded')})`;
          }
        } else {
          // It terminates with a nav prop
          return displayEntity(entity, currentDesc);
        }
      });
    }
  }

  // Step 2 Push headers in the result
  result.push(headers);

  // Step 3 Use extractors to convert the entities to strings and push them in the result
  for (const entity of response.Result) {
    const row: string[] = [];
    let index = 0;
    for (const extract of extracts) {
      row[index++] = extract(entity);
    }

    result.push(row);
  }

  // Finally: Return the result
  return result;
}
