import { MeasurementUnit } from './dto/measurement-unit';
import { Injectable } from '@angular/core';
import { Custody } from './dto/custody';
import { Role } from './dto/role';
import { View } from './dto/view';
import { LocalUser, UserSettingsForClient } from './dto/local-user';
import { Culture } from './dto/culture';
import { DtoKeyBase } from './dto/dto-key-base';
import { SettingsForClient } from './dto/settings';
import { PermissionsForClient } from './dto/permission';
import { GlobalSettingsForClient } from './dto/global-settings';
import { TenantForClient } from './dto/tenant';

export enum MasterStatus {

  // The master data is currently being fetched from the server
  loading = 1,

  // The last fetch of data from the server completed successfully
  loaded = 2,

  // The last fetch of data from the server completed with an error
  error = 3,
}

export enum DetailsStatus {

  // The details record is being fetched from the server
  loading = 1,

  // The last fetch of the details record from the server completed successfully
  loaded = 2,

  // The last fetch of details record from the server resulted in an error
  error = 3,

  // The details record is set to be modified or is currently being modified
  edit = 4,
}

// Represents a collection of savable entities, indexed by their IDs
export class EntityWorkspace<T extends DtoKeyBase> {
  [id: string]: T;
}

// This contains all the state that is specific to a particular tenant
export class TenantWorkspace {

  ////// Globals
  // cannot navigate to any tenant screen until these global values are initialized via a router guard
  settings: SettingsForClient;
  settingsVersion: string;

  permissions: PermissionsForClient;
  permissionsVersion: string;

  userSettings: UserSettingsForClient;
  userSettingsVersion: string;

  // Keeps the state of every master-details pair in screen mode
  mdState: { [key: string]: MasterDetailsStore };

  MeasurementUnits: EntityWorkspace<MeasurementUnit>;
  Custodies: EntityWorkspace<Custody>;
  Roles: EntityWorkspace<Role>;
  Views: EntityWorkspace<View>;
  LocalUsers: EntityWorkspace<LocalUser>;
  Cultures: EntityWorkspace<Culture>;

  constructor(private workspaceService: WorkspaceService) {
    this.reset();
  }

  public reset() {

    this.mdState = {};

    this.MeasurementUnits = new EntityWorkspace<MeasurementUnit>();
    this.Custodies = new EntityWorkspace<Custody>();
    this.Roles = new EntityWorkspace<Role>();
    this.Views = new EntityWorkspace<View>();
    this.LocalUsers = new EntityWorkspace<LocalUser>();
    this.Cultures = new EntityWorkspace<Culture>();
  }

  ////// the methods below provide easy access to the global tenant values
  get(collection: string, id: number | string) {
    if (!id) {
      return null;
    }

    return this[collection][id];
  }

  // don't change signature, a lot of HTML binds to this
  get primaryPostfix(): string {
    if (this.settings && this.settings.SecondaryLanguageId) {
      return ` (${this.settings.PrimaryLanguageSymbol})`;
    }

    return '';
  }

  // don't change signature, a lot of HTML binds to this
  get secondaryPostfix(): string {
    if (this.settings && this.settings.SecondaryLanguageId) {
      return ` (${this.settings.SecondaryLanguageSymbol})`;
    }

    return '';
  }

  get isPrimaryLanguage(): boolean {
    return !this.isSecondaryLanguage;
  }

  get isSecondaryLanguage(): boolean {
    if (!!this.settings) {
      const secondLang = this.settings.SecondaryLanguageId || '??';
      const currentUserLang = this.workspaceService.ws.culture || 'en';

      return secondLang === currentUserLang ||
        secondLang.startsWith(currentUserLang) ||
        currentUserLang.startsWith(secondLang);
    }

    return false;
  }

  getMultilingualValue(collection: string, id: number | string, propName: string) {
    if (!!id) {
      const item = this.get(collection, id);
      return this.getMultilingualValueImmediate(item, propName);
    }

    return null;
  }

