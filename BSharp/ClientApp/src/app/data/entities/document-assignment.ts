// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';

export class DocumentAssignment extends EntityWithKey {
    DocumentId: number;
    AssigneeId: number;
    Comment: string;
    CreatedAt: string;
    CreatedById: number;
    OpenedAt: string;
}
