import { EntitiesResponse } from './dto/get-response';
import { WorkspaceService } from './workspace.service';
import { GetByIdResponse } from './dto/get-by-id-response';
import { DtoKeyBase } from './dto/dto-key-base';

// This handy function takes the entities from the response and all their related entities
// adds them to the workspace indexed by their IDs and returns the IDs of the entities
// this pattern ensures that entities are not spread everywhere in the app, but are
// maintained in a centralized workspace instead, so an update to the entity name for example
// will see that reflected updated everywhere where that name is displayed
export function addToWorkspace(response: EntitiesResponse, workspace: WorkspaceService,
  workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase }): (number | string)[] {

  // Add related entities
  const relatedEntities = response.RelatedEntities;
  addRelatedEntitiesToWorkspace(relatedEntities, workspace, workspaceApplyFns);

  // Add main entities
  {
    const mainEntities = response.Data;
    const collectionName = response.CollectionName;
    if (!collectionName) {
      // Programmer mistake
      console.error('collectionName is not specified by the server');
    } else {
      for (let i = 0; i < mainEntities.length; i++) {
        const freshItem = mainEntities[i];
        const staleItem = workspace.current[collectionName][freshItem.Id];
        apply(freshItem, staleItem, workspace, collectionName, workspaceApplyFns);
      }
    }

    // Return the IDs of the main entities
    return mainEntities.map(e => e.Id);
  }
}

export function addSingleToWorkspace(response: GetByIdResponse, workspace: WorkspaceService,
  workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase }): (number | string) {

  // Add related entities
  const relatedEntities = response.RelatedEntities;
  addRelatedEntitiesToWorkspace(relatedEntities, workspace, workspaceApplyFns);

  // Add main entities
  const freshItem = response.Entity;
  const collectionName = response.CollectionName;
  if (!collectionName) {
    // Programmer mistake
    console.error('collectionName is not specified by the server');
  } else {
    const staleItem = workspace.current[collectionName][freshItem.Id];
    apply(freshItem, staleItem, workspace, collectionName, workspaceApplyFns);
  }

  // Return the IDs of the main entities
  return freshItem.Id;
}

export function addRelatedEntitiesToWorkspace(relatedEntities: { [key: string]: DtoKeyBase[] },
  workspace: WorkspaceService, workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase }) {
  if (!!relatedEntities) {
    const collectionNames = Object.keys(relatedEntities);
    for (let c = 0; c < collectionNames.length; c++) {
      const collectionName = collectionNames[c];
      const collection = relatedEntities[collectionName];
      for (let i = 0; i < collection.length; i++) {
        const freshItem = collection[i];
        const staleItem = workspace.current[collectionName][freshItem.Id];
        apply(freshItem, staleItem, workspace, collectionName, workspaceApplyFns);
      }
    }
  }
}

function apply(freshItem: DtoKeyBase, staleItem: DtoKeyBase, workspace: WorkspaceService, collectionName: string,
  workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase }) {
    if (!!staleItem) {
      const applyFn = workspaceApplyFns ? workspaceApplyFns[collectionName] : null;
      workspace.current[collectionName][freshItem.Id] = applyFn ? applyFn(staleItem, freshItem) : freshItem;
    } else {
      workspace.current[collectionName][freshItem.Id] = freshItem;
    }
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
    a.download = fileName;
    a.click();
    a.remove();

    // Best practice to prevent a memory leak, especially in a SPA
    window.URL.revokeObjectURL(url);
  }
}

export enum Key {
  Tab = 9,
  Enter = 13,
  Escape = 27,
  Space = 32,
  PageUp = 33,
  PageDown = 34,
  End = 35,
  Home = 36,
  ArrowLeft = 37,
  ArrowUp = 38,
  ArrowRight = 39,
  ArrowDown = 40
}

export function toString(value: any): string {
  return (value !== undefined && value !== null) ? `${value}` : '';
}
