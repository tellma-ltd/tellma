import {
  Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, Output,
  EventEmitter, SimpleChanges, OnChanges
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
  itemSize = 31;

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
  headerTemplate: TemplateRef<any>; // Used in smart screens when there are dynamic columns all sharing the same template

  @Input()
  rowTemplate: TemplateRef<any>; // Used in smart screens when there are dynamic columns all sharing the same template

  @Input()
  visibleRows: number;

  @Input()
  highlightFunc: (entity: EntityForSave) => true;

  @Output()
  insert = new EventEmitter<EntityForSave>();

  @Output()
  delete = new EventEmitter<EntityForSave>();

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
