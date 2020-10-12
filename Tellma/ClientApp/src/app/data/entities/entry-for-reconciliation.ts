// tslint:disable:variable-name
// tslint:disable:max-line-length
import { EntityForSave } from './base/entity-for-save';

export interface EntryForReconciliation extends EntityForSave {
    PostingDate?: string;
    Direction?: 1 | -1;
    MonetaryValue?: number;
    ExternalReference?: string;
}
