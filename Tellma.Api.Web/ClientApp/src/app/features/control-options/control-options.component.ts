// tslint:disable:member-ordering
import { Component, HostBinding, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Control, metadata } from '~/app/data/entities/base/metadata';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { isSpecified } from '~/app/data/util';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { TenantWorkspace, WorkspaceService } from '~/app/data/workspace.service';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 't-control-options',
  templateUrl: './control-options.component.html',
  providers: [{ provide: NG_VALUE_ACCESSOR, multi: true, useExisting: ControlOptionsComponent }]
})
export class ControlOptionsComponent implements ControlValueAccessor, OnChanges {

  @Input()
  control: Control;

  @Input()
  isEdit: boolean;

  @HostBinding('class')
  classValue = 'row m-0';

  public optionsJSON: string;
  public options: any = {};

  constructor(private workspace: WorkspaceService) { }

  ngOnChanges(changes: SimpleChanges): void {
    // if (!!changes.control && !changes.control.isFirstChange()) {
    //   // Make sure the options are compatible with the new control
    //   this.options = {};
    //   this.onOptionsChange();
    // }
  }

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  // ControlValueAccessor
  public isDisabled = false;
  public onChangeFn: (val: any) => void = _ => { };
  public onTouchedFn: () => void = () => { };

  writeValue(json: string): void {
    this.optionsJSON = json;
    if (!json) {
      this.options = {};
    } else {
      try {
        this.options = JSON.parse(json);
      } catch (e) {
        console.error('Failed to parse ControlOptions JSON', e);
        this.options = {};
      }
    }
  }

  registerOnChange(fn: any): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouchedFn = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  /////////////////////// UI binding ////////////////////////

  public onOptionsChange() {
    // Make the JSON smaller
    for (const key of Object.keys(this.options)) {
      if (!isSpecified(this.options[key]) || this.options[key].length === 0) {
        delete this.options[key];
      }
    }

    this.onTouchedFn();
    this.optionsJSON = JSON.stringify(this.options);
    if (this.optionsJSON === '{}') {
      this.onChangeFn(undefined);
    } else {
      this.onChangeFn(this.optionsJSON);
    }
  }

  public get showDecimalPlaces(): boolean {
    return this.control === 'number' || this.control === 'percent';
  }

  public get showPrefix(): boolean {
    return this.control === 'serial';
  }

  public get showCodeWidth(): boolean {
    return this.control === 'serial';
  }

  public definitionIdDisplay(defId: number) {
    const ws = this.ws;
    const defs = ws.definitions;
    switch (this.control) {
      case 'Document':
        return ws.getMultilingualValueImmediate(defs.Documents[defId], 'TitleSingular');
      case 'Resource':
        return ws.getMultilingualValueImmediate(defs.Resources[defId], 'TitleSingular');
      case 'Agent':
        return ws.getMultilingualValueImmediate(defs.Agents[defId], 'TitleSingular');
      case 'Lookup':
        return ws.getMultilingualValueImmediate(defs.Lookups[defId], 'TitleSingular');
      default:
        return defId;
    }
  }

  private _definitionIdOptionsControl: Control;
  private _definitionIdOptionsDefs: DefinitionsForClient;
  private _definitionIdOptionsResult: SelectorChoice[];

  public get definitionIdOptions(): SelectorChoice[] {
    const ws = this.ws;
    const defs = ws.definitions;
    const control = this.control;
    if (this._definitionIdOptionsDefs !== defs || this._definitionIdOptionsControl !== control) {
      this._definitionIdOptionsDefs = defs;
      this._definitionIdOptionsControl = control;

      let result: SelectorChoice[];
      switch (control) {
        case 'Document':
          result = Object.keys(defs.Documents).map(defId => {
            const def = defs.Documents[defId];
            return {
              value: +defId,
              name: () => ws.getMultilingualValueImmediate(def, 'TitleSingular')
            };
          });
          break;
        case 'Resource':
          result = Object.keys(defs.Resources).map(defId => {
            const def = defs.Resources[defId];
            return {
              value: +defId,
              name: () => ws.getMultilingualValueImmediate(def, 'TitleSingular')
            };
          });
          break;
        case 'Agent':
          result = Object.keys(defs.Agents).map(defId => {
            const def = defs.Agents[defId];
            return {
              value: +defId,
              name: () => ws.getMultilingualValueImmediate(def, 'TitleSingular')
            };
          });
          break;
        case 'Lookup':
          result = Object.keys(defs.Lookups).map(defId => {
            const def = defs.Lookups[defId];
            return {
              value: +defId,
              name: () => ws.getMultilingualValueImmediate(def, 'TitleSingular')
            };
          });
          break;
        default:
          result = [];
      }

      this._definitionIdOptionsResult = result;
    }

    return this._definitionIdOptionsResult;
  }

  public setDefId(defId: string) {
    if (!!defId) {
      this.options.definitionId = +defId;
    } else {
      delete this.options.definitionId;
    }
    this.onOptionsChange();
  }

  public get showDefinitionId(): boolean {
    switch (this.control) {
      case 'Document':
      case 'Resource':
      case 'Agent':
      case 'Lookup':
        return true;
    }

    return false;
  }

  public get showFilter(): boolean {
    return !!this.control && !!metadata[this.control];
  }

  public get showChoices(): boolean {
    return this.control === 'choice';
  }

  // Grid management

  public rowDrop(event: CdkDragDrop<any[]>, collection: any[]) {
    moveItemInArray(collection, event.previousIndex, event.currentIndex);

    this.onOptionsChange();
  }

  public onDeleteRow(row: any, collection: any[]) {
    const index = collection.indexOf(row);
    if (index >= 0) {
      collection.splice(index, 1);
    }

    this.onOptionsChange();
  }

  public onInsertRow(collection: any[], create?: () => any) {
    const item = !!create ? create() : { };
    collection.push(item);

    this.onOptionsChange();
  }

  public onInsertChoice() {
    this.options.choices = this.options.choices || [];
    this.onInsertRow(this.options.choices);
  }
}
