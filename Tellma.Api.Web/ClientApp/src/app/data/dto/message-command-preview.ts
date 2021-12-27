import { Observable } from 'rxjs';

export interface MessageCommandPreview {
    Version?: string;
    Messages?: MessagePreview[];
}

export interface MessagePreview {
    PhoneNumber?: string;
    Content?: string;
}
