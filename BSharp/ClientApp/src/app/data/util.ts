import { GetResponse } from './dto/get-response';
import { WorkspaceService } from './workspace.service';

// This handy function takes the entities from the response and all their related entities
// adds them to the workspace indexed by their IDs and returns the IDs of the entities
// this pattern ensures that entities are not spread everywhere in the app, but are
// maintained in a centralized workspace instead, so an update to the entity name for example
// will see that reflected updated everywhere where that name is displayed
export function addToWorkspace(response: GetResponse, workspace: WorkspaceService): (number | string)[] {

  // Add related entities
  const relatedEntities = response.RelatedEntities;
  const collectionNames = Object.keys(relatedEntities);
  for (let c = 0; c < collectionNames.length; c++) {
    const collectionName = collectionNames[c];
    const collection = relatedEntities[collectionName];
    for (let i = 0; i < collection.length; i++) {
      const entity = collection[i];
      workspace.current[collectionName][entity.Id] = entity;
    }
  }

  // Add main entities
  {
    const mainEntities = response.Data;
    const collectionName = response.CollectionName;
    for (let i = 0; i < mainEntities.length; i++) {
      const entity = mainEntities[i];
      workspace.current[collectionName][entity.Id] = entity;
    }

    // Return the IDs of the main entities
    return mainEntities.map(e => e.Id);
  }

}
