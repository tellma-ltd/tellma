import { EntityForSave } from './base/entity-for-save';

export interface ResourceUnitForSave extends EntityForSave {
    UnitId?: number;
    Multiplier?: number;
}

export interface ResourceUnit extends ResourceUnitForSave {
    ResourceId?: number;
    CreatedAt?: string;
    CreatedById?: number | string;
    ModifiedAt?: string;
    ModifiedById?: number | string;
}
