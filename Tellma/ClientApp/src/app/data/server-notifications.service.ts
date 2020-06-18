import { Injectable } from '@angular/core';
import { HubConnectionBuilder, LogLevel, HubConnection, HubConnectionState } from '@microsoft/signalr';
import { appsettings } from './global-resolver.guard';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { from, Observable, of, Subject } from 'rxjs';
import { WorkspaceService } from './workspace.service';

/**
 * This is a wrapper around the SignalR library for two way real time communication with the server
 * Given the overhead that two-way communication incurs, its use is restricted here to specific scenarios:
 * 1 - Number of assigned documents (inbox) has changed (red badge)
 * 2 - Number of notifications has changed (2nd red badge, to be implemented)
 * 3 - The cache has been invalidated
 */
@Injectable({
  providedIn: 'root'
})
export class ServerNotificationsService {

  private _connection: HubConnection;
  private _signedin = false;
  private _tenantId: number; // The current tenantId
  private _inboxChanged$ = new Subject<InboxNotification>();
  private _notificationsChanged$ = new Subject<NotificationsNotification>();
  private _cacheInvalidated$ = new Subject<CacheNotification>();

  private state: {
    [tenantId: number]: TenantState;
  } = {};

  constructor(private authStorage: OAuthStorage) { }

  private initConnection() {
    if (!this._connection) {
      const url = appsettings.apiAddress + 'api/hubs/notifications';
      const conn = new HubConnectionBuilder()
        .withUrl(url, { accessTokenFactory: () => this.authStorage.getItem('access_token') })
        .configureLogging(LogLevel.None)
        .build();

      // Implement the client-side methods
      conn.on(MethodNames.UpdateInbox, this.handleFreshInboxCountsAndNotify);
      conn.on(MethodNames.UpdateNotifications, this.handleFreshNotifications);
      conn.on(MethodNames.InvalidateCache, this.handleInvalidateCache);

      // Handle unexpected closing
      conn.onclose(this.onclose);

      this._connection = conn;
    }
  }

  private async start(): Promise<void> {
    if (!this._signedin) {
      return;
    }

    this.initConnection();

    try {
      await this._connection.start();
      // this.workspace.offline = false;
    } catch (err) {
      // this.workspace.offline = true;
      // console.error('Error starting SignalR connection...');

      // Keep trying every second while the user is sigend in
      setTimeout(_ => this.start(), 1000);
    }

    this.recap();
  }

  private async closeAndCleanup(): Promise<void> {
    try {
      await this._connection.stop();
    } finally {
      // Cleanup the state
      delete this._tenantId;
      this.state = {};
    }
  }

  private recap = async () => {

    if (!!this._tenantId && this._connection.state === HubConnectionState.Connected) {
      try {
        const summary: ServerNotificationSummary = await this._connection.invoke(MethodNames.RecapOf, { TenantId: this._tenantId });
        this.handleFreshInboxCountsAndNotify(summary.Inbox);
        this.handleFreshNotificationsAndNotify(summary.Notifications);
      } catch (err) {
        console.error(MethodNames.RecapOf + ' returned an Error...', err);
      }
    }
  }

  private onclose = (err: Error): void => {
    if (!!err) {
      // console.error('SignalR connection closed unexpectedly...');

      // // Set offline
      // this.workspace.offline = true;

      // Mark all states as stale
      for (const tenantId of Object.keys(this.state)) {
        this.state[tenantId].isFresh = false;
      }

      // Try again
      this.start();
    } else {
      // this.workspace.offline = false; // Offline is no longer tracked
    }
  }

  private handleFreshInboxCountsAndNotify = (serverNotification: InboxNotification) => {
    if (!serverNotification) {
      return;
    }

    const updated = this.handleFreshInboxCounts(
      serverNotification.TenantId,
      serverNotification.ServerTime,
      serverNotification.Count,
      serverNotification.UnknownCount
    );

    if (updated) {
      this._inboxChanged$.next(serverNotification);
    }
  }

