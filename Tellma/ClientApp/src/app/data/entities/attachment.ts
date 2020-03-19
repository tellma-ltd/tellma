import { EntityForSave } from './base/entity-for-save';

export interface AttachmentForSave extends EntityForSave {
    FileName?: string;
    FileExtension?: string;
    File?: string;

    // Only for client side
    toJSON?: () => AttachmentForSave;
    file?: File;
    downloading?: boolean;
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
