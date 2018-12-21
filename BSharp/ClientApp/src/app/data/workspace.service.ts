import { Injectable } from '@angular/core';
import { DtoForSaveKeyBase } from './dto/dto-for-save-key-base';
import { MeasurementUnit } from './dto/measurement-unit';

// The Workspace of the application stores ALL application wide state that survives navigation between screens
// having all the state in one place is important for security, as it makes it easy to clear the state upon signing out
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
    return this.ws.tenants[this.ws.tenantId];
  }

  // Wipes the application state clean
  public reset() {
    this.ws = new Workspace();
    this.ws.tenants = {};
  }
}

export class Workspace {
  ////// Global state
  // Current UI culture selected by the user
  culture: string;

  // Current tenantID selected by the user
  tenantId: number;

  tenants: { [tenantId: number]: TenantWorkspace }
}

export class TenantWorkspace {
  ////// Tenant state
  name: string;
  name2: string;
  userName: string;
  userName2: string;

  MeasurementUnits : EntityWorkspace<MeasurementUnit>;

  constructor() {
    this.reset();
  }

  public reset() {
    this.MeasurementUnits = new EntityWorkspace<MeasurementUnit>();
  }
}

export class EntityWorkspace<T extends DtoForSaveKeyBase> {
  [id: string]: T
}
