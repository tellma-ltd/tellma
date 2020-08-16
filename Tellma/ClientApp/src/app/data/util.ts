import { EntitiesResponse } from './dto/entities-response';
import { WorkspaceService, EntityWorkspace } from './workspace.service';
import { GetByIdResponse } from './dto/get-by-id-response';
import { EntityWithKey } from './entities/base/entity-with-key';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Observer } from 'rxjs';
import { EntityDescriptor, PropDescriptor, NavigationPropDescriptor, metadata } from './entities/base/metadata';
import { formatNumber, formatDate } from '@angular/common';
import { Entity } from './entities/base/entity';

// This handy function takes the entities from the response and all their related entities
// adds them to the workspace indexed by their IDs and returns the IDs of the entities
// this pattern ensures that entities are not spread everywhere in the app, but are
// maintained in a centralized workspace instead, so an update to the entity name for example
// will see that reflected updated everywhere where that name is displayed
export function addToWorkspace(response: EntitiesResponse, workspace: WorkspaceService): (number | string)[] {

  // Merge fresh entities in the workspace
  const relatedEntities = response.RelatedEntities;
  mergeEntitiesInWorkspace(relatedEntities, workspace);

  const result = {};
  result[response.CollectionName] = response.Result;
  mergeEntitiesInWorkspace(result, workspace);

  // Notify everyone
  workspace.notifyStateChanged();

  // Return the IDs of the main entities
  return response.Result.map(e => e.Id);
}

export function addSingleToWorkspace(response: GetByIdResponse, workspace: WorkspaceService): (number | string) {

  // Merge fresh entities in the workspace
  const entities = response.RelatedEntities;
  mergeEntitiesInWorkspace(entities, workspace);

  const result = {};
  result[response.CollectionName] = [response.Result];
  mergeEntitiesInWorkspace(result, workspace);

  // Notify everyone
  workspace.notifyStateChanged();

  // Return the ID of the result
  return response.Result.Id;
}

export function mergeEntitiesInWorkspace(entities: { [key: string]: EntityWithKey[] }, workspace: WorkspaceService) {
  if (!!entities) {
    const collectionNames = Object.keys(entities);
    for (const collectionName of collectionNames) {
      const collection = entities[collectionName];
      const wsCollection = workspace.current[collectionName];
      if (!wsCollection) {
        // dev mistake
        console.error(`Could not find collection '${collectionName}' in the workspace`);
      }

      for (const freshItem of collection) {
        const staleItem = wsCollection[freshItem.Id];
        apply(freshItem, staleItem, wsCollection);
      }
    }
  }
}

function apply(freshItem: EntityWithKey, staleItem: EntityWithKey, wsCollection: any) {
  if (!!staleItem) {
    wsCollection[freshItem.Id] = JSON.parse(JSON.stringify(merge(freshItem, staleItem)));
  } else {
    wsCollection[freshItem.Id] = freshItem;
  }
}

