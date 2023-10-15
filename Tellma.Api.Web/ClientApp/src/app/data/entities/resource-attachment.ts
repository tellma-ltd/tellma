import { EntityForSave } from './base/entity-for-save';

export interface ResourceAttachmentForSave extends EntityForSave {
    CategoryId?: number;
    FileName?: string;
    FileExtension?: string;
    File?: string;

    // Only for client side
    toJSON?: () => ResourceAttachmentForSave;
    file?: File;
    downloading?: boolean;
}

export interface ResourceAttachment extends ResourceAttachmentForSave {
    ResourceId?: number;
    FileId?: string;
    Size?: number;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
