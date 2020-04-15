import { EntitiesResponse } from './dto/entities-response';
import { WorkspaceService } from './workspace.service';
import { GetByIdResponse } from './dto/get-by-id-response';
import { EntityWithKey } from './entities/base/entity-with-key';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Observer } from 'rxjs';
import { EntityDescriptor } from './entities/base/metadata';

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
  result[response.CollectionName] = [ response.Result ];
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
    document.body.appendChild(a);
    a.setAttribute('style', 'display: none');
    a.href = url;
    a.download = fileName || 'file';
    a.click();
    a.remove();

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
        return friendlyStructure(res.status, trx.instant(`Error_UnableToReachServer`));

      case 400: // Bad Request
      case 422: // Unprocessible entity
        if (error.error instanceof Blob) {
          // TODO: Need a better solution to handle blobs
          return friendlyStructure(res.status, trx.instant(`Error_UnkownClientError`));
        } else {
          // These two status codes mean a friendly error is already coming from the server
          return friendlyStructure(res.status, res.error);
        }

      case 401:  // Unauthorized
        return friendlyStructure(res.status, trx.instant(`Error_LoginSessionExpired`));

      case 403:  // Forbidden
        return friendlyStructure(res.status, trx.instant(`Error_AccountDoesNotHaveSufficientPermissions`));

      case 404: // Not found
        return friendlyStructure(res.status, trx.instant(`Error_RecordNotFound`));

      case 500:  // Internal Server Error
        return friendlyStructure(res.status, trx.instant(`Error_UnhandledServerError`));

      default:  // Any other HTTP error
        return friendlyStructure(res.status, trx.instant(`Error_UnkownServerError`));
    }

  } else {
    console.error(error);
    return friendlyStructure(null, trx.instant(`Error_UnkownClientError`));
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
