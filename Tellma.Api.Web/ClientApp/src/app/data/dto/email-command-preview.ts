export interface EmailCommandPreview {
    Version?: string;
    Emails?: EmailPreview[];
}

export interface EmailPreview {
    Version?: string;
    To?: string[];
    Cc?: string[];
    Bcc?: string[];
    Subject?: string;
    Body?: string;
    Attachments?: AttachmentPreview[];
}

export interface AttachmentPreview {
    DownloadName?: string;
    Body?: string;
}

export interface EmailCommandVersions {
    Version?: string;
    Emails?: EmailVersion[];
}

export interface EmailVersion {
    Index?: number;
    Version?: string;
}
