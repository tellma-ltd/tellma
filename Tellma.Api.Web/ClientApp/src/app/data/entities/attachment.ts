import { EntityForSave } from './base/entity-for-save';

export interface AttachmentForSave extends EntityForSave {
    FileName?: string;
    FileExtension?: string;
    File?: string;
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
