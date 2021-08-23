import { Injectable } from '@angular/core';
import { HubConnectionBuilder, LogLevel, HubConnection, HubConnectionState } from '@microsoft/signalr';
import { appsettings } from './global-resolver.guard';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { from, Observable, of, Subject } from 'rxjs';
import { ApiService } from './api.service';
import { WorkspaceService } from './workspace.service';
import {
  InboxStatusToSend,
  CacheStatusToSend,
  NotificationSummary
} from './dto/server-notification-summary';
import { tap, catchError } from 'rxjs/operators';
import { FriendlyError } from './util';

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
  private _inboxChanged$ = new Subject<InboxStatusToSend>();
  private _cacheInvalidated$ = new Subject<CacheStatusToSend>();

  private _currentTenantId;

  private state: {
    [tenantId: number]: TenantState;
  } = {};


  constructor(private authStorage: OAuthStorage, private api: ApiService, private wss: WorkspaceService) {
    // Listen to workspace changes, if the tenantId changes, recap summary of the new tenant (if it isn't already fresh)
    wss.stateChanged$.subscribe(() => {
      if (wss.isApp && this._currentTenantId !== wss.ws.tenantId) {
        this._currentTenantId = wss.ws.tenantId;

        const tenantState = this.state[wss.ws.tenantId];
        if (!tenantState || !tenantState.isFresh) {
          this.recap();
        }
      }
    });
  }

  /**
   * Creates and configures the _connection object
   */
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

  /**
   * If the user is signed in this establishes the SignalR connection, or keeps trying relentlessly
   */
  private async start(): Promise<void> {
    if (!this._signedin) {
      return;
    }

    this.initConnection();

    try {
      await this._connection.start();
      // console.log('SignalR connection established');
    } catch (err) {
      // Keep trying every second while the user is sigend in
      setTimeout(_ => this.start(), 1000);
    }

    this.recap();
  }

  private onclose = (err: Error): void => {
    // console.error('SignalR connection closed unexpectedly: ', err);

    // Mark all states as stale
    for (const tenantId of Object.keys(this.state)) {
      this.state[tenantId].isFresh = false;
    }

    // Try again
    this.start();
  }

  private async closeAndCleanup(): Promise<void> {
    // console.log('SignalR: closeAndCleanup');
    try {
      await this._connection.stop();
    } finally {
      // Cleanup the state
      this.state = {};
    }
  }

  private recap = async () => {
    if (!!this.wss.isApp && this._connection.state === HubConnectionState.Connected) {
      this.api.notificationsRecap().pipe(
        tap((summary: NotificationSummary) => {
          this.handleFreshInboxCountsAndNotify(summary.Inbox);

          // Mark the state of this tenant as fresh
          const inbox = summary.Inbox;
          if (!!inbox && !!inbox.TenantId) {
            const tenantState = this.state[summary.Inbox.TenantId];
            if (!!tenantState) {
              tenantState.isFresh = true;
            }
          }
        }),
        catchError((err: FriendlyError) => {
          // console.error('Recap returned an Error...', err);
          return of();
        })
      ).subscribe();
    }
  }

  private handleFreshInboxCountsAndNotify = (serverNotification: InboxStatusToSend) => {
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

  public handleFreshNotifications = (tenantId: number, serverTime: string, count: number, unknownCount: number): boolean => {
    const tenantState = (this.state[tenantId] = this.state[tenantId] || { isFresh: true });

    const localNotification = tenantState.notifications;
    if (!localNotification || localNotification.serverTime < serverTime) {
      tenantState.notifications = { count, unknownCount, serverTime };
      return true;
    }

    return false;
  }

  private handleInvalidateCache = (cacheStatus: CacheStatusToSend, notify = true) => {
    // TODO: Invalidate Cache
    if (notify) {
      // this._notificationsChanged$.next(serverNotification);
    }
  }


  // Public API

  /**
   * Starts the SignalR connection and recaps the state summary from the server
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
   * Returns the state of the last tenantId set with connect(tenantId)
   */
  public get tenantState(): TenantState {
    if (this.wss.isApp) {
      const tenantId = this.wss.ws.tenantId;
      return this.state[tenantId];
    }

    return undefined;
  }

  /**
   * Emits whenever a new inbox notification arrives from the server
   */
  public get inboxChanged$(): Observable<InboxStatusToSend> {
    return this._inboxChanged$;
  }

  // /**
  //  * Emits whenever a new notifications notification arrives from the server
  //  */
  // public get notificationsChanged$(): Observable<NotificationsNotification> {
  //   return this._notificationsChanged$;
  // }

  /**
   * Emits whenever a new inbox notification has arrived from the server
   */
  public get cacheInvalidated$(): Observable<CacheStatusToSend> {
    return this._cacheInvalidated$;
  }
}

/////////////// DTOs

enum MethodNames {
  UpdateInbox = 'UpdateInbox',
  UpdateNotifications = 'UpdateNotifications',
  InvalidateCache = 'InvalidateCache',
}

export interface TenantState {
  notifications?: { count: number, unknownCount: number, serverTime: string };
  inbox?: { count: number, unknownCount: number, serverTime: string };
  isFresh: boolean;
}
