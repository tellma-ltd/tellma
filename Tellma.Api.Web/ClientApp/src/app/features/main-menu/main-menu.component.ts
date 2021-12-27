// tslint:disable:member-ordering
import { Component, OnInit, HostListener, ViewChild, ElementRef, AfterViewInit, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { isSpecified, Key } from '~/app/data/util';
import { TenantWorkspace, WorkspaceService } from '~/app/data/workspace.service';
import { timer } from 'rxjs';
import { DOCUMENT } from '@angular/common';
import { DefinitionsForClient, DefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { PermissionsForClientViews } from '~/app/data/dto/permissions-for-client';
import { CustomUserSettingsService } from '~/app/data/custom-user-settings.service';
import { UserSettingsForClient } from '~/app/data/dto/user-settings-for-client';
import { AdminUserSettingsForClient } from '~/app/data/dto/admin-user-settings-for-client';

interface MenuSectionInfo {
  label?: string;
  background: string;
  items: MenuItemInfo[];
}

interface MenuItemInfo {
  icon: string;
  label: string;
  link: string;
  paramsFunc?: () => Params;
  linkArray?: (string | Params)[];
  view?: string;
  canView?: () => boolean;
  sortKey: number;
}

@Component({
  selector: 't-main-menu',
  templateUrl: './main-menu.component.html'
})
export class MainMenuComponent implements OnInit, AfterViewInit, OnDestroy {

  // IMPORTANT: if you change these you must change the corresponding
  // SASS variables in main-menu.component.scss accordingly
  MAX_TILES = 6;
  TILE_WIDTH = 135;
  TILE_MARGIN = 4;
  CONTAINER_MARGIN = 15;

  // private fields
  private currentSection = -1;
  private currentItem = -1;
  private currentXMemory = -1;

  // public fields
  public search: string;
  public rendered = false;

  // the search field
  @ViewChild('searchInput', { static: true })
  searchInput: ElementRef;

  //////////////////////// Lifecycle functions

  // constructor
  constructor(
    private router: Router, private route: ActivatedRoute, @Inject(DOCUMENT) private document: Document,
    private translate: TranslateService, private workspace: WorkspaceService, private userSettings: CustomUserSettingsService) { }

  // Angular lifecycle hooks
  ngOnInit() {
    const count = this.mainMenu.reduce((sum, obj) => sum + obj.items.length, 0);

    // this adds a cool background to the main menu, unaffected by scrolling
    // this.document.body.classList.add('t-banner');

    // if the main menu is enormous, it causes an uncomfortable lag before navigation
    // we eliminate this lag by not rendering the menu items immediately if they are too many
    if (count < 60) {
      this.render();
    }

    if (this.workspace.lastNavigation === 'popstate') {
      this.search = this.ws.mainMenuSearch;
      if (!!this.search) {
        this.render();
      }
    }
  }

  ngAfterViewInit() {
    if (!this.rendered) {
      timer(1).subscribe(() => this.render());
    }
  }

  ngOnDestroy() {
    // other screens have a simple grey background
    // this.document.body.classList.remove('t-banner');

    this.ws.mainMenuSearch = this.search;
  }

  render() {
    this.rendered = true;
  }

  //////////////////////// End - Lifecycle functions

  mainMenuBase: { [section: string]: MenuSectionInfo } = {
    Mail: {
      background: 't-blue1',
      items: [
        { label: 'Inbox', icon: 'inbox', link: '../inbox', sortKey: 10 },
        { label: 'Outbox', icon: 'share', link: '../outbox', sortKey: 20 },
      ]
    },
    Financials: {
      background: 't-green1',
      items: [
        {
          label: 'Accounts', icon: 'coins', link: '../accounts',
          view: 'accounts', sortKey: 100
        },
        {
          label: 'AccountStatement', icon: 'file-alt', link: '../account-statement',
          canView: () => this.canView('accounts') && this.canView('details-entries'),
          paramsFunc: () => this.userSettings.get<Params>('account-statement/arguments'),
          sortKey: 101
        },
        {
          label: 'EntryTypes', icon: 'sitemap', link: '../entry-types',
          view: 'entry-types', sortKey: 200
        },
        {
          label: 'AccountTypes', icon: 'sitemap', link: '../account-types',
          view: 'account-types', sortKey: 300
        },
        {
          label: 'AccountClassifications', icon: 'indent', link: '../account-classifications',
          view: 'account-classifications', sortKey: 500
        },
        {
          label: 'Centers', icon: 'object-group', link: '../centers',
          view: 'centers', sortKey: 600
        },
        {
          label: 'FinancialSettings', icon: 'cog', link: '../financial-settings',
          view: 'financial-settings', sortKey: 700
        },
      ]
    },
    Cash: {
      background: 't-teal2',
      items: [
        {
          label: 'ExchangeRates', icon: 'exchange-alt', link: '../exchange-rates',
          view: 'exchange-rates', sortKey: 100
        },
        {
          label: 'BankReconciliation', icon: 'tasks', link: '../reconciliation',
          view: 'reconciliation', sortKey: 200,
          paramsFunc: () => this.userSettings.get<Params>('reconciliation/arguments'),
        }
      ]
    },
    FixedAssets: {
      background: 't-blue2',
      items: []
    },
    Inventory: {
      background: 't-green2',
      items: []
    },
    Production: {
      background: 't-teal3',
      items: []
    },
    Purchasing: {
      background: 't-blue3',
      items: []
    },
    Marketing: {
      background: 't-green3',
      items: []
    },
    Sales: {
      background: 't-teal1',
      items: []
    },
    HumanCapital: {
      background: 't-blue1',
      items: []
    },
    Payroll: {
      background: 't-green1',
      items: []
    },
    Investments: {
      background: 't-teal2',
      items: []
    },
    Maintenance: {
      background: 't-blue2',
      items: []
    },
    Administration: {
      background: 't-green2',
      items: [
        {
          label: 'Units', icon: 'ruler', link: '../units',
          view: 'units', sortKey: 200
        },
        {
          label: 'Currencies', icon: 'euro-sign', link: '../currencies',
          view: 'currencies', sortKey: 300
        },
        // {
        //   label: 'IfrsConcepts', icon: 'stream', link: '../ifrs-concepts',
        //   view: 'ifrs-concepts', sortKey: 600
        // },
        {
          label: 'MessageCommands', icon: 'sms', link: '../message-commands',
          sortKey: 350, canView: () =>
            this.workspace.globalSettings.SmsEnabled &&
            this.workspace.currentTenant.settings.SmsEnabled &&
            this.canView('message-commands'),
        },
        {
          label: 'NotificationCommands', icon: 'bell', link: '../notification-commands',
          sortKey: 350
        },
        {
          label: 'Emails', icon: 'envelope', link: '../emails',
          sortKey: 400, canView: () => this.workspace.globalSettings.EmailEnabled && this.canView('emails'),
        },
        {
          label: 'SmsMessages', icon: 'sms', link: '../sms-messages',
          sortKey: 500, canView: () =>
            this.workspace.globalSettings.SmsEnabled &&
            this.workspace.currentTenant.settings.SmsEnabled &&
            this.canView('sms-messages'),
        },
        {
          label: 'GeneralSettings', icon: 'cog', link: '../general-settings',
          view: 'general-settings', sortKey: 700
        },
      ]
    },
    Security: {
      background: 't-teal3',
      items: [
        {
          label: 'Users', icon: 'users', link: '../users',
          view: 'users', sortKey: 100
        },
        {
          label: 'Roles', icon: 'shield-alt', link: '../roles',
          view: 'roles', sortKey: 200
        },
      ]
    },
    Studio: {
      background: 't-black',
      items: [
        {
          label: 'LookupDefinitions', icon: 'tools', link: '../lookup-definitions',
          view: 'lookup-definitions', sortKey: 100
        },
        {
          label: 'AgentDefinitions', icon: 'tools', link: '../agent-definitions',
          view: 'agent-definitions', sortKey: 200
        },
        {
          label: 'ResourceDefinitions', icon: 'tools', link: '../resource-definitions',
          view: 'resource-definitions', sortKey: 300
        },
        {
          label: 'LineDefinitions', icon: 'tools', link: '../line-definitions',
          view: 'line-definitions', sortKey: 400
        },
        {
          label: 'DocumentDefinitions', icon: 'tools', link: '../document-definitions',
          view: 'document-definitions', sortKey: 500
        },
        {
          label: 'PrintingTemplates', icon: 'file-code', link: '../printing-templates',
          view: 'printing-templates', sortKey: 600
        },
        {
          label: 'ReportDefinitions', icon: 'tools', link: '../report-definitions',
          view: 'report-definitions', sortKey: 700
        },
        {
          label: 'DashboardDefinitions', icon: 'tools', link: '../dashboard-definitions',
          view: 'dashboard-definitions', sortKey: 800
        },
        {
          label: 'NotificationTemplates', icon: 'file-code', link: '../notification-templates',
          view: 'notification-templates', sortKey: 900
        },
        {
          label: 'MessageTemplates', icon: 'file-code', link: '../message-templates',
          sortKey: 1000, canView: () =>
            this.workspace.globalSettings.SmsEnabled &&
            this.workspace.currentTenant.settings.SmsEnabled &&
            this.canView('message-templates'),
        },
      ]
    },
    Help: {
      background: 't-blue2',
      items: [
        // TODO: About + Licensing
        // TODO: Documentation
      ]
    },

    Miscellaneous: {
      background: 't-grey',
      items: [
      ]
    }
  };

  public get ws(): TenantWorkspace {
    return this.workspace.currentTenant;
  }

  public get mainMenu(): MenuSectionInfo[] {
    this.initializeMainMenu();
    return this._mainMenu;
  }

  public get quickAccess(): MenuItemInfo[] {
    this.initializeMainMenu();
    return this._quickAccess;
  }

  _permissions: PermissionsForClientViews = null;
  _definitions: DefinitionsForClient = null;
  _settings: SettingsForClient = null;
  _mainMenu: MenuSectionInfo[];
  _quickAccess: MenuItemInfo[];
  _currentCulture: string;
  _userSettings: UserSettingsForClient | AdminUserSettingsForClient;

  public initializeMainMenu(): void {
    const ws = this.workspace.currentTenant;
    if (this._definitions !== ws.definitions ||
      this._settings !== ws.settings ||
      this._userSettings !== ws.userSettings ||
      this._currentCulture !== this.workspace.ws.culture ||
      this._permissions !== ws.permissions) {

      this._definitions = ws.definitions;
      this._settings = ws.settings;
      this._userSettings = this.workspace.current.userSettings;
      this._currentCulture = this.workspace.ws.culture;
      this._permissions = ws.permissions;

      // Clone the main menu base and add to the clone
      // const menu = JSON.parse(JSON.stringify(this.mainMenuBase)) as { [section: string]: MenuSectionInfo };
      const menu: { [section: string]: MenuSectionInfo } = { ... this.mainMenuBase };
      for (const sectionKey of Object.keys(this.mainMenuBase)) {
        menu[sectionKey] = { ...menu[sectionKey] }; // clone section
        menu[sectionKey].items = menu[sectionKey].items.map(e => ({ ...e })); // clone items array

        // Localize the labels
        for (const item of menu[sectionKey].items) {
          item.label = this.translate.instant(item.label);
        }
      }

      // add custom screens from definitions
      this.addDefinitions(menu, ws.definitions.Lookups, 'lookups');
      this.addDefinitions(menu, ws.definitions.Agents, 'agents');
      this.addDefinitions(menu, ws.definitions.Resources, 'resources');
      this.addDefinitions(menu, ws.definitions.Documents, 'documents');

      this.addReportDefinitions(menu);
      this.addDashboardDefinitions(menu);
      this.addTemplateDefinitions(menu);

      // Set the mainMenu field and sort the items based on sortKey
      this._mainMenu = Object.keys(menu).map(sectionKey => ({
        label: this.translate.instant('Menu_' + sectionKey),
        items: menu[sectionKey].items.sort((x1, x2) => x1.sortKey - x2.sortKey),
        background: menu[sectionKey].background
      }));

      // Quick access menu
      const settingsQuickAccessLinks = this.userSettings.get<string[]>('main-menu/quick-access');
      if (!!settingsQuickAccessLinks && !!settingsQuickAccessLinks.length) {
        // First hash the links in a dictionary
        const settingsQuickAccessLinksDic: { [link: string]: true } = {};
        for (const link of settingsQuickAccessLinks) {
          settingsQuickAccessLinksDic[link] = true;
        }

        // Then loop over the main menu, to verify that the links exist (in case a link is no longer present/valid)
        const validQuickAccessLinksDic: { [link: string]: MenuItemInfo } = {};
        for (const section of this._mainMenu) {
          for (const item of section.items) {
            if (settingsQuickAccessLinksDic[item.link]) { // It's one of the quick access items
              validQuickAccessLinksDic[item.link] = item; // Add the item to quick access
            }
          }
        }

        this._quickAccess = settingsQuickAccessLinks.map(e => validQuickAccessLinksDic[e]).filter(e => !!e);

      } else {
        this._quickAccess = [];
      }

      // Useful for keeping all occurrences of menu sections in sync
      // console.log(Object.keys(this.mainMenuBase).join(`', '`)); // xyz-definition.ts
      // console.log(Object.keys(this.mainMenuBase).join(`", "`)); // XyzDefinition.cs
      // console.log(Object.keys(this.mainMenuBase).map(e => 'Menu_' + e).join(`", "`)); // XyzDefinition.cs
    }
  }

  private addReportDefinitions(menu: { [section: string]: MenuSectionInfo }) {
    const ws = this.workspace.currentTenant;
    const sharedDefIds = {};
    for (const defId of ws.reportIds) {
      sharedDefIds[defId] = true;
    }

    for (const definitionId of Object.keys(ws.definitions.Reports)) {
      const definition = ws.definitions.Reports[+definitionId];
      if (!definition.ShowInMainMenu) {
        continue;
      }

      if (sharedDefIds[definitionId]) {

        // Get the label
        const label = ws.getMultilingualValueImmediate(definition, 'Title') || this.translate.instant('Untitled');
        const sortKey = definition.MainMenuSortKey;
        const icon = definition.MainMenuIcon || 'folder';

        // Get the section
        let menuSection: string;
        if (menu[definition.MainMenuSection]) {
          menuSection = definition.MainMenuSection;
        } else {
          menuSection = 'Miscellaneous';
        }

        menu[menuSection].items.push({
          label,
          sortKey,
          icon,
          link: `../report/${definitionId}`,
          paramsFunc: () => {
            // This is to solve the unfortunate 'null' bug that kept haunting us for a long time even after we fixed it
            const params = this.userSettings.get<Params>(`report/${definitionId}/arguments`);
            const paramsWithoutNulls = {};
            if (!!params) {
              for (const key of Object.keys(params)) {
                const value = params[key];
                if (isSpecified(value)) {
                  paramsWithoutNulls[key] = value;
                }
              }
            }
            return paramsWithoutNulls;
          },
        });
      }
    }
  }

  private addDashboardDefinitions(menu: { [section: string]: MenuSectionInfo }) {
    const ws = this.workspace.currentTenant;
    const sharedDefIds = {};
    for (const defId of ws.dashboardIds) {
      sharedDefIds[defId] = true;
    }

    for (const definitionId of Object.keys(ws.definitions.Dashboards)) {
      const definition = ws.definitions.Dashboards[+definitionId];
      if (!definition.ShowInMainMenu) {
        continue;
      }

      if (sharedDefIds[definitionId]) {

        // Get the label
        const label = ws.getMultilingualValueImmediate(definition, 'Title') || this.translate.instant('Untitled');
        const sortKey = definition.MainMenuSortKey;
        const icon = definition.MainMenuIcon || 'folder';

        // Get the section
        let menuSection: string;
        if (menu[definition.MainMenuSection]) {
          menuSection = definition.MainMenuSection;
        } else {
          menuSection = 'Miscellaneous';
        }

        menu[menuSection].items.push({
          label,
          sortKey,
          icon,
          link: `../dashboard/${definitionId}`
        });
      }
    }
  }

  private addTemplateDefinitions(menu: { [section: string]: MenuSectionInfo }) {
    const ws = this.workspace.currentTenant;
    for (const templateId of ws.templateIds) {
      const template = ws.definitions.PrintingTemplates[templateId];
      if (!!template) {
        // Get the label
        const label = ws.getMultilingualValueImmediate(template, 'Name') || this.translate.instant('Untitled');
        const sortKey = template.MainMenuSortKey;
        const icon = template.MainMenuIcon || 'folder';

        // Get the section
        let menuSection: string;
        if (menu[template.MainMenuSection]) {
          menuSection = template.MainMenuSection;
        } else {
          menuSection = 'Miscellaneous';
        }

        menu[menuSection].items.push({
          label,
          sortKey,
          icon,
          link: `../print/${templateId}`,
          paramsFunc: () => this.userSettings.get<Params>(`print/${templateId}/arguments`),
        });
      }
    }
  }

  public linkArray(item: MenuItemInfo): (string | Params)[] {
    if (!item.linkArray) {
      item.linkArray = [item.link];

      // Additional parameters are added to some screens
      if (!!item.paramsFunc) {
        const params = item.paramsFunc();
        if (!!params) {
          item.linkArray.push(params);
        }
      }
    }

    return item.linkArray;
  }

  private addDefinitions(
    menu: { [section: string]: MenuSectionInfo },
    definitions: { [defId: string]: DefinitionForClient },
    url: string, titleFunc?: (def: DefinitionForClient) => string) {
    if (!!definitions) {

      titleFunc = titleFunc || (d => this.workspace.currentTenant.getMultilingualValueImmediate(d, 'TitlePlural')
        || this.translate.instant('Untitled'));
      for (const definitionId of Object.keys(definitions).filter(defId => this.canView(`${url}/${defId}`))) {

        // get the definition
        const definition = definitions[definitionId];

        // get the label
        const label = titleFunc(definition);

        // add the menu section if missing
        if (!menu[definition.MainMenuSection]) {
          definition.MainMenuSection = 'Miscellaneous';
        }

        // push the menu item
        menu[definition.MainMenuSection].items.push({
          label,
          sortKey: definition.MainMenuSortKey,
          icon: definition.MainMenuIcon || 'folder',
          link: `../${url}/${definitionId}`
        });

        // if (url === 'agents') {


        //   menu[definition.MainMenuSection].items.push({
        //     label: this.translate.instant('StatementOf0', {
        //       0 : this.workspace.currentTenant.getMultilingualValueImmediate(definition, 'TitleSingular')
        //     }),
        //     sortKey: definition.MainMenuSortKey + 1,
        //     icon: definition.MainMenuIcon || 'folder',
        //     link: `../agent-statement/${definitionId}`,
        //     paramsFunc: null // TODO
        //   });
        // }
      }
    }
  }

  // this captures all keydown events from the root document
  // in here we allow the user to navigate the focus around the main menu
  // tiles with the arrow keys, ala metro style
  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (this.workspace.ignoreKeyDownEvents) {
      return;
    }

    let key: string = event.key;

    // Focus on the search field as soon as the user starts typing letters or numbers
    if (!this.searchInputIsFocused && !!key && key.trim().length === 1) {
      this.currentXMemory = -1;
      this.searchInput.nativeElement.value = '';
      this.searchInput.nativeElement.focus();
    }

    // custom keyboard interactions only occur when the user is not pressing Alt or Ctrl
    // for example alt-leftArrow and alt-rightArrow are universal keyboard for forward and backward browser navigation
    if (event.altKey || event.ctrlKey) {
      return;
    }

    if (Key[key]) {

      // reverse left and right arrows for RTL languages
      if (this.workspace.ws.isRtl) {
        if (key === Key.ArrowRight) {
          key = Key.ArrowLeft;
        } else if (key === Key.ArrowLeft) {
          key = Key.ArrowRight;
        }
      }

      switch (key) {
        case Key.Escape: {
          this.currentXMemory = -1;
          this.search = '';
          this.currentItem = -1;
          this.currentSection = -1;

          (this.document.activeElement as any).blur();

          break;
        }
        case Key.Tab: {
          this.currentXMemory = -1;
          break;
        }
        case Key.ArrowLeft:
          if (this.currentIdExists) {
            this.currentXMemory = -1;
            const gridModel = this.computeGridModel();

            let nextX = gridModel.currentX;
            const nextY = gridModel.currentY;

            nextX = nextX - 1;
            if (nextX < 0) {
              // (1) to wrap around
              // nextY = nextY - 1;
              // if (nextY < 0) {
              //   nextY = gridModel.height - 1;
              // }

              // nextX = gridModel.width - 1;
              // while (!gridModel.grid[nextY][nextX] && nextX > 0) {
              //   nextX--;
              // }

              // (1) to not wrap around
              nextX = 0;
            }

            const indices = gridModel.grid[nextY][nextX];
            this.focusTile(indices.sectionIndex, indices.itemIndex, event);
          }
          break;
        case Key.ArrowRight:
          if (!this.showNoItemsFound && !this.searchInputIsFocused) {
            this.currentXMemory = -1;
            let nextX = 0;
            let nextY = 0;
            const gridModel = this.computeGridModel();

            if (this.currentIdExists) {

              nextX = gridModel.currentX;
              nextY = gridModel.currentY;

              nextX = nextX + 1;

              // (1) to wrap around
              // while (!gridModel.grid[nextY][nextX] && nextX < gridModel.width) {
              //   nextX++;
              // }
              // if (nextX >= gridModel.width) {
              //   // nextY = nextY + 1;
              //   // if (nextY >= gridModel.height) {
              //   //   nextY = 0;
              //   // }

              //   nextX = 0;
              // }

              // (2) to not wrap around
              if (!gridModel.grid[nextY][nextX]) {
                nextX = nextX - 1;
              }
            }

            const indices = gridModel.grid[nextY][nextX];
            this.focusTile(indices.sectionIndex, indices.itemIndex, event);

          }
          break;
        case Key.ArrowUp:
          if (this.currentIdExists) {
            const gridModel = this.computeGridModel();

            let nextX = this.currentXMemory < 0 ? gridModel.currentX : this.currentXMemory;
            let nextY = gridModel.currentY;

            nextY = nextY - 1;
            if (nextY < 0) {
              // (1) to wrap around
              // nextY = gridModel.height - 1;

              // (2) to not wrap around
              nextY = 0;
            }

            this.currentXMemory = Math.max(this.currentXMemory, nextX);
            while (!gridModel.grid[nextY][nextX] && nextX > 0) {
              nextX--;
            }
            const indices = gridModel.grid[nextY][nextX];
            this.focusTile(indices.sectionIndex, indices.itemIndex, event);
          }
          break;
        case Key.ArrowDown:
          // the down arrow works even if no tile is highlighted
          // in which case we highlight the very first tile
          if (!this.showNoItemsFound) {
            let nextX = 0;
            let nextY = 0;
            const gridModel = this.computeGridModel();

            if (this.currentIdExists) {

              nextX = this.currentXMemory < 0 ? gridModel.currentX : this.currentXMemory;
              nextY = gridModel.currentY;

              nextY = nextY + 1;
              if (nextY >= gridModel.height) {
                // (1) to wrap around
                // nextY = 0;

                // (2) to not wrap around
                nextY = gridModel.height - 1;
              }

              this.currentXMemory = Math.max(this.currentXMemory, nextX);
              while (!gridModel.grid[nextY][nextX] && nextX > 0) {
                nextX--;
              }
            } else {
              this.currentXMemory = -1;
            }

            const indices = gridModel.grid[nextY][nextX];
            this.focusTile(indices.sectionIndex, indices.itemIndex, event);
          }

          break;
      }
    }
  }

  private get searchInputIsFocused(): boolean {
    return this.searchInput.nativeElement === this.document.activeElement;
  }

  private focusTile(sectionIndex: number, itemIndex: number, event: KeyboardEvent) {
    const elem = this.document.getElementById(this.getId(sectionIndex, itemIndex));
    elem.focus();
    event.preventDefault();
    event.stopPropagation();
  }

  private get rowTiles(): number {
    // this computes the number of tiles in each row using media queries
    for (let i = this.MAX_TILES; i > 0; i--) {
      const minWidth = (this.CONTAINER_MARGIN * 2) + (this.TILE_WIDTH + (2 * this.TILE_MARGIN)) * i;
      if (window.matchMedia(`(min-width: ${minWidth}px)`).matches) {
        return i;
      }
    }
  }

  private computeGridModel(): {
    currentX: number,
    currentY: number,
    width: number,
    height: number,
    grid: { sectionIndex: number, itemIndex: number }[][]
  } {
    const rowTiles = this.rowTiles;
    let currentY = -1;
    let currentX = -1;
    const gridModel: { sectionIndex: number, itemIndex: number }[][] = [];

    for (let s = 0; s < this.sectionsLength; s++) {
      const sectionItems = this.getSectionItems(s);
      if (this.showSectionIndex(s)) {
        const visibleItems = sectionItems.map((e, index) => ({ sectionIndex: s, itemIndex: index }))
          .filter((indices) => this.showItemIndex(indices.sectionIndex, indices.itemIndex));

        for (let i = 0; i < visibleItems.length; i = i + rowTiles) {
          const row = visibleItems.slice(i, i + rowTiles);
          gridModel.push(row);

          for (let col = 0; col < rowTiles; col++) {
            const cell = row[col];
            if (!!cell && cell.itemIndex === this.currentItem && cell.sectionIndex === this.currentSection) {
              currentY = gridModel.length - 1;
              currentX = col;
            }
          }
        }
      }
    }

    return {
      currentX,
      currentY,
      width: rowTiles,
      height: gridModel.length,
      grid: gridModel
    };
  }

  private get sectionsLength(): number {
    return 1 + this.mainMenu.length;
  }

  private getSectionItems(index: number) {
    if (index === 0) {
      return this.quickAccess;
    } else {
      return this.mainMenu[index - 1].items;
    }
  }

  get currentIdExists(): boolean {
    return this.currentSection >= 0 && this.currentItem >= 0;
  }

  public onFocus(section: number, item: number) {
    this.currentSection = section;
    this.currentItem = item;
  }

  public onBlur() {
    this.currentSection = -1;
    this.currentItem = -1;
  }

  public getId(section: number, item: number) {
    return `${section}_${item}`;
  }

  onNavigate(url: string) {
    this.router.navigate([url], { relativeTo: this.route });
  }

  showSection(items: MenuItemInfo[]): boolean {
    if (!!this.search && items === this.quickAccess) {
      return false;
    }

    return !!items && items.some(e => this.showItem(e));
  }

  showItem(item: MenuItemInfo): boolean {
    const term = this.search;
    return (!item.view || this.canView(item.view)) && (!item.canView || item.canView()) &&
      (!term || this.translate.instant(item.label).toLowerCase().indexOf(term.toLowerCase()) !== -1);
  }

  showSectionIndex(sectionIndex: number) {
    const section = this.getSectionItems(sectionIndex);
    return this.showSection(section);
  }

  showItemIndex(sectionIndex: number, itemIndex: number) {
    const item = this.getSectionItems(sectionIndex)[itemIndex];
    return this.showItem(item);
  }

  get showNoItemsFound(): boolean {
    return this.mainMenu.every(section => !this.showSection(section.items)) && !this.showSection(this.quickAccess);
  }

  public canView(view: string) {
    return this.workspace.currentTenant.canRead(view);
  }

  /**
   * This is to help the business analyst choose the right sort key for a new screen
   */
  public onSectionDoubleClick(section: MenuSectionInfo) {
    if (!section || !section.items) {
      return;
    }
    console.log(`------- Section: ${section.label} -------`);
    for (const item of section.items) {
      console.log(`${item.sortKey} ${item.label}`);
    }

    // {
    //   const uq = new NgbCalendarUmAlQura();
    //   const date = new Date(2074, 8, 8);
    //   const hDate = uq.fromGregorian(date);
    //   const dateBack = uq.toGregorian(hDate);

    //   console.log(toLocalDateOnlyISOString(date), hDate, toLocalDateOnlyISOString(dateBack));
    // }

    // if (false) {
    //   const uq = new NgbCalendarUmAlQura();
    //   const data = [['Gregorian', 'H Year', 'H Month', 'H Day']];
    //   const date = new Date(1900, 3, 30);
    //   let errors = 0;
    //   while (date < new Date(2077, 10, 17)) {
    //     const { year, month, day } = uq.fromGregorian(date);

    //     // Test
    //     const dateString = toLocalDateOnlyISOString(date);
    //     if (year > 1500) {
    //       console.error(dateString, { year, month, day });
    //     }

    //     const dateBack = uq.toGregorian(new NgbDate(year, month, day));
    //     const dateBackString = toLocalDateOnlyISOString(dateBack);
    //     if (dateString !== dateBackString && errors < 100) {
    //       console.error(dateString, { year, month, day }, dateBackString);
    //       errors++;
    //     }

    //     data.push([dateString, year + '', month + '', day + '']);
    //     date.setDate(date.getDate() + 1);
    //   }

    //   const blob = csvPackage(data);
    //   downloadBlob(blob, 'UmAlQura.csv');
    // }
  }

  public onMenuItemClick(item: MenuItemInfo) {

    if (!!this.quickAccess[0] && item.link === this.quickAccess[0].link) {
      // Clicked the first item in quick access, nothing to do
      return;
    }

    // Get the array of quick access links (excluding with the new link created or moved to first position)
    const newLinks = this.quickAccess.filter(e => e.link !== item.link).map(e => e.link);
    newLinks.unshift(item.link);

    // Keep the array of quick access list at 6 tiles
    while (newLinks.length > 6) {
      newLinks.pop();
    }

    // Save the in the user settings
    const newLinksString = JSON.stringify(newLinks);
    this.userSettings.save('main-menu/quick-access', newLinksString);
  }
}
