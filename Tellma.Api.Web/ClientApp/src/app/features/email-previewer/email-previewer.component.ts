// tslint:disable:member-ordering
import { Component, Input, OnDestroy, OnInit, TemplateRef } from '@angular/core';
import { merge, Observable, of, Subject, Subscription } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { EmailCommandPreview, EmailCommandVersions, EmailPreview } from '~/app/data/dto/email-command-preview';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-email-previewer',
  templateUrl: './email-previewer.component.html',
  styles: [
  ]
})
export class EmailPreviewerComponent implements OnInit, OnDestroy {

  private _subscriptions = new Subscription();
  private notifyFetch$ = new Subject<void>();

  @Input()
  emailCommandPreview: () => Observable<EmailCommandPreview>;

  @Input()
  emailPreview: (index: number, version?: string) => Observable<EmailPreview>;

  @Input()
  refresh: Observable<void>;

  @Input()
  areRequiredParamsMissing = () => false

  @Input()
  toolbarButtons: TemplateRef<any>;

  constructor(private workspace: WorkspaceService) { }

  ngOnInit(): void {
    // Hook the observables
    let allSignals: Observable<void> = this.notifyFetch$;
    if (!!this.refresh) {
      allSignals = merge(this.refresh, allSignals);
    }

    this._subscriptions.add(allSignals.pipe(
      switchMap(_ => this.doFetch())
    ).subscribe());

    // First time always fetch
    this.fetch();
  }

  ngOnDestroy(): void {
    if (!!this._subscriptions) {
      this._subscriptions.unsubscribe();
    }

    this.cancelEmailSub();
  }

  public isLoading = false;
  public errorFunc: () => string;

  public emailCommand: EmailCommandPreview;

  private fetch() {
    this.notifyFetch$.next();
  }

  private doFetch(): Observable<void> {

    this.cancelEmailSub();
    this.errorFunc = null;

    if (this.areRequiredParamsMissing()) {
      return of(null);
    }

    this.isLoading = true;
    return this.emailCommandPreview().pipe(
      tap((cmd: EmailCommandPreview) => {
        this.emailCommand = cmd;

        if (this.selectedIndex >= 0) {
          // If we are already in details mode, make sure the displayed email is fully loaded
          this.onPreviewIndex(this.selectedIndex);
        }
      }),
      catchError(friendlyError => {
        this.emailCommand = null;
        this.errorFunc = () => friendlyError.error;
        return of(null);
      }),
      finalize(() => {
        this.isLoading = false;
      })
    );
  }

  // Binding
  private empty: EmailPreview[] = [];
  private get emails(): EmailPreview[] {
    return !!this.emailCommand ? this.emailCommand.Emails : this.empty;
  }

  public searchTerm: string;
  public skip = 0;
  public top = 20;

  _searchTerm: string;
  _emails: EmailPreview[];
  _emailsCopyResult: EmailPreview[];

  public get emailsCopy(): EmailPreview[] {
    if (this._searchTerm !== this.searchTerm ||
      this._emails !== this.emails) {

      this._searchTerm = this.searchTerm;
      this._emails = this.emails;
      if (!this.emails) {
        this._emailsCopyResult = [];
      } else {
        if (!this.searchTerm) {
          this._emailsCopyResult = this.emails.slice();
        } else {
          const searchLower = this.searchTerm.toLowerCase();
          this._emailsCopyResult = this.emails.filter(email => {
            return (!!email.To && email.To.some(e => e.includes(searchLower))) ||
              (!!email.Cc && email.Cc.some(e => e.includes(searchLower))) ||
              (!!email.Bcc && email.Bcc.some(e => e.includes(searchLower))) ||
              (!!email.Subject && email.Subject.includes(searchLower));
          });
        }
      }
    }

    return this._emailsCopyResult;
  }

  //////// Paging

  _skip: number;
  _top: number;
  _emailsCopy: EmailPreview[];
  _pagedEmailsCopyResult: EmailPreview[];

