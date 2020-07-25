import { EntityForSave } from './base/entity-for-save';

export interface RelationUserForSave extends EntityForSave {
    UserId?: number;
}

export interface RelationUser extends RelationUserForSave {
    RelationId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
