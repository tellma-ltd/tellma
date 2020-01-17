import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { ReportView, modifiedPropDesc } from '../report-results/report-results.component';
import { WorkspaceService, ReportArguments, ReportStore, DEFAULT_PAGE_SIZE, TenantWorkspace } from '~/app/data/workspace.service';
import {
  ChoicePropDescriptor, StatePropDescriptor, PropDescriptor, entityDescriptorImpl, EntityDescriptor, metadata, getChoices
} from '~/app/data/entities/base/metadata';
import { TranslateService } from '@ngx-translate/core';
import { FilterTools } from '~/app/data/filter-expression';
import { ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { isSpecified } from '~/app/data/util';
import { Observable, Subject, Subscription } from 'rxjs';
import { ActivatedRoute, Router, Params, ParamMap } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ReportDefinition } from '~/app/data/entities/report-definition';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

interface ParameterInfo { label: () => string; key: string; desc: PropDescriptor; isRequired: boolean; }

@Component({
  selector: 'b-report',
  templateUrl: './report.component.html',
  styles: []
})
export class ReportComponent implements OnInit, OnDestroy {

  @Input()
  mode: 'screen' | 'preview' = 'screen';

  @Input()
  previewDefinition: ReportDefinitionForClient; // Used in preview mode

  @Input()
  get showChart(): boolean {
    return this._showChart;
  }

  set showChart(v: boolean) {
    if (this._showChart !== v) {
      this.view = !!v ? ReportView.chart : ReportView.pivot;
      this._showChart = v;
    }
  }

  private _parameterCount: number = null;
  private _showChart: boolean;
  private _subscriptions: Subscription;
  private refresh$ = new Subject<void>();
  private _currentFilter: string;
  private _currentDefinition: ReportDefinitionForClient;
  private _currentParameters: ParameterInfo[] = [];
  private _parametersErrorMessage: string;
  private _views: { view: ReportView, label: string, icon: string }[] = [
    { view: ReportView.pivot, label: 'Table', icon: 'table' },
    { view: ReportView.chart, label: 'Chart', icon: 'chart-pie' },
  ];

  private definitionId: string;

  public view: ReportView;
  public arguments: ReportArguments = {};
  public immutableArguments: ReportArguments = {};

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute, public modalService: NgbModal) { }

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
          this.state.skip = +params.get('skip') || 0;
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
              case 'serial':
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

    // Set to default
    this.view = this.view || (!!this.definition.Chart && !!this.definition.DefaultsToChart ? ReportView.chart : ReportView.pivot);
  }

  ngOnDestroy() {
    this._subscriptions.unsubscribe();
  }

  get isScreenMode(): boolean {
    return this.mode === 'screen';
  }

  get isPreviewMode(): boolean {
    return this.mode === 'preview';
  }

  get definition(): ReportDefinitionForClient {
    return this.isScreenMode ?
      this.workspace.current.definitions.Reports[this.definitionId] :
      this.previewDefinition;
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
      this.workspace.current.reportState[this.stateKey] = new ReportStore();
    }

    return this.workspace.current.reportState[this.stateKey];
  }

  private urlStateChange(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL

    if (this.isScreenMode) {
      const params: Params = {};

      if (!!this.definition && this.definition.Type === 'Summary') {
        params.view = this.view;
      }

      if (!!this.definition && this.definition.Type === 'Details' && !!this.state.skip) {
        params.skip = this.state.skip;
      }

      this.parameters.forEach(p => {
        const value = this.arguments[p.key];
        if (isSpecified(value)) {
          params[p.key] = value + '';
        }
      });

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    }
  }

  // UI Bindings

  get title(): string {
    const title = this.workspace.current.getMultilingualValueImmediate(this.definition, 'Title');
    return title; // this.isScreenMode ? title : `${title} (${this.translate.instant('Preview')})`;
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
      this._currentDefinition !== this.definition) {

      try {
        this._parametersErrorMessage = null;
        this._currentParameters = [];
        this._currentDefinition = this.definition;
        this._currentFilter = this.definition.Filter;
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

          let propDesc: PropDescriptor = Object.keys(entityDesc.properties)
            .map(e => entityDesc.properties[e])
            .find(e => e.control === 'navigation' && e.foreignKeyName === atom.property)
            || immediatePropDesc; // Else rely on the descriptor of the prop itself

          if (!propDesc) {
            throw new Error(`Property '${atom.property}' does not exist on '${entityDesc.titlePlural()}'`);
          }

          if (!!atom.modifier) {
            // A modifier is specified, the prop descriptor is hardcoded per modifier
            propDesc = modifiedPropDesc(propDesc, atom.modifier, this.translate);
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
            paramInfo.label = !!p.Label ? () => this.workspace.current.getMultilingualValueImmediate(p, 'Label') : paramInfo.label;
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

      // When the number of parameters changes it might change the size of
      // the chart underneath it, so we trigger a recalculation of the size
      if (this._currentParameters.length !== this._parameterCount && this._parameterCount !== null && this.isChart) {
        window.dispatchEvent(new Event('resize')); // So the chart would resize
      }

      this._parameterCount = this._currentParameters.length;
    }

    return this._currentParameters;
  }

  public choices(desc: ChoicePropDescriptor | StatePropDescriptor): SelectorChoice[] {
    return getChoices(desc);
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

  public get showReportViewToggle(): boolean {
    return !!this.definition && this.definition.Type === 'Summary' && !!this.definition.Chart;
  }

  public get stateKey(): string {
    return this.mode === 'screen' ? this.definitionId : '<preview>'; // In preview mode a local state is used anyways
  }

  public get isChart(): boolean {
    return this.view === ReportView.chart && !!this.definition && this.definition.Type === 'Summary';
  }

  public onArgumentChange() {
    this.immutableArguments = { ...this.arguments };
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

  public onSkipChange(skip: number) {
    this.urlStateChange();
  }


  public get showPagingControls(): boolean {
    return !!this.definition && this.definition.Type === 'Details' && !this.definition.Top;
  }

  public get skip(): number {
    return this.showPagingControls ? this.state.skip : 0;
  }

  get from(): number {
    return Math.min(this.state.skip + 1, this.total);
  }

  get to(): number {
    const s = this.state;
    return Math.min(s.skip + DEFAULT_PAGE_SIZE, s.total);
  }

  get total(): number {
    return this.state.total;
  }

  onFirstPage() {
    this.state.skip = 0;
  }

  get canFirstPage(): boolean {
    return this.canPreviousPage;
  }

  onPreviousPage() {
    const s = this.state;
    s.skip = Math.max(s.skip - DEFAULT_PAGE_SIZE, 0);

    this.onSkipChange(s.skip); // to update the URL state
    this.refresh$.next();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + DEFAULT_PAGE_SIZE;

    this.onSkipChange(s.skip); // to update the URL state
    this.refresh$.next();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  public get flip() {

    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  public get overflow(): string {
    // Charts lag a bit when you resize the screen causing the scroller to briefly appear
    // But in reality they always occupy 100% hight and never exceed it
    // We use this trick to avoid the brief appearance of the scroller
    return this.isChart ? 'hidden' : 'auto';
  }

  public get description(): string {
    return this.workspace.current.getMultilingualValueImmediate(this.definition, 'Description');
  }

  public get disableRefresh(): boolean {
    return !this.areAllRequiredParamsSpecified || this.state.disableFetch;
  }
}
