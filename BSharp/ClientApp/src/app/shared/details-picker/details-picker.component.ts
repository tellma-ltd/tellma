import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild, HostBinding, TemplateRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgbDropdown } from '@ng-bootstrap/ng-bootstrap';
import { PlacementArray } from '@ng-bootstrap/ng-bootstrap/util/positioning';
import { fromEvent, of, Subject, Subscription } from 'rxjs';
import { catchError, debounceTime, map, switchMap, tap, expand } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { GetResponse } from '~/app/data/dto/get-response';
import { DtoKeyBase } from '~/app/data/dto/dto-key-base';
import { WorkspaceService } from '~/app/data/workspace.service';
import { addToWorkspace } from '~/app/data/util';
import { DetailsBaseComponent } from '../details-base/details-base.component';
import { MasterBaseComponent } from '../master-base/master-base.component';


enum SearchStatus {
  showSpinner = 'showSpinner',
  showResults = 'showResults',
  showError = 'showError'
}

enum Key {
  Tab = 9,
  Enter = 13,
  Escape = 27,
  Space = 32,
  PageUp = 33,
  PageDown = 34,
  End = 35,
  Home = 36,
  ArrowLeft = 37,
  ArrowUp = 38,
  ArrowRight = 39,
  ArrowDown = 40
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

  @ViewChild(MasterBaseComponent)
  master: MasterBaseComponent;

  @ViewChild(DetailsBaseComponent)
  details: DetailsBaseComponent;

  @HostBinding('class.w-100')
  w100 = true;

  @Input()
  apiEndpoint: string;

  @Input()
  expand: string;

  @Input()
  collection: string;

  @Input()
  filter: string;

  @Input()
  masterTemplate: TemplateRef<any>;

  @Input()
  detailsTemplate: TemplateRef<any>;

  @Input()
  focusIf = false;
  // set focusIf(v: boolean) {
  //   if (this._focusIf !== v) {
  //     this._focusIf = v;
  //   }
  // }

  private MIN_CHARS_TO_SEARCH = 2;
  private SEARCH_PAGE_SIZE = 15;

  private _focusIf = false;
  private cancelRunningCall$ = new Subject<void>();
  private userInputSubscription: Subscription;
  private _status: SearchStatus = null;
  private _isDisabled = false;
  private _searchResults: (string | number)[] = [];
  private _highlightedIndex = 0;
  private chosenItem: string | number;
  private _errorMessage: string;
  private api = this.apiService.crudFactory(this.apiEndpoint, this.cancelRunningCall$); // for intellisense

  @Input()
  formatter: (id: number | string) => string = (id: number | string) => {
    const item = this.workspace.current.get(this.collection, id);
    return !!item ? item.Name : '';
  }

  ///////////////// Lifecycle Hooks
  constructor(private apiService: ApiService, private workspace: WorkspaceService) { }

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
            filter: this.filter
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
    ).subscribe((results: GetResponse<DtoKeyBase>) => {
      // Populate the dropdown with the results
      if (!!results) {
        this._searchResults = addToWorkspace(results, this.workspace);
      }

      // Auto select the first result
      this._highlightedIndex = 0; // auto select the first item
    });
  }

  ngOnDestroy(): void {
    // cleanup duty
    if (!!this.userInputSubscription) {
      this.userInputSubscription.unsubscribe();
    }
  }

  ///////////////// Helper Functions
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

  private chooseItem(item: string | number) {
    // Restart input stream
    this.cancelRunningCall$.next(null);

    this.chosenItem = item;

    // Show the selection in the input box
    this.updateUI(item);

    // Signal ControlValueAccessor
    this.onChange(item); // TODO only the ID

    // Close the dropdown
    this.status = null;
  }

  private updateUI(item: any) {

    const display = !!item ? this.formatter(item) : '';
    this.input.nativeElement.value = display;
  }

  private toString(value: any): string {
    return (value !== undefined && value !== null) ? `${value}` : '';
  }

  ///////////////// Implementation of ControlValueAccessor
  private onChange = (e: any) => { };
  private onTouched = () => { };

  writeValue(item: any): void {
    // Restart input stream
    this.cancelRunningCall$.next(null);

    // Make it the chosen item;
    this.chosenItem = item;

    // Show the selection in the input box
    this.updateUI(item);
  }

  registerOnChange(fn: (val: any) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
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

    if (Key[this.toString(event.which)]) {
      let offset = 0;
      // if (this.showEditSelected) {
      //   offset = offset + 1;
      // }
      if (this.showCreateNew) {
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

          if (this._highlightedIndex < this.searchResults.length) {
            // Retrieve the selected value
            const chosenValue = this._searchResults[this._highlightedIndex];

            if (!!chosenValue) {

              // Event has been handled
              event.preventDefault();
              event.stopPropagation();

            }

            this.chooseItem(chosenValue);

          } else {

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
    return !!this.detailsTemplate;
  }

  get highlightCreateNew(): boolean {
    return this.indexCreateNew === this.highlightedIndex;
  }

  get showMagnifier(): boolean {
    return !!this.masterTemplate;
  }
}
