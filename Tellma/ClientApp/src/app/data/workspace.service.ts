// tslint:disable:variable-name
import { Unit } from './entities/unit';
import { Injectable } from '@angular/core';
import { Role } from './entities/role';
import { UserSettingsForClient } from './dto/user-settings-for-client';
import { EntityWithKey } from './entities/base/entity-with-key';
import { SettingsForClient } from './dto/settings-for-client';
import { PermissionsForClient } from './dto/permissions-for-client';
import { GlobalSettingsForClient } from './dto/global-settings';
import { UserCompany } from './dto/user-company';
import { Subject, Observable } from 'rxjs';
import { Agent } from './entities/agent';
import { User } from './entities/user';
import { DefinitionsForClient, ReportDefinitionForClient } from './dto/definitions-for-client';
import { Lookup } from './entities/lookup';
import { Currency } from './entities/currency';
import { Resource } from './entities/resource';
import { AccountClassification } from './entities/account-classification';
import { Action } from './views';
import { AccountType } from './entities/account-type';
import { Account } from './entities/account';
import { PropDescriptor, EntityDescriptor } from './entities/base/metadata';
import { Entity } from './entities/base/entity';
import { Aggregation, ReportDefinition, Modifier } from './entities/report-definition';
import { Center } from './entities/center';
import { EntryType } from './entities/entry-type';
import { Document } from './entities/document';
import { isSpecified } from './util';
import { DetailsEntry } from './entities/details-entry';
import { Line } from './entities/line';
import { ExchangeRate } from './entities/exchange-rate';
import { AdminSettingsForClient } from './dto/admin-settings-for-client';
import { AdminPermissionsForClient } from './dto/admin-permissions-for-client';
import { AdminUserSettingsForClient } from './dto/admin-user-settings-for-client';
import { AdminUser } from './entities/admin-user';
import { IdentityServerUser } from './entities/identity-server-user';
import { InboxRecord } from './entities/inbox-record';
import { OutboxRecord } from './entities/outbox-record';
import { IfrsConcept } from './entities/ifrs-concept';
import { MarkupTemplate } from './entities/markup-template';

enum WhichWorkspace {
  /**
   * Application workspace (For a specific tenant)
   */
  app = 1,

  /**
   * Admin console workspace
   */
  admin = 2
}

export enum MasterStatus {

  /**
   * The master data is currently being fetched from the server
   */
  loading = 1,

  /**
   * The last fetch of data from the server completed successfully
   */
  loaded = 2,

  /**
   * The last fetch of data from the server completed with an error
   */
  error = 3,
}

/**
 * Flat or Tree
 */
export enum MasterDisplayMode {

  /**
   * shows a flat table in table view, and plain tiles in tiles view
   */
  flat = 'flat',

  /**
   * shows a tree table in table view, and a tiles tree in tiles view
   */
  tree = 'tree',
}

export enum DetailsStatus {

  /**
   * The details record is being fetched from the server
   */
  loading = 1,

  /**
   * The last fetch of the details record from the server completed successfully
   */
  loaded = 2,

  /**
   * The last fetch of details record from the server resulted in an error
   */
  error = 3,

  /**
   * The details record is set to be modified or is currently being modified
   */
  edit = 4,
}

export enum ReportStatus {

  // The master data is currently being fetched from the server
  loading = 1,

  // The last fetch of data from the server completed successfully
  loaded = 2,

  // The last fetch of data from the server completed with an error
  error = 3,

  // The report definition or parameters are not yet complete
  information = 4,
}

/**
 * different preservation modes when refrshing the tree
 */
export enum TreeRefreshMode {

  /**
   * when refreshing the tree, preserve the expansions and master states and only include
   */
  includeIfAncestorsLoaded,

  /**
   * when refreshing the tree, preserve the expansions but update parent statuses to loaded
   */
  preserveExpansions,

  /**
   * when refreshing the tree, ignore the old one entirely and start anew
   */
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

// Represents a collection of savable entities, indexed by their IDs
export interface EntityWorkspace<T extends EntityWithKey> {
  [id: string]: T;
}

export abstract class SpecificWorkspace {

  permissions: PermissionsForClient | AdminPermissionsForClient;
  permissionsVersion: string;

