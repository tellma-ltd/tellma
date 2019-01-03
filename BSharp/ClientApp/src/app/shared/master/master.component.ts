import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { ApiService } from 'src/app/data/api.service';
import { DtoForSaveKeyBase } from 'src/app/data/dto/dto-for-save-key-base';
import { GetResponse } from 'src/app/data/dto/get-response';
import { TemplateArguments_Format } from 'src/app/data/dto/template-arguments';
import { addToWorkspace, downloadBlob } from 'src/app/data/util';
import { MasterDetailsStore, MasterStatus, WorkspaceService } from 'src/app/data/workspace.service';

enum SearchView {
  tiles = 'tiles',
  table = 'table',
  export = 'export'
}

@Component({
  selector: 'b-master',
  templateUrl: './master.component.html',
  styleUrls: ['./master.component.css']
})
export class MasterComponent implements OnInit, OnDestroy {

  @Input()
  masterCrumb: string;

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
  multiselectActions: {
    template: TemplateRef<any>,
    action: (p: (string | number)[]) => Observable<any>
  }[] = [];

  @Input()
  includeInactiveLabel: string;

  @Input()
  filterDefinition: {
    [groupName: string]: {
      template: TemplateRef<any>,
      expression: string
    }[]
  } = {};

  @Input()
  expand: string;

  @Input()
  additionalCommands: TemplateRef<any>[]; // TODO

  @Input() // popup: limits the tiles to only 2 per row, hides import, export and multiselect
  mode: 'popup' | 'screen' = 'screen';

  @Input()
  exportPageSize = 10000;

  @Input()
  exportFileName: string;

  @Input()
  searchView: SearchView;

  @Output()
  select = new EventEmitter<number | string>();

  @Output()
  create = new EventEmitter<void>();

  @ViewChild('errorModal')
  public errorModal: TemplateRef<any>;

  private PAGE_SIZE = 50;
  private localState = new MasterDetailsStore();  // Used in popup mode
  private searchChanged$ = new Subject<string>();
  private notifyFetch$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private _formatChoices: { name: string, value: any }[];
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense

  public checked = {};
  public exportFormat: 'csv' | 'xlsx' = 'xlsx';
  public exportSkip = 0;
  public showExportSpinner = false;
  public exportErrorMessage: string;
  public actionErrorMessage: string;
  public actionValidationErrors: { [id: string]: string[] } = {};

  constructor(private workspace: WorkspaceService, private api: ApiService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService, public modalService: NgbModal) {

    // Use some RxJS magic to refresh the data as the user changes the parameters
    const searchBoxSignals = this.searchChanged$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.state.skip = 0),
      tap(() => this.exportSkip = 0),
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
    if (this.mode === 'screen') {
      // in popup mode don't read from query parameters
      this.route.paramMap.subscribe((params: ParamMap) => {
        this.searchView = params.has('view') ?
          SearchView[params.get('view')] : SearchView.tiles; // tiles by default
      });
    }

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
    s.detailsId = null; // clear the cached details item
    this.checked = {}; // clear all selection
    this.actionValidationErrors = {}; // clear validation errors
    s.masterStatus = MasterStatus.loading;