function merge(freshItem: EntityWithKey, staleItem: EntityWithKey): EntityWithKey {

  // every property in the DTO tree is tagged with metadata, either loaded = 2, restricted = 1 or not loaded = 0
  // this method merges two DTOs the stale on the client and the fresh one from the server, it recursively
  // traverses the DTO tree and copies the fields one by one, the field value with the higher metadata wins
  // for example if a field comes from the server restricted = 1, and on the client loaded = 2, the client value wins

  // LIMITATION: this doesn't currently support arrays of arrays, but for now we don't execpt any to come by since
  // DTOs are loaded directly from EF which won't generate such a DTO

  const staleMetadata = staleItem.EntityMetadata;
  const freshMetadata = freshItem.EntityMetadata;

  const result = staleItem; // to force update the UI bindings
  result.Id = freshItem.Id;

  const metadataArray = Object.keys(freshItem.EntityMetadata).concat(Object.keys(staleItem.EntityMetadata));
  metadataArray.forEach(prop => {
    const freshPropMetadata = freshMetadata[prop] || 0;
    const stalePropMetadata = staleMetadata[prop] || 0;
    const freshValue = freshItem[prop];
    const staleValue = staleItem[prop];

    if (freshPropMetadata >= stalePropMetadata) {
      result[prop] = freshValue;
      result.EntityMetadata[prop] = freshPropMetadata;
    } else {
      result[prop] = staleValue;
      result.EntityMetadata[prop] = stalePropMetadata;
    }

    // if the property is a navigation property or an array, and both values
    // are not null then it's not one or the other, we have to merge them
    if (!!freshValue && !!staleValue && freshPropMetadata === 2 && stalePropMetadata === 2) {
      if (!!freshValue.EntityMetadata || !!staleValue.EntityMetadata) {
        // a navigation property, call merge recursively
        result[prop] = merge(freshValue, staleValue);
      } else if (freshValue.constructor === Array || staleValue.constructor === Array) {
        result[prop] = mergeArrays(freshValue, staleValue);
      }
    }
  });

  return result;
}

function mergeArrays(freshArray: EntityWithKey[], staleArray: EntityWithKey[]): EntityWithKey[] {
  // to efficiently retrieve the lines in the stale array, this pays off for huge arrays
  const staleArrayHash: { [id: string]: EntityWithKey } = {};
  staleArray.forEach(staleLine => {
    staleArrayHash[staleLine.Id] = staleLine;
  });

  for (let i = 0; i < freshArray.length; i++) {
    const freshLine = freshArray[i];
    const staleLine = staleArrayHash[freshLine.Id];

    if (!!staleLine) {
      freshArray[i] = merge(freshLine, staleLine);
    }
  }

  return freshArray;
}

export function downloadBlob(blob: Blob, fileName: string) {
  // Helper function to download a blob from memory to the user's computer,
  // Without having to open a new window first
  if (window.navigator && window.navigator.msSaveOrOpenBlob) {
    // To support IE and Edge
    window.navigator.msSaveOrOpenBlob(blob, fileName);
  } else {

    // Create an in memory url for the blob, further reading:
    // https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL
    const url = window.URL.createObjectURL(blob);

    // Below is a trick for downloading files without opening
    // a new window. This is a more elegant user experience
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName || 'file';
    a.click();

    // Best practice to prevent a memory leak, especially in a SPA
    window.URL.revokeObjectURL(url);
  }
}

export enum Key {
  Tab = 'Tab',
  Enter = 'Enter',
  Escape = 'Escape',
  Space = 'Space',
  ArrowLeft = 'ArrowLeft',
  ArrowUp = 'ArrowUp',
  ArrowRight = 'ArrowRight',
  ArrowDown = 'ArrowDown'
}

/**
 * Useful in forms to determine if a required value is specified
 * @param value the value that the
 */
export function isSpecified(value: any) {
  return !!value || value === 0 || value === false;
}

// Processed http error
export interface FriendlyError {
  status: number;
  error: any;
}

// Function to turn status codes into friendly localized human-readable errors
export function friendlify(error: any, trx: TranslateService): FriendlyError {
  const friendlyStructure = (status: number, err: any) => {
    return {
      status,
      error: err
    };
  };

  // Translates HttpClient's errors into human-friendly errors
  if (error instanceof HttpErrorResponse) {
    const res = error as HttpErrorResponse;

    switch (res.status) {
      case 0: // Offline
      case 504: // Service worker reports
        return friendlyStructure(res.status, trx.instant('Error_UnableToReachServer'));

      case 400: // Bad Request
      case 422: // Unprocessible entity
        // These two status codes mean a friendly error is already coming from the server
        return friendlyStructure(res.status, res.error);

      case 401:  // Unauthorized
        return friendlyStructure(res.status, trx.instant('Error_LoginSessionExpired'));

      case 403:  // Forbidden
        return friendlyStructure(res.status, trx.instant('Error_AccountDoesNotHaveSufficientPermissions'));

      case 404: // Not found
        return friendlyStructure(res.status, trx.instant('Error_RecordNotFound'));

      case 500:  // Internal Server Error
        return friendlyStructure(res.status, trx.instant('Error_UnhandledServerError'));

      default:  // Any other HTTP error
        return friendlyStructure(res.status, trx.instant('Error_UnkownServerError'));
    }

  } else {
    console.error(error);
    return friendlyStructure(null, trx.instant('Error_UnkownClientError'));
  }
}

