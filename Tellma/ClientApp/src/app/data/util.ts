import { EntitiesResponse } from './dto/entities-response';
import { WorkspaceService, TenantWorkspace } from './workspace.service';
import { GetByIdResponse } from './dto/get-by-id-response';
import { EntityWithKey } from './entities/base/entity-with-key';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { from, Observable, Observer, throwError } from 'rxjs';
import {
  EntityDescriptor, PropDescriptor, NavigationPropDescriptor, metadata, Control, PropVisualDescriptor, Collection, entityDescriptorImpl
} from './entities/base/metadata';
import { formatNumber, formatDate, formatPercent } from '@angular/common';
import { Entity } from './entities/base/entity';
import { insert, set, getSelection } from 'text-field-edit';
import { concatMap, map } from 'rxjs/operators';
import { formatSerial } from './entities/document';

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

function safeToOpenDirectly(blob: Blob): boolean {
  switch (blob.type) {
    case 'application/pdf':
    case 'image/jpeg':
    case 'image/png':
      return true;
  }
  return false;
}

export function downloadBlob(blob: Blob, fileName: string) {
  // Helper function to download a blob from memory to the user's computer,
  // Without having to open a new window first
  if (window.navigator && window.navigator.msSaveOrOpenBlob) {
    // To support IE and Edge
    if (safeToOpenDirectly(blob)) {
      window.navigator.msSaveOrOpenBlob(blob, fileName);
    } else {
      window.navigator.msSaveBlob(blob, fileName);
    }
  } else {

    // Create an in memory url for the blob, further reading:
    // https://developer.mozilla.org/en-US/docs/Web/API/URL/createObjectURL
    const url = window.URL.createObjectURL(blob);

    if (safeToOpenDirectly(blob)) {
      // Opens the file in a new browser tab
      window.open(url);

      // const win = window.open();

      // // Title
      // const title = win.document.createElement('title');
      // title.appendChild(win.document.createTextNode(fileName));

      // // Icon
      // const link = win.document.createElement('link');
      // link.rel = 'icon';
      // // link.type = 'image/x-icon';
      // link.href = 'favicon.ico';


      // // Body
      // const iframe = win.document.createElement('iframe');
      // iframe.src = url;
      // iframe.width = '100%';
      // iframe.height = '100%';
      // iframe.style.border = 'none';

      // win.document.head.appendChild(title);
      // win.document.head.appendChild(link);
      // win.document.body.appendChild(iframe);
      // win.document.body.style.margin = '0';

    } else {
      console.log(blob.type);

      // Below is a trick for downloading files without opening a new browser tab
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName || 'file';
      a.click();
    }

    // Best practice to prevent a memory leak, especially in a SPA
    window.URL.revokeObjectURL(url);
  }
}

const _maxAttachmentSize = 20 * 1024 * 1024;

