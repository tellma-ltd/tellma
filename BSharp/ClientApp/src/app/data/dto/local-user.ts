import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { RoleMembership, RoleMembershipForSave } from './role-membership';

export class LocalUserForSave<TRoleMembership = RoleMembershipForSave> extends DtoForSaveKeyBase {
    Name: string;
    Name2: string;
    Email: string;
    Roles: TRoleMembership[];
    AgentId: number | string;
    Image: string;
}

export class LocalUser extends LocalUserForSave<RoleMembership> {
    ExternalId: string;
    ImageId: string;
    IsActive: boolean;
    LastAccess: string;
    CreatedAt: string;
    CreatedById: number | string;
    ModifiedAt: string;
    ModifiedById: number | string;
}

export class UserSettingsForClientForSave {
}

export class UserSettingsForClient {
    UserId: number;
    ImageId: string;
    Name: string;
    Name2: string;
}