export function getDataURL(blob: Blob): Observable<string> {
  const reader = new FileReader();
  const obs$ = new Observable<string>((obs: Observer<string>) => {
    // implement the observable contract based on reader
    reader.onloadend = () => {

      const data = reader.result as string;
      obs.next(data);
      obs.complete();
    };

    reader.onerror = (e) => {
      obs.error(e);
    };
  });

  reader.readAsDataURL(blob);
  return obs$;
}

export function fileSizeDisplay(fileSize: number): string {
  if (fileSize === null || fileSize === undefined) {
    return '';
  }

  let unitIndex = 0;
  const stepSize = 1024;
  while (fileSize >= stepSize || -fileSize >= stepSize) {
    fileSize /= stepSize;
    unitIndex++;
  }
  return (unitIndex ? fileSize.toFixed(1) + ' ' : fileSize) + ' KMGTPEZY'[unitIndex] + 'B';
}

export function computeSelectForDetailsPicker(desc: EntityDescriptor, additionalSelect: string): string {
  // Computes the select parameter for details picker and the details screen in popup mode
  const resultPaths: { [key: string]: true } = {};

  // Basic select
  if (!!desc.select) {
    desc.select.forEach(s => resultPaths[s] = true);
  }

  if (!!desc.definitionIds) {
    resultPaths.DefinitionId = true;
  }

  // custom select
  if (!!additionalSelect) {
    additionalSelect.split(',').forEach(s => resultPaths[s] = true);
  }

  return Object.keys(resultPaths).join(',');
}

/**
 * Returns the date part of the argument as per the local time formatted as ISO 8601, for example: '2020-03-17'
 */
export function toLocalDateISOString(date: Date): string {

  // Year
  let year = date.getFullYear().toString();
  if (year.length < 4) {
    year = '000'.substring(0, 4 - year.length) + year;
  }

  // Month
  let month = (date.getMonth() + 1).toString();
  if (month.length < 2) {
    month = '0' + month;
  }

  // Day
  let day = date.getDate().toString();
  if (day.length < 2) {
    day = '0' + day;
  }

  return `${year}-${month}-${day}`;
}

function closePrint() {
  // Cleanup duty once the user closes the print dialog
  document.body.removeChild(this.__container__);
  window.URL.revokeObjectURL(this.__url__);
}

function setPrintFactory(url: string): () => void {
  // As soon as the iframe is loaded and ready
  return function setPrint() {
    this.contentWindow.__container__ = this;
    this.contentWindow.__url__ = url;
    this.contentWindow.onbeforeunload = closePrint;
    this.contentWindow.onafterprint = closePrint;
    this.contentWindow.focus(); // Required for IE
    this.contentWindow.print();
  };
}

/**
 * Attaches the given blob in a hidden iFrame and calls print() on
 * that iframe, and then takes care of cleanup duty afterwards.
 * This function was inspired from MDN: https://mzl.la/2YfOs1v
 */
export function printBlob(blob: Blob): void {
  const url = window.URL.createObjectURL(blob);
  const iframe = document.createElement('iframe');
  iframe.onload = setPrintFactory(url);
  iframe.style.position = 'fixed';
  iframe.style.right = '0';
  iframe.style.bottom = '0';
  iframe.style.width = '0';
  iframe.style.height = '0';
  iframe.style.border = '0';
  iframe.sandbox.add('allow-same-origin');
  iframe.sandbox.add('allow-modals');
  iframe.src = url;
  document.body.appendChild(iframe);
}

