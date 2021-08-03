import { Entity } from './base/entity';

export interface RequiredSignature extends Entity {
    LineId?: number; // Not grouped by
    ToState?: number;
    RuleType?: string;
    RoleId?: number;
    CustodianId?: number;
    UserId?: number;
    LineSignatureId?: number; // Not grouped by
    SignedById?: number;
    SignedAt?: string;
    OnBehalfOfUserId?: number;
    LastUnsignedState?: number;
    LastNegativeState?: number;
    CanSign?: boolean;
    ProxyRoleId?: number;
    CanSignOnBehalf?: boolean;
    ReasonId?: number;
    ReasonDetails?: string;

    // IMPORTANT: When adding more properties here, remember to also add them
    // in document-details.component.ts in _requiredSignatureProps
}
