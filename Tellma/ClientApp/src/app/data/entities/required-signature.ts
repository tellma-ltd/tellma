import { Entity } from './base/entity';

export interface RequiredSignature extends Entity {
    LineId?: number;
    ToState?: number;
    RuleType?: string;
    RoleId?: number;
    SignedById?: number;
    SignedAt?: string;
    OnBehalfOfUserId?: number;
    CanSign?: boolean;
    ProxyRoleId?: number;
    CanSignOnBehalf?: boolean;

    // IMPORTANT: When adding more properties here, remember to also add them
    // in document-details.component.ts in _requiredSignatureProps
}