  getMultilingualValueImmediate(item: any, propName: string) {
    if (!!propName) {
      const propName2 = propName + '2';
      if (!!item) {
        if (this.isSecondaryLanguage && !!item[propName2]) {
          return item[propName2];
        } else {
          return item[propName];
        }
      }
    }

    return null;
  }

  public canRead(viewId: string) {
    if (!viewId) {
      return false;
    }

    if (viewId === 'all') {
      return true;
    }

    const viewPerms = this.permissions[viewId];
    const allPerms = this.permissions['all'];
    return (!!viewPerms || !!allPerms);
  }

  public canCreate(viewId: string) {
    if (!viewId) {
      return false;
    }

    if (viewId === 'all') {
      return true;
    }

    const viewPerms = this.permissions[viewId];
    const allPerms = this.permissions['all'];
    return (!!viewPerms && (viewPerms.Create || viewPerms.Update || viewPerms.Sign))
      || (!!allPerms && (allPerms.Create || allPerms.Update || allPerms.Sign));
  }

  public canUpdate(viewId: string, createdById: string | number) {

    if (!viewId) {
      return false;
    }

    const viewPerms = this.permissions[viewId];
    const allPerms = this.permissions['all'];
    // const userId = this.userSettings.UserId;
    // (userId === createdById) ||
    return (!!viewPerms && (viewPerms.Update || viewPerms.Sign))
      || (!!allPerms && (allPerms.Update || allPerms.Sign));
  }
}

// This contains the application state during a particular user session
export class Workspace {
  ////// Global state
  // Current UI culture selected by the user
  culture: string;
  isRtl = false;
  errorMessage: string;

  // The user's companies
  companiesStatus: MasterStatus;
  companies: TenantForClient[];

  // Current tenantID selected by the user
  tenantId: number;

  tenants: { [tenantId: number]: TenantWorkspace };

  constructor() {
    this.tenants = {};
  }
}

export class MasterDetailsStore {

  top = 40;
  skip = 0;
  search: string = null;
  orderBy: string = null;
  desc: boolean;
  total = 0;
  expand: string;
  inactive = false;
  filterState: {
    [groupName: string]: {
      [expression: string]: boolean
    }
  } = {};

  bag: { [key: string]: any; };
  masterIds: (string | number)[] = [];
  masterStatus: MasterStatus;
  errorMessage: string;

  detailsId: string | number;
  detailsStatus: DetailsStatus;

  public delete(ids: (string | number)[]) {
    // removes a deleted item in memory and updates the stats

    this.total = Math.max(this.total - ids.length, 0);
    this.masterIds = this.masterIds.filter(e => ids.indexOf(e) === -1);
  }

  public insert(ids: (string | number)[]) {
    // adds a newly created item in memory and updates the stats
    this.total = this.total + ids.length;
    this.masterIds = ids.concat(this.masterIds);
  }
}

// The Workspace of the application stores ALL application wide in-memory state that survives
// navigation between screens(But does not survive a tab refresh) having all the state in one
// place is important for security, as it makes it easy to clear the state upon signing out
@Injectable({
  providedIn: 'root'
})
export class WorkspaceService {

  // This redirection makes it easy to wipe the workspace clean when signing out
  public ws: Workspace;

  // Those are user-independent, company-independent and don't even require a sign-in, so they should never be cleared
  public globalSettings: GlobalSettingsForClient;
  public globalSettingsVersion: string;

  constructor() {
    this.reset();
  }

  // Syntactic sugar for current tenant workspace
  public get current(): TenantWorkspace {

    const tenantId = this.ws.tenantId;
    if (!!tenantId) {
      if (!this.ws.tenants[tenantId]) {
        this.ws.tenants[tenantId] = new TenantWorkspace(this);
      }

      return this.ws.tenants[tenantId];
    } else {
      // this only happens when the state is being cleared
      return new TenantWorkspace(new WorkspaceService());
    }
  }

  // Wipes the application state clean, usually upon signing out
  public reset() {
    this.ws = new Workspace();
  }
}
