import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ReportView } from '../report-results/report-results.component';
import { WorkspaceService, ReportArguments, ReportStore, DEFAULT_PAGE_SIZE } from '~/app/data/workspace.service';
import {
  ChoicePropDescriptor, StatePropDescriptor, PropDescriptor, entityDescriptorImpl, EntityDescriptor, metadata
} from '~/app/data/entities/base/metadata';
import { TranslateService } from '@ngx-translate/core';
import { FilterTools } from '~/app/data/filter-expression';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { DefinitionsForClient, ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { isSpecified } from '~/app/data/util';
import { Observable, Subject, Subscription } from 'rxjs';
import { ActivatedRoute, Router, Params, ParamMap } from '@angular/router';

interface ParameterInfo { label: () => string; key: string; desc: PropDescriptor; isRequired: boolean; }

@Component({
  selector: 'b-report',
  templateUrl: './report.component.html',
  styles: []
})
export class ReportComponent implements OnInit, OnDestroy {

  @Input()
  mode: 'screen' | 'preview' = 'screen';

  private _subscriptions: Subscription;
  private refresh$ = new Subject<void>();
  private _currentFilter: string;
  private _currentLanguage: string;
  private _currentSettings: SettingsForClient;
  private _currentDefinitions: DefinitionsForClient;
  private _currentParameters: ParameterInfo[] = [];
  private _parametersErrorMessage: string;
  private _views: { view: ReportView, label: string, icon: string }[] = [
    { view: ReportView.pivot, label: 'Table', icon: 'table' },
    { view: ReportView.card, label: 'Card', icon: 'tachometer-alt' },
    { view: ReportView.bars_vertical, label: 'BarChart', icon: 'chart-bar' },
    { view: ReportView.line, label: 'LineChart', icon: 'chart-area' },
    { view: ReportView.pie, label: 'PieChart', icon: 'chart-pie' }
  ];

  private definitionId: string;
  private _skip = 0;

  public view: ReportView = ReportView.pivot;
  public arguments: ReportArguments = {};
  public immutableArguments: ReportArguments = {};

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute) { }

  ngOnInit() {
    // Pick up state from the URL
    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.subscribe((params: ParamMap) => {


      // This triggers changes on the screen
      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (!definitionId || !this.workspace.current.definitions.Reports[definitionId]) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }

        // here we pick up the screen state
        if (this.definition.Type === 'Summary') {
          const view = params.get('view');
          if (!!view && !!ReportView[view]) {
            this.view = ReportView[view];
          }
        }

        if (this.definition.Type === 'Details') {
          this._skip = +params.get('skip') || 0;
        }

        // Read the arguments from the URL
        for (const p of this.parameters) {
          const urlStringValue = params.get(p.key) || null;
          let urlValue: any = null;
          if (urlStringValue === null) {
            urlValue = null;
          } else {
            switch (p.desc.control) {
              case 'text':
              case 'date':
              case 'datetime':
                urlValue = urlStringValue;
                break;
              case 'number':
                urlValue = +urlStringValue;
                break;
              case 'boolean':
                urlValue = urlStringValue.toLowerCase() === 'true';
                break;
              case 'choice':
              case 'state':
                urlValue = (typeof p.desc.choices[0] === 'string') ? urlStringValue : +urlStringValue;
                break;
              case 'navigation':
                // developer mistake
                console.error(`Use of a navigation property for parameter @${p.key}`);
                break;
            }
          }

          this.arguments[p.key] = urlValue;
        }

        this.immutableArguments = { ...this.arguments };
      }
    }));
  }

  ngOnDestroy() {
    this._subscriptions.unsubscribe();
  }

  get isScreenMode(): boolean {
    return true;
  }

  get definition(): ReportDefinitionForClient {
    return this.workspace.current.definitions.Reports[this.definitionId];
  }

  get entityDescriptor(): EntityDescriptor {
    if (!this.definition) {
      return null;
    }

    const coll = this.definition.Collection;
    const definitionId = this.definition.DefinitionId;
    return !!coll ? metadata[coll](this.workspace.current, this.translate, definitionId) : null;
  }

  get apiEndpoint(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.apiEndpoint : null;
  }

  public get state(): ReportStore {

    if (!this.workspace.current.reportState[this.stateKey]) {
      // this.workspace.current.reportState = {};
      this.workspace.current.reportState[this.stateKey] = new ReportStore();
      // this.immutableArguments = {};
    }

    return this.workspace.current.reportState[this.stateKey];
  }

  private urlStateChange(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL

    const params: Params = {};

    if (!!this.definition && this.definition.Type === 'Summary') {
      params.view = this.view;
    }

    if (!!this.definition && this.definition.Type === 'Details' && !!this.skip) {
      params.skip = this.skip;
    }

    this.parameters.forEach(p => {
      const value = this.arguments[p.key];
      if (isSpecified(value)) {
        params[p.key] = value + '';
      }
    });

    this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
  }

  // private updateUrlState(key: string, value: any, replaceUrl = true) {
  //   const params = this.route.snapshot.params;
  //   if (value === null || value === undefined) {
  //     delete params[key];
  //   } else {
  //     params[key] = value;
  //   }

  //   this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl });
  // }

  // UI Bindings

  get title(): string {
    return this.workspace.current.getMultilingualValueImmediate(this.definition, 'Title');
  }

  get views() {
    // For the report views toggle
    return this._views;
  }

  get parameters() {
    if (!this.definition || !this.definition.Filter) {
      this._currentParameters = [];
    }

    if (this.definition.Filter !== this._currentFilter ||
      this.workspace.current.settings !== this._currentSettings ||
      this.workspace.current.definitions !== this._currentDefinitions ||
      this.translate.currentLang !== this._currentLanguage) {

      try {
        this._parametersErrorMessage = null;
        this._currentParameters = [];
        this._currentFilter = this.definition.Filter;
        this._currentSettings = this.workspace.current.settings;
        this._currentDefinitions = this.workspace.current.definitions;
        this._currentLanguage = this.translate.currentLang;

        // (1) parse the filter to get the list of placeholder atoms
        const exp = FilterTools.parse(this.definition.Filter);
        const placeholderAtoms = FilterTools.placeholderAtoms(exp);
        const paramsFromFilterPlaceholders: { [key: string]: ParameterInfo } = {};
        for (const atom of placeholderAtoms) {
          const key = atom.value.substr(1);
          const keyLower = key.toLowerCase();
          const entityDesc = entityDescriptorImpl(
            atom.path,
            this.definition.Collection,
            this.definition.DefinitionId,
            this.workspace.current,
            this.translate);

          // Check if the filtered property is a foreign key of another nav,
          // property, if so use the descriptor of that nav property instead
          const immediatePropDesc = entityDesc.properties[atom.property];
          if (!!immediatePropDesc && immediatePropDesc.control === 'navigation') {
            throw new Error(`Cannot terminate a filter path with a navigation property like '${atom.property}'`);
          }

          const propDesc: PropDescriptor = Object.keys(entityDesc.properties)
            .map(e => entityDesc.properties[e])
            .find(e => e.control === 'navigation' && e.foreignKeyName === atom.property)
            || immediatePropDesc; // Else rely on the descriptor of the prop itself
          if (!propDesc) {
            throw new Error(`Property '${atom.property}' does not exist on '${entityDesc.titlePlural}'`);
          }

          paramsFromFilterPlaceholders[keyLower] = {
            label: propDesc.label,
            key,
            desc: propDesc,
            isRequired: false
          };
        }

        // (2) Override defaults using values from definitions.Parameters;
        // The parameter definitions can override 3 things (1) order (2) label (3) is required
        const params = this.definition.Parameters || [];
        for (const p of params) {
          const keyLower = p.Key.toLowerCase();
          const paramInfo = paramsFromFilterPlaceholders[keyLower];
          if (!!paramInfo) {
            paramInfo.label = () => !!p.Label ? this.workspace.current.getMultilingualValueImmediate(p, 'Label') : paramInfo.label();
            paramInfo.isRequired = p.IsRequired;
            this._currentParameters.push(paramInfo);
            delete paramsFromFilterPlaceholders[keyLower];
          }
        }

        // (3) Add the remaining parameters from filter that have no definitions
        for (const key of Object.keys(paramsFromFilterPlaceholders)) {
          this._currentParameters.push(paramsFromFilterPlaceholders[key]);
        }

      } catch (ex) {
        console.error(ex.message);
        this._parametersErrorMessage = ex;
      }
    }

    return this._currentParameters;
  }

  public choices(desc: ChoicePropDescriptor | StatePropDescriptor) {
    desc.selector = desc.selector || desc.choices.map(c => ({ value: c, name: desc.format(c) }));
    return desc.selector;
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get canExport(): boolean {
    return true;
  }

  public onExport() {
    alert('TODO');
  }

  public onRefresh() {
    this.refresh$.next();
  }

  public get showPagingControls(): boolean {
    return !!this.definition && this.definition.Type === 'Details' && !this.definition.Top;
  }

  public get showReportViewToggle(): boolean {
    return !!this.definition && this.definition.Type === 'Summary';
  }

  public get stateKey(): string {
    return this.mode === 'screen' ? this.definitionId : '<preview>'; // TODO: In preview mode do not persist the state
  }

  public get skip(): number {
    return this.showPagingControls ? this._skip : 0;
  }

  get from(): number {
    return Math.min(this._skip + 1, this.total);
  }

  get to(): number {
    return Math.min(this._skip + DEFAULT_PAGE_SIZE, this.total);
  }

  get total(): number {
    return this.state.total;
  }

  onFirstPage() {
    this._skip = 0;
    this.urlStateChange();
  }

  get canFirstPage(): boolean {
    return this.canPreviousPage;
  }

  onPreviousPage() {
    this._skip = Math.max(this._skip - DEFAULT_PAGE_SIZE, 0);
    this.urlStateChange();
  }

  get canPreviousPage(): boolean {
    return this._skip > 0;
  }

  onNextPage() {
    this._skip = this._skip + DEFAULT_PAGE_SIZE;
    this.urlStateChange();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  public get flip() {

    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get isChart(): boolean {
    return this.view !== 'pivot';
  }

  public onArgumentChange() {
    this.immutableArguments = { ...this.arguments };
    this._skip = 0;
    this.urlStateChange();
  }

  public isActive(view: ReportView) {
    return view === this.view;
  }

  public onView(view: ReportView) {
    this.view = view;
    this.urlStateChange();
  }

  public get showParametersSection(): boolean {
    return this.showParameters || this.showParametersErrorMessage;
  }

  public get showParameters(): boolean {
    return this.parameters.length > 0;
  }

  public get showParametersErrorMessage(): boolean {
    return !!this._parametersErrorMessage;
  }

  public get parametersErrorMessage(): string {
    return this._parametersErrorMessage;
  }

  public get areAllRequiredParamsSpecified(): boolean {
    const args = this.arguments;
    return this.parameters
      .filter(p => p.isRequired)
      .every(p => isSpecified(args[p.key]));
  }

  public get refresh(): Observable<void> {
    return this.refresh$;
  }
}
