export interface TenantNotification {

    /**
     * The server time when the notification occurred
     */
    ServerTime: string;

    /**
     * The tenant Id that this event is associated with
     */
    TenantId: number;
  }

export interface ServerNotificationSummary {
    Inbox: InboxNotification;
    Notifications: NotificationsNotification;
}

export interface InboxNotification extends TenantNotification {
    Count?: number;
    UnknownCount?: number;
    UpdateInboxList?: boolean;
}

export interface NotificationsNotification extends TenantNotification {
    Count?: number;
    UnknownCount?: number;
}

export interface CacheNotification extends TenantNotification {
  Type: CacheType;
}

export type CacheType = 'Definitions' | 'Settings' | 'Permissions' | 'UserSettings';
