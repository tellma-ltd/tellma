import { Component, OnInit, Input, TemplateRef, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { DtoForSaveKeyBase } from '~/app/data/dto/dto-for-save-key-base';

@Component({
  selector: 'b-table',
  templateUrl: './table.component.html',
})
export class TableComponent implements OnInit {

  @Input()
  dataSource: DtoForSaveKeyBase[] = [];

  @Input()
  columnPaths: string[] = [];

  @Input()
  columnTemplates: {
    [path: string]: {
      headerTemplate: TemplateRef<any>,
      rowTemplate: TemplateRef<any>,
      weight: number
    }
  } = {};

  @Input()
  isEdit = false;

  @Input()
  onNewItem: (item: DtoForSaveKeyBase) => DtoForSaveKeyBase;

  @Input()
  filter: (item: DtoForSaveKeyBase) => boolean;

  constructor() { }

  ngOnInit() {
  }

  trackBy(item: DtoForSaveKeyBase) {
    return item.Id || item;
  }

  onNewLine() {
    let newItem: DtoForSaveKeyBase = { Id: null, EntityState: 'Inserted' };
    if (this.onNewItem) {
      newItem = this.onNewItem(newItem);
    }

    this.dataSource.push(newItem);
  }

  onDeleteLine(index: number) {
    const item = this.dataSource[index];
    if (item.EntityState === 'Inserted') {
      this.dataSource.splice(index, 1);
    } else {
      item.EntityState = 'Deleted';
    }
  }

  onUpdateLine(item: DtoForSaveKeyBase) {
    if (!item.EntityState) {
      item.EntityState = 'Updated';
    }
  }

  colWith(colPath: string) {
    // This returns an html percentage width based on the weights assigned to this column and all the other columns

    // Get the weight of this column
    const weight = this.columnTemplates[colPath].weight || 1;

    // Get the total weight of the other columns
    let totalWeight = 0;
    for (let i = 0; i < this.columnPaths.length; i++) {
      const path = this.columnPaths[i];
      if (this.columnTemplates[path]) {
        totalWeight = totalWeight + (this.columnTemplates[path].weight || 1);
      }
    }

    // Calculate the percentage, (
    // if totalweight = 0 this method will never be called in the first place)
    return ((weight / totalWeight) * 100) + '%';
  }

  showLine(item: DtoForSaveKeyBase) {
    return (!this.filter || this.filter(item)) && item.EntityState !== 'Deleted';
  }

  get showLineNumbers() {
    return false;
  }
}