export function onFileSelected(
  input: HTMLInputElement,
  pendingFilesSize: number,
  translate: TranslateService) {
  const files = input.files as FileList;
  if (!files) {
    return;
  }

  // Convert the FileList to an array
  const filesArray: File[] = [];
  // tslint:disable-next-line:prefer-for-of
  for (let i = 0; i < files.length; i++) {
    filesArray.push(files[i]);
  }

  // Clear the input field
  input.value = '';

  // Calculate total size of files
  const totalSize = filesArray
    .map(e => e.size || 0)
    .reduce((total, v) => total + v, 0);

  // Make sure total size of selected files doesn't exceed maximum size
  if (totalSize > _maxAttachmentSize) {
    const msg = translate.instant('Error_FileSizeExceedsMaximumSizeOf0', { size: fileSizeDisplay(_maxAttachmentSize) });
    return throwError(msg);
  }

  if (pendingFilesSize + totalSize > _maxAttachmentSize) {
    const msg = translate.instant('Error_PendingFilesExceedMaximumSizeOf0', { size: fileSizeDisplay(_maxAttachmentSize) });
    return throwError(msg);
  }

  return from(filesArray).pipe(
    map(file => getDataURL(file).pipe(map(dataUrl => ({ file, dataUrl })))),
    concatMap(obs => obs),
    map(({ file, dataUrl }) => {
      // Get the base64 value from the data URL
      const commaIndex = dataUrl.indexOf(',');
      const fileBytes = dataUrl.substr(commaIndex + 1);
      const fileNamePieces = file.name.split('.');
      const extension = fileNamePieces.length > 1 ? fileNamePieces.pop() : null;
      const fileName = fileNamePieces.join('.') || '???';

      const attachment = {
        Id: null,
        File: fileBytes,
        FileName: fileName,
        FileExtension: extension,
      };

      return { attachment, file };
    })
  );

  // const observables = filesArray.map(file => getDataURL(file).pipe(
  //   map(dataUrl => {
  //     // Get the base64 value from the data URL
  //     const commaIndex = dataUrl.indexOf(',');
  //     const fileBytes = dataUrl.substr(commaIndex + 1);
  //     const fileNamePieces = file.name.split('.');
  //     const extension = fileNamePieces.length > 1 ? fileNamePieces.pop() : null;
  //     const fileName = fileNamePieces.join('.') || '???';

  //     const attachment = {
  //       Id: null,
  //       File: fileBytes,
  //       FileName: fileName,
  //       FileExtension: extension,
  //     };

  //     return { attachment, file };
  //   })
  // ));

  // return zip(observables);
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
  return (unitIndex ? fileSize.toFixed(1) : fileSize) + ' KMGTPEZY'[unitIndex] + 'B';
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
        if (prop.datatype === 'entity') {
          currentDesc = metadataFactory(prop.control)(ws, trx, prop.definitionId);
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

            const entitiesOfType = relatedEntities[navProp.control];

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
 * Overrides the default behavior of the TAB key and implements insertion of tab and block
 * indentation instead, for a more natural experience for textareas used to edit code
 */
export function onCodeTextareaKeydown(txtarea: HTMLTextAreaElement, e: KeyboardEvent, setter: (x: string) => void) {
  if ((e.keyCode || e.which) === 9) {

    // Prevent TAB's default behavior
    e.preventDefault();

    // IF the user highlight spans multiple lines, we indent all the highlighted lines (block-indent)
    const selection = getSelection(txtarea);
    if (!!selection && /\r|\n/.test(selection)) {

      // Get selection boundaries
      const selectionFrom = Math.min(txtarea.selectionStart, txtarea.selectionEnd);
      const selectionTo = Math.max(txtarea.selectionStart, txtarea.selectionEnd);

      // Get the entire text
      const text = txtarea.value;

      // Find the beginning of the first highlighted line
      let startOfFirstLine = selectionFrom;
      while (startOfFirstLine > 0) {
        if (text[startOfFirstLine - 1] === '\n') {
          break;
        }
        startOfFirstLine--;
      }

      // Split the text on the selection boundaries
      const before = text.slice(0, startOfFirstLine);
      const after = text.slice(selectionTo);
      let between = text.slice(startOfFirstLine, selectionTo);

      // Indent all lines in the between section
      between = '\t' + between.replace(/\r|\n/g, '\n\t');

      // Reassemble the pieces and replace the textarea contents with the new result
      const finalText = before + between + after;
      set(txtarea, finalText);

      // Reset the selection where it was before pressing TAB
      txtarea.selectionStart = selectionFrom + (startOfFirstLine === selectionFrom ? 0 : 1); // +1 To account for the extra tab
      txtarea.selectionEnd = before.length + between.length;
    } else {
      // ELSE: Relies on text-field-edit to insert an undo-able tab character
      insert(txtarea, '\t');
    }
  }
}

/**
 * The Levenshtein distance between two strings, with a maximum cap for optimization (smallest cap is 1).
 * The code is modified from https://bit.ly/2TiTyWZ, credit to Andrei Mackenzie
 */
export function getEditDistance(a: string, b: string, cap: number = 10000000000): number {
  a = a || '';
  b = b || '';

  // Optimizations
  if (a === b) {
    return 0;
  } else if (cap <= 1) {
    return 1;
  } else if (a.length === 0) {
    return Math.min(cap, b.length);
  } else if (b.length === 0) {
    return Math.min(cap, a.length);
  }

  const matrix = [];

  // Reused varaiables
  let capped = true;
  let distance: number;
  let substitutionCost: number;

  // Increment along the first column of each row
  let i = 0;
  for (; i <= b.length; i++) {
    matrix[i] = [i];
  }

  // Increment each column in the first row
  let j = 0;
  for (; j <= a.length; j++) {
    matrix[0][j] = j;
  }

  // Fill in the rest of the matrix
  for (i = 1; i <= b.length; i++) {
    capped = true;
    for (j = 1; j <= a.length; j++) {
      if (b.charAt(i - 1) === a.charAt(j - 1)) {
        substitutionCost = 0;
      } else {
        substitutionCost = 1;
      }

      distance = Math.min(
        matrix[i - 1][j] + 1, // deletion
        matrix[i][j - 1] + 1, // insertion
        matrix[i - 1][j - 1] + substitutionCost); // substitution

      matrix[i][j] = distance;

      // Optimization: If the entire row is >= cap, then we terminate the outer loop
      if (capped && distance < cap) {
        capped = false;
      }
    }

    if (capped) {
      return cap;
    }
  }

  return matrix[b.length][a.length];
}

export function colorFromExtension(extension: string): string {
  const icon = iconFromExtension(extension);
  switch (icon) {
    case 'file-pdf': return '#CA342B';
    case 'file-word': return '#345692';
    case 'file-excel': return '#316F3E';
    case 'file-powerpoint': return '#BD4D2D';
    case 'file-archive': return '#E5BE36';
    case 'file-image': return '#3E7A7E';
    case 'file-video': return '#A12F5E'; // CC5747
    case 'file-audio': return '#BA7D27';

    case 'file-alt': // text files
    case 'file': return '#6c757d';
  }

  return null;
}

export function iconFromExtension(extension: string): string {
  if (!extension) {
    return 'file';
  } else {
    extension = extension.toLowerCase();
    switch (extension) {
      case 'pdf':
        return 'file-pdf';

      case 'doc':
      case 'docx':
        return 'file-word';

      case 'xls':
      case 'xlsx':
        return 'file-excel';

      case 'ppt':
      case 'pptx':
        return 'file-powerpoint';

      case 'txt':
      case 'rtf':
        return 'file-alt';

      case 'zip':
      case 'rar':
      case '7z':
      case 'tar':
        return 'file-archive';

      case 'jpg':
      case 'jpeg':
      case 'jpe':
      case 'jif':
      case 'jfif':
      case 'jfi':
      case 'png':
      case 'ico':
      case 'gif':
      case 'webp':
      case 'tiff':
      case 'tif':
      case 'psd':
      case 'raw':
      case 'arw':
      case 'cr2':
      case 'nrw':
      case 'k25':
      case 'bmp':
      case 'dib':
      case 'heif':
      case 'heic':
      case 'ind':
      case 'indd':
      case 'indt':
      case 'jp2':
      case 'j2k':
      case 'jpf':
      case 'jpx':
      case 'jpm':
      case 'mj2':
      case 'svg':
      case 'svgz':
      case 'ai':
      case 'eps':
        return 'file-image';

      case 'mpg':
      case 'mp2':
      case 'mpeg':
      case 'mpe':
      case 'mpv':
      case 'ogg':
      case 'mp4':
      case 'm4p':
      case 'm4v':
      case 'avi':
      case 'wmv':
      case 'mov':
      case 'qt':
      case 'flv':
      case 'swf':
        return 'file-video';

      case 'mp3':
      case 'aac':
      case 'wma':
      case 'flac':
      case 'alac':
      case 'wav':
      case 'aiff':
        return 'file-audio';

      default:
        return 'file';
    }
  }
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
    case 'percent': {
      if (value === undefined) {
        return null;
      }
      const digitsInfo = `1.${prop.minDecimalPlaces}-${prop.maxDecimalPlaces}`;
      return isSpecified(value) ? formatPercent(value, 'en-GB', digitsInfo) : '';
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
    case 'choice': {
      return !!prop && !!prop.format ? prop.format(value) : null;
    }
    case 'serial': {
      return !!prop ? formatSerial(value, prop.prefix, prop.codeWidth) : (value + '');
    }
    default:
      // Programmer error
      if (prop.datatype === 'entity') {
        throw new Error('calling "displayValue" on a navigation property, use "displayEntity" instead');
      } else {
        console.error(prop);
        throw new Error(`calling "displayValue" on a property of an unknown control`);
      }
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

/**
 * Constructs a PropDescriptor from scratch or overrides and existing one using the provided control and control options values.
 */
export function descFromControlOptions(
  ws: TenantWorkspace,
  control: Control,
  optionsJSON: string,
  desc?: PropVisualDescriptor): PropVisualDescriptor {

  // Optimization: Nothing to override
  if (!!desc && desc.control === control && !optionsJSON) {
    return desc;
  }

  let options: any;
  if (!!optionsJSON) {
    try {
      options = JSON.parse(optionsJSON);
    } catch { }
  }
  options = options || {};

  desc = desc || { control: 'text' }; // Makes the subsequent logic tidier
  control = control || desc.control;

  switch (control) {
    case 'text':
    case 'boolean':
    case 'date':
    case 'datetime':
      return { control };

    case 'number':
    case 'percent':
      let minDecimalPlaces = 0;
      if (isSpecified(options.minDecimalPlaces)) {
        minDecimalPlaces = options.minDecimalPlaces;
      } else if (desc.control === 'number' || desc.control === 'percent') {
        minDecimalPlaces = desc.minDecimalPlaces;
      }

      let maxDecimalPlaces = Math.max(minDecimalPlaces, 4);
      if (isSpecified(options.maxDecimalPlaces)) {
        maxDecimalPlaces = options.maxDecimalPlaces;
      } else if (desc.control === 'number' || desc.control === 'percent') {
        maxDecimalPlaces = desc.maxDecimalPlaces;
      }

      let alignment: 'right' | 'left' | 'center' = 'right';
      if (isSpecified(options.alignment)) {
        alignment = options.alignment;
      } else if (desc.control === 'number' || desc.control === 'percent') {
        alignment = desc.alignment;
      }

      return { control, minDecimalPlaces, maxDecimalPlaces, alignment };

    case 'choice':
      let choices: (string | number)[] = []; // default value
      let format = (c: string | number) => c + ''; // default value
      let color: (c: string | number) => string; // No default value
      if (!!options.choices && options.choices.length > 0) {
        const choicesArray = options.choices as { value: string, name: string, name2: string, name3: string }[];
        const choicesDic: { [key: string]: () => string } = {};
        for (const e of choicesArray) {
          choicesDic[e.value] = () => ws.getMultilingualValueImmediate(e, 'name');
        }

        choices = choicesArray.map((e) => e.value);
        format = (c: string) => choicesDic[c] ? choicesDic[c]() : c;
      } else if (desc.control === 'choice') {
        choices = desc.choices;
        format = desc.format;
        color = desc.color; // Only override this when overriding the others
      }

      return { control, choices, format, color };

    case 'serial':
      let prefix = '';
      if (isSpecified(options.prefx)) {
        prefix = options.prefix;
      } else if (desc.control === 'serial') {
        prefix = desc.prefix;
      }

      let codeWidth = 4;
      if (isSpecified(options.prefx)) {
        codeWidth = options.codeWidth;
      } else if (desc.control === 'serial') {
        codeWidth = desc.codeWidth;
      }

      return { control, prefix, codeWidth };

    default:
      let filter: string;
      if (isSpecified(options.filter)) {
        filter = options.filter;
      } else if (desc.control === control) {
        filter = desc.filter;
      }

      let definitionId: number;
      if (isSpecified(options.definitionId)) {
        definitionId = options.definitionId;
      } else if (desc.control === control) {
        definitionId = desc.definitionId;
      }

      return { control, filter, definitionId };
  }
}

/**
 * Auto-calculate the PropDescriptor for displaying custom parameters
 */
export function computePropDesc(
  ws: WorkspaceService,
  trx: TranslateService,
  collection: Collection,
  definitionId: number,
  path: string[],
  property: string,
  modifier: string): PropDescriptor {
  const entityDesc = entityDescriptorImpl(
    path,
    collection,
    definitionId,
    ws,
    trx);

  let propDesc: PropDescriptor;
  let propName = property;
  if (propName === 'Node' && !!entityDesc.properties.ParentId) {
    propDesc = entityDesc.properties.ParentId;
    propName = 'ParentId';
  } else {
    propDesc = entityDesc.properties[propName];
    if (!!propDesc && propDesc.datatype === 'entity') {
      throw new Error(`Cannot terminate a filter path with a navigation property like '${propName}'`);
    }
  }

  // Check if the filtered property is a foreign key of another nav,
  // property, if so use the descriptor of that nav property instead
  propDesc = Object.keys(entityDesc.properties)
    .map(e => entityDesc.properties[e])
    .find(e => e.datatype === 'entity' && e.foreignKeyName === propName)
    || propDesc; // Else rely on the descriptor of the prop itself

  if (!propDesc) {
    throw new Error(`Property '${propName}' does not exist on '${entityDesc.titlePlural()}'`);
  }

  if (!!modifier) {
    // A modifier is specified, the prop descriptor is hardcoded per modifier
    propDesc = modifiedPropDesc(propDesc, modifier, trx);
  }

  return propDesc;
}

/**
 * Produces a new PropDescriptor based on an old PropDescriptor + modifier
 */
export function modifiedPropDesc(propDesc: PropDescriptor, modifier: string, trx: TranslateService): PropDescriptor {
  const oldLabel = propDesc.label;
  const label = () => `${oldLabel()} (${trx.instant('Modifier_' + modifier)})`;
  switch (modifier) {
    case 'dayofyear':
    case 'day':
    case 'week':
      propDesc = {
        datatype: 'integral',
        control: 'number',
        label,
        minDecimalPlaces: 0,
        maxDecimalPlaces: 0
      };
      break;
    case 'year':
      propDesc = {
        datatype: 'integral',
        control: 'choice',
        label,
        choices: [...Array(30).keys()].map(y => y + 2000),
        format: (c: number | string) => !c ? '' : c.toString()
      };
      break;
    case 'quarter':
      propDesc = {
        datatype: 'integral',
        control: 'choice',
        label,
        choices: [1, 2, 3, 4],
        format: (c: number | string) => !c ? '' : trx.instant(`ShortQuarter${c}`)
      };
      break;
    case 'month':
      propDesc = {
        datatype: 'integral',
        control: 'choice',
        label,
        choices: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        format: (c: number | string) => !c ? '' : trx.instant(`ShortMonth${c}`)
      };
      break;
    case 'weekday':
      propDesc = {
        datatype: 'integral',
        control: 'choice',
        label,
        choices: [2 /* Mon */, 3, 4, 5, 6, 7, 1 /* Sun */],
        // SQL Server numbers the days differently from ngb-datepicker
        format: (c: number) => !c ? '' : trx.instant(`ShortDay${(c - 1) === 0 ? 7 : c - 1}`)
      };
      break;
  }
  return propDesc;
}

export function updateOn(desc: PropVisualDescriptor): 'change' | 'blur' {

  switch (desc.control) {
    case 'text':
    case 'number':
    case 'percent':
    case 'serial':
    case 'date':
    case 'datetime':
      return 'blur';
    case 'choice':
    case 'boolean':
      return 'change';
    default:
      const x = desc.filter; // So it will complain if we forget a control
      return !!x ? 'change' : 'change';
  }
}
