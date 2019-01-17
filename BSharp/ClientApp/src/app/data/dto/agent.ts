import { Custody } from './custody';

export class AgentForSave extends Custody {
  IsRelated: boolean;
  TaxIdentificationNumber: string;
  Title: string;
  Title2: string;
  Gender: 'M' | 'F';
}

export class Agent extends AgentForSave {
  CustodyType: string;
  AgentType: string;
  IsActive: boolean;
  CreatedAt: string;
  CreatedBy: string;
  ModifiedAt: string;
  ModifiedBy: string;
}

// Choice list (Also repeated in measurement units master template)
export const Agent_Gender = {
  'M': 'Agent_Male',
  'F': 'Agent_Female',
};
