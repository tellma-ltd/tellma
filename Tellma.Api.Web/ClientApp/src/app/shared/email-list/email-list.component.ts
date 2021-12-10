// tslint:disable:member-ordering
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { EmailPreview } from '~/app/data/dto/email-command-preview';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-email-list',
  templateUrl: './email-list.component.html',
  styles: [
  ]
})
export class EmailListComponent implements OnInit {

  @Input()
  public emails: EmailPreview[];

  @Input()
  public isLoading: boolean;

  @Input()
  public errorFunc: () => string;

  @Output()
  public preview = new EventEmitter<EmailPreview>();

  constructor(private workspace: WorkspaceService) { }

  ngOnInit(): void {
  }

  public addressesDisplay(addresses: string[]) {
    if (!addresses || !addresses.length) {
      return '';
    } else {
      return addresses.join('; ');
    }
  }

  private _lastPreviewed: EmailPreview;

  public onPreviewEmail(email: EmailPreview) {
    this._lastPreviewed = email;
    this.preview.emit(email);
  }

  public isRecentlyViewed(email: EmailPreview) {
    return this._lastPreviewed === email;
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
            return !!email.Subject && email.Subject.toLowerCase().includes(searchLower) ||
              !!email.To && email.To.some(e => !!e && e.toLowerCase().includes(searchLower));
          });
        }
      }
    }

    return this._emailsCopyResult;
  }

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

  //////// Paging

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
}
