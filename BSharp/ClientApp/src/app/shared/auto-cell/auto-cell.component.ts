import { Component, OnInit, Input, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy, SimpleChanges, OnChanges } from '@angular/core';
import {
  metadata, BooleanPropDescriptor, ChoicePropDescriptor, StatePropDescriptor,
  NumberPropDescriptor, EntityDescriptor, PropDescriptor
} from '~/app/data/entities/base/metadata';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Entity } from '~/app/data/entities/base/entity';

@Component({
  selector: 'b-auto-cell',
  templateUrl: './auto-cell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoCellComponent implements OnInit, OnChanges, OnDestroy {

  // This component automatically displays the property value from its metadata

  @Input()
  baseCollection: string;

  @Input()
  entity: Entity;

  @Input()
  path: string;

  @Input()
  subtype: string;

  _subscription: Subscription;

  // The method 'recompute' efficiently populates all the following
  // values once, it is run once at the beginning and every time the
  // input changes or the workspace changes
  _entityDescriptor: EntityDescriptor;
  _propDescriptor: PropDescriptor;
  _metavalue: -1 | 0 | 1 | 2;
  _value: any;
  _control: string;

  // Constructor and lifecycle hooks
  constructor(private ws: WorkspaceService, private translate: TranslateService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this._subscription = this.ws.stateChanged$.subscribe({
      next: () => {
        this.recompute();
        this.cdr.markForCheck();
      }
    });
  }

  ngOnChanges(_: SimpleChanges) {
    this.recompute();
  }

  ngOnDestroy() {
    if (!!this._subscription) {
      this._subscription.unsubscribe();
    }
  }

  // For computing values and definitions

  private metadataFactory(collection: string) {
    const factory = metadata[collection]; // metadata factory for User
    if (!factory) {
      throw new Error(`The collection ${collection} does not exist`);
    }

    return factory;
  }

  private recompute() {

    // clear previous values
    this._entityDescriptor = null;
    this._propDescriptor = null;
    this._metavalue = 2;
    this._value = null;
    this._control = null;

    try {
      if (!this.baseCollection) {
        throw new Error(`The baseCollection is not specified`);
      }

      const pathArray = (this.path || '').split('/').map(e => e.trim()).filter(e => !!e);

      this._entityDescriptor = this.metadataFactory(this.baseCollection)(this.ws.current, this.translate, this.subtype);
      this._value = this.entity;

      if (pathArray.length === 0) {
        this._propDescriptor = null;
        this._metavalue = 2;
        this._control = 'navigation';

      } else {
        let currentCollection = this.baseCollection;
        let currentSubtype = this.subtype;

        for (let i = 0; i < pathArray.length; i++) {
          const step = pathArray[i];

          this._propDescriptor = this._entityDescriptor.properties[step];
          if (!this._propDescriptor) {
            throw new Error(`'${step}' does not exist on '${currentCollection}', subtype:'${currentSubtype}'`);

          } else {

            // always set the control
            this._control = this._propDescriptor.control;

            if (this._propDescriptor.control === 'navigation') {

              currentCollection = this._propDescriptor.collection || this._propDescriptor.type;
              currentSubtype = this._propDescriptor.subtype;
              this._entityDescriptor = this.metadataFactory(currentCollection)(this.ws.current, this.translate, currentSubtype);

              if (this._metavalue === 2 && this._value && this._value.EntityMetadata) {
                this._metavalue = step === 'Id' ? 2 : this._value.EntityMetadata[step] || 0;

                const fkValue = this._value[this._propDescriptor.foreignKeyName];
                this._value = this.ws.current[currentCollection][fkValue];

              } else {
                this._metavalue = 0;
              }
            } else {
              // only allowed at the last step
              if (i !== pathArray.length - 1) {
                throw new Error(`'${step}' is not a navigation property on '${currentCollection}', subtype:'${currentSubtype}'`);
              }

              // set the property and control at the end
              if (this._metavalue === 2 && this._value && this._value.EntityMetadata) {
                this._metavalue = step === 'Id' ? 2 : this._value.EntityMetadata[step] || 0;
                this._value = this._value[step] || null;
              } else {
                this._metavalue = 0;
              }
            }
          }
        }
      }
    } catch (ex) {

      this._entityDescriptor = null;
      this._propDescriptor = null;
      this._metavalue = -1;
      this._value = ex.message;
      this._control = 'error';

      console.error(ex.message);
    }
  }

  // UI Binding

  get control(): string {
    return this._control;
  }

  get metavalue(): -1 | 0 | 1 | 2 { // -1=Error, 0=Not Loaded, 1=Restricted, 2=Loaded
    return this._metavalue;
  }

  get value(): any {
    return this._value;
  }

  get choiceValue(): string {
    const prop = this._propDescriptor as ChoicePropDescriptor;
    const value = this.value;
    return !!prop && !!prop.format ? prop.format(value) : null;
  }

  get stateValue(): string {
    const prop = this._propDescriptor as StatePropDescriptor;
    const value = this.value;
    return !!prop && !!prop.format ? prop.format(value) : null;
  }

  get stateColor(): string {
    const prop = this._propDescriptor as StatePropDescriptor;
    const value = this.value;
    return (!!prop && !!prop.color ? prop.color(value) : null) || 'transparent';
  }

  get digitsInfo(): string {
    const prop = this._propDescriptor as NumberPropDescriptor;
    return `1.${prop.minDecimalPlaces}-${prop.maxDecimalPlaces}`;
  }

  get alignment(): string {
    return (this._propDescriptor as NumberPropDescriptor).alignment;
  }

  get booleanValue(): string {
    const prop = this._propDescriptor as BooleanPropDescriptor;
    const value = this.value as boolean;
    return (!!prop && !!prop.format) ? prop.format(value) : this.translate.instant(value ? 'Yes' : 'No');
  }

  get navigationValue(): any {
    const entityDesc = this._entityDescriptor;
    const value = this.value; // Should return the entity itself
    return !!entityDesc.format ? entityDesc.format(value) : '(Format function missing)';
  }

}
