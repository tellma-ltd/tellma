import { Component, Input, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { merge, Observable, of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, switchMap, tap } from 'rxjs/operators';
import { ApiService } from '../../data/api.service';
import { DtoForSaveKeyBase } from '../../data/dto/dto-for-save-key-base';
import { GetResponse } from '../../data/dto/get-response';
import { addToWorkspace } from '../../data/util';
import { MasterDetailsStore, MasterStatus, WorkspaceService } from '../../data/workspace.service';
import { LoadOnDemandListViewEventData, ListViewEventData, RadListView } from 'nativescript-ui-listview';
import { localize } from 'nativescript-localize';

@Component({
  selector: 'b-measurement-units-master',
  templateUrl: './measurement-units-master.component.html',
  styleUrls: ['./measurement-units-master.component.scss']
})
export class MeasurementUnitsMasterComponent implements OnInit, OnDestroy {

  @Input()
  masterCrumb: string;

  @Input()
  apiEndpoint = 'measurement-units';

  // @Input()
  // tileTemplate: TemplateRef<any>;

  @Input()
  showCreateButton = true;

  @Input()
  allowMultiselect = false;

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

  private PAGE_SIZE = 50;
  private searchChanged$ = new Subject<string>();
  private notifyFetch$ = new Subject();
  private notifyDestruct$ = new Subject<void>();
  private crud = this.api.crudFactory(this.apiEndpoint, this.notifyDestruct$); // Just for intellisense
  private listView: RadListView;

  public checked = {};
  public exportFormat: 'csv' | 'xlsx' = 'xlsx';
  public exportSkip = 0;
  public showExportSpinner = false;
  public exportErrorMessage: string;
  public actionErrorMessage: string;
  public actionValidationErrors: { [id: string]: string[] } = {};

  constructor(private workspace: WorkspaceService, private api: ApiService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService) {

    console.log(localize('app.name'));

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

    this.workspace.ws.tenantId = 101;

    // Remove previous Ids from the store
    let s = this.state;
    s.masterStatus = MasterStatus.loading;
    this.checked = {}; // clear all selection
    this.actionValidationErrors = {}; // clear validation errors

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
        s.detailsId = null; // clear the cached details item
        s.masterStatus = MasterStatus.loaded;
        s.top = response.Top;
        s.skip = response.Skip;
        s.orderBy = response.OrderBy;
        s.desc = response.Desc;
        s.total = response.TotalCount;
        s.bag = response.Bag;
        const ids = addToWorkspace(response, this.workspace);
        if (s.skip === 0) {
          // replace existing
          s.masterIds = ids;
        } else {
          // Append
          s.masterIds = this.uniqueArray(s.masterIds.concat(ids));
        }
        if (!!this.listView) {
          const moreItemsToLoad = (response.Top + response.Skip) < response.TotalCount;
          this.listView.notifyLoadOnDemandFinished(!moreItemsToLoad);
          this.listView.notifyPullToRefreshFinished();
        }
      }),
      catchError((friendlyError) => {
        s = this.state; // get the source
        s.masterStatus = MasterStatus.error;
        s.errorMessage = friendlyError.error;
        if (!!this.listView) {
          this.listView.notifyPullToRefreshFinished();
          this.listView.notifyLoadOnDemandFinished();
        }
        return of(null);
      })
    );
  }

  uniqueArray(array: (number | string)[]) {
    const hash = {};
    for (let i = 0; i < array.length; ++i) {
      const item = array[i];
      if (!!hash[item]) {
        array.splice(i--, 1);
      } else {
        hash[item] = true;
      }
    }

    return array;
  }

  public get state(): MasterDetailsStore {
    // Important to always reference the source, and not take a local reference
    // on some occasions the source can be reset and using a local reference can cause bugs

    // screens on the other hand use a global store
    if (!this.workspace.current.mdState[this.apiEndpoint]) {
      this.workspace.current.mdState[this.apiEndpoint] = new MasterDetailsStore();
    }

    return this.workspace.current.mdState[this.apiEndpoint];

  }

  ////////////// UI Bindings below

  public get ws() {
    return this.workspace.current.MeasurementUnits;
  }

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
      // Otherwise displaly the selected count while the data is loading
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

  onCreate() {
    this.router.navigate(['.', 'new'], { relativeTo: this.route });
  }

  onRefresh(args: ListViewEventData) {
    this.state.skip = 0;
    this.listView = args.object;
    this.fetch();
  }

  onLoadMore(args: LoadOnDemandListViewEventData) {
    const s = this.state;
    const moreItemsToLoad = (s.top + s.skip) < s.total;
    args.returnValue = moreItemsToLoad;

    this.listView = args.object;
    this.onNextPage();
  }

  onImport() {
    this.router.navigate(['.', 'import'], { relativeTo: this.route });
  }

  onSelect(id: number | string) {
    this.router.navigate(['.', id], { relativeTo: this.route });
  }

  get showCreate() {
    return this.showCreateButton;
  }

  get canCreate() {
    return true; // TODO !this.canCreatePred || this.canCreatePred();
  }

  trackById(_: any, id: number | string) {
    return id;
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

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get errorPopoverPlacement() {
    return this.workspace.ws.isRtl ? 'left' : 'right';
  }

  public get filterDropdownPlacement(): string {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  // Multiselect-related stuff
  public get canCheckAll(): boolean {
    return this.masterIds.length > 0;
  }

  public get areAnyChecked(): boolean {
    return !!this.listView && !!this.listView.getSelectedItems().length;
  }

  public get checkedCount(): number {
    return this.checkedIds.length;
  }

  public get checkedIds(): (number | string)[] {
    return this.listView.getSelectedItems();
  }

  onCheck(args: ListViewEventData) {
    this.listView = args.object;
    const ids = args.object.getSelectedItems();

    ids.forEach(id => {
      this.checked[id] = true;
    });
  }

  onUncheck(id: number | string) {
    this.checked[id] = false;
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
    alert(this.actionErrorMessage);
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