// IMPORTANT: Keep in sync with function in CsvPackager.cs
export function csvPackage(data: string[][]): Blob {

  if (!data) {
    throw new Error(`The data is null`);
  }

  if (data.length === 0) {
    throw new Error(`Must supply the CSV header at least`);
  }

  const columnCount = data[0].length;
  const resultArray: string[] = [];
  const newLine = `
`;

  for (const row of data) {
    if (row.length !== columnCount) {
      throw new Error(`Number of columns is inconsistent across rows`);
    }

    const rowString: string = row.map(field => processFieldForCsv(field)).join(',');
    resultArray.push(rowString);
  }

  const result = resultArray.join(newLine);
  return new Blob(['\ufeff' + result], { type: 'text/csv' });
}

// IMPORTANT: Keep in sync with function in CsvPackager.cs
function processFieldForCsv(field: string) {
  if (field == null) {
    // Null in - Null out
    return null;
  }

  // Escape every double quote with another double quote
  field = field.replace('"', '""');

  // Surround any field that contains double quotes, new lines, or commas with double quotes
  if (field.indexOf('"') > -1 || field.indexOf(',') > -1 || field.indexOf('\n') > -1 || field.indexOf('\r') > -1) {
    field = `"${field}"`;
  }

  // Return the processed field
  return field;
}

export function formatAccounting(amount: number, digitsInfo: string): string {
  if (!!amount || amount === 0) {
    const result = formatNumber(Math.abs(amount), 'en-GB', digitsInfo);
    if (amount >= 0) {
      return ` ${result} `;
    } else {
      return `(${result})`;
    }
  } else {
    return '';
  }
}

function metadataFactory(collection: string) {
  const factory = metadata[collection]; // metadata factory for User
  if (!factory) {
    throw new Error(`The collection ${collection} does not exist`);
  }

  return factory;
}

export interface ColumnDescriptor {
  path: string;
  display?: string;
}