  public canRead(view: string) {
    if (!view) {
      return false;
    }

    const viewPerms = this.permissions[view];
    const allPerms = this.permissions.all;
    return (!!viewPerms || !!allPerms);
  }

  public canCreate(view: string) {
    return this.canDo(view, 'Update', null);
  }

  public canUpdate(view: string, createdById: string | number) {
    return this.canDo(view, 'Update', createdById);
  }

  public canDo(view: string, action: Action, createdById: string | number) {

    if (!view) {
      return false;
    }

    const viewPerms = this.permissions[view];
    const allPerms = this.permissions.all;
    // const userId = this.userSettings.UserId;
    // (userId === createdById) ||
    return (!!viewPerms && (viewPerms[action] || viewPerms.All))
      || (!!allPerms && (allPerms[action] || allPerms.All));
  }
}

export class AdminWorkspace extends SpecificWorkspace {
  settings: AdminSettingsForClient;
  settingsVersion: string;

  userSettings: AdminUserSettingsForClient;
  userSettingsVersion: string;

  /**
   * Tells the UnsavedChangesGuard to step aside
   */
  unauthorized: boolean;

  /**
   * Keeps the state of every master-details pair in screen mode
   */
  mdState: { [key: string]: MasterDetailsStore };

  /**
   * Master screens use this to remember the last opened master screen,
   * if it's not the same as the current screen, the current screen may
   * wish to refresh
   */
  mdLastKey: string;

  AdminUser: EntityWorkspace<AdminUser>;
  IdentityServerUser: EntityWorkspace<IdentityServerUser>;

  constructor(private workspaceService: WorkspaceService) {
    super();
    this.reset();
  }

  public reset() {

    this.mdState = {};

    this.AdminUser = {};
    this.IdentityServerUser = {};

    this.notifyStateChanged();
  }

  notifyStateChanged() {
    this.workspaceService.notifyStateChanged();
  }

  ////// the methods below provide easy access to the global tenant values
  get(collection: string, id: number | string) {
    if (!id) {
      return null;
    }

    if (!this[collection]) {
      // Developer error
      console.error(`Collection '${collection}' doesn't exist in the workspace`);
    }

    return this[collection][id];
  }
}

// This contains all the state that is specific to a particular tenant
export class TenantWorkspace extends SpecificWorkspace {

  ////// Globals
  // cannot navigate to any tenant screen until these global values are initialized via a router guard
  settings: SettingsForClient;
  settingsVersion: string;

  userSettings: UserSettingsForClient;
  userSettingsVersion: string;

  definitions: DefinitionsForClient;
  definitionsVersion: string;

  /**
   * Tells the UnsavedChangesGuard to step aside
   */
  unauthorized: boolean;

  /**
   * Keeps the state of every master-details pair in screen mode
   */
  mdState: { [key: string]: MasterDetailsStore };

  /**
   * Master screens use this to remember the last opened master screen,
   * if it's not the same as the current screen, the current screen may
   * wish to refresh
   */
  mdLastKey: string;

  /**
   * Keeps the state of every report widget
   */
  reportState: { [key: string]: ReportStore };

  Unit: EntityWorkspace<Unit>;
  Role: EntityWorkspace<Role>;
  User: EntityWorkspace<User>;
  Agent: EntityWorkspace<Agent>;
  Lookup: EntityWorkspace<Lookup>;
  Currency: EntityWorkspace<Currency>;
  Resource: EntityWorkspace<Resource>;
  AccountClassification: EntityWorkspace<AccountClassification>;
  IfrsConcept: EntityWorkspace<IfrsConcept>;
  AccountType: EntityWorkspace<AccountType>;
  Account: EntityWorkspace<Account>;
  ReportDefinition: EntityWorkspace<ReportDefinition>;
  Center: EntityWorkspace<Center>;
  EntryType: EntityWorkspace<EntryType>;
  Document: EntityWorkspace<Document>;
  Line: EntityWorkspace<Line>;
  ExchangeRate: EntityWorkspace<ExchangeRate>;
  DetailsEntry: EntityWorkspace<DetailsEntry>;
  InboxRecord: EntityWorkspace<InboxRecord>;
  OutboxRecord: EntityWorkspace<OutboxRecord>;
  MarkupTemplate: EntityWorkspace<MarkupTemplate>;

  constructor(private workspaceService: WorkspaceService) {
    super();
    this.reset();
  }

