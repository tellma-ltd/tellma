import { Component, OnInit, Input } from '@angular/core';
import {
  metadata, BooleanPropDescriptor,
  ChoicePropDescriptor, StatePropDescriptor, NumberPropDescriptor, propDescriptorImpl, dtoDescriptorImpl
} from '~/app/data/dto/metadata';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { DtoKeyBase } from '~/app/data/dto/dto-key-base';

@Component({
  selector: 'b-switch',
  templateUrl: './switch.component.html',
  styleUrls: ['./switch.component.scss']
})
export class SwitchComponent implements OnInit {

  // This component automatically displays the property value from its metadata

  @Input()
  baseCollection: string;

  @Input()
  entityId: string | number;

  @Input()
  path: string;

  @Input()
  subtype: string;

  _previousPath: string;
  _pathArray: string[];

  constructor(private ws: WorkspaceService, private translate: TranslateService) { }

  ngOnInit() {
  }

  get pathArray(): string[] {
    if (this.path !== this._previousPath || !this._pathArray) {
      this._previousPath = this.path;
      this._pathArray = (this.path || '').split('/').map(e => e.trim()).filter(e => !!e);
    }

    return this._pathArray;
  }

  private dtoDescriptor(ignoreLast = false) {
    return dtoDescriptorImpl(this.pathArray, this.baseCollection, this.subtype, this.ws.current, this.translate, ignoreLast);
  }

  private propDescriptor() {
    return propDescriptorImpl(this.pathArray, this.baseCollection, this.subtype, this.ws.current, this.translate);
  }

  private entity(meta: { meta: 0 | 1 | 2 } = null): DtoKeyBase {

    if (!this.baseCollection) {
      throw new Error(`The baseCollection is not specified, therefore cannot retrieve the value`);
    }

    if (!this.entityId) {
      // any path based on a null Id returns a null value
      return null;
    }

    let currentEntity = this.ws.current[this.baseCollection][this.entityId];
    let currentDtoDescriptor = metadata[this.baseCollection](this.ws.current, this.translate, this.subtype);

    const pathArray = this.pathArray;
    for (let i = 0; i < pathArray.length - 1; i++) {

      // get the property descriptor
      const step = pathArray[i];

      // in case the navigation property was forbidden or not even loaded
      const s = currentEntity.EntityMetadata[step] || 0;
      if (s < 2) {
        meta.meta = s;
        return null;
      }

      const currentPropDescriptor = currentDtoDescriptor.properties[step];
      if (!currentPropDescriptor) {
        throw new Error(`Property '${step}' does not exist`);

      } else if (currentPropDescriptor.control !== 'navigation') {
        throw new Error(`'${step}' is not a nav property`);

      } else {
        const coll = currentPropDescriptor.collection || currentPropDescriptor.type;
        const subtype = currentPropDescriptor.subtype;
        const id = currentEntity[currentPropDescriptor.foreignKeyName];
        if (!id) {
          return null;
        }

        currentEntity = this.ws.current[coll][id];
        currentDtoDescriptor = metadata[coll](this.ws.current, this.translate, subtype);
      }
    }

    return currentEntity;
  }

  get control(): string {
    if (!this.path || this.path.length === 0) {
      return 'navigation';
    } else {
      try {
        return this.propDescriptor().control;
      } catch (ex) {
        console.error(ex.message);
        return 'error';
      }
    }
  }

  // UI bindings

  get value(): any {

    try {
      const entity = this.entity();
      const pathArray = this.pathArray;
      if (pathArray.length === 0) {
        return entity;
      } else {
        if (!entity) {
          return null;
        }

        const propName = pathArray[pathArray.length - 1];
        const dtoDescriptor = this.dtoDescriptor(true);
        const propDescriptor = dtoDescriptor.properties[propName];
        if (!propDescriptor) {
          return `Property '${propName}' does not exist`;

        } else if (propDescriptor.control === 'navigation') {
          const coll = propDescriptor.collection || propDescriptor.type;
          const id = entity[propDescriptor.foreignKeyName];
          return this.ws.current[coll][id];

        } else {
          return entity[propName];
        }
      }

    } catch (ex) {
      console.error(ex.message);
      return '(Error)';
    }
  }

  get metavalue(): -1 | 0 | 1 | 2 { // -1=Error, 0=Not Loaded, 1=Restricted, 2=Loaded
    try {
      const pathArray = this.pathArray;
      if (pathArray.length === 0) {
        return 2;
      } else {
        const meta: { meta: 0 | 1 | 2 } = { meta: 2 };
        const entity = this.entity(meta);

        // this means somewhere along the nav chain, there is a nav property that isn't loaded
        if (meta.meta === 0) {
          // here we check whether the nav property was even
          // valid to begin with or valid but not loaded
          return this.control === 'error' ? -1 : 0;
        } else if (meta.meta === 1) {
          // not allowed to see it
          return 1;
        }

        // after the previous check, this means the property is loaded and it is null
        if (!entity) {
          return 2;
        }

        const propName = pathArray[pathArray.length - 1];
        if (propName === 'Id') {
          return 2; // Id is always loaded
        }

        const result = entity.EntityMetadata[propName] || 0;
        if (result === 0) {
          // here we check whether the nav property was even
          // valid to begin with or valid but not loaded
          return this.control === 'error' ? -1 : 0;
        } else {
          return result;
        }
      }
    } catch (ex) {
      console.error(ex.message);
      return -1;
    }
  }

  get textValue(): string {
    return this.value;
  }

  get choiceValue(): string {
    const prop = <ChoicePropDescriptor>this.propDescriptor();
    const value = this.value;
    return !!prop && !!prop.format ? prop.format(value) : null;
  }

  get stateValue(): string {
    const prop = <StatePropDescriptor>this.propDescriptor();
    const value = this.value;
    return !!prop && !!prop.format ? prop.format(value) : null;
  }

  get stateColor(): string {
    const prop = <StatePropDescriptor>this.propDescriptor();
    const value = this.value;
    return (!!prop && !!prop.color ? prop.color(value) : null) || 'transparent';
  }

  get digitsInfo(): string {
    const prop = <NumberPropDescriptor>this.propDescriptor();
    return `1.${prop.minDecimalPlaces}-${prop.maxDecimalPlaces}`;
  }

  get alignment(): string {
    const prop = <NumberPropDescriptor>this.propDescriptor();
    return prop.alignment;
  }

  get booleanValue(): string {
    const prop = <BooleanPropDescriptor>this.propDescriptor();
    const value = <boolean>this.value;
    return (!!prop && !!prop.format) ? prop.format(value) : this.translate.instant(value ? 'Yes' : 'No');
  }

  get navigationValue(): any {
    const dto = this.dtoDescriptor();
    const value = this.value; // Should return the DTO itself
    return !!dto.format ? dto.format(value) : '(Format function missing)';
  }

}