export function composeEntities(
  entities: Entity[],
  columns: ColumnDescriptor[],
  collection: string,
  defId: number,
  ws: WorkspaceService,
  trx: TranslateService
) {
  const relatedEntities = ws.current;

  // This array will contain the final result
  const result: string[][] = [];

  // This is the base descriptor
  const baseDesc: EntityDescriptor = metadataFactory(collection)(ws, trx, defId);

  // Step 1: Prepare the headers and extractors
  const headers: string[] = []; // Simple array of header displays
  const extracts: ((e: Entity) => string)[] = []; // Array of functions, one for each column to get the string value

  for (const col of columns) {
    const pathArray = (col.path || '').split('/').map(e => e.trim()).filter(e => !!e);

    // This will contain the display steps of a single header. E.g. Item / Created By / Name
    const headerArray: string[] = [];
    const navProps: NavigationPropDescriptor[] = [];
    let finalPropDesc: PropDescriptor = null;

    // Loop over all steps except last one
    let isError = false;
    let currentDesc = baseDesc;

    for (let i = 0; i < pathArray.length; i++) {
      const step = pathArray[i];
      const prop = currentDesc.properties[step];
      if (!prop) {
        isError = true;
        break;
      } else {
        headerArray.push(prop.label());
        if (prop.control === 'navigation') {
          currentDesc = metadataFactory(prop.collection || prop.type)(ws, trx, prop.definition);
          navProps.push(prop);
        } else if (i !== pathArray.length - 1) {
          // Only navigation properties are allowed unless this is the last one
          isError = true;
        } else {
          finalPropDesc = prop;
        }
      }
    }

    if (isError) {
      headers.push(`(${trx.instant('Error')})`);
      extracts.push(_ => `(${trx.instant('Error')})`);
    } else {
      headers.push(col.display || headerArray.join(' / ') || baseDesc.titleSingular() || trx.instant('DisplayName'));
      extracts.push(entity => {
        let i = 0;
        for (; i < navProps.length; i++) {
          const navProp = navProps[i];
          const propName = pathArray[i];

          if (entity.EntityMetadata[propName] === 2 || propName === 'Id') {

            const entitiesOfType = relatedEntities[navProp.collection || navProp.type];

            // Get the foreign key
            const fkValue = entity[navProp.foreignKeyName];
            if (!fkValue) {
              return ''; // The nav entity is null
            }

            // Get the nav entity
            entity = entitiesOfType[fkValue];
            if (!entity) {
              // Anomaly from Server
              console.error(`Property ${propName} loaded but null, even though FK ${navProp.foreignKeyName} is loaded`);
              return `(${trx.instant('Error')})`;
            }
          } else if (entity.EntityMetadata[propName] === 1) {
            // Masked because of user permissions
            return `*******`;
          } else {
            // Bug
            return `(${trx.instant('NotLoaded')})`;
          }
        }

        // Final step
        if (!!finalPropDesc) {
          const propName = pathArray[i];
          if (entity.EntityMetadata[propName] === 2 || propName === 'Id') {
            const val = entity[propName];
            return displayValue(val, finalPropDesc, trx);
          } else if (entity.EntityMetadata[propName] === 1) {
            // Masked because of user permissions
            return `*******`;
          } else {
            // Bug
            return `(${trx.instant('NotLoaded')})`;
          }
        } else {
          // It terminates with a nav prop
          return displayEntity(entity, currentDesc);
        }
      });
    }
  }

  // Step 2 Push headers in the result
  result.push(headers);

  // Step 3 Use extractors to convert the entities to strings and push them in the result
  for (const entity of entities) {
    const row: string[] = [];
    let index = 0;
    for (const extract of extracts) {
      row[index++] = extract(entity);
    }

    result.push(row);
  }

  // Finally: Return the result
  return result;
}

export function composeEntitiesFromResponse(
  response: EntitiesResponse,
  columns: ColumnDescriptor[],
  collection: string,
  defId: number,
  ws: WorkspaceService,
  trx: TranslateService): string[][] {

  addToWorkspace(response, ws);
  return composeEntities(response.Result, columns, collection, defId, ws, trx);
}

/**
 * Returns a string representation of the value based on the property descriptor.
 * IMPORTANT: Does not support navigation property descriptors, use displayEntity instead
 * @param value The value to represent as a string
 * @param prop The property descriptor used to format the value as a string
 */
export function displayValue(value: any, prop: PropDescriptor, trx: TranslateService): string {
  switch (prop.control) {
    case 'text': {
      return value;
    }
    case 'number': {
      if (value === undefined) {
        return null;
      }
      const digitsInfo = `1.${prop.minDecimalPlaces}-${prop.maxDecimalPlaces}`;
      return formatAccounting(value, digitsInfo);
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
      return !!prop && !!prop.format ? prop.format(value) : value === true ? trx.instant('Yes') : value === false ? trx.instant('No') : '';
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
  return !!entityDesc.format ? (!!entity ? entityDesc.format(entity) : '') : '(Format function missing)';
}

export function onCodeTextareaKeydown(elem: HTMLTextAreaElement, e: KeyboardEvent, setter: (x: string) => void) {

  const keycode = e.keyCode || e.which;
  if (keycode === 9) {
    e.preventDefault();

    const start = elem.selectionStart;
    const end = elem.selectionEnd;

    // Insert the tab at the caret position
    elem.value = elem.value || '';
    elem.value = elem.value.substring(0, start) + '\t' + elem.value.substring(end);

    // Return the caret where it was
    elem.selectionStart = elem.selectionEnd = start + 1;

    setter(elem.value);
  }
}