  public reset() {

    this.mdState = {};
    this.reportState = {};

    this.Unit = {};
    this.Role = {};
    this.User = {};
    this.Agent = {};
    this.Lookup = {};
    this.Currency = {};
    this.Resource = {};
    this.AccountClassification = {};
    this.IfrsConcept = {};
    this.AccountType = {};
    this.Account = {};
    this.ReportDefinition = {};
    this.Center = {};
    this.EntryType = {};
    this.Document = {};
    this.Line = {};
    this.ExchangeRate = {};
    this.DetailsEntry = {};
    this.MarkupTemplate = {};
    this.InboxRecord = {};
    this.OutboxRecord = {};

    this.notifyStateChanged();
  }

  notifyStateChanged() {
    this.workspaceService.notifyStateChanged();
  }

  ////// the methods below provide easy access to the global tenant values
  get(collection: string, id: number | string) {
    if (!id) {
      return null;
    }

    if (!this[collection]) {
      // Developer error
      console.error(`Collection '${collection}' doesn't exist in the workspace`);
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

  getMultilingualValueImmediate(item: any, propName: string): string {
    if (!!propName) {
      const propName2 = propName + '2';
      const propName3 = propName + '3';
      if (!!item) {
        if (this.isSecondaryLanguage && !!item[propName2]) {
          return item[propName2];
        } else if (this.isTernaryLanguage && !!item[propName3]) {
          return item[propName3];
        } else {
          return item[propName];
        }
      }
    }

    return null;
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
  companies: UserCompany[];
  isAdmin: boolean; // whether the user is permitted in the admin console

  which: WhichWorkspace;

  // Current tenantID selected by the user
  tenantId: number;
  tenants: { [tenantId: number]: TenantWorkspace };

  admin: AdminWorkspace;

  constructor() {
    this.tenants = {};
  }
}
export type SingleSeries = { name: ChartDimensionCell, value: number, extra?: any }[];
export type MultiSeries = { name: ChartDimensionCell, series: SingleSeries }[];
export interface ReportArguments { [key: string]: any; }
export interface PivotTable {

  /**
   * The upper part of the pivot table featuring the column labels
   */
  columnHeaders: (DimensionCell | LabelCell)[][];

  /**
   * The bottom part of the pivot table feature the row labels and the measure values
   */
  rows: (DimensionCell | LabelCell | MeasureCell)[][];
}

export interface MeasureInfo {
  key: string;
  desc: PropDescriptor;
  aggregation: Aggregation;
  label: () => string;
}

export interface DimensionInfo {
  key: string;
  path: string;
  modifier: Modifier;
  propDesc: PropDescriptor;

  /**
   * This is set in the case of navigation properties
   */
  entityDesc?: EntityDescriptor;
  idKey?: string;
  selectKeys?: { prop: string, path: string }[];

  autoExpand: boolean;
  label: () => string;
}

export class DimensionCell {
  type: 'dimension';
  path: string;
  modifier: Modifier;
  value: any;
  valueId: any;
  propDesc: PropDescriptor;
  entityDesc?: EntityDescriptor;
  isExpanded: boolean;
  hasChildren: boolean;
  level: number;
  index: number;
  parent: DimensionCell;
  isTotal?: boolean;

  // The column span if all parents were expanded
  expandedColSpan?: number;

  // These change dynamically
  rowSpan?: number;
  colSpan?: number;
  isVisible?: boolean;
}

export class ChartDimensionCell {

  constructor(
    public display: string,
    public path: string,
    public modifier: Modifier,
    public valueId: any,
    public propDesc: PropDescriptor,
    public entityDesc: EntityDescriptor,
    public parent?: ChartDimensionCell) { }

  toString() {
    // ngx-charts throws an error if you return null
    // The value returned must uniquely identify the dimension
    // formatting will be handled in the tooltip template
    return isSpecified(this.valueId) ? this.valueId.toString() : '';
  }
}

export interface LabelCell {
  type: 'label';
  label: () => string;
  level: number;
  parent: DimensionCell;
  isTotal?: boolean;

  // The column span if all parents were expanded
  expandedColSpan?: number;

  // Change dynamically
  rowSpan?: number;
  colSpan?: number;
  isVisible?: boolean;
}

export interface MeasureCell {
  type: 'measure';
  values: any[];
  counts: number[]; // for aggregating averages
  parent: DimensionCell; // column
  isTotal?: boolean;
}

export class ReportStore {
  definition: ReportDefinitionForClient;
  skip = 0;
  top = 0;
  total = 0;
  reportStatus: ReportStatus;
  errorMessage: string;
  information: () => string;
  arguments: ReportArguments = {};
  result: Entity[] = [];
  filter: string; // the one used to retrieve the result
  disableFetch = false; // set it to true upon a catastrophic failure from a bad definition

  //////////// Pivot table

  /**
   * The the actual row dimensions that will be
   * displayed, without the property dimensions.
   */
  realRows: DimensionInfo[];

  /**
   * The the actual column dimensions that will be
   * displayed, without the property dimensions.
   */
  realColumns: DimensionInfo[];

  /**
   * realRows + realColumns unique over the key property
   */
  uniqueDimensions: DimensionInfo[];

  /**
   * precomputed and used by the charts
   */
  singleNumericMeasure: MeasureInfo;

  /**
   * Precalculated values for efficiently displaying
   * the measures
   */
  measures: MeasureInfo[];
  pivot: PivotTable;
  currentResultForPivot: any;

  //////////// Charts

  /**
   * For number card
   */
  point: string;
  currentResultForPoint: any;

  /**
   * For single series charts (e.g. bar and pie charts)
   */
  single: SingleSeries;
  currentResultForSingle: any;
  currentLangForSingle: string;

  /**
   * For multi-series charts (e.g. line chart)
   */
  multi: MultiSeries;
  currentResultForMulti: any;
  currentLangForMulti: string;
}

export const DEFAULT_PAGE_SIZE = 25;
export const MAXIMUM_COUNT = 10000; // IMPORTANT: Keep in sync with server side

export class MasterDetailsStore {

  displayMode: MasterDisplayMode;
  top = DEFAULT_PAGE_SIZE;
  skip = 0;
  search: string = null;
  orderby: string = null;
  total = 0;
  expand: string;
  inactive = false;
  select: string = null;
  builtInFilter = '';
  builtInFilterSelections: {
    [groupName: string]: {
      [expression: string]: boolean
    }
  } = {};
  customFilter: string = null;

  collectionName: string;
  extras: { [key: string]: any; };
  flatIds: (string | number)[] = []; // in flat mode
  treeNodes: NodeInfo[] = []; // in tree mode
  masterStatus: MasterStatus;
  errorMessage: string;

  detailsId: string | number;
  detailsStatus: DetailsStatus;

  // custom details state
  detailsState: { [key: string]: any } = {}; // Stores other miscellaneous

  public get isTreeMode(): boolean {
    return this.displayMode === MasterDisplayMode.tree && (!this.orderby || this.orderby === 'Node');
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

      // for a performant lookup
      const deletedIdsDic = {};
      ids.forEach(id => deletedIdsDic[id] = true);

      // for each deleted item reduce all its ancestors (that are not also being deleted) by its child count
      ids.forEach(id => {
        const item = entityWs[id];
        let parent = item;
        while (!!parent.ParentId && !deletedIdsDic[parent.ParentId]) {
          parent = entityWs[parent.ParentId];

          parent.ChildCount -= item.ChildCount;
          if (item.ActiveChildCount) {
            parent.ActiveChildCount -= item.ActiveChildCount;
          }
        }
      });

      const beforeCount = this.treeNodes.length;
      this.treeNodes = this.treeNodes.filter(node => ids.indexOf(node.id) === -1);
      this.updateTreeNodes([], entityWs, TreeRefreshMode.includeIfAncestorsLoaded);
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

        // if (!!oldEntity.ParentId) {
        //   const oldParent =
        // }

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

          oldAncestor.ChildCount -= oldEntity.ChildCount;
          if (oldAncestor.ActiveChildCount) {
            oldAncestor.ActiveChildCount -= oldEntity.ActiveChildCount;
          }
        }

        const beforeCount = this.treeNodes.length;
        this.updateTreeNodes([oldEntity.Id], entityWs, TreeRefreshMode.includeIfAncestorsLoaded);
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
      this.updateTreeNodes(ids, entityWs, TreeRefreshMode.includeIfAncestorsLoaded);
      const afterCount = this.treeNodes.length;

      // in tree mode the total is never the entire table count, just the number of items displayed
      this.total = this.total + afterCount - beforeCount;
    } else {

      // adds a newly created item in memory and updates the stats
      const newIds = ids.filter(id => this.flatIds.indexOf(id) < 0);
      this.flatIds = newIds.concat(this.flatIds); // add all ids in the beginning
      this.total = this.total + newIds.length;
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

  private addNodeToDictionary(
    id: string | number,
    rootsInfo: { lastIndex: number },
    entityWs: any, nodesDic: { [key: string]: NodeInfo },
    currentNodesDic: { [key: string]: NodeInfo },
    mode: TreeRefreshMode): NodeInfo {

    const existing = nodesDic[id];
    if (!!existing) {
      return existing;
    } else {
      const item = entityWs[id];

      // When instructed, ensure the ancestors are all present and loaded, return otherwise
      if (mode === TreeRefreshMode.includeIfAncestorsLoaded) {
        let ancestor = item;

        while (!!ancestor.ParentId) {
          const ancestorNode = currentNodesDic[ancestor.ParentId];
          if (!ancestorNode || ancestorNode.status !== MasterStatus.loaded) {
            return null;
          }

          // Go up one level
          ancestor = entityWs[ancestor.ParentId];
        }
      }

      // get (or create) the parent
      let newParentNode: NodeInfo = null;
      if (!!item.ParentId) {


        newParentNode = this.addNodeToDictionary(item.ParentId, rootsInfo, entityWs, nodesDic, currentNodesDic, mode);
        if (!!newParentNode) {
          newParentNode.status = MasterStatus.loaded;
          newParentNode.isExpanded = true;

          // keep the expansion state from before the refresh
          // When mode === cleanSlate, this dictionary will be empty anyways
          const oldParentNode = currentNodesDic[item.ParentId];
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

      // if an old node was loading its children in progress, we retain its status and isExpand
      // useful when you expand multiple nodes at the same time
      const oldNode = currentNodesDic[id];
      if (!!oldNode && oldNode.status === MasterStatus.loading) {
        n.status = oldNode.status;
        n.isExpanded = oldNode.isExpanded;
      }

      nodesDic[id] = n;
      return n;
    }
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

  // This is to tell components that auto-capture user input (main menu, master screens etc...)
  // that user input is needed for something else so do not capture
  public ignoreKeyDownEvents = false;

  /**
   * Indicates that the client appears to be offline, signals the root shell to show an offline indicator
   */
  public offline = false;

  /**
   * To communicate to a details component that it should launch in edit mode
   */
  public isEdit?: boolean;

  /**
   * To communicate to a details component that it should clone the provided Id
   */
  public cloneId?: number | string;

  /**
   * Notifies that something has changed in workspace.
   * Used by OnPush components to mark for check
   */
  stateChanged$: Observable<void> = new Subject<void>();

  constructor() {
    this.reset();
  }

  // Syntactic sugar for current tenant workspace
  public get current(): TenantWorkspace | AdminWorkspace {

    if (this.isApp) {
      return this.currentTenant;
    } else if (this.isAdmin) {
      return this.admin;
    } else {
      throw new Error(`Unsupported workspace ${this.ws.which}`);
    }
  }

  public get admin(): AdminWorkspace {
    if (!this.ws.admin) {
      this.ws.admin = new AdminWorkspace(this);
    }

    return this.ws.admin;
  }

  public get currentTenant(): TenantWorkspace {
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
    this.notifyStateChanged();
  }

  setTenantId(tenantId: number) {
    if (this.ws.tenantId !== tenantId || this.ws.which !== WhichWorkspace.app) {
      this.ws.tenantId = tenantId;
      this.ws.which = WhichWorkspace.app;
      this.notifyStateChanged();
    }
  }

  setAdmin() {
    if (this.ws.which !== WhichWorkspace.admin) {
      this.ws.which = WhichWorkspace.admin;
      this.notifyStateChanged();
    }
  }

  get isApp(): boolean {
    return this.ws.which === WhichWorkspace.app;
  }

  get isAdmin(): boolean {
    return this.ws.which === WhichWorkspace.admin;
  }

  notifyStateChanged() {
    // This notifies OnPush components to mark for check
    // It is the duty for anyone modifying the
    // workspace to remember to call this method
    (this.stateChanged$ as Subject<void>).next();
  }
}