    // Retrieve the entities
    return this.crud.get({
      top: this.PAGE_SIZE,
      skip: s.skip,
      orderBy: s.orderBy,
      desc: s.desc,
      search: s.search,
      filter: this.filter(),
      expand: this.expand,
      inactive: s.inactive
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
      this.exportSkip = 0;
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

  get showImport(): boolean {
    return !this.isPopupMode && this.showImportButton;
  }

  get showExport(): boolean {
    return !this.isPopupMode && this.showExportButton;
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

  trackById(_, id: number | string) {
    return id;
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

  get formatChoices(): { name: string, value: any }[] {

    if (!this._formatChoices) {
      this._formatChoices = Object.keys(TemplateArguments_Format)
        .map(key => ({ name: TemplateArguments_Format[key], value: key }));
    }

    return this._formatChoices;
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

  public get tableColumnPathsAndExtras() {
    // This method conditionally adds the multi-select column
    let result = this.tableColumnPaths;

    if (!result) {
      result = [];
    }

    if (this.allowMultiselect) {
      result = result.slice();
      result.unshift('errors');
      result.unshift('multiselect');
    }

    return result;
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
    const from = this.fromExport;
    const to = this.toExport;
    const format = this.exportFormat;
    this.showExportSpinner = true;

    const s = this.state;

    this.crud.export({
      top: this.exportPageSize,
      skip: this.exportSkip,
      orderBy: s.orderBy,
      desc: s.desc,
      search: s.search,
      filter: this.filter(),
      expand: null,
      inactive: s.inactive,
      format: format
    }).subscribe(
      (blob: Blob) => {
        this.showExportSpinner = false;
        const fileName = `${this.exportFileName || this.translate.instant('Export')} ${from}-${to} ${new Date().toDateString()}.${format}`;
        downloadBlob(blob, fileName);
      },
      (friendlyError: any) => {
        this.showExportSpinner = false;
        this.exportErrorMessage = friendlyError.error;
      }
    );
  }

  // Multiselect-related stuff
  public get canCheckAll(): boolean {
    return this.masterIds.length > 0;
  }

  public get areAllChecked(): boolean {
    return this.masterIds.length > 0 && this.masterIds.every(id => !!this.checked[id]);
  }

  public get areAnyChecked(): boolean {
    return this.masterIds.some(id => !!this.checked[id]);
  }

  public get checkedCount(): number {
    return this.checkedIds.length;

  }

  public get checkedIds(): (number | string)[] {
    return this.masterIds.filter(id => !!this.checked[id]);
  }

  onCheckAll() {
    if (this.areAllChecked) {
      // Uncheck all
      this.checked = {};

    } else {
      // Check all
      this.masterIds.forEach(id => this.checked[id] = true);
    }
  }

  onCancelMultiselect() {
    this.checked = {};
    this.actionValidationErrors = {};
  }

  onAction(action: (p: (string | number)[]) => Observable<void>) {
    // clear any previous errors
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};

    const ids = this.checkedIds;
    action(ids).subscribe(
      () => this.checked = {},
      (friendlyError: any) => {
        this.handleActionError(ids, friendlyError);
      }
    );
  }

  onDelete() {
    // clear any previous errors
    this.actionErrorMessage = null;
    this.actionValidationErrors = {};

    const ids = this.checkedIds;
    this.crud.delete(ids).subscribe(
      () => {
        // Update the UI to reflect deletion of items
        this.state.delete(ids);
        this.checked = {};
        if (this.masterIds.length === 0 && this.total > 0) {
          // auto refresh if the user deleted the entire page
          this.fetch();
        }
      },
      (friendlyError: any) => {
        this.handleActionError(ids, friendlyError);
      }
    );
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

  // Filter-related stuff

  private filter(): string {
    const filterState = this.state.filterState;
    const disjunctions: string[] = [];
    const groupNames = Object.keys(this.filterDefinition);
    for (let i = 0; i < groupNames.length; i++) {
      const groupName = groupNames[i];
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

    let conjunction = disjunctions.join(') and (');
    if (!!conjunction) {
      conjunction = '(' + conjunction + ')';
    }
    return conjunction;
  }

  onIncludeInactive(): void {
    const s = this.state;
    s.inactive = !s.inactive;
    this.fetch();
  }

  get isIncludeInactive(): boolean {
    return this.state.inactive;
  }

  onFilterCheck(groupName: string, expression: string) {
    const filterGroups = this.state.filterState;
    if (!filterGroups[groupName]) {
      filterGroups[groupName] = {};
    }

    const group = filterGroups[groupName];
    group[expression] = !group[expression];

    this.fetch();
  }

  isFilterChecked(groupName: string, expression: string): boolean {
    const s = this.state.filterState;
    return !!s[groupName] && !!s[groupName][expression];
  }

  get isAnyFilterChecked(): boolean {
    // This code checks whether any expression in any group is checked, also if include inactive is checked
    return this.state.inactive || Object.keys(this.filterDefinition).some(groupName => {
      const group = this.filterDefinition[groupName];
      return group.some(e => this.isFilterChecked(groupName, e.expression));
    });
  }

  onClearFilter() {
    if (this.isAnyFilterChecked) {
      const s = this.state;
      s.inactive = false;
      s.filterState = {};
      this.fetch();
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

  isRecentlyViewed(id: number | string) {
    return this.state.detailsId === id;
  }

}
