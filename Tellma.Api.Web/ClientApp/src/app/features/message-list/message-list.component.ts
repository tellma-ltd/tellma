// tslint:disable:member-ordering
import { Component, Input } from '@angular/core';
import { MessagePreview } from '~/app/data/dto/message-command-preview';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-message-list',
  templateUrl: './message-list.component.html',
  styles: [
  ]
})
export class MessageListComponent {

  @Input()
  public messages: MessagePreview[];

  @Input()
  public isLoading: boolean;

  @Input()
  public errorFunc: () => string;

  constructor(private workspace: WorkspaceService) { }

  public searchTerm: string;
  public skip = 0;
  public top = 20;

  _searchTerm: string;
  _messages: MessagePreview[];
  _messagesCopyResult: MessagePreview[];

  public get messagesCopy(): MessagePreview[] {
    if (this._searchTerm !== this.searchTerm ||
      this._messages !== this.messages) {

      this._searchTerm = this.searchTerm;
      this._messages = this.messages;
      if (!this.messages) {
        this._messagesCopyResult = [];
      } else {
        if (!this.searchTerm) {
          this._messagesCopyResult = this.messages.slice();
        } else {
          const searchLower = this.searchTerm.toLowerCase();
          this._messagesCopyResult = this.messages.filter(message => {
            return !!message.PhoneNumber && message.PhoneNumber.toLowerCase().includes(searchLower) ||
              !!message.Content && message.Content.toLowerCase().includes(searchLower);
          });
        }
      }
    }

    return this._messagesCopyResult;
  }

  //////// Paging

  _skip: number;
  _top: number;
  _messagesCopy: MessagePreview[];
  _pagedMessagesCopyResult: MessagePreview[];

  public get pagedMessagesCopy(): MessagePreview[] {
    const messagesCopy = this.messagesCopy;
    if (this._skip !== this.skip ||
      this._top !== this.top ||
      this._messagesCopy !== messagesCopy) {

      this._skip = this.skip;
      this._top = this.top;
      this._messagesCopy = messagesCopy;
      this._pagedMessagesCopyResult = messagesCopy.slice(this.skip, this.skip + this.top);
    }

    return this._pagedMessagesCopyResult;
  }

  public get total(): number {
    return this.messagesCopy.length;
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
    return this.showData && this.messagesCopy.length === 0;
  }

  public get flip() {
    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  ////// Details

  public get isSingleMessage(): boolean {
    return !!this.messages && this.messages.length === 1;
  }

  public _selectedIndex = -1;

  public get selected(): MessagePreview {
    return this.isSingleMessage ? this.messagesCopy[0] : this.messagesCopy[this._selectedIndex];
  }

  public get order(): number {
    return this._selectedIndex + 1;
  }

  private _lastPreviewed: MessagePreview;

  public onPreviewMessage(msg: MessagePreview) {
    this._selectedIndex = this.messagesCopy.findIndex(e => e === msg); // To reveal the details view
    this._lastPreviewed = msg; // To highlight the row in search view
  }

  public isRecentlyViewed(msg: MessagePreview) {
    return this._lastPreviewed === msg;
  }

  public backToSearch() {
    this._selectedIndex = -1;
  }

  public onPreviousItem() {
    this._selectedIndex--;
  }

  public get canPreviousItem(): boolean {
    return this.order > 1;
  }

  public onNextItem() {
    this._selectedIndex++;
  }

  public get canNextItem(): boolean {
    return this.order < this.total;
  }
}
