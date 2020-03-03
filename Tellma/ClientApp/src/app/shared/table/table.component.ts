import { Component, OnInit, Input, TemplateRef, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';
import { EntityForSave } from '~/app/data/entities/base/entity-for-save';

@Component({
  selector: 't-table',
  templateUrl: './table.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableComponent implements OnInit {

  // This is the crown jewel of our framework, a reusable minimum-configuration editable grid
  // with Excel-like editing experience, and virtual scrolling built in so it can handle 100s of
  // thousands of rows.

  private HEADER_HEIGHT = 41;
  private PH = 'PH';

  private _isEdit = false;
  private _dataSource: EntityForSave[] = [];
  private _dataSourceCopy: EntityForSave[] = [];

  @Input()
  itemSize = 31;

  @Input()
  minWidth = 600; // min width before the table shows horiztonal scroll bar

  @Input()
  onNewItem: (item: EntityForSave) => EntityForSave;

  @Input()
  set isEdit(isEdit: boolean) {
    if (this._isEdit !== isEdit) {
      this._isEdit = isEdit;

      if (isEdit) {
        this.addPlaceholder(true);
      } else {
        this.removePlaceholder(true);
      }
    }
  }

  get isEdit(): boolean {
    return this._isEdit;
  }

  @Input()
  set dataSource(v: EntityForSave[]) {
    if (this._dataSource !== v) {
      this._dataSource = v;
      // Clone and add placeholder if necessary
      if (!!v) {
        this._dataSourceCopy = v.slice();
        if (this.isEdit) {
          this.addPlaceholder(false);
        }
      }
    }
  }

  get dataSource(): EntityForSave[] {
    return this._dataSource;
  }

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

  @Output()
  insert = new EventEmitter<EntityForSave>();

  @Output()
  delete = new EventEmitter<EntityForSave>();

  private addPlaceholder(updateArrayRef: boolean): void {

    if (!!this._dataSourceCopy) {
      let placeholder: EntityForSave = {};
      if (this.onNewItem) {
        placeholder = this.onNewItem(placeholder);
      }

      placeholder[this.PH] = true;
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

  constructor() { }

  ngOnInit() {
  }

  trackBy(item: EntityForSave) {
    return item.Id || item;
  }

  onDeleteLine(index: number) {
    const item = this._dataSourceCopy[index];
    if (item[this.PH]) {
      console.log(item);
      // Placeholders don't do anything
    } else {

      // Remove from original
      this._dataSource.splice(index, 1);

      // Remove from copy
      const copyOfCopy = this._dataSourceCopy.slice();
      copyOfCopy.splice(index, 1);
      this._dataSourceCopy = copyOfCopy;

      // Tell the outside world that an item has been deleted
      this.delete.emit(item);
    }
  }

  onUpdateLine = (item: EntityForSave) => {
    if (!!item[this.PH]) {
      // This is the add-new placeholder which appears as an extra line in edit mode
      // mark it as a proper item by deleting the PH flag
      delete item[this.PH];

      // Add the item to the original array
      this._dataSource.push(item);

      // Add a new placeholder to replace the old one
      this.addPlaceholder(true);

      // Tell the outside world that an item has been inserted
      this.insert.emit(item);
    }
  }

  colWith(colPath: string) {
    // This returns an html percentage width based on the weights assigned to this column and all the other columns

    // Get the weight of this column
    const weight = this.columnTemplates[colPath].weight || 1;

    // Get the total weight of the other columns
    let totalWeight = 0;
    for (const path of this.columnPaths) {
      if (this.columnTemplates[path]) {
        totalWeight = totalWeight + (this.columnTemplates[path].weight || 1);
      }
    }

    // Calculate the percentage, (
    // if totalweight = 0 this method will never be called in the first place)
    return ((weight / totalWeight) * 100) + '%';
  }

  get visibleDataCount() {
    return !!this.dataSourceCopy ? this.dataSourceCopy.length : 0;
  }

  get dataSourceCopy() {
    return this._dataSourceCopy;
  }

  get maxVisibleRows(): number {
    return Math.ceil(9 * 30 / this.itemSize);
  }

  get tableHeight(): string {
    const headerHeight = this.HEADER_HEIGHT;
    const rowHeight = this.itemSize;
    const maxVisibleRows = this.maxVisibleRows;
    const actualRows = this.dataSourceCopy.length;
    const visibleRows = Math.min(maxVisibleRows, actualRows);
    const placeHolderRows = actualRows < 3 ? 3 - actualRows : 0;
    const height = headerHeight + visibleRows * rowHeight + placeHolderRows * 31;
    return (height + 10) + 'px';
  }

  get tableMaxHeight(): string {
    const headerHeight = this.HEADER_HEIGHT;
    const rowHeight = this.itemSize;
    const maxVisibleRows = this.maxVisibleRows;
    const height = headerHeight + maxVisibleRows * rowHeight;
    return (height + 1) + 'px';
  }
}
