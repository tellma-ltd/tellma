import { Observable } from 'rxjs';

export interface MessageCommandPreview {
    Version?: string;
    Messages?: MessagePreview[];
}

export interface MessagePreview {
    ToPhoneNumber?: string;
    Content?: string;
}
