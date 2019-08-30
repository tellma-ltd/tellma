import { EntityWithKey } from './base/entity-with-key';

export class AgentForSave extends EntityWithKey {

  Name: string;
  Name2: string;
  Name3: string;
  Code: string;
  PreferredLanguage: string;
  Image: string;
}

export class Agent extends AgentForSave {
  ImageId: string;
  IsActive: boolean;
  CreatedAt: string;
  CreatedById: number | string;
  ModifiedAt: string;
  ModifiedById: number | string;
}
