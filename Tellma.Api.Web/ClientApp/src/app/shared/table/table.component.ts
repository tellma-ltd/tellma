import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import {
  Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, Output,
  EventEmitter, SimpleChanges, OnChanges, ViewChild
} from '@angular/core';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';

@Component({
  selector: 't-table',
  templateUrl: './table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableComponent implements OnInit, OnChanges {

  // This is the crown jewel of our framework, a reusable minimum-configuration editable grid
  // with Excel-like editing experience, and virtual scrolling built in so it can handle 100s of
  // thousands of rows.

  private HEADER_HEIGHT = 41;
  private PH = 'PH';

  private _dataSourceCopy: EntityForSave[] = [];

  @Input()
  itemSize = 33;

  @Input()
  minWidth = 600; // min width before the table shows horiztonal scroll bar

  @Input()
  onNewItem: (item: EntityForSave) => EntityForSave;

  @Input()
  isEdit: boolean;

  @Input()
  dataSource: EntityForSave[];

  @Input()
  columnPaths: string[] = [];

  @Input()
  columnTemplates: {
    [path: string]: {
      headerTemplate: TemplateRef<any>,
      rowTemplate: TemplateRef<any>,
      weight: number,
      argument?: any // Passed to the template as is
    }
  } = {};

  @Input()
  toolbar: TemplateRef<any>; // Used in smart screens for delete-all and auto-generate

  @Input()
  commands: TemplateRef<any>; // Used in smart screens for highlight row menu

  @Input()
  headerTemplate: TemplateRef<any>; // Used in smart screens when there are dynamic columns all sharing the same template

  @Input()
  rowTemplate: TemplateRef<any>; // Used in smart screens when there are dynamic columns all sharing the same template

  @Input()
  visibleRows: number;

  @Input()
  highlightFunc: (entity: EntityForSave) => true;

  @Input()
  attentionItem: any; // The row representing this item will be highlighted light green

  @Output()
  insert = new EventEmitter<EntityForSave>();

  @Output()
  delete = new EventEmitter<EntityForSave>();

  @ViewChild(CdkVirtualScrollViewport, { static: true })
  viewPort: CdkVirtualScrollViewport;

  /////////// Search Stuff

  @Input()
  isSearchResult: (item: EntityForSave, term: string) => boolean; // When this function is supplied, we show the search function

  public searchTerm: string; // Bound to UI
  public searchResultCount = 0;
  public currentSearchResultOrder = 0;
  private currentSearchResult: EntityForSave;
  private currentSearchResultIndex = -1;

  private updateCounts() {
    const term = this.searchTerm;

    this.searchResultCount = 0;
    this.currentSearchResultOrder = 0;

    if (!term) {
      return; // Optimization
    }

    // Count the result
    const data = this.dataSourceCopy;
    for (const item of data) {
      if (this.isSearchResult(item, term)) {
        this.searchResultCount++;
        if (item === this.currentSearchResult) {
          this.currentSearchResultOrder = this.searchResultCount;
        }
      }
    }
  }

  public onSearchTermChange(term: string): void {
    this.searchTerm = term;

    this.fixIndex();
    if (this.currentSearchResultIndex < 0) {
      delete this.currentSearchResult;
    }

    // In this function, we update the currentSearchItem and we scroll the view port to it
    if (!term) {
      delete this.currentSearchResult;
      this.currentSearchResultIndex = -1;
    } else {
      if (this.isSearchResult(this.currentSearchResult, term)) {
        // The current search item is still valid, do nothing...
      } else {
        // Start from the view port and work your way outwards until you find the nearest search item...
        // How to determine the rows within the view
        const data = this.dataSourceCopy;
        const { first: firstIndex, last: lastIndex } = this.calculateVisibleRange(data.length);

        delete this.currentSearchResult;
        this.currentSearchResultIndex = -1;

        // Look inside the visible range
        for (let i = firstIndex; i <= lastIndex; i++) {
          const item = data[i];
          if (this.isSearchResult(item, term)) {
            this.currentSearchResult = item;
            this.currentSearchResultIndex = i;
            break;
          }
        }

        // Look outside the visible range
        if (!this.currentSearchResult) {
          let i = 1;
          while (true) {
            const indexBefore = firstIndex - i;
            const indexAfter = lastIndex + i;
            if (indexBefore >= 0) {
              const itemBefore = data[indexBefore];
              if (this.isSearchResult(itemBefore, term)) {
                this.currentSearchResult = itemBefore;
                this.currentSearchResultIndex = indexBefore;
                this.scrollTo(indexBefore - 2);
                break;
              }
            } else if (indexAfter < data.length) {
              const itemAfter = data[indexAfter];
              if (this.isSearchResult(itemAfter, term)) {
                this.currentSearchResult = itemAfter;
                this.currentSearchResultIndex = indexAfter;
                this.scrollTo(indexAfter - 2);
                break;
              }
            } else {
              break;
            }

            i++;
          }
        }
      }
    }

    this.updateCounts();
  }

  private calculateVisibleRange(dataLength: number): { first: number, last: number } {
    const top = this.viewPort.measureScrollOffset();
    const bottom = top + this.viewPort.getViewportSize();

    // rows between these two ranges are visible
    const firstIndex = Math.max(Math.ceil((top - this.HEADER_HEIGHT) / this.itemSize), 0);
    const lastIndex = Math.min(Math.floor((bottom - this.HEADER_HEIGHT) / this.itemSize), dataLength) - 1;

    return { first: firstIndex, last: lastIndex };
  }

  private fixIndex() {
    // Ensures that currentSearchResultIndex is in sync with currentSearchResult
    const data = this.dataSourceCopy;
    if (!this.currentSearchResult) {
      this.currentSearchResultIndex = -1;
    } else if (data[this.currentSearchResultIndex] !== this.currentSearchResult) {
      this.currentSearchResultIndex = data.indexOf(this.currentSearchResult);
    }
  }

  public canPrevious(): boolean {
    return !!this.searchTerm;
  }
  public onPrevious() {
    // First make sure the index is correct;
    this.fixIndex();

    const data = this.dataSourceCopy;
    if (data.length > 0) {
      const startIndex = (this.currentSearchResultIndex < 0 ? data.length : this.currentSearchResultIndex) - 1;
      let i = startIndex;
      i = i < 0 ? data.length - 1 : i;

      do {
        const item = data[i];
        if (this.isSearchResult(item, this.searchTerm)) {
          this.currentSearchResult = item;
          this.currentSearchResultIndex = i;

          // If it is outside the visible area, scroll to it
          const { first, last } = this.calculateVisibleRange(data.length);
          if (i < first || i > last) {
            this.scrollTo(i - 2);
          }

          break;
        }
        i--;
        i = i < 0 ? data.length - 1 : i;
      } while  (i !== startIndex);
    }

    // For good measure
    this.updateCounts();
  }

  public canNext(): boolean {
    return !!this.searchTerm;
  }
  public onNext() {
    // First make sure the index is correct;
    this.fixIndex();

    const data = this.dataSourceCopy;
    if (data.length > 0) {
      const startIndex = this.currentSearchResultIndex + 1; // if current index = -1, start at 0
      let i = startIndex % data.length;

      do {
        const item = data[i];
        if (this.isSearchResult(item, this.searchTerm)) {
          this.currentSearchResult = item;
          this.currentSearchResultIndex = i;

          // If it is outside the visible area, scroll to it
          const { first, last } = this.calculateVisibleRange(data.length);
          if (i < first || i > last) {
            this.scrollTo(i - 2);
          }

          break;
        }

        i = (i + 1) % data.length; // Wrap around
      } while (i !== startIndex);
    }

    // For good measure
    this.updateCounts();
  }

  public isCurrentSearchResult(item: EntityForSave) {
    return item === this.currentSearchResult;
  }

  public isSearchResultInner(item: EntityForSave): boolean {
    return !!this.isSearchResult && this.isSearchResult(item, this.searchTerm);
  }

  ////////// End Search Stuff

  // public functions
  public scrollTo(index: number) {
    if (!!this.viewPort) {
      if (index < 0) {
        index = 0;
      }

      const offset = this.HEADER_HEIGHT + (this.itemSize * index);
      this.viewPort.scrollToOffset(offset);
    }
  }

  // public functions
  public scrollToEnd() {
    this.scrollTo(this.dataSourceCopy.length - 1);
  }

  constructor() { }

  ngOnInit() {
  }

  ngOnChanges(changes: SimpleChanges) {

    let placeholderJustAdded = false;
    if (changes.dataSource) {
      const dataSource = changes.dataSource.currentValue as EntityForSave[];
      if (!!dataSource) {
        this._dataSourceCopy = dataSource.slice();
        if (this.isEdit) {
          this.addPlaceholder(false);
          placeholderJustAdded = true;
        }
      } else {
        this._dataSourceCopy = null;
      }

      // For good measure
      this.updateCounts();
    }

    if (changes.isEdit) {
      const isEdit = changes.isEdit.currentValue as boolean;
      if (isEdit) {
        if (!placeholderJustAdded) {
          this.addPlaceholder(true);
        }
      } else {
        this.removePlaceholder(true);
      }
    }
  }

  private addPlaceholder(updateArrayRef: boolean): void {

    if (!!this._dataSourceCopy) {
      let placeholder: EntityForSave = { Id: 0 };
      placeholder[this.PH] = true;
      if (this.onNewItem) {
        placeholder = this.onNewItem(placeholder);
      }

      this._dataSourceCopy.push(placeholder);

      if (updateArrayRef) {
        this._dataSourceCopy = this._dataSourceCopy.slice();
      }
    }
  }

  private removePlaceholder(updateArray: boolean): void {
    if (!!this._dataSourceCopy && !!this._dataSourceCopy.length) {
      const placeholder = this._dataSourceCopy[this._dataSourceCopy.length - 1];
      if (placeholder[this.PH]) {
        this._dataSourceCopy.splice(this._dataSourceCopy.length - 1, 1);
      }
    }

    if (updateArray) {
      this._dataSourceCopy = this._dataSourceCopy.slice();
    }
  }

  // UI Bindings

  public trackBy(item: EntityForSave) {
    return item.Id || item;
  }

  public onDeleteLine(index: number) {
    const item = this._dataSourceCopy[index];
    if (item[this.PH]) {
      // Placeholders don't do anything
    } else {

      // Remove from original
      this.dataSource.splice(index, 1);

      // Remove from copy
      const copyOfCopy = this._dataSourceCopy.slice();
      copyOfCopy.splice(index, 1);
      this._dataSourceCopy = copyOfCopy;

      this.updateCounts();

      // Tell the outside world that an item has been deleted
      this.delete.emit(item);
    }
  }

  public onUpdateLine = (item: EntityForSave) => {
    if (!!item[this.PH]) {
      // This is the add-new placeholder which appears as an extra line in edit mode
      // mark it as a proper item by deleting the PH flag
      delete item[this.PH];

      // Add the item to the original array
      this.dataSource.push(item);

      // Add a new placeholder to replace the old one
      this.addPlaceholder(true);

      // Tell the outside world that an item has been inserted
      this.insert.emit(item);
    }
  }

  public colWith(colPath: string) {
    // This returns an html percentage width based on the weights assigned to this column and all the other columns

    // Get the weight of this column
    const weight = this.columnTemplates[colPath].weight;

    // 0 width indicates a column that fits its contents
    if (!weight) {
      return '0px';
    }

    // Get the total weight of the other columns
    let totalWeight = 0;
    for (const path of this.columnPaths) {
      if (this.columnTemplates[path]) {
        totalWeight = totalWeight + (this.columnTemplates[path].weight || 0);
      }
    }

    // Calculate the percentage, (
    // if totalweight = 0 this method will never be called in the first place)
    return ((weight / totalWeight) * 100) + '%';
  }

  public get visibleDataCount() {
    return !!this.dataSourceCopy ? this.dataSourceCopy.length : 0;
  }

  public get dataSourceCopy() {
    return this._dataSourceCopy;
  }

  public get maxVisibleRows(): number {
    return this.visibleRows || Math.ceil(9 * 30 / this.itemSize);
  }

  public get tableHeight(): number {
    // const contentHeight = this.contentHeight;
    // const tableMaxHeight = this.tableMaxHeight;
    return Math.min(this.contentHeight, this.tableMaxHeight);
  }

  /**
   * Determines the full height of the content if it were to be displayed in its entirety
   */
  private get contentHeight(): number {
    const headerHeight = this.HEADER_HEIGHT;
    const rowHeight = this.itemSize;
    const actualRows = this.dataSourceCopy.length;
    const placeHolderRows = actualRows < 3 ? 3 - actualRows : 0;
    const hScrollerHeight = 20;
    const height = headerHeight + actualRows * rowHeight + placeHolderRows * 31 + hScrollerHeight;

    return height;
  }

  /**
   * Calculates the maximum height the whole table should take regardless of content
   */
  private get tableMaxHeight(): number {
    const headerHeight = this.HEADER_HEIGHT;
    const rowHeight = this.itemSize;
    const maxVisibleRows = this.maxVisibleRows;
    const hScrollerHeight = 20;
    const height = headerHeight + maxVisibleRows * rowHeight + hScrollerHeight;

    return height;
  }

  public highlight(entity: EntityForSave): boolean {
    return !!this.highlightFunc ? this.highlightFunc(entity) : false;
  }
}