  public get pagedEmailsCopy(): EmailPreview[] {
    const emailsCopy = this.emailsCopy;
    if (this._skip !== this.skip ||
      this._top !== this.top ||
      this._emailsCopy !== emailsCopy) {

      this._skip = this.skip;
      this._top = this.top;
      this._emailsCopy = emailsCopy;
      this._pagedEmailsCopyResult = emailsCopy.slice(this.skip, this.skip + this.top);
    }

    return this._pagedEmailsCopyResult;
  }

  public get total(): number {
    return this.emailsCopy.length;
  }

  public get from(): number {
    return this.total === 0 ? 0 : this.skip + 1;
  }

  public get to(): number {
    return Math.min(this.skip + this.top, this.total);
  }

  public onPreviousPage() {
    this.skip = Math.max(this.skip - this.top, 0);
  }

  public get canPreviousPage(): boolean {
    return this.skip > 0;
  }

  public onNextPage() {
    this.skip = this.skip + this.top;
  }

  public get canNextPage(): boolean {
    return this.to < this.total;
  }

  public get showData(): boolean {
    return !this.isLoading && !this.errorFunc;
  }
  public get showNoItemsFound(): boolean {
    return this.showData && this.emailsCopy.length === 0;
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  ////// Details

  public get isSingleEmail(): boolean {
    return !!this.emails && this.emails.length === 1;
  }

  public get isMultiEmail(): boolean {
    return !!this.emails && this.emails.length > 1;
  }

  public selectedIndex = -1;

  public get selected(): EmailPreview {
    return this.isSingleEmail ? this.emailsCopy[0] : this.emailsCopy[this.selectedIndex];
  }

  public get order(): number {
    return this.selectedIndex + 1;
  }

  private _lastPreviewed: EmailPreview;

  public onPreviewEmail(email: EmailPreview) {
    const index = this.emailsCopy.findIndex(e => e === email);
    this.onPreviewIndex(index);
  }

  private _emailSub: Subscription;

  private cancelEmailSub(): void{
    if (!!this._emailSub) {
      this._emailSub.unsubscribe();
      delete this._emailSub;
    }
  }

  public onPreviewIndex(emailsCopyIndex: number) {
    this.cancelEmailSub();

    // i is the index in emailsCopy, we need the index in emails
    const cmd = this.emailCommand;
    const emailCopy = this.emailsCopy[emailsCopyIndex];

    // If this index is out of bounds return to search screen
    if (!emailCopy) {
      this.selectedIndex = -1; // Go back to search
      return;
    }

    const emailsIndex = cmd.Emails.indexOf(emailCopy);

    this.selectedIndex = emailsCopyIndex; // Reveals the details view
    this._lastPreviewed = emailCopy;

    if (!emailCopy.Version) {
      this.isLoading = true;
      this._emailSub = this.emailPreview(emailsIndex, this.emailCommand.Version).subscribe(email => {
        cmd.Emails = cmd.Emails.slice(); // To trigger change
        cmd.Emails[emailsIndex] = email;
        this._lastPreviewed = email; // For highlight
      }, (friendlyError) => {
        this.errorFunc = () => friendlyError.error;
        return of(null);
      }, () => {
        this.isLoading = false;
      });
    }
  }

  public isRecentlyViewed(email: EmailPreview) {
    return this._lastPreviewed === email;
  }

  public backToSearch() {
    this.cancelEmailSub();
    this.selectedIndex = -1;
  }

  public onPreviousItem() {
    this.onPreviewIndex(this.selectedIndex - 1);
  }

  public get canPreviousItem(): boolean {
    return this.order > 1;
  }

  public onNextItem() {
    this.onPreviewIndex(this.selectedIndex + 1);
  }

  public get canNextItem(): boolean {
    return this.order < this.total;
  }

  public onRefresh() {
    if (!this.isLoading) {
      this.notifyFetch$.next();
    }
  }

  public get canRefresh(): boolean {
    return !this.areRequiredParamsMissing();
  }

  public addressesDisplay(addresses: string[]) {
    if (!addresses || !addresses.length) {
      return '';
    } else {
      return addresses.join('; ');
    }
  }
}
