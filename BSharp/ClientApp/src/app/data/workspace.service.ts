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
import { IfrsNote } from './dto/ifrs-note';
import { ProductCategory } from './dto/product-category';
import { Subject } from 'rxjs';
import { DtoForSaveKeyBase } from './dto/dto-for-save-key-base';

export enum MasterStatus {

  // The master data is currently being fetched from the server
  loading = 1,

  // The last fetch of data from the server completed successfully
  loaded = 2,

  // The last fetch of data from the server completed with an error
  error = 3,
}

export enum MasterDisplayMode {

  // shows a flat table in table view, and plain tiles in tiles view
  flat = 1,

  // shows a tree table in table view, and a tiles tree in tiles view
  tree = 2,
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
  IfrsNote: EntityWorkspace<IfrsNote>;
  ProductCategory: EntityWorkspace<ProductCategory>;

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
    this.IfrsNote = new EntityWorkspace<IfrsNote>();
    this.ProductCategory = new EntityWorkspace<ProductCategory>();
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

  // don't change signature, a lot of HTML binds to this
  get ternaryPostfix(): string {
    if (this.settings && this.settings.TernaryLanguageId) {
      return ` (${this.settings.TernaryLanguageSymbol})`;
    }

    return '';
  }

  get isPrimaryLanguage(): boolean {
    return !this.isSecondaryLanguage && !this.isTernaryLanguage;
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

  get isTernaryLanguage(): boolean {
    if (!!this.settings) {
      const ternaryLang = this.settings.TernaryLanguageId || '??';
      const currentUserLang = this.workspaceService.ws.culture || 'en';

      return ternaryLang === currentUserLang ||
        ternaryLang.startsWith(currentUserLang) ||
        currentUserLang.startsWith(ternaryLang);
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

  displayMode: MasterDisplayMode;
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

  collectionName: string;
  bag: { [key: string]: any; };
  flatIds: (string | number)[] = []; // in flat mode
  treeNodes: NodeInfo[] = []; // in tree mode
  masterStatus: MasterStatus;
  errorMessage: string;

  detailsId: string | number;
  detailsStatus: DetailsStatus;

  public get isTreeMode(): boolean {
    return this.displayMode === MasterDisplayMode.tree && (!this.orderBy || this.orderBy === 'Node');
  }

  private _oldTreeNodes: NodeInfo[];
  private _masterIds: (string | number)[];
  public get masterIds(): (string | number)[] {

    if (this.isTreeMode) {
      if (this._oldTreeNodes !== this.treeNodes) {
        this._oldTreeNodes = this.treeNodes;
        this._masterIds = this.treeNodes
          .filter(node => node.fromResult)
          .map(node => node.id);
      }
      return this._masterIds;
    } else {
      return this.flatIds;
    }
  }

  public delete(ids: (string | number)[], entityWs: any) {

    // removes a deleted item in memory and updates the stats
    if (this.isTreeMode) {

      // for each deleted item
      ids.forEach(id => {
        const item = entityWs[id];

        // go over old ancestors and reduce their ChildCount by 1
        let ancestor = item;
        while (!!ancestor.ParentId) {
          ancestor = entityWs[ancestor.ParentId];
          ancestor.ChildCount = ancestor.ChildCount - 1;
          if (ancestor.ActiveChildCount) {
            ancestor.ActiveChildCount = ancestor.ActiveChildCount - (item.IsActive ? 1 : 0);
          }
        }
      });

      const beforeCount = this.treeNodes.length;
      this.treeNodes = this.treeNodes.filter(node => ids.indexOf(node.id) === -1);
      this.updateTreeNodes([], entityWs, TreeRefreshMode.includeIfParentIsLoaded);
      const afterCount = this.treeNodes.length;

      // in tree mode the total is never the entire table count, just the number of items displayed
      this.total = this.total + afterCount - beforeCount;
    } else {
      this.flatIds = this.flatIds.filter(e => ids.indexOf(e) === -1);
      this.total = Math.max(this.total - ids.length, 0);
    }
  }

  public update(oldEntity: any, entityWs: any) {
    if (this.isTreeMode) {

      // if parent Id changes then we go over the previous ancestors and reduce their
      // ChildCount, keeping in mind that the new ancestors (which some may be common
      // with the old ancestors) have had their counts updated already from the server

      const newEntity = entityWs[oldEntity.Id];
      if (oldEntity.ParentId === newEntity.ParentId) {
        return; // nothing to do
      } else {

        // go over old ancestors and reduce their ChildCount by 1
        let oldAncestor = oldEntity;
        outer_loop: while (!!oldAncestor.ParentId) {
          oldAncestor = entityWs[oldAncestor.ParentId];

          let newAncestor = newEntity;
          while (!!newAncestor.ParentId) {
            newAncestor = entityWs[newAncestor.ParentId];
            if (newAncestor === oldAncestor) {
              break outer_loop;
            }
          }

          oldAncestor.ChildCount = oldAncestor.ChildCount - 1;
          if (oldAncestor.ActiveChildCount) {
            oldAncestor.ActiveChildCount = oldAncestor.ActiveChildCount - (oldEntity.IsActive ? 1 : 0);
          }
        }

        const beforeCount = this.treeNodes.length;
        this.updateTreeNodes([], entityWs, TreeRefreshMode.includeIfParentIsLoaded);
        const afterCount = this.treeNodes.length;

        // in tree mode the total is never the entire table count, just the number of items displayed
        this.total = this.total + afterCount - beforeCount;
      }
    }
  }

  public insert(ids: (number | string)[], entityWs: any) {


    // here we try to be intelligent on where to add the item
    if (this.isTreeMode) {

      const beforeCount = this.treeNodes.length;
      this.updateTreeNodes(ids, entityWs, TreeRefreshMode.includeIfParentIsLoaded);
      const afterCount = this.treeNodes.length;

      // in tree mode the total is never the entire table count, just the number of items displayed
      this.total = this.total + afterCount - beforeCount;
    } else {

      // adds a newly created item in memory and updates the stats
      this.flatIds = ids.concat(this.flatIds); // add all ids in the beginning
      this.total = this.total + ids.length;
    }
  }

  public updateTreeNodes(ids: (string | number)[], entityWs: any, mode: TreeRefreshMode, highlightSuppliedIds?: boolean): void {

    // for brevity
    const s = this;

    if (mode === TreeRefreshMode.cleanSlate) {
      this.treeNodes = [];
    }

    // put the current nodes (if any) in a dictionary for fast retrieval
    const currentNodesDic: { [key: string]: NodeInfo } = {};
    s.treeNodes.forEach(node => {
      currentNodesDic[node.id] = node;
    });

    // prepare collections
    const nodesDic: { [key: string]: NodeInfo } = {};
    const rootsInfo = { lastIndex: 0 };

    // prepares a nodes dictionary using recursive method
    s.masterIds.concat(ids).forEach(id => this.addNodeToDictionary(id, rootsInfo, entityWs, nodesDic, currentNodesDic, mode));
    const listOfNodes = Object.keys(nodesDic).map(key => nodesDic[key]);

    // when instructed, mark the supplied ids as fromResult and highlight it
    if (highlightSuppliedIds) {
      ids.forEach(id => {
        const node = nodesDic[id];
        if (!!node) {
          // To highlight search results
          node.highlight = true;
          node.fromResult = true;
        }
      });
    } else {
      listOfNodes.forEach(node => {
        node.fromResult = true;
      });
    }

    // old values always preserved
    this.treeNodes.forEach(oldNode => {
      const newNode = nodesDic[oldNode.id];
      if (!!newNode) {
        newNode.highlight = oldNode.highlight;
        newNode.fromResult = oldNode.fromResult;
      }
    });

    // assign the list of nodes to the state object ordered by Path
    s.treeNodes = listOfNodes.sort(nodeCompare);
  }

  private addNodeToDictionary(id: string | number, rootsInfo: { lastIndex: number }, entityWs: any, nodesDic: { [key: string]: NodeInfo },
    currentNodesDic: { [key: string]: NodeInfo }, mode: TreeRefreshMode): NodeInfo {

    const existing = nodesDic[id];
    if (!!existing) {
      return existing;
    } else {
      const item = entityWs[id];

      // get (or create) the parent and set its status and isExpand according to refreshMode
      let newParentNode: NodeInfo = null;
      if (!!item.ParentId) {
        const oldParentNode = currentNodesDic[item.ParentId];

        // When instructed, ensure the parent is present and loaded, return otherwise
        if (mode === TreeRefreshMode.includeIfParentIsLoaded) {
          if (!oldParentNode || oldParentNode.status !== MasterStatus.loaded) {
            return null;
          }
        }

        newParentNode = this.addNodeToDictionary(item.ParentId, rootsInfo, entityWs, nodesDic, currentNodesDic, mode);
        if (!!newParentNode) {
          newParentNode.status = MasterStatus.loaded;
          newParentNode.isExpanded = true;

          // keep the expansion state from before the refresh
          if (!!oldParentNode) {
            newParentNode.isExpanded = oldParentNode.isExpanded;
          }
        }
      }

      // create the current node and add it to the dictionary
      const n = new NodeInfo();
      n.id = id;
      n.level = item.Level;
      n.isExpanded = false;
      n.hasChildren = this.inactive ? (item.ChildCount > 1) : (item.ActiveChildCount - (item.IsActive ? 1 : 0) > 0);
      n.parent = newParentNode;
      n.status = null;

      // set the index among the children (later used for sorting)
      if (!!newParentNode) {
        n.index = ++newParentNode.lastChildIndex;
      } else {
        n.index = ++rootsInfo.lastIndex;
      }

      nodesDic[id] = n;
      return n;
    }
  }
}

// this method compares two nodes based on the paths from the root
const nodeCompare = (n1: NodeInfo, n2: NodeInfo) => {

  let p1 = n1;
  let p2 = n2;

  // goes up the chain until we are at the same level
  if (n1.level < n2.level) {
    for (let i = n2.level - n1.level; i--;) {
      p2 = p2.parent;
    }
  } else {
    for (let i = n1.level - n2.level; i--;) {
      p1 = p1.parent;
    }
  }

  if (p1 === p2) {
    // this means one of the nodes is a child of the other
    // compare levels to make the child larger than the parent
    return n1.level - n2.level;

  } else {

    // go up the chain until you find siblings of a common parent
    while (!!p1.parent && p1.parent !== p2.parent) {
      p1 = p1.parent;
      p2 = p2.parent;
    }

    // compare the indices of the siblings
    return p1.index - p2.index;
  }
};

// different preservation modes when refrshing the tree
export enum TreeRefreshMode {

  // when refreshing the tree, preserve the expansions and master states and only include
  includeIfParentIsLoaded,

  // when refreshing the tree, preserve the expansions but update parent statuses to loaded
  preserveExpansions,

  // when refreshing the tree, ignore the old one entirely and start anew
  cleanSlate
}

export class NodeInfo {
  id: (string | number);
  level: number;
  index: number;
  lastChildIndex = 0;
  isExpanded: boolean;
  hasChildren: boolean;
  parent: NodeInfo;
  isAdded = false;
  highlight = false;
  fromResult = false;
  status: MasterStatus;
  notifyCancel$: Subject<void>; // cancels calls on this node's children
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
