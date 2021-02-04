// tslint:disable:member-ordering
import { Component, OnInit, Input, OnDestroy, ViewChild, TemplateRef } from '@angular/core';
import { ReportView, ReportResultsComponent } from '../report-results/report-results.component';
import {
  WorkspaceService,
  ReportArguments,
  ReportStore,
  MasterStatus,
  MAXIMUM_COUNT
} from '~/app/data/workspace.service';
import {
  ChoicePropDescriptor,
  EntityDescriptor,
  metadata,
  getChoices,
  PropVisualDescriptor
} from '~/app/data/entities/base/metadata';
import { TranslateService } from '@ngx-translate/core';
import { ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { isSpecified, updateOn } from '~/app/data/util';
import { Observable, Subject, Subscription } from 'rxjs';
import { ActivatedRoute, Router, Params, ParamMap } from '@angular/router';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { formatNumber } from '@angular/common';
import { ParameterInfo, QueryexUtil } from '~/app/data/queryex-util';

@Component({
  selector: 't-report',
  templateUrl: './report.component.html',
  styles: []
})
export class ReportComponent implements OnInit, OnDestroy {

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  @Input()
  mode: 'screen' | 'preview' = 'screen';

  @Input()
  disableDrilldown = false;

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

  private badDefinition = false;
  private _parameterCount: number = null;
  private _showChart: boolean;
  private _subscriptions: Subscription;
  private refresh$ = new Subject<void>();
  private export$ = new Subject<string>();
  private _currentDefinition: ReportDefinitionForClient;
  private _currentEntityDescriptor: EntityDescriptor;
  private _currentParameters: ParameterInfo[] = [];
  private _views: { view: ReportView, label: string, icon: string }[] = [
    { view: ReportView.pivot, label: 'Table', icon: 'table' },
    { view: ReportView.chart, label: 'Chart', icon: 'chart-pie' },
  ];

  private definitionId: number;

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

        const definitionId = +params.get('definitionId');

        if (!definitionId || !this.workspace.currentTenant.definitions.Reports[definitionId]) {
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
          const urlStringValue = params.get(p.keyLower) || null;
          let urlValue: any = null;
          if (urlStringValue === null) {
            urlValue = null;
          } else {
            try {
              switch (p.datatype) {
                case 'datetimeoffset':
                case 'datetime':
                case 'date':
                case 'string':
                  urlValue = urlStringValue;
                  break;
                case 'numeric':
                  urlValue = +urlStringValue;
                  break;
                case 'bit':
                  urlValue = urlStringValue.toLowerCase() === 'true';
                  break;
                case 'boolean':
                case 'hierarchyid':
                case 'geography':
                case 'entity':
                case 'null':
                default:
                  console.error(`Unsupported parameter datatype ${p.datatype}.`);
                  break;
              }
            } catch (ex) {
              console.error(ex);
            }
          }

          this.arguments[p.keyLower] = urlValue;
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
      this.workspace.currentTenant.definitions.Reports[this.definitionId] :
      this.previewDefinition;
  }

  get entityDescriptor(): EntityDescriptor {
    if (!this.definition) {
      return null;
    }

    const coll = this.definition.Collection;
    const definitionId = this.definition.DefinitionId;
    return !!coll ? metadata[coll](this.workspace, this.translate, definitionId) : null;
  }

  get apiEndpoint(): string {
    const desc = this.entityDescriptor;
    return !!desc ? desc.apiEndpoint : null;
  }

  public get state(): ReportStore {

    const key = this.stateKey;
    const rs = this.workspace.currentTenant.reportState;
    if (!rs[key]) {
      rs[key] = new ReportStore();
    }

    return rs[key];
  }

  public get stateKey(): string {
    return this.mode === 'screen' ? this.definitionId.toString() : '<preview>'; // In preview mode a local state is used anyways
  }

  private urlStateChanged(): void {
    // We wish to store part of the page state in the URL
    // This method is called whenever that part of the state has changed
    // Below we capture the new URL state, and then navigate to the new URL

    if (this.isScreenMode) {
      const params: Params = {};

      if (!!this.definition && this.definition.Type === 'Summary') {
        params.view = this.view;
      }

      if (!!this.definition && this.definition.Type === 'Details') {
        const s = this.state;
        if (!!s.skip) {
          params.skip = s.skip;
        }

        if (!!s.orderbyKey) {
          params.$orderby = `${s.orderbyKey} ${s.orderbyDir}`;
        }
      }

      this.parameters.forEach(p => {
        const value = this.arguments[p.keyLower];
        if (isSpecified(value)) {
          params[p.keyLower] = value + '';
        }
      });

      this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
    }
  }

  public onOrderByChange() {
    this.urlStateChanged();
  }

  // UI Bindings

  get title(): string {
    const title = this.workspace.currentTenant.getMultilingualValueImmediate(this.definition, 'Title');
    return title;
  }

  get views() {
    // For the report views toggle
    return this._views;
  }

  get parameters(): ParameterInfo[] {
    if (this.definition !== this._currentDefinition || this.entityDescriptor !== this._currentEntityDescriptor) {
      this._currentDefinition = this.definition;
      this._currentEntityDescriptor = this.entityDescriptor;

      this._currentParameters = [];
      if (!!this.definition) {
        try {
          const { parameters } = QueryexUtil.getReportInfos(this.definition, this.workspace, this.translate);
          this._currentParameters = parameters;
          this.badDefinition = false;
        } catch {
          // Problems will be reported by the preview
          this.badDefinition = true;
        }
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

  public choices(desc: ChoicePropDescriptor): SelectorChoice[] {
    return getChoices(desc);
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get canExport(): boolean {
    return true;
  }

  public exportStatus = MasterStatus.loaded;
  public errorMessage: string;

  public onExport() {
    let title = this.title;
    if (!!title) {
      title = title + '.csv';
    }

    this.export$.next(title);
  }

  public get showExportSpinner() {
    return this.exportStatus === MasterStatus.loading;
  }

  public onExportStarting() {
    this.exportStatus = MasterStatus.loading;
  }

  public onExportSuccess() {
    this.exportStatus = MasterStatus.loaded;
  }

  public onExportError(err: string) {
    this.exportStatus = MasterStatus.error;

    this.errorMessage = err;
    this.modalService.open(this.errorModal);
  }

  public onRefresh() {
    this.refresh$.next();
  }

  public get showReportViewToggle(): boolean {
    return !!this.definition && this.definition.Type === 'Summary' && !!this.definition.Chart;
  }

  public get isChart(): boolean {
    return this.view === ReportView.chart && !!this.definition && this.definition.Type === 'Summary';
  }

  public onArgumentChange() {
    this.immutableArguments = { ...this.arguments };
    this.urlStateChanged();
  }

  public updateOn(desc: PropVisualDescriptor): 'change' | 'blur' {
    return updateOn(desc);
  }

  public isActive(view: ReportView) {
    return view === this.view;
  }

  public onView(view: ReportView) {
    this.view = view;
    this.urlStateChanged();
  }

  public get showParametersSection(): boolean {
    return this.showParameters && !this.collapseParameters;
  }

  public get showParameters(): boolean {
    return this.parameters.length > 0;
  }

  public get areAllRequiredParamsSpecified(): boolean {
    const args = this.arguments;
    return this.parameters.every(p => !p.isRequired || isSpecified(args[p.keyLower]));
  }

  public get showPreview(): boolean {
    // We show the preview in case of bad definition cause it displays the error message
    return this.badDefinition || this.areAllRequiredParamsSpecified;
  }

  public get disableRefresh(): boolean {
    return this.badDefinition || !this.areAllRequiredParamsSpecified;
  }

  public get refresh(): Observable<void> {
    return this.refresh$;
  }

  public get export(): Observable<string> {
    return this.export$;
  }

  public onSkipChange(skip: number) {
    this.urlStateChanged();
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
    return Math.min(s.skip + ReportResultsComponent.DEFAULT_PAGE_SIZE, s.total);
  }

  get total(): number {
    return this.state.total;
  }

  get totalDisplay(): string {
    const total = this.total;
    if (total >= MAXIMUM_COUNT) {
      return formatNumber(MAXIMUM_COUNT - 1, 'en-GB') + '+';
    } else {
      return formatNumber(total, 'en-GB');
    }
  }

  onPreviousPage() {
    const s = this.state;
    s.skip = Math.max(s.skip - ReportResultsComponent.DEFAULT_PAGE_SIZE, 0);

    this.onSkipChange(s.skip); // to update the URL state
    this.refresh$.next();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + ReportResultsComponent.DEFAULT_PAGE_SIZE;

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
    return this.workspace.currentTenant.getMultilingualValueImmediate(this.definition, 'Description');
  }

  public get showEditDefinition(): boolean {
    return this.isScreenMode;
  }

  public get showDataDropdown(): boolean {
    return !!this.description || this.showEditDefinition;
  }

  public onEdit(): void {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../report-definitions', this.definitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(_ => delete ws.isEdit);
  }

  // Collapse parameters
  public get collapseParameters(): boolean {
    return this.state.collapseParams;
  }

  public set collapseParameters(v: boolean) {
    const s = this.state;
    if (s.collapseParams !== v) {
      s.collapseParams = v;
    }
  }

  // Hovering over a reconciled entry/external entry highlights all entries/external entries with yellow marker

  public onToggleCollapseParameters() {
    this.collapseParameters = !this.collapseParameters;
  }

}
