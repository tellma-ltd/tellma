import { EntityForSave } from './base/entity-for-save';

export interface AgentAttachmentForSave extends EntityForSave {
    CategoryId?: number;
    FileName?: string;
    FileExtension?: string;
    File?: string;

    // Only for client side
    toJSON?: () => AgentAttachmentForSave;
    file?: File;
    downloading?: boolean;
}

export interface AgentAttachment extends AgentAttachmentForSave {
    AgentId?: number;
    FileId?: string;
    Size?: number;
    CreatedAt?: string;
    CreatedById?: number;
    ModifiedAt?: string;
    ModifiedById?: number;
}
