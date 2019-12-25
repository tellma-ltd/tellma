// tslint:disable:variable-name
import { EntityWithKey } from './base/entity-with-key';

export class DocumentSignature extends EntityWithKey {
    DocumentId: number;
    SignedAt: string;
    OnBehalfOfUserId: number;
    RoleId: number;
    CreatedAt: string;
    CreatedById: number;
}
