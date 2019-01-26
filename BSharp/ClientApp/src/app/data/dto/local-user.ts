import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { RoleMembership, RoleMembershipForSave } from './role-membership';

export class LocalUserForSave<TRoleMembership = RoleMembershipForSave> extends DtoForSaveKeyBase {
    Name: string;
    Name2: string;
    Email: string;
    Roles: TRoleMembership[];
    AgentId: number | string;
}

export class LocalUser extends LocalUserForSave<RoleMembership> {
    ExternalId: string;
    IsActive: boolean;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}
