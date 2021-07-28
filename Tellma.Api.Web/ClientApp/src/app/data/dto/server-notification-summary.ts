export interface TenantStatusToSend {

    /**
     * The server time when the notification occurred
     */
    ServerTime: string;

    /**
     * The tenant Id that this event is associated with
     */
    TenantId: number;
  }

export interface NotificationSummary {
    Inbox: InboxStatusToSend;
}

export interface InboxStatusToSend extends TenantStatusToSend {
    Count?: number;
    UnknownCount?: number;
    UpdateInboxList?: boolean;
}

export interface CacheStatusToSend extends TenantStatusToSend {
  Type: CacheType;
}

export type CacheType = 'Definitions' | 'Settings' | 'Permissions' | 'UserSettings';
