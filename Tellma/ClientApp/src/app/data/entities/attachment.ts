import { EntityWithKey } from './base/entity-with-key';

export interface AttachmentForSave extends EntityWithKey {
    FileName?: string;
    File?: string;

    // Only for client side
    toJSON?: () => AttachmentForSave;
    file?: File;
}

export interface Attachment extends AttachmentForSave {
    DocumentId?: number;
    FileId?: string;
    Size?: number;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