  public handleFreshInboxCounts = (tenantId: number, serverTime: string, count: number, unknownCount: number): boolean => {
    const tenantState = (this.state[tenantId] = this.state[tenantId] || { isFresh: true });

    const localNotification = tenantState.inbox;
    if (!localNotification || localNotification.serverTime < serverTime) {
      tenantState.inbox = { count, unknownCount, serverTime };
      return true;
    }

    return false;
  }

  private handleFreshNotificationsAndNotify = (serverNotification: NotificationsNotification) => {
    if (!serverNotification) {
      return;
    }

    const updated = this.handleFreshNotifications(
      serverNotification.TenantId,
      serverNotification.ServerTime,
      serverNotification.Count,
      serverNotification.UnknownCount
    );

    if (updated) {
      this._notificationsChanged$.next(serverNotification);
    }
  }

  public handleFreshNotifications = (tenantId: number, serverTime: string, count: number, unknownCount: number): boolean => {
    const tenantState = (this.state[tenantId] = this.state[tenantId] || { isFresh: true });

    const localNotification = tenantState.notifications;
    if (!localNotification || localNotification.serverTime < serverTime) {
      tenantState.notifications = { count, unknownCount, serverTime };
      return true;
    }

    return false;
  }

  private handleInvalidateCache = (serverNotification: CacheNotification, notify = true) => {
    // TODO: Invalidate Cache
    if (notify) {
      this._notificationsChanged$.next(serverNotification);
    }
  }


  // Public API

  /**
   * Starts the SignalR connection and recaps from the server the state summary
   */
  public signin(): Observable<void> {
    if (!this._signedin) {
      this._signedin = true;
      return from(this.start());
    } else {
      return of();
    }
  }

  /**
   * Closes the SignalR connection and cleans up the state
   */
  public signout(): Observable<void> {
    if (this._signedin) {
      this._signedin = false;
      return from(this.closeAndCleanup());
    } else {
      return of();
    }
  }

  /**
   * Sets the current tenant Id and if the state of that tenant is stale recaps from the server
   */
  public connect(tenantId: number): Observable<void> {
    if (this._tenantId !== tenantId) {
      this._tenantId = tenantId;
      const tenantState = this.state[tenantId];
      if (!!tenantState && tenantState.isFresh) {
        return of();
      } else {
        return from(this.recap());
      }
    } else {
      return of();
    }
  }

  /**
   * Returns the state of the last tenantId set with connect(tenantId)
   */
  public get tenantState(): TenantState {
    return !!this._tenantId ? this.state[this._tenantId] : undefined;
  }

  /**
   * Emits whenever a new inbox notification arrives from the server
   */
  public get inboxChanged$(): Observable<InboxNotification> {
    return this._inboxChanged$;
  }

  /**
   * Emits whenever a new notifications notification arrives from the server
   */
  public get notificationsChanged$(): Observable<NotificationsNotification> {
    return this._notificationsChanged$;
  }

  /**
   * Emits whenever a new inbox notification has arrived from the server
   */
  public get cacheInvalidated$(): Observable<CacheNotification> {
    return this._cacheInvalidated$;
  }
}

/////////////// DTOs

interface ServerNotificationSummary {
  Inbox: InboxNotification;
  Notifications: NotificationsNotification;
}

interface InboxNotification extends TenantNotification {
  Count?: number;
  UnknownCount?: number;
  UpdateInboxList?: boolean;
}

interface NotificationsNotification extends TenantNotification {
  Count?: number;
  UnknownCount?: number;
}

interface CacheNotification extends TenantNotification {
  Type: CacheType;
}

export type CacheType = 'Definitions' | 'Settings' | 'Permissions' | 'UserSettings';

enum MethodNames {
  RecapOf = 'RecapOf',
  UpdateInbox = 'UpdateInbox',
  UpdateNotifications = 'UpdateNotifications',
  InvalidateCache = 'InvalidateCache',
}

interface TenantNotification {

  /**
   * The server time when the notification occurred
   */
  ServerTime: string;

  /**
   * The tenant Id that this event is associated with
   */
  TenantId: number;
}

export interface TenantState {
  notifications?: { count: number, unknownCount: number, serverTime: string };
  inbox?: { count: number, unknownCount: number, serverTime: string };
  isFresh: boolean;
}
