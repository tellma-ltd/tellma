import { Component, OnInit, OnDestroy, ViewChild, ElementRef, HostListener, Inject } from '@angular/core';
import { ApiService } from '~/app/data/api.service';
import { Subject, fromEvent, Subscription } from 'rxjs';
import { WorkspaceService, MasterStatus } from '~/app/data/workspace.service';
import { UserCompany } from '~/app/data/dto/user-company';
import { DOCUMENT } from '@angular/common';
import { Key } from '~/app/data/util';

@Component({
  selector: 't-companies',
  templateUrl: './companies.component.html'
})
export class CompaniesComponent implements OnInit, OnDestroy {

  private PAGE_SIZE = 10;
  private notifyDestruct$ = new Subject<void>();
  private crud = this.api.companiesApi(this.notifyDestruct$); // Just for intellisense
  private _errorMessage: string = null;
  private userInputSubscription: Subscription;

  // filter criteria
  private _searchTerm: string;
  private _top: number;
  private _skip: number;

  private _oldSearchTerm: string;
  private _oldTop: number;
  private _oldSkip: number;
  private _oldCompanies: UserCompany[];
  private _oldFilteredCompanies: UserCompany[];

  // after applying the filter criteria
  private _filteredCompanies: UserCompany[] = [];
  private _pagedCompanies: UserCompany[] = [];


  @ViewChild('input', { static: true })
  input: ElementRef;

  constructor(
    private api: ApiService, private workspace: WorkspaceService,
    @Inject(DOCUMENT) private document: Document) { }

  ngOnInit() {

    this.crud = this.api.companiesApi(this.notifyDestruct$);
    const ws = this.workspace.ws;
    this._searchTerm = null;
    this.resetPage();


    if (ws.companiesStatus !== MasterStatus.loaded) {
      this.doRefresh();
    }

    this.userInputSubscription = fromEvent(this.input.nativeElement, 'input').subscribe(_ => {
      this.resetPage();
      this._searchTerm = this.input.nativeElement.value;
    });
  }

  private resetPage() {
    this._top = this.PAGE_SIZE;
    this._skip = 0;
  }

  private doRefresh() {
    const ws = this.workspace.ws;
    this.resetPage();
    ws.companiesStatus = MasterStatus.loading;
    this.crud.getForClient().subscribe(e => {
      ws.companies = e;
      ws.companiesStatus = MasterStatus.loaded;
      this.workspace.notifyStateChanged();
    }, friendlyError => {
      ws.companiesStatus = MasterStatus.error;
      this._errorMessage = friendlyError.error;
      this.workspace.notifyStateChanged();
    });
  }

  ngOnDestroy() {
    this.notifyDestruct$.next();
    if (!!this.userInputSubscription) {
      this.userInputSubscription.unsubscribe();
    }
  }

  private get searchInputIsFocused(): boolean {
    return this.input.nativeElement === this.document.activeElement;
  }

  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    let key: string = event.key;

    // Focus on the search field as soon as the user starts typing letters or numbers
    if (!this.searchInputIsFocused && !!event.key && event.key.trim().length === 1) {
      this.input.nativeElement.value = '';
      this.input.nativeElement.focus();
    }

    if (Key[key]) {

      // reverse left and right arrows for RTL languages
      if (this.workspace.ws.isRtl) {
        if (key === Key.ArrowRight) {
          key = Key.ArrowLeft;
        } else if (key === Key.ArrowLeft) {
          key = Key.ArrowRight;
        }
      }

      switch (key) {
        case Key.Escape: {
            this.input.nativeElement.value = '';
            this._searchTerm = '';
            (this.document.activeElement as any).blur();

            break;
          }
      }
    }
  }

  // UI Bindings

  public get filteredCompanies() {
    const ws = this.workspace.ws;
    if (
      this._oldSearchTerm !== this._searchTerm ||
      this._oldSkip !== this._skip ||
      this._oldTop !== this._top ||
      this._oldCompanies !== ws.companies) {

      this._oldSearchTerm = this._searchTerm;
      this._oldSkip = this._skip;
      this._oldTop = this._top;
      this._oldCompanies = ws.companies;

      const t = !!this._searchTerm ? this._searchTerm.toLowerCase() : null;
      this._filteredCompanies = !ws.companies ? null : ws.companies
          .filter(c => !this._searchTerm ||
            (!!c.Name && c.Name.toLowerCase().indexOf(t) !== -1) ||
            (!!c.Name2 && c.Name2.toLowerCase().indexOf(t) !== -1) ||
            (!!c.Name3 && c.Name3.toLowerCase().indexOf(t) !== -1));
    }

    return this._filteredCompanies;
  }

  public get pagedCompanies() {
    if (
      this._oldSkip !== this._skip || this._oldTop !== this._top ||
      this._oldFilteredCompanies !== this.filteredCompanies) {

      this._oldSkip = this._skip;
      this._oldTop = this._top;
      this._oldFilteredCompanies = this.filteredCompanies;

      this._pagedCompanies = !this.filteredCompanies ? null :
        this.filteredCompanies.slice(this._skip, this._top + this._skip);
    }

    return this._pagedCompanies;
  }

  public get errorMessage() {
    return this._errorMessage;
  }
  public onRefresh() {
    if (this.workspace.ws.companiesStatus !== MasterStatus.loading) {
      this.doRefresh();
    }
  }

  public get showErrorMessage(): boolean {
    return this.workspace.ws.companiesStatus === MasterStatus.error;
  }

  public get showNoMemberships(): boolean {
    return this.workspace.ws.companiesStatus === MasterStatus.loaded &&
      this.workspace.ws.companies.length === 0;
  }

  public get showNoItemsFound(): boolean {
    return this.workspace.ws.companiesStatus === MasterStatus.loaded &&
      !this.showNoMemberships &&
      this.filteredCompanies.length === 0;
  }

  public get showCompanies(): boolean {
    return this.workspace.ws.companiesStatus === MasterStatus.loaded;
  }

  public get showProgress(): boolean {
    return this.workspace.ws.companiesStatus === MasterStatus.loading;
  }

  public onNextPage(): void {
    if (this.canNextPage) {
      this._skip = this._skip + this.PAGE_SIZE;
    }
  }

  public get canNextPage(): boolean {
    return !!this.filteredCompanies && this.filteredCompanies.length > this._skip + this.PAGE_SIZE;
  }

  public onPrevPage(): void {
    if (this.canPrevPage) {
      this._skip = this._skip - this.PAGE_SIZE;
    }
  }

  public get canPrevPage(): boolean {
    return this._skip > 0;
  }

  public get flip(): string {
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  trackById(_: any, company: UserCompany) {
    return company.Id;
  }


  get from(): number {
    return Math.min(this._skip + 1, this.total);
  }

  get to(): number {
    if (!!this.pagedCompanies) {
      // If the data is loaded, just count the data
      return Math.max(this._skip + this.pagedCompanies.length, 0);
    } else {
      // Otherwise dispaly the selected count while the data is loading
      return Math.min(this._skip + this._top, this.total);
    }
  }

  get total(): number {
    const companies = this.filteredCompanies;
    return !!companies ? companies.length : 0;
  }
}
