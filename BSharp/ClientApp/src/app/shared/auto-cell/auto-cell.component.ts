import { Component, OnInit, Input, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy, SimpleChanges, OnChanges } from '@angular/core';
import { metadata, StatePropDescriptor, NumberPropDescriptor, EntityDescriptor, PropDescriptor } from '~/app/data/entities/base/metadata';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { Entity } from '~/app/data/entities/base/entity';
import { formatNumber, formatDate } from '@angular/common';

@Component({
  selector: 'b-auto-cell',
  templateUrl: './auto-cell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoCellComponent implements OnInit, OnChanges, OnDestroy {

  // This component automatically displays the property value from its metadata

  @Input()
  collection: string;

  @Input()
  definition: string;

  @Input()
  path: string;

  @Input()
  entity: any;

  @Input()
  propDescriptor: PropDescriptor; // When set it (1) ignores collection, definition and path (2) assumes the entity is the immediate value

  @Input()
  entityDescriptor: EntityDescriptor;

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

      this._value = this.entity;

      if (!!this.propDescriptor) {
        // The parent of the component did all the heavy lifting and supplied these values
        this._propDescriptor = this.propDescriptor;
        this._entityDescriptor = this.entityDescriptor;
        this._metavalue = 2;
        this._control = this._propDescriptor.control;

      } else {
        if (!this.collection) {
          throw new Error(`The collection is not specified`);
        }

        const pathArray = (this.path || '').split('/').map(e => e.trim()).filter(e => !!e);
        this._entityDescriptor = this.metadataFactory(this.collection)(this.ws.current, this.translate, this.definition);

        if (pathArray.length === 0) {
          this._propDescriptor = null;
          this._metavalue = 2;
          this._control = 'navigation';

        } else {
          let currentCollection = this.collection;
          let currentDefinition = this.definition;

          for (let i = 0; i < pathArray.length; i++) {
            const step = pathArray[i];

            this._propDescriptor = this._entityDescriptor.properties[step];
            if (!this._propDescriptor) {
              throw new Error(`'${step}' does not exist on '${currentCollection}', definition:'${currentDefinition}'`);

            } else {

              // always set the control
              this._control = this._propDescriptor.control;

              if (this._propDescriptor.control === 'navigation') {

                currentCollection = this._propDescriptor.collection || this._propDescriptor.type;
                currentDefinition = this._propDescriptor.definition;
                this._entityDescriptor = this.metadataFactory(currentCollection)(this.ws.current, this.translate, currentDefinition);

                if (this._metavalue === 2 && !!this._value && this._value.EntityMetadata) {
                  this._metavalue = step === 'Id' ? 2 : this._value.EntityMetadata[step] || 0;

                  const fkValue = this._value[this._propDescriptor.foreignKeyName];
                  this._value = this.ws.current[currentCollection][fkValue];

                } else {
                  this._metavalue = 0;
                }
              } else {
                // only allowed at the last step
                if (i !== pathArray.length - 1) {
                  throw new Error(`'${step}' is not a navigation property on '${currentCollection}', definition:'${currentDefinition}'`);
                }

                // set the property and control at the end
                if (this._metavalue === 2 && this._value && this._value.EntityMetadata) {
                  this._metavalue = step === 'Id' ? 2 : this._value.EntityMetadata[step] || 0;
                  this._value = this._value[step];
                } else {
                  this._metavalue = 0;
                }
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
    }
  }

  // UI Binding

  get control(): string {
    return this._control;
  }

  get metavalue(): -1 | 0 | 1 | 2 { // -1=Error, 0=Not Loaded, 1=Restricted, 2=Loaded
    return this._metavalue;
  }

  get errorMessage(): string {
    return this._metavalue === -1 ? this._value : '';
  }

  get displayValue(): string {
    return displayValue(this._value, this._propDescriptor, this.translate);
  }

  get stateColor(): string {
    const prop = this._propDescriptor as StatePropDescriptor;
    const value = this._value;
    return (!!prop && !!prop.color ? prop.color(value) : null) || 'transparent';
  }

  get alignment(): string {
    return (this._propDescriptor as NumberPropDescriptor).alignment;
  }

  get navigationValue(): any {
    // "this._value" should return the entity itself
    return displayEntity(this._value, this._entityDescriptor);
  }
}

/**
 * Returns a string representation of the value based on the property descriptor.
 * IMPORTANT: Does not support navigation property descriptors, use displayEntity instead
 * @param value The value to represent as a string
 * @param prop The property descriptor used to format the value as a string
 */
export function displayValue(value: any, prop: PropDescriptor, trx: TranslateService) {
  switch (prop.control) {
    case 'text': {
      return value;
    }
    case 'number': {
      if (value === undefined) {
        return null;
      }
      const locale = 'en-GB';
      const digitsInfo = `1.${prop.minDecimalPlaces}-${prop.maxDecimalPlaces}`;
      return formatNumber(value, locale, digitsInfo);
    }
    case 'date': {
      if (value === undefined) {
        return null;
      }
      const format = 'yyyy-MM-dd';
      const locale = 'en-GB';
      return formatDate(value, format, locale);
    }
    case 'datetime': {
      if (value === undefined) {
        return null;
      }
      const format = 'yyyy-MM-dd HH:mm';
      const locale = 'en-GB';
      return formatDate(value, format, locale);
    }
    case 'boolean': {
      return !!prop && !!prop.format ? prop.format(value) : trx.instant(value ? 'Yes' : 'No');
    }
    case 'choice':
    case 'state': {
      return !!prop && !!prop.format ? prop.format(value) : null;
    }
    case 'serial': {
      return !!prop && !!prop.format ? prop.format(value) : (value + '');
    }
    case 'navigation':
    default:
      // Programmer error
      throw new Error('calling "displayValue" on a navigation property, use "displayEntity" instead');
  }
}

/**
 * Returns a string representation of the entity based on the entity descriptor.
 * @param entity The entity to represent as a string
 * @param entityDesc The entity descriptor used to format the entity as a string
 */
export function displayEntity(entity: Entity, entityDesc: EntityDescriptor) {
  return !!entityDesc.format ? entityDesc.format(entity) : '(Format function missing)';
}
