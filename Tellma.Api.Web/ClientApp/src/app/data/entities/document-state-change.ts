// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';
import { DocumentState } from './document';

export interface DocumentStateChange extends EntityWithKey {
    DocumentId?: number;
    FromState?: DocumentState;
    ToState?: DocumentState;
    ModifiedAt?: string;
    ModifiedById?: number;
}
