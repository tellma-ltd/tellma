import {
  Component, EventEmitter, Input, OnDestroy, OnInit, Output, TemplateRef,
  ViewChild, OnChanges, SimpleChanges, ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { merge, Observable, of, Subject, Subscription } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap, tap, finalize } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { GetResponse, EntitiesResponse } from '~/app/data/dto/get-response';
import { TemplateArguments_format } from '~/app/data/dto/template-arguments';
import { addToWorkspace, downloadBlob } from '~/app/data/util';
import {
  MasterDetailsStore,
  MasterStatus,
  WorkspaceService,
  NodeInfo,
  MasterDisplayMode,
  TreeRefreshMode,
  DEFAULT_PAGE_SIZE as DEFAULT_PAGE_SIZE
} from '~/app/data/workspace.service';
import { FlatTreeControl } from '@angular/cdk/tree';
import { metadata, EntityDescriptor, entityDescriptorImpl } from '~/app/data/entities/base/metadata';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { handleFreshUserSettings } from '~/app/data/tenant-resolver.guard';
import { StorageService } from '~/app/data/storage.service';

enum SearchView {
  tiles = 'tiles',
  table = 'table'
}

export interface MultiselectAction {
  template: TemplateRef<any>;
  action: (ids: (string | number)[]) => Observable<any>;
  canAction?: (ids: (string | number)[]) => boolean;
  actionTooltip?: (ids: (string | number)[]) => string;
  showAction?: (ids: (string | number)[]) => boolean;
}

