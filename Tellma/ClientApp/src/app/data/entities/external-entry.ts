import { EntityForSave } from './base/entity-for-save';

export interface ExternalEntryForSave extends EntityForSave {
    PostingDate?: string;
    Direction?: 1 | -1;
    AccountId?: number;
    CustodyId?: number;
    MonetaryValue?: number;
    ExternalReference?: string;
}

export interface ExternalEntry extends ExternalEntryForSave {
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
