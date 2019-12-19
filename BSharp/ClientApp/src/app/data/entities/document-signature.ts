// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';

export class DocumentSignature extends EntityWithKey {
    DocumentId: number;
    SignedAt: string;
    AgentId: number;
    RoleId: number;
    CreatedAt: string;
    CreatedById: number;
}
