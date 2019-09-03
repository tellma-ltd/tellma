import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild, HostBinding, TemplateRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgbDropdown, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { PlacementArray } from '@ng-bootstrap/ng-bootstrap/util/positioning';
import { fromEvent, of, Subject, Subscription } from 'rxjs';
import { catchError, debounceTime, map, switchMap, tap, expand, exhaustMap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { GetResponse } from '~/app/data/dto/get-response';
import { WorkspaceService } from '~/app/data/workspace.service';
import { addToWorkspace, Key, toString, addSingleToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';
import { metadata } from '~/app/data/entities/base/metadata';
import { GetByIdResponse } from '~/app/data/dto/get-by-id-response';

enum SearchStatus {
  showSpinner = 'showSpinner',
  showResults = 'showResults',
  showError = 'showError'
}

@Component({
  selector: 'b-details-picker',
  templateUrl: './details-picker.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: DetailsPickerComponent }]
})
export class DetailsPickerComponent implements AfterViewInit, OnDestroy, ControlValueAccessor {

  ///////////////// Input and Other Fields

  @ViewChild('input')
  input: ElementRef;

  @ViewChild(NgbDropdown)
  resultsDropdown: NgbDropdown;

  @ViewChild('detailsWrapperTemplate')
  detailsWrapperTemplate: TemplateRef<any>;

  @ViewChild('masterWrapperTemplate')
  masterWrapperTemplate: TemplateRef<any>;

  @ViewChild('detailsOptionsTemplate')
  detailsOptionsTemplate: TemplateRef<any>;

  @HostBinding('class.w-100')
  w100 = true;

  @Input()
  apiEndpoint: string;

  @Input()
  expand: string;

  @Input()
  select = null;

  @Input()
  collection: string;

  @Input()
  subtype: string;

  @Input()
  filter: string;

  @Input()
  masterTemplate: TemplateRef<any>;

  @Input()
  detailsTemplate: TemplateRef<any>;

  @Input()
  detailsOptions: { id: string, name: string }[] = [];

  @Input()
  focusIf = false;

  private MIN_CHARS_TO_SEARCH = 2;
  private SEARCH_PAGE_SIZE = 15;

  private cancelRunningCall$ = new Subject<void>();
  private notifyFetchUnloadedItem$ = new Subject<string | number>();
  private userInputSubscription: Subscription;
  private notifyFethcUnloadedItemSubscription: Subscription;
  private _status: SearchStatus = null;
  private _isDisabled = false;
  private _searchResults: (string | number)[] = [];
  private _highlightedIndex = 0;
  private chosenItem: string | number = null;
  private _errorMessage: string;
  private _initialText: string;
  private _viewId: string;
  private langChangeSubscription: Subscription;
  private api = this.apiService.crudFactory(this.apiEndpoint, this.cancelRunningCall$); // for intellisense

  @Input()
  formatter: (item: any) => string = (item: any) => {
    return metadata[this.collection](this.workspace.current, this.translate, this.subtype).format(item);
  }

  ///////////////// Lifecycle Hooks
  constructor(private apiService: ApiService, private workspace: WorkspaceService,
    public modalService: NgbModal, private translate: TranslateService) {

    this.notifyFethcUnloadedItemSubscription = this.notifyFetchUnloadedItem$.pipe(
      exhaustMap((id) => this.doFetchUnloadedItem(id))
    ).subscribe();
  }

  public focus = () => {
    this.input.nativeElement.focus();
  }

  ngAfterViewInit() {

    if (this.focusIf) {
      this.input.nativeElement.focus();
    }

    this.api = this.apiService.crudFactory(this.apiEndpoint, this.cancelRunningCall$);

    // use some RxJS magic to listen to user input and call the backend
    // in order to show the results in a dropdown
    this.userInputSubscription = fromEvent(this.input.nativeElement, 'input').pipe(
      map((e: any) => <string>e.target.value),
      tap(term => {

        // here capture what the user is typing, in case s/he clicks on 'Create'
        // we pass this value to the details template which can use it as an initial
        // value for the name saving the user from having to type again what s/he just typed
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
          return this.api.get({
            search: term,
            top: this.SEARCH_PAGE_SIZE,
            skip: 0,
            expand: this.expand,
            filter: this.filter,
            select: this.select
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
      }

      // Auto select the first result
      this._highlightedIndex = 0; // auto select the first item
    });

    // it's frequently the case that the displayed value depends on the translation
    this.langChangeSubscription = this.translate.onLangChange.subscribe(() => {
      this.updateUI(this.chosenItem);
    });
  }

  ngOnDestroy(): void {
    // cleanup duty
    if (!!this.userInputSubscription) {
      this.userInputSubscription.unsubscribe();
    }

    if (!!this.langChangeSubscription) {
      this.langChangeSubscription.unsubscribe();
    }
    if (!!this.notifyFethcUnloadedItemSubscription) {
      this.notifyFethcUnloadedItemSubscription.unsubscribe();
    }
  }

  ///////////////// Helper Functions

  private formatterInner: (id: number | string) => string = (id: number | string) => {
    // all this does is fetch the entity from the server in case it wasn't found in the workspace
    const item = this.workspace.current.get(this.collection, id);
    if (!!id && !item) {
      this.fetchUnloadedItem(id);
      return '';
    } else {
      return this.formatter(item);
    }
  }

  private fetchUnloadedItem(id: string | number) {
    this.notifyFetchUnloadedItem$.next(id);
  }

  private doFetchUnloadedItem(id: string | number) {
    return this.api.getById(id, {
      expand: this.expand,
      select: this.select
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

    if (Key[toString(event.which)]) {
      let offset = 0;
      // if (this.showEditSelected) {
      //   offset = offset + 1;
      // }
      if (this.showCreateNew && this.canCreateNew) {
        offset = offset + 1;
      }
      const maxIndex = this._searchResults.length - 1 + offset;

      switch (event.which) {
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

  onFocus(item: any) {
    this.chooseItem(item);
  }

  // get indexEditSelected(): number {
  //   return this.searchResults ? this.searchResults.length : 0;
  // }

  // get showEditSelected(): boolean {
  //   return false;
  // }

  // get highlightEditSelected(): boolean {
  //   return this.indexEditSelected === this.highlightedIndex;
  // }

  // get indexCreateNew(): number {
  //   const base = this.indexEditSelected;
  //   const offset = this.showEditSelected ? 1 : 0;
  //   return base + offset;
  // }

  get indexCreateNew(): number {
    return this.searchResults ? this.searchResults.length : 0;
  }

  get showCreateNew(): boolean {
    return !!this.detailsTemplate && (this.showNoItemsFound || this.showResults);
  }

  get highlightCreateNew(): boolean {
    return this.indexCreateNew === this.highlightedIndex && this.canCreateNew;
  }

  get canCreateNewPermissions(): boolean {
    return !!this.detailsOptions && (this.detailsOptions.length !== 1 ||
      (this.canCreatePermissions(this.detailsOptions[0].id)));
  }

  get canCreateNew(): boolean {
    return this.canCreateNewPermissions;
  }

  get createNewTooltip(): string {
    return this.canCreateNewPermissions ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

  get showMagnifier(): boolean {
    return !!this.masterTemplate;
  }

  // The following methods handle displaying and interacting with master and details template

  onMagnifier() {

    // it would be confusing if the user opens the details form the master
    // and find the text s/he typed in the input field a while ago
    this._initialText = '';

    this.modalService.open(this.masterWrapperTemplate, { windowClass: 'b-master-modal' })

      // this guarantees that the input will be focused again when the modal closes
      .result.then(this.onFocusInput, this.onFocusInput);
  }

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

    // if we don't use timeout it doesn't work for some reason
    setTimeout(() => {
      this.openCreateModal();
    }, 1);
  }

  onCreateFromExternal = () => {
    this.openCreateModal();
  }

  onCreateFromFocus = () => {
    if (!this.canCreateNew) {
      return;
    }

    // The value is already captured in onBlur() which is triggered before onCreateFromFocus()
    this.openCreateModal();
  }

  private openCreateModal = () => {
    if (this.detailsOptions.length > 1) {
      this.modalService.open(this.detailsOptionsTemplate)
        .result.then(
          (viewId) => {
            if (!this.canCreateFromOptions(viewId)) {
              return;
            }
            this.openCreateModalInner(viewId);
          },
          (_: any) => {

          }
        );
    } else {
      const detailsOption = this.detailsOptions[0];
      this.openCreateModalInner(!!detailsOption ? detailsOption.id : null);
    }
  }

  private openCreateModalInner = (viewId?: string) => {
    // Launch the details modal
    this._viewId = viewId;
    this.modalService.open(this.detailsWrapperTemplate, { windowClass: 'b-details-modal' })

      // this guarantees that the input will be focused again when the modal closes
      .result.then(this.onFocusInput, this.onFocusInput);
  }

  onFocusInput = () => {
    this.input.nativeElement.focus();
  }

  get initialText(): string {
    return this._initialText;
  }

  get viewId(): string {
    return this._viewId;
  }

  public canCreatePermissions = (viewId: string): boolean => {
    return this.workspace.current.canCreate(viewId);
  }

  public canCreateFromOptions = (viewId: string): boolean => {
    return this.canCreatePermissions(viewId);
  }

  public createFromOptionsTooltip = (viewId: string): string => {
    return this.canCreatePermissions(viewId) ? '' : this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions');
  }

}