@Component({
  selector: 'b-master',
  templateUrl: './master.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MasterComponent implements OnInit, OnDestroy, OnChanges {

  @Input()
  masterCrumb: string;

  @Input()
  collection: string; // This is one of two properties that define the screen

  @Input()
  definition: string; // This is one of two properties that define the screen

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
  inactiveFilter = 'IsActive eq true';

  @Input()
  multiselectActions: MultiselectAction[] = [];

  @Input()
  includeInactiveLabel: string;

  @Input()
  selectDefault: string;

  @Input()
  selectForTiles: string;

  @Input()
  filterDefault: string;

  @Input()
  skipInput: number;

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

  private localState = new MasterDetailsStore();  // Used in popup mode
  private searchChanged$ = new Subject<string>();
  private notifyFetch$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private notifySaveSettingsOnServer$ = new Subject<{ key: string, value: string }>();
  private _formatChoices: { name: string, value: any }[];
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
  public exportFormat: 'csv' | 'xlsx';
  public exportSkip = 0;
  public showExportSpinner = false;
  public exportErrorMessage: string;
  public actionErrorMessage: string;
  public actionValidationErrors: { [id: string]: string[] } = {};
  public treeControl = new FlatTreeControl<NodeInfo>(node => node.level - 1, node => node.hasChildren);

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private router: Router, private cdr: ChangeDetectorRef,
    private route: ActivatedRoute, private translate: TranslateService, public modalService: NgbModal, private storage: StorageService) {
  }

  ngOnInit() {

    // Use some RxJS magic to refresh the data as the user changes the parameters
    const searchBoxSignals = this.searchChanged$.pipe(
      debounceTime(175),
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

    this._subscriptions.add(this.notifySaveSettingsOnServer$.pipe(
      switchMap(args => this.doSaveSettingsOnServer(args.key, args.value))
    ).subscribe());

    this._subscriptions.add(this.workspace.stateChanged$.subscribe({
      next: () => this.cdr.markForCheck()
    }));

    // Reset the state of the master component state
    this.localState = new MasterDetailsStore();
    this._formatChoices = null;
    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);
    this.checked = {};
    this.exportFormat = 'xlsx';
    this.exportSkip = 0;
    this.showExportSpinner = false;
    this.exportErrorMessage = null;
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
    this.searchView = (!!window && window.innerWidth >= 1050) ? SearchView.table : SearchView.tiles;
    let hasChanged = false;

    // default display mode
    let displayMode: MasterDisplayMode = this.enableTreeView ?
      MasterDisplayMode.tree : MasterDisplayMode.flat; // Default search view

    // here we handle the URL parameters
    // taking a snapshot is enough as this is only required first time the screen loads
    // subsequent changes (while the component is alive) are set in the state before the url
    // is updated, to handle popup mode (where the URL params are not used)
    const params = this.route.snapshot.paramMap;
    if (this.isPopupMode) {

      // select
      const select = this.selectFromUserSettings || this.selectDefault || '';
      if (select !== this.state.select) {
        this.state.select = select;
        hasChanged = true;
      }

      // filter
      const filter = this.filterDefault || null;
      if (filter !== this.state.customFilter) {
        this.state.customFilter = filter;
        hasChanged = true;
      }

    } else {

      // this is only in screen mode
      const view = params.get('view');
      if (!!view && !!SearchView[view]) {
        this.searchView = SearchView[view];
      }

      // display mode: has a precise default value
      displayMode = !!params.get('display') && !!MasterDisplayMode[params.get('display')] ?
        MasterDisplayMode[params.get('display')] : displayMode; // Default search view

      // select
      const urlSelect = params.get('select') || this.selectFromUserSettings || this.selectDefault || '';
      if (urlSelect !== this.state.select) {
        this.state.select = urlSelect;
        hasChanged = true;
      }

      // filter
      const urlFilter = params.get('filter') || null;
      if (urlFilter !== this.state.customFilter) {
        this.state.customFilter = urlFilter;
        hasChanged = true;
      }

      // search
      const urlSearch = params.get('search') || null;
      if (urlSearch !== this.state.search) {
        this.state.search = urlSearch;
        hasChanged = true;
      }

      // orderby
      const urlOrderby = params.get('orderby') || null;
      if (urlOrderby !== this.state.orderby) {
        this.state.orderby = urlOrderby;
        hasChanged = true;
      }

      // skip
      let urlSkip = +params.get('skip') || 0;
      urlSkip = urlSkip < 0 ? 0 : urlSkip;
      if (urlSkip !== this.state.skip) {
        this.state.skip = urlSkip;
        hasChanged = true;
      }
    }

    // // level (This is not part of the screen URL)
    // const level = this.levelFromUserSettings || 1;
    // if (level !== this.state.level) {
    //   this.state.level = level;
    //   hasChanged = true;
    // }

    // display mode: has a precise default value
    if (this.state.displayMode !== displayMode) {
      this.state.displayMode = displayMode;
      hasChanged = true;
    }

    // (hasChanged === true) means we navigated to this screen with different url params than last time
    // (masterStatus !== loaded) means we navigated to this master screen for the first time or after another screen
    if (hasChanged || this.state.masterStatus !== MasterStatus.loaded) {
      this.fetch();
    }
  }

  ngOnDestroy() {
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
    const screenDefProperties = [changes.collection, changes.definition];

    const anyChanges = screenDefProperties.some(prop => !!prop);
    const notFirstChange = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());

    if (anyChanges && notFirstChange) {

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
      const filter = s.inactive ? null : `(${this.inactiveFilter}) or ActiveChildCount gt 0`;
      const parentIds: (string | number)[] = this.parentIdsFromUserSettings;

      obs$ = this.crud.getChildrenOf({
        i: parentIds,
        select,
        filter,
        roots: true
      });

    } else {
      const top = DEFAULT_PAGE_SIZE;
      const skip = s.skip;
      const orderby = isTree ? 'Node' : s.orderby;
      const search = s.search;

      // compute the filter
      let filter = this.filter();
      if (!s.inactive) {
        if (!!filter) {
          filter = `(${filter}) and (${this.inactiveFilter})`;
        } else {
          filter = this.inactiveFilter;
        }
      }

      // Retrieve the entities
      obs$ = this.crud.get({
        top,
        skip,
        orderby,
        search,
        select,
        filter,
      }).pipe(
        tap((response: GetResponse) => {
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
        s.bag = response.Bag;
        s.collectionName = response.CollectionName;

        // add to the relevant collection depending on mode
        if (this.isTreeMode) {
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

    const filter = s.inactive ? null : `(${this.inactiveFilter}) or ActiveChildCount gt 0`;
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
      if (!this.workspace.current.mdState[this.apiEndpoint]) {
        this.workspace.current.mdState = {}; // This forces any other master/details screen to refresh
        this.workspace.current.mdState[this.apiEndpoint] = new MasterDetailsStore();
      }

      return this.workspace.current.mdState[this.apiEndpoint];
    }
  }

  private get viewId(): string {
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

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    }
  }

  // Calculated screen properties

  get entityDescriptor(): EntityDescriptor {
    const coll = this.collection;
    return !!coll ? metadata[coll](this.workspace.current, this.translate, this.definition) : null;
  }

  get apiEndpoint(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.apiEndpoint : null;
  }

  get titleSingular(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.titleSingular : null;
  }

  private computeSelect(): string {
    const select = this.state.select || '';
    const resultPaths: { [path: string]: boolean } = {};
    const baseEntityDescriptor = this.entityDescriptor;

    // (0) add select for tiles
    if (this.selectForTiles) {
      this.selectForTiles.split(',').forEach(e => resultPaths[e] = true);
    }

    // (1) append the current entity type default properties (usually 'Name', 'Name2' and 'Name3')
    baseEntityDescriptor.select.forEach(e => resultPaths[e] = true);

    // (2) append the definitoinId if any, it must always be loaded
    if (!!baseEntityDescriptor.selectForDefinition) {
      resultPaths[baseEntityDescriptor.selectForDefinition] = true;
    }

    // (3) replace every path that terminates with a nav property (e.g. 'Unit' => 'Unit/Name,Unit/Name2,Unit/Name3')
    select.split(',').forEach(path => {

      const steps = path.split('/').map(e => e.trim());
      path = steps.join('/'); // to trim extra spaces

      try {
        const currentDesc = entityDescriptorImpl(steps, this.collection,
          this.definition, this.workspace.current, this.translate);

        currentDesc.select.forEach(descSelect => resultPaths[`${path}/${descSelect}`] = true);
      } catch {
        resultPaths[path] = true;
      }
    });

    // (4) in tree mode we add tree stuff and ensure that Parent has the same selects as the children
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

  private get selectKey(): string {
    return `${this.collection + (!!this.definition ? '/' + this.definition : '')}/select`;
  }

  private get selectFromUserSettings(): string {
    return this.customSettings[this.selectKey];
  }

  private saveSelectToUserSettings(select: string) {
    this.saveToUserSettings(this.selectKey, select);
  }

  private get parentIdsKey(): string {
    return `${this.collection + (!!this.definition ? '/' + this.definition : '')}/parent_ids`;
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

  private get customSettings() {
    const settings = this.workspace.current.userSettings;
    settings.CustomSettings = settings.CustomSettings || {};
    return settings.CustomSettings;
  }

  private saveToUserSettings(key: string, value: string) {
    const settings = this.workspace.current.userSettings;
    if (!settings.CustomSettings) {
      settings.CustomSettings = {};
    }

    if (!!value) {
      settings.CustomSettings[key] = value;
    } else if (settings.CustomSettings[key]) {
      delete settings.CustomSettings[key];
    }

    // switch map ensures that only the last save is persisted in the workspace
    this.notifySaveSettingsOnServer$.next({ key, value });
  }

  private doSaveSettingsOnServer(key: string, value: string): Observable<any> {
    return this.api.usersApi(this.notifyDestruct$).saveForClient(key, value)
      .pipe(
        tap(x => handleFreshUserSettings(x, this.workspace.ws.tenantId, this.workspace.current, this.storage))
      );
  }

  // private get levelKey(): string {
  //   return `${this.collection + (!!this.definition ? '/' + this.definition : '')}/level`;
  // }

  // private get levelFromUserSettings(): number {
  //   const settings = this.workspace.current.userSettings;
  //   settings.CustomSettings = settings.CustomSettings || {};
  //   return parseInt(settings.CustomSettings[this.levelKey], null);
  // }

  // private saveLevel(level: number) {

  //   const levelString = !!level ? level + '' : null;

  //   // Save the new value with the server, to be used again afterwards
  //   const settings = this.workspace.current.userSettings;
  //   if (!settings.CustomSettings) {
  //     settings.CustomSettings = {};
  //   }
  //   settings.CustomSettings[this.levelKey] = levelString;
  //   this.api.usersApi(this.notifyDestruct$).saveForClient(this.levelKey, levelString)
  //     .pipe(tap(x => handleFreshUserSettings(x, this.workspace.ws.tenantId, this.workspace.current, this.storage)))
  //     .subscribe();
  // }

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

  onOrderBy(path: string) {
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
        result = this.entityDescriptor.orderby.join(',');
      } else {

        try {
          const entityDesc = entityDescriptorImpl(result.split('/'),
            this.collection, this.definition, this.workspace.current, this.translate);

          if (!!entityDesc) {
            result = entityDesc.orderby.map(e => `${result}/${e}`).join(',');
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
      return Math.max(s.skip + s.masterIds.length, 0);
    } else {
      // Otherwise dispaly the selected count while the data is loading
      return Math.min(s.skip + s.top, this.total);
    }
  }

  get total(): number {
    return this.state.total;
  }

  get bag(): any {
    return this.state.bag;
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
    return !this.isPopupMode && this.showImportButton;
  }

  get showExport(): boolean {
    return !this.isPopupMode && this.showExportButton;
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

  onTreeMode() {
    if (this.state.displayMode !== MasterDisplayMode.tree) {
      this.state.displayMode = MasterDisplayMode.tree;
      this.state.orderby = null;
      this.fetch();
      this.urlStateChange();
    }
  }

  onFlatMode() {
    if (this.state.displayMode !== MasterDisplayMode.flat) {
      this.state.displayMode = MasterDisplayMode.flat;
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

  onImport() {
    if (!this.canImport) {
      return;
    }
    this.router.navigate(['.', 'import'], { relativeTo: this.route });
  }

  onSelect(id: number | string) {
    if (this.isPopupMode) {
      this.choose.emit(id);
    } else {
      this.router.navigate(['.', id], { relativeTo: this.route });
    }
  }

  get showCreate() {
    return this.showCreateButton;
  }

  get canCreatePermissions(): boolean {
    return this.workspace.current.canCreate(this.viewId);
  }

  get canCreate(): boolean {
    return this.canCreatePermissions;
  }

  get createTooltip(): string {
    return this.canCreatePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  get canImportPermissions(): boolean {
    return this.workspace.current.canCreate(this.viewId);
  }

  get canImport(): boolean {
    return this.canImportPermissions;
  }

  get importTooltip(): string {
    return this.canImportPermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  get canExportPermissions(): boolean {
    return this.workspace.current.canRead(this.viewId);
  }

  get canExport(): boolean {
    return this.canExportPermissions;
  }

  get exportTooltip(): string {
    return this.canExportPermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  get showDelete() {
    return this.showDeleteButton;
  }

  get showDeleteWithDescendants() {
    return this.showDelete && this.isTreeMode;
  }

  get canDeletePermissions(): boolean {
    return this.workspace.current.canDo(this.viewId, 'Delete', null);
  }

  get canDelete(): boolean {
    return this.canDeletePermissions;
  }

  get deleteTooltip(): string {
    return this.canDeletePermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  trackById(_: any, id: number | string) {
    return id;
  }

  trackByNodeId(_: any, node: NodeInfo) {
    return node.id;
  }

  colWidth(colPath: string) {
    // This returns an html percentage width based on the weights assigned to this column and all the other columns

    // Get the weight of this column
    const weight = colPath === '(Description)' ? 2 : 1;

    // Get the total weight of all columns
    const totalWeight = 2 + this.tableColumnPaths.length; // to account for the description column

    // if totalweight = 0 this method will never be called in the first place)
    return ((weight / totalWeight) * 100) + '%';
  }

  get formatChoices(): { name: string, value: any }[] {

    if (!this._formatChoices) {
      this._formatChoices = Object.keys(TemplateArguments_format)
        .map(key => ({ name: TemplateArguments_format[key], value: key }));
    }

    return this._formatChoices;
  }

  get search(): string {
    return this.state.search;
  }

  set search(val: string) {
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
  get showExportPaging(): boolean {
    return this.maxTotalExport < this.total;
  }

  get fromExport(): number {
    return Math.min(this.exportSkip + 1, this.totalExport);
  }

  get toExport(): number {
    return Math.min(this.exportSkip + this.maxTotalExport, this.totalExport);
  }

  get totalExport(): number {
    return this.total;
  }

  get canPreviousPageExport() {
    return this.exportSkip > 0;
  }

  get canNextPageExport() {
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

  onExport() {
    if (!this.canExport) {
      return;
    }

    const from = this.fromExport;
    const to = this.toExport;
    const format = this.exportFormat;
    this.exportErrorMessage = null;
    this.showExportSpinner = true;

    const s = this.state;

    this.crud.export({
      top: this.exportPageSize,
      skip: this.exportSkip,
      orderby: s.orderby,
      search: s.search,
      filter: this.filter(),
      expand: null,
      inactive: s.inactive,
      format
    }).pipe(tap(
      (blob: Blob) => {
        this.showExportSpinner = false;
        const fileName = `${this.exportFileName || this.translate.instant('Export')} ${from}-${to} ${new Date().toDateString()}.${format}`;
        downloadBlob(blob, fileName);
      },
      (friendlyError: any) => {
        this.showExportSpinner = false;
        this.exportErrorMessage = friendlyError.error;
      },
      () => {
        this.showExportSpinner = false;
      }
    ),
      finalize(() => this.cdr.markForCheck())
    ).subscribe();
  }

  public get showExportErrorMessage(): boolean {
    return !!this.exportErrorMessage;
  }

  // Multiselect-related stuff
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

  onCheckAll() {
    if (this.areAllChecked) {
      // Uncheck all
      this.checked = {};

    } else {
      // Check all
      this.displayedIds.forEach(id => this.checked[id] = true);
    }
  }

  get displayedIds(): (string | number)[] {
    if (this.isTreeMode) {
      return this.treeNodes.filter(node => this.showTreeNode(node)).map(node => node.id);
    } else {
      return this.flatIds;
    }
  }

  onCancelMultiselect() {
    this.checked = {};
    this.actionValidationErrors = {};
  }

  canAction(action: MultiselectAction) {
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

  actionTooltip(action: MultiselectAction) {
    if (!!action.actionTooltip) {
      return action.actionTooltip(this.checkedIds);
    } else {
      // don't show a tooltip by default
      return '';
    }
  }

  onAction(action: MultiselectAction) {
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

  onDelete() {
    // clear any previous errors
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};

    const ids = this.checkedIds;
    this.crud.delete(ids).pipe(tap(
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

  onDeleteWithDescendants() {
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

              friendlyError.error[key].forEach(errorMessage => {
                // action errors map ids to list of errors messages
                this.actionValidationErrors[id].push(errorMessage);
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

  private filter(): string {
    const filterState = this.state.builtInFilterSelections;
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
    return (!!custom && !!builtin) ? `(${custom}) and (${builtin})` :
      !!custom ? custom : !!builtin ? builtin : null;
  }

  onIncludeInactive(): void {
    const s = this.state;
    s.inactive = !s.inactive;
    this.fetch();
    this.urlStateChange();
  }

  get isIncludeInactive(): boolean {
    return this.state.inactive;
  }

  onFilterCheck(groupName: string, expression: string) {
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

  isFilterChecked(groupName: string, expression: string): boolean {
    const s = this.state.builtInFilterSelections;
    return !!s[groupName] && !!s[groupName][expression];
  }

  get isAnyFilterChecked(): boolean {
    // when this is true the UI shows the red circle
    // This code checks whether any expression in any group is checked, also if include inactive is checked
    return this.state.inactive || this.isAnyFilterCheckedOtherThanInactive;
  }

  get isAnyFilterCheckedOtherThanInactive(): boolean {
    // when this is true, the way data is queried in tree view changes from paged to not paged
    return Object.keys(this.filterDefinition).some(groupName => {
      const group = this.filterDefinition[groupName];
      return group.some(e => this.isFilterChecked(groupName, e.expression));
    }) || !!this.state.customFilter;
  }

  onClearFilter() {
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

  get groupNames(): string[] {
    return Object.keys(this.filterDefinition);
  }

  filterTemplates(groupName: string): {
    template: TemplateRef<any>,
    expression: string
  }[] {
    return this.filterDefinition[groupName];
  }

  // END filter related stuff

  isRecentlyViewed(id: number | string) {
    return this.state.detailsId === id;
  }

  onCancel() {
    this.cancel.emit();
  }

  get customFilter(): string {
    return this.state.customFilter;
  }

  set customFilter(v: string) {
    v = v || null;
    if (this.state.customFilter !== v) {
      this.state.customFilter = v;
      this.state.skip = 0;
      this.exportSkip = 0;
      this.fetch();
      this.urlStateChange();
    }
  }

  get stateSelect(): string {
    return this.state.select;
  }

  set stateSelect(v: string) {
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

  get editingColumns(): boolean {
    return this._editingColumns;
  }

  set editingColumns(v: boolean) {
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

  // public get canExpandNextLevel(): boolean {
  //   return true; // TODO
  // }

  // public get showExpandNextLevel(): boolean {
  //   return this.isTreeMode && !this.searchOrFilter;
  // }

  // public onExpandNextLevel(): void {
  //   this.state.level++;
  //   this.fetch();
  //   this.saveLevel(this.state.level);
  // }

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

    // if (this.state.level !== 1) {
    //   this.state.level = 1;
    //   this.saveLevel(null);
    // }
  }

  // public get level(): number {
  //   return this.state.level;
  // }
}
