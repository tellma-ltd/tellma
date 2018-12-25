import { Component, OnInit, OnDestroy, Input, TemplateRef, Output, EventEmitter } from '@angular/core';
import { WorkspaceService, MasterDetailsStore, MasterStatus } from 'src/app/data/workspace.service';
import { ApiService } from 'src/app/data/api.service';
import { Router, ActivatedRoute, ParamMap, Params } from '@angular/router';
import { Observable, Subject, merge, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, tap, switchMap, catchError } from 'rxjs/operators';
import { GetResponse } from 'src/app/data/dto/get-response';
import { DtoForSaveKeyBase } from 'src/app/data/dto/dto-for-save-key-base';
import { addToWorkspace } from 'src/app/data/util';
import { resetComponentState } from '@angular/core/src/render3/instructions';

enum SearchView {
  tiles = 'tiles',
  table = 'table'
}

@Component({
  selector: 'b-master',
  templateUrl: './master.component.html',
  styleUrls: ['./master.component.css']
})
export class MasterComponent implements OnInit, OnDestroy {

  @Input()
  title: string;

  @Input()
  apiEndpoint: string;

  @Input()
  tileTemplate: TemplateRef<any>;

  @Input()
  tableColumnTemplates: {
    name: string,
    headerTemplate: TemplateRef<any>,
    rowTemplate: TemplateRef<any>,
      weight: string
  }[] = [];

  @Input()
  tableColumnPaths: string[];

  @Input()
  showCreateButton = true;

  @Input()
  showImportButton = true;

  @Input()
  showExportButton = true;

  @Input()
  allowMultiselect = true;

  @Input()
  multiselectActions: TemplateRef<void>[]; // TODO

  @Input()
  expand: string;

  @Input()
  additionalCommands: TemplateRef<void>[]; // TODO

  @Input() // popup: limits the tiles to only 2 per row, hides import, export and multiselect
  mode: 'popup' | 'screen' = 'screen';

  @Output()
  select = new EventEmitter<number | string>();

  @Output()
  create = new EventEmitter<void>();

  private PAGE_SIZE = 50;
  private localState = new MasterDetailsStore();  // Used in popup mode
  private searchView: SearchView;
  private searchChanged$ = new Subject<string>();
  private notifyFetch$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private crud: any;

