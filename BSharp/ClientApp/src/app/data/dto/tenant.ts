import { DtoForSaveKeyBase } from './dto-for-save-key-base';
import { DtoBase } from './dto-base';

export class TenantForSave extends DtoForSaveKeyBase {

}

export class Tenant extends TenantForSave {

}

export class TenantForClient extends DtoBase {
    Id: number;
    Name: string;
    Name2: string;
    ImageId: string;
}
