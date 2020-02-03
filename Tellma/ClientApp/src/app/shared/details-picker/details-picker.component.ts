import {
  Component, ElementRef, Input, OnDestroy, ViewChild, HostBinding, TemplateRef, OnChanges, SimpleChanges, OnInit
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgbDropdown, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { PlacementArray } from '@ng-bootstrap/ng-bootstrap/util/positioning';
import { fromEvent, of, Subject, Subscription } from 'rxjs';
import { catchError, debounceTime, map, switchMap, tap, exhaustMap, filter } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { GetResponse } from '~/app/data/dto/get-response';
import { WorkspaceService } from '~/app/data/workspace.service';
import { addToWorkspace, Key, addSingleToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';
import { metadata, EntityDescriptor } from '~/app/data/entities/base/metadata';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';

enum SearchStatus {
  showSpinner = 'showSpinner',
  showResults = 'showResults',
  showError = 'showError'
}

@Component({
  selector: 't-details-picker',
  templateUrl: './details-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DetailsPickerComponent }]
})
export class DetailsPickerComponent implements OnInit, OnChanges, OnDestroy, ControlValueAccessor {

  ///////////////// Input and Other Fields
  @Input()
  expand: string;

  @Input()
  additionalSelect: string = null;

  @Input()
  filter: string;

  @Input()
  collection: string;

  @Input()
  definitionIds: string[] = [];

  @Input()
  masterTemplate: TemplateRef<any>;

  @Input()
  detailsTemplate: TemplateRef<any>;

  @ViewChild('input', { static: true })
  input: ElementRef;

  @ViewChild(NgbDropdown, { static: true })
  resultsDropdown: NgbDropdown;

  @ViewChild('detailsWrapperTemplate', { static: true })
  detailsWrapperTemplate: TemplateRef<any>;

  @ViewChild('masterWrapperTemplate', { static: true })
  masterWrapperTemplate: TemplateRef<any>;

  @ViewChild('detailsOptionsTemplate', { static: true })
  detailsOptionsTemplate: TemplateRef<any>;

  @ViewChild('masterOptionsTemplate', { static: true })
  masterOptionsTemplate: TemplateRef<any>;

  @HostBinding('class.w-100')
  w100 = true;

  private MIN_CHARS_TO_SEARCH = 1;
  private SEARCH_PAGE_SIZE = 15;

  private cancelRunningCall$ = new Subject<void>();
  private notifyFetchUnloadedItem$ = new Subject<string | number>();
  private subscriptions: Subscription;
  private _status: SearchStatus = null;
  private _isDisabled = false;
  private _searchResults: (string | number)[] = [];
  private _highlightedIndex = 0;
  private chosenItem: string | number = null;
  private _errorMessage: string;
  private _initialText: string;
  private _definitionId: string;
  // private _cacheMode = false;
  private _idString = 'new';
  private api = this.apiService.crudFactory('', null); // for intellisense

  @Input()
  formatter: (item: any) => string = (item: any) => {
    const definition = this.definitionIdsSingleOrDefault;
    return metadata[this.collection](this.workspace.current, this.translate, definition).format(item);
  }

  ///////////////// Lifecycle Hooks
  constructor(
    private apiService: ApiService, private workspace: WorkspaceService,
    public modalService: NgbModal, private translate: TranslateService) {

  }

  public focus = () => {
    this.input.nativeElement.focus();
  }

  get chosenItemDefinition(): string {
    const defIds = this.entityDescriptor().definitionIds;
    const chosenItem = this.workspace.current[this.collection][this.chosenItem];
    return !!defIds ? (!!chosenItem ? chosenItem.DefinitionId : null) : null;
  }

  ngOnInit() {

    // If there is 0 or 1 definitionId, use the specific API, otherwise use the generic one with a filter
    const apiEndpoint = this.apiEndpoint(this.definitionIdsSingleOrDefault);
    this.api = this.apiService.crudFactory(apiEndpoint, this.cancelRunningCall$);

    this.subscriptions = new Subscription();
    this.subscriptions.add(this.notifyFetchUnloadedItem$.pipe(
      exhaustMap((id) => this.doFetchUnloadedItem(id))
    ).subscribe());

    // // this.apiEndpoint(this.definitionId)

    // // Here we do cache mode
    // this.subscriptions.add(fromEvent(this.input.nativeElement, 'input').pipe(
    //   filter(_ => this._cacheMode),
    //   map((e: any) => e.target.value as string),
    //   tap(term => {
    //     //
    //   })
    // ).subscribe());

    // use some RxJS magic to listen to user input and call the backend
    // in order to show the results in a dropdown
    this.subscriptions.add(fromEvent(this.input.nativeElement, 'input').pipe(
      //   filter(_ => !this._cacheMode),
      map((e: any) => e.target.value as string),
      tap(term => {

        // here capture what the user is typing, in case s/he clicks on 'Create'
        // we pass this value to the details template which can use it as an initial
        // value for the name saving the user from having to type it again
        this._initialText = term;

        // As soon as the user starts typing:
        this._searchResults = []; // clear the results
        this.status = null; // hide the dropdown
        this.cancelRunningCall$.next(); // cancel any existing backend call immediately

        // If the user cleared the value
        if (!term) {
          this.chooseItem(null);
        }
      }),
      debounceTime(200), // takes it easy on the poor server
      switchMap(term => {
        if (!term || term.length < this.MIN_CHARS_TO_SEARCH) {
          return of(null);
        } else {
          this.status = SearchStatus.showSpinner;
          return this.api.get({ // TODO don't always use the API
            search: term,
            top: this.SEARCH_PAGE_SIZE,
            skip: 0,
            expand: this.expand,
            filter: this.queryFilter,
            select: this.computeSelect()
          }).pipe(
            tap(() => this.status = SearchStatus.showResults),
            catchError(friendlyError => {
              this._errorMessage = friendlyError.error;
              this.status = SearchStatus.showError;
              return of(null);
            })
          );
        }
      })
    ).subscribe((results: GetResponse) => {
      // Populate the dropdown with the results
      if (!!results) {
        this._searchResults = addToWorkspace(results, this.workspace);
        // if (results.TotalCount > 500) {
        //   // Next call will retrieve the entire table and search it in memory
        //   this._cacheMode = true;
        // }
      }

      // Auto select the first result
      this._highlightedIndex = 0; // auto select the first item
    }));

    // Listen to changes in the application language and update the UI
    this.subscriptions.add(this.translate.onLangChange.subscribe(() => {
      this.updateUI(this.chosenItem);
    }));
  }

  private computeSelect(): string {
    const desc = this.entityDescriptor();

    const resultPaths: { [key: string]: true } = {};

    // Basic select
    if (!!desc.select) {
      desc.select.forEach(s => resultPaths[s] = true);
    }

    if (!!desc.definitionIds) {
      resultPaths.DefinitionId = true;
    }

    // custom select
    if (!!this.additionalSelect) {
      this.additionalSelect.split(',').forEach(s => resultPaths[s] = true);
    }

    return Object.keys(resultPaths).join(',');
  }

  ngOnChanges(changes: SimpleChanges) {
    // the combination of these properties define a new details picker
    const screenDefProperties = [changes.definitions, changes.collection];
    const screenDefChanges = screenDefProperties.some(prop => !!prop && !prop.isFirstChange());
    if (screenDefChanges) {

      this.ngOnDestroy();
      this.ngOnInit();
    }
  }

  ngOnDestroy(): void {
    // cleanup duty
    if (!!this.subscriptions) {
      this.subscriptions.unsubscribe();
    }
  }

  ///////////////// Helper Functions

  entityDescriptor(definitionId?: string): EntityDescriptor {
    const coll = this.collection;
    return !!coll ? metadata[coll](this.workspace.current, this.translate, definitionId) : null;
  }

  apiEndpoint(definitionId: string): string {
    const meta = this.entityDescriptor(definitionId);
    return !!meta ? meta.apiEndpoint : null;
  }

  private fetchUnloadedItem(id: string | number) {
    this.notifyFetchUnloadedItem$.next(id);
  }

  private doFetchUnloadedItem(id: string | number) {
    return this.api.getById(id, {
      expand: this.expand,
      select: this.computeSelect()
    }).pipe(
      tap((response: GetByIdResponse) => {
        addSingleToWorkspace(response, this.workspace);
        if (this.chosenItem === id) {
          this.updateUI(id);
        }
      }),
      catchError(_ => {
        this.chooseItem(null);
        return of(null);
      })
    );
  }

  private get status(): SearchStatus {
    return this._status;
  }

  private set status(val: SearchStatus) {

    if (!this._status && !!val) {
      this.resultsDropdown.open();
    }
    if (!!this._status && !val) {
      this.resultsDropdown.close();
    }

    this._status = val;
  }

  onDocumentClick(event: any) {
    if (event.target !== this.input.nativeElement) {
      this.status = null;
    }
  }

  private chooseItem(id: string | number) {

    id = id || null; // Standardise empty value

    if (this.chosenItem !== id) {
      this.chosenItem = id;

      // Signal ControlValueAccessor
      this.onChange(id);
    }

    // Show the selection in the input box
    this.updateUI(id);

    // Restart input stream
    this.cancelRunningCall$.next(null);

    // Close the dropdown
    this.status = null;
  }

  private updateUI(id: any) {
    const display = !!id ? this.formatterInner(id) : '';
    this.input.nativeElement.value = display;
  }

  private get isDefinitioned(): boolean {
    return !!this.entityDescriptor().definitionIds;
  }

  private get allDefinitionIds(): string[] {
    // If the api is definitioned, and definitionIds was not supplied, this method
    // Returns the full list of definitionIds form the definitions
    if (this.isDefinitioned) { // Definitioned API
      if (!this.definitionIds || this.definitionIds.filter(e => !!e).length === 0) { // The definitionId were not specified
        return this.entityDescriptor().definitionIds;
      }
    }

    return this.definitionIds.filter(e => !!e);
  }

  private get definitionIdsSingleOrDefault() {
    const defIds = this.allDefinitionIds;
    return !!defIds && defIds.length === 1 ? defIds[0] : null;
  }

  ///////////////// Implementation of ControlValueAccessor

  private onChange = (_: any) => { };
  private onTouched = () => { };

  writeValue(id: any): void {

    id = id || null;

    // Restart input stream
    this.cancelRunningCall$.next(null);

    // Make it the chosen item;
    this.chosenItem = id;

    // Show the selection in the input box
    this.updateUI(id);
  }

  registerOnChange(fn: (id: any) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    if (isDisabled) {
      this.cancelRunningCall$.next();
    }
    this._isDisabled = isDisabled;
  }

  ////////////////// UI Bindings

  public get queryFilter(): string {
    // IF this is a definitioned API and the definition id is ambigious
    // then we add the definitions to the filter
    if (this.isDefinitioned &&
      !this.definitionIdsSingleOrDefault &&
      !!this.definitionIds &&
      this.definitionIds.filter(e => !!e).length > 0) {

      const definitionfilter = this.definitionIds
        .filter(e => !!e)
        .map(e => `DefinitionId eq '${e.replace('\'', '\'\'')}'`)
        .reduce((e1, e2) => `${e1} or ${e2}`);

      if (!!this.filter) {
        return `${definitionfilter} and ${this.filter}`;

      } else {
        return definitionfilter;
      }
    } else {
      return this.filter;
    }
  }

  get searchResults(): (string | number)[] {
    return this._searchResults;
  }

  get highlightedIndex(): number {
    return this._highlightedIndex;
  }

  get isDisabled(): boolean {
    return this._isDisabled;
  }

  get errorMessage(): string {
    return this._errorMessage;
  }

  get showSpinner(): boolean {
    return this.status === SearchStatus.showSpinner;
  }

  get showError(): boolean {
    return this.status === SearchStatus.showError;
  }

  get showNoItemsFound(): boolean {
    return this.status === SearchStatus.showResults &&
      (!!this._searchResults && this._searchResults.length === 0);
  }

  get showResults(): boolean {
    return this.status === SearchStatus.showResults &&
      (!!this._searchResults && this._searchResults.length > 0);
  }

  get placement(): PlacementArray {
    return this.workspace.ws.isRtl ?
      ['bottom-right', 'bottom-left', 'bottom'] :
      ['bottom-left', 'bottom-right', 'bottom'];
  }

  public formatterInner: (id: number | string) => string = (id: number | string) => {
    // all this does is fetch the entity from the server in case it wasn't found in the workspace
    const item = this.workspace.current.get(this.collection, id);
    if (!!id && !item) {
      this.fetchUnloadedItem(id);
      return '';
    } else {
      return this.formatter(item);
    }
  }

  onMouseEnter(i: number) {
    this._highlightedIndex = i;
  }

  onBlur() {

    // Restart input stream and cancel existing backend calls
    this.cancelRunningCall$.next();

    // Signal on touched
    this.onTouched();

    // Set the displayed value appropriately
    const item = this.chosenItem;
    this.updateUI(item);
  }

  onKeyDown(event: KeyboardEvent) {
    // Use key events on the input to highlight and select from the search results
    // without losing the focus from the input element

    if (!this.status) {
      return;
    }

    const key: string = event.key;
    if (Key[key]) {
      let offset = 0;
      if (this.showCreateNew && this.canCreateNew) {
        offset = offset + 1;
      }
      const maxIndex = this._searchResults.length - 1 + offset;

      switch (key) {
        case Key.ArrowDown:
          // Event was handled
          event.preventDefault();

          if (!!this._searchResults) {
            if (this._highlightedIndex < maxIndex) {
              // Increment the highlighted index if we're not at the end
              this._highlightedIndex++;
            } else {
              this._highlightedIndex = 0;
            }
          }
          break;

        case Key.ArrowUp:
          // Event was handled
          event.preventDefault();

          if (!!this._searchResults) {

            if (this._highlightedIndex <= 0) {
              this._highlightedIndex = maxIndex;
            } else {
              // Decrement the highlighted index if we're not at the beginning
              this._highlightedIndex--;
            }
          }
          break;

        case Key.Enter:
        case Key.Tab:

          if (this._highlightedIndex === this.indexCreateNew && this.showCreateNew) {
            this.onCreateFromKeyDown();
          } else {
            // Retrieve the selected value
            const chosenValue = this._searchResults[this._highlightedIndex];
            if (!!chosenValue) {

              // Event has been handled
              event.preventDefault();
              event.stopPropagation();
            }

            this.chooseItem(chosenValue);
          }

          break;

        case Key.Escape:
          // Event was handled
          event.preventDefault();

          // Restart input stream and cancel existing backend calls
          this.cancelRunningCall$.next(null);

          // Close the dropdown
          this.status = null;
          break;
      }
    }
  }

  public onFocus(item: any) {
    this.chooseItem(item);
  }

  // Create New

  public get indexCreateNew(): number {
    return this.searchResults ? this.searchResults.length : 0;
  }

  public get showCreateNew(): boolean {
    return !!this.detailsTemplate && (this.showNoItemsFound || this.showResults);
  }

  public get highlightCreateNew(): boolean {
    return this.indexCreateNew === this.highlightedIndex && this.canCreateNew;
  }

  private hasCreatePermissions = (definitionId: string): boolean => {
    // This returns false if the API is definitioned, but definitionId was not supplied
    const view = this.apiEndpoint(definitionId);
    return this.workspace.current.canCreate(view);
  }

  private get canCreateNewInner(): boolean {
    if (this.isDefinitioned) {
      const defId = this.definitionIdsSingleOrDefault;
      if (!!defId) {
        return this.hasCreatePermissions(defId);
      } else {
        // The definition can't be uniquely determined, so return true
        return true;
      }
    } else {
      return this.hasCreatePermissions(null);
    }
  }

  public get canCreateNew(): boolean {
    return this.canCreateNewInner;
  }

  public get createNewTooltip(): string {
    return this.canCreateNewInner ? '' :
      this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  // Edit Selected

  private get hasEditPermissions(): boolean {
    return this.canUpdatePermissions(this.chosenItemDefinition);
  }

  public get canEdit(): boolean {
    return this.hasEditPermissions;
  }

  public get showMagnifier(): boolean {
    return !!this.masterTemplate;
  }

  public get showEditSelected(): boolean {
    return !!this.detailsTemplate && !!this.chosenItem;
  }

  public get disableEditSelected(): boolean {
    return !this.canUpdatePermissions(this.chosenItemDefinition);
  }

  // The following methods handle displaying and interacting with master and details template

  onUpdate = (id: number | string) => {
    // Called externally by the master or the details template
    // to specify the item Id just saved or selected
    this.chooseItem(id);
  }

  onCreateFromKeyDown = () => {
    if (!this.canCreateNew) {
      return;
    }

    // close the dropdown
    this.status = null;

    // otherwise Angular complains that the value has changed after it has been checked
    this.onTouched();

    // Open the modal
    this.openCreateModal();
  }

  onCreateFromFocus = () => {
    if (!this.canCreateNew) {
      return;
    }

    // The value is already captured in onBlur() which is triggered before onCreateFromFocus()
    this.openCreateModal();
  }

  onEditFromFocus = () => {
    if (!this.canEdit) {
      return;
    }

    this.openEditModalInner();
  }

  public openSearchModal = () => {
    this.openSearchModalInner(this.definitionIdsSingleOrDefault);
  }

  private openSearchModalInner(definitionId?: string) {

    // it would be confusing if the user opens the details form the master
    // and find the text s/he typed in the input field a while ago
    this._initialText = '';
    this._definitionId = definitionId;

    this.modalService.open(this.masterWrapperTemplate, { windowClass: 't-master-modal' })

      // this guarantees that the input will be focused again when the modal closes
      .result.then(this.onFocusInput, this.onFocusInput);
  }

  private openCreateModal = () => {
    if (this.isDefinitioned && !this.definitionIdsSingleOrDefault) {
      // Without the setTimeout it misbehaves when createFromFocus,
      // applying the Enter press on the modal itself
      setTimeout(() => {
        this.modalService.open(this.detailsOptionsTemplate)
          .result.then(
            (definitionId) => {
              if (!this.canCreateFromOptions(definitionId)) {
                return;
              }
              this.openCreateModalInner(definitionId);
            },
            (_: any) => {
            }
          );
      }, 0);

    } else {
      // get the first one or null
      this.openCreateModalInner(this.definitionIdsSingleOrDefault);
    }
  }

  private openCreateModalInner = (definitionId?: string) => {
    // Launch the details modal
    this._definitionId = definitionId;
    this._idString = 'new';

    this.modalService.open(this.detailsWrapperTemplate, { windowClass: 't-details-modal' })

      // this guarantees that the input will be focused again when the modal closes
      .result.then(this.onFocusInput, this.onFocusInput);
  }

  private openEditModalInner = () => {
    if (!!this.collection && !!this.workspace.current[this.collection] && !!this.workspace.current[this.collection][this.chosenItem]) {
      this._idString = this.chosenItem.toString();
      this._definitionId = this.chosenItemDefinition;

      if (!!this._idString) {
        this.modalService.open(this.detailsWrapperTemplate, { windowClass: 't-details-modal' })

          // this guarantees that the input will be focused again when the modal closes
          .result.then(this.onFocusInput, this.onFocusInput);
      }
    }
  }

  onFocusInput = () => {
    this.input.nativeElement.focus();
  }

  get initialText(): string {
    return this._initialText;
  }

  get definitionId(): string {
    return this._definitionId;
  }

  public canUpdatePermissions = (definitionId: string): boolean => {
    const view = this.apiEndpoint(definitionId);
    return this.workspace.current.canUpdate(view, null);
  }

  public canCreateFromOptions = (definitionId: string): boolean => {
    return this.hasCreatePermissions(definitionId);
  }

  public createFromOptionsTooltip = (definitionId: string): string => {
    return this.hasCreatePermissions(definitionId) ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  public optionName(definitionId: string) {
    return this.entityDescriptor(definitionId).titleSingular();
  }

  public get idString(): string {
    return this._idString;
  }

  public get editSelectedLeftMargin(): string {
    return !this.workspace.ws.isRtl ? '-24px' : null;
  }

  public get editSelectedRightMargin(): string {
    return this.workspace.ws.isRtl ? '-24px' : null;
  }

  public get inputLeftPadding(): string {
    return this.showEditSelected ? this.workspace.ws.isRtl ? '24px!important' : null : null;
  }

  public get inputRightPadding(): string {
    return this.showEditSelected ? !this.workspace.ws.isRtl ? '24px!important' : null : null;
  }

  public get createOptions(): string[] {
    return this.allDefinitionIds;
  }
}