  constructor(private workspace: WorkspaceService, private api: ApiService,
    private router: Router, private route: ActivatedRoute) {


    // Use some RxJS magic to refresh the data as the user changes the parameters
    const searchBoxSignals = this.searchChanged$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.state.skip = 0),
    );

    const otherSignals = this.notifyFetch$;
    const allSignals = merge(searchBoxSignals, otherSignals);
    allSignals.pipe(
      switchMap(() => this.doFetch())
    ).subscribe();
  }

  ngOnInit() {

    // Set the crud API
    this.crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$);

    // Set the view from the URL or to 'tiles' by default
    this.route.paramMap.subscribe((params: ParamMap) => {
      this.searchView = params.has('view') ?
        SearchView[params.get('view')] : SearchView.tiles; // tiles by default
    });

    // Unless the data is already loaded, start loading
    if (this.state.masterStatus !== MasterStatus.loaded) {
      this.fetch();
    }
  }

  ngOnDestroy() {
    // This cancels any asynchronous backend calls
    this.notifyDestruct$.next();
  }

  private fetch() {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {

    // Remove previous Ids from the store
    let s = this.state;
    s.masterIds = [];
    this.checked = {}; // clear all selection
    s.masterStatus = MasterStatus.loading;

    // Retrieve the entities
    return this.crud.get({
      top: this.PAGE_SIZE,
      skip: s.skip,
      orderBy: s.orderBy,
      desc: s.desc,
      search: s.search,
      filter: s.filter,
      expand: this.expand,
      inactive: false
    }).pipe(
      tap((response: GetResponse<DtoForSaveKeyBase>) => {
        s = this.state; // get the source
        s.masterStatus = MasterStatus.loaded;
        s.top = response.Top;
        s.skip = response.Skip;
        s.orderBy = response.OrderBy;
        s.desc = response.Desc;
        s.total = response.TotalCount;
        s.bag = response.Bag;
        s.masterIds = addToWorkspace(response, this.workspace);
      }),
      catchError((friendlyError) => {
        s = this.state; // get the source
        s.masterStatus = MasterStatus.error;
        s.errorMessage = friendlyError.error;
        return of(null);
      })
    );
  }

  public get state(): MasterDetailsStore {
    // Important to always reference the source, and not take a local reference
    // on some occasions the source can be reset and a local reference can cause bugs
    if (this.mode === 'popup') {

      // popups use a local store that vanishes when the popup is destroyed
      if (!this.localState) {
        this.localState = new MasterDetailsStore();
      }

      return this.localState;
    } else { // this.mode === 'screen'

      // screens on the other hand use a global store
      if (!this.workspace.current.mdState[this.apiEndpoint]) {
        this.workspace.current.mdState[this.apiEndpoint] = new MasterDetailsStore();
      }

      return this.workspace.current.mdState[this.apiEndpoint];
    }
  }

  private urlStateChange(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL
    const params: Params = {
      view: this.searchView
    };

    this.router.navigate(['.', params], { relativeTo: this.route });
  }

  ////////////// UI Bindings below

  get errorMessage() {
    return this.state.errorMessage;
  }

  get masterIds() {
    return this.state.masterIds;
  }

  get orderBy() {
    return this.state.orderBy;
  }

  onOrderBy(orderBy: string) {
    const s = this.state;
    if (!!orderBy) {
      if (s.orderBy !== orderBy) {
        s.orderBy = orderBy;
        s.desc = false;
      } else {
        if (!s.desc) {
          s.desc = true;
        } else {
          s.orderBy = null;
        }
      }
      s.skip = 0;
      this.fetch();
    }
  }

  get desc() {
    return this.state.desc;
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
  }

  get canFirstPage(): boolean {
    return this.canPreviousPage;
  }

  onPreviousPage() {
    const s = this.state;
    s.skip = Math.max(s.skip - this.PAGE_SIZE, 0);
    this.fetch();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + this.PAGE_SIZE;
    this.fetch();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
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
      (!this.masterIds || this.masterIds.length === 0);
  }

  get isPopupMode(): boolean {
    return this.mode === 'popup';
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
    this.router.navigate(['.', 'import'], { relativeTo: this.route });
  }

  onExport() {
    // TODO
  }

  onSelect(id: number | string) {
    if (this.isPopupMode) {
      this.select.emit(id);
    } else {
      this.router.navigate(['.', id], { relativeTo: this.route });
    }
  }

  get showCreate() {
    return this.showCreateButton;
  }

  get canCreate() {
    return true; // TODO !this.canCreatePred || this.canCreatePred();
  }


  colWith(colPath: string) {
    // This returns an html percentage width based on the weights assigned to this column and all the other columns

    // Get the weight of this column
    const weight = this.tableColumnTemplates[colPath].weight || 1;

    // Get the total weight of the other columns
    let totalWeight = 0;
    for (let i = 0; i < this.tableColumnPaths.length; i++) {
      const path = this.tableColumnPaths[i];
      if (this.tableColumnTemplates[path]) {
        totalWeight = totalWeight + (this.tableColumnTemplates[path].weight || 1);
      }
    }

    // Calculate the percentage, (
    // if totalweight = 0 this method will never be called in the first place)
    return ((weight / totalWeight) * 100) + '%';
  }

  get search(): string {
    return this.state.search;
  }

  set search(val: string) {
    const s = this.state;
    if (s.search !== val) {
      s.search = val;
    }
    this.searchChanged$.next(val);
  }

  public get flip() {
    // This is to flip the icons
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get tableColumnPathsAndExtras() {
    // This method conditionally adds the multi-select column
    let result = this.tableColumnPaths;

    if (!result) {
      result = [];
    }

    if (this.allowMultiselect) {
      result = result.slice();
      result.unshift('multiselect');
    }

    return result;
  }
  
  // The multi-select checkboxes bind to properties in this object
  public checked = {};

  public get areAllChecked(): boolean {
    return this.masterIds.length > 0 && this.masterIds.every(id => !!this.checked[id]);
  }

  onCheckAll() {
    if (this.areAllChecked) {
      // Uncheck all
      this.masterIds.forEach(id => this.checked[id] = false);
    } else {
      // Check all
      this.masterIds.forEach(id => this.checked[id] = true);
    }
  }

}
