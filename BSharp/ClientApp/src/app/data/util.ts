import { EntitiesResponse } from './dto/get-response';
import { WorkspaceService } from './workspace.service';

// This handy function takes the entities from the response and all their related entities
// adds them to the workspace indexed by their IDs and returns the IDs of the entities
// this pattern ensures that entities are not spread everywhere in the app, but are
// maintained in a centralized workspace instead, so an update to the entity name for example
// will see that reflected updated everywhere where that name is displayed
export function addToWorkspace(response: EntitiesResponse, workspace: WorkspaceService): (number | string)[] {

  // Add related entities
  const relatedEntities = response.RelatedEntities;
  if (!!relatedEntities) {
    const collectionNames = Object.keys(relatedEntities);
    for (let c = 0; c < collectionNames.length; c++) {
      const collectionName = collectionNames[c];
      const collection = relatedEntities[collectionName];
      for (let i = 0; i < collection.length; i++) {
        const entity = collection[i];
        workspace.current[collectionName][entity.Id] = entity;
      }
    }
  }

  // Add main entities
  {
    const mainEntities = response.Data;
    const collectionName = response.CollectionName;
    if (!collectionName) {
      console.error('collectionName is not specified by the server');
    } else {
      for (let i = 0; i < mainEntities.length; i++) {
        const entity = mainEntities[i];
        workspace.current[collectionName][entity.Id] = entity;
      }
    }

    // Return the IDs of the main entities
    return mainEntities.map(e => e.Id);
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

    // Best practice to prevent a memory leak, especially in a SPA like bSharp
    window.URL.revokeObjectURL(url);
  }
}

