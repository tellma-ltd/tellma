import { DtoForSaveKeyBase } from './dto/dto-for-save-key-base';
import { MeasurementUnit, MeasurementUnitForSave } from './dto/measurement-unit';
import { Injectable } from '@angular/core';
import { Custody } from './dto/custody';
import { Role } from './dto/role';
import { View } from './dto/view';
import { LocalUser } from './dto/local-user';
import { DtoKeyBase } from './dto/dto-key-base';

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
export class EntityWorkspace<T extends DtoForSaveKeyBase> {
  [id: string]: T;
}

// This contains all the state that is specific to a particular tenant
export class TenantWorkspace {
  ////// Tenant state
  name: string;
  name2: string;
  userName: string;
  userName2: string;

  // Keeps the state of every
  mdState: { [key: string]: MasterDetailsStore };

  MeasurementUnits: EntityWorkspace<MeasurementUnit>;
  Custodies: EntityWorkspace<Custody>;
  Roles: EntityWorkspace<Role>;
  Views: EntityWorkspace<View>;
  LocalUsers: EntityWorkspace<LocalUser>;

  get(collection: string, id: number | string) {
    if (!id) {
      return null;
    }

    return this[collection][id];
  }

  constructor() {
    this.reset();
  }

  public reset() {

    this.mdState = {};
    this.MeasurementUnits = new EntityWorkspace<MeasurementUnit>();
    this.Custodies = new EntityWorkspace<Custody>();
    this.Roles = new EntityWorkspace<Role>();
    this.Views = new EntityWorkspace<View>();
    this.LocalUsers = new EntityWorkspace<LocalUser>();
  }
}

// This contains the application state during a particular user session
export class Workspace {
  ////// Global state
  // Current UI culture selected by the user
  culture: string;
  isRtl = false;

  // Current tenantID selected by the user
  tenantId: number;

  tenants: { [tenantId: number]: TenantWorkspace };

  constructor() {
    this.tenants = {};
  }
}

export class MasterDetailsStore {

  top = 50; // +
  skip = 0;
  search: string;
  orderBy: string;
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

  constructor() {
    this.reset();
  }

  // Syntactic sugar for current tenant workspace
  public get current(): TenantWorkspace {
    if (!this.ws.tenants[this.ws.tenantId]) {
      this.ws.tenants[this.ws.tenantId] = new TenantWorkspace();
    }

    return this.ws.tenants[this.ws.tenantId];
  }

  // Wipes the application state clean, usually upon signing out
  public reset() {
    this.ws = new Workspace();
  }
}
