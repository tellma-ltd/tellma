import { EntityForSave } from './base/entity-for-save';

export interface RelationAttachmentForSave extends EntityForSave {
    CategoryId?: number;
    FileName?: string;
    FileExtension?: string;
    File?: string;

    // Only for client side
    toJSON?: () => RelationAttachmentForSave;
    file?: File;
    downloading?: boolean;
}

export interface RelationAttachment extends RelationAttachmentForSave {
    RelationId?: number;
    FileId?: string;
    Size?: number;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
