// tslint:disable:member-ordering
import { formatNumber } from '@angular/common';
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Subject, Subscription } from 'rxjs';
import { ReportDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { QueryexUtil } from '~/app/data/queryex-util';
import { MasterStatus, MAXIMUM_COUNT, ReportStore, WorkspaceService } from '~/app/data/workspace.service';
import { ReportResultsComponent } from '../report-results/report-results.component';

@Component({
  selector: 't-drilldown',
  templateUrl: './drilldown.component.html',
  styles: []
})
export class DrilldownComponent implements OnInit, OnDestroy {

  private _subscriptions: Subscription;
  private refresh$ = new Subject<void>();
  private export$ = new Subject<string>();
  private definitionId: number;
  private badDefinition: boolean;

  private filter: string;
  private cacheBuster: string;

  @ViewChild('errorModal', { static: true })
  public errorModal: TemplateRef<any>;

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute, public modalService: NgbModal) { }

  ngOnInit(): void {
    // Pick up state from the URL
    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.subscribe((params: ParamMap) => {

      const definitionId = +params.get('definitionId');
      if (!definitionId || !this.workspace.currentTenant.definitions.Reports[definitionId]) {
        this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
      }

      this.definitionId = definitionId;
      this.filter = params.get('filter');
      this.cacheBuster = params.get('cache_buster');
    }));
  }

  ngOnDestroy() {
    this._subscriptions.unsubscribe();
  }

  private get originalDefinition(): ReportDefinitionForClient {
    return this.workspace.currentTenant.definitions.Reports[this.definitionId];
  }

  private urlStateChanged(): void {
    const params: Params = {};
    const s = this.state;

    if (!!this.cacheBuster) {
      params.cache_buster = this.cacheBuster;
    }

    if (!!this.filter) {
      params.filter = this.filter;
    }

    if (!!s.orderbyKey) {
      params.$orderby = `${s.orderbyKey} ${s.orderbyDir}`;
    }

    this.router.navigate(['.', params], { relativeTo: this.route, replaceUrl: true });
  }

  // UI Bindings

  public get modifiedDefinition(): ReportDefinitionForClient {

    const originalDef = this.originalDefinition;
    const s = this.state;
    if (this.filter !== s.drilldownFilter || this.cacheBuster !== s.drilldownCacheBuster || originalDef !== s.drilldownOriginalDefinition) {
      s.drilldownFilter = this.filter;
      s.drilldownCacheBuster = this.cacheBuster;
      s.drilldownOriginalDefinition = originalDef;

      const modified = JSON.parse(JSON.stringify(originalDef)) as ReportDefinitionForClient;
      modified.Type = 'Details';
      modified.Filter = this.filter;
      s.drilldownModifiedDefinition = modified;
    }

    return this.state.drilldownModifiedDefinition;
  }

  private get stateKey(): string {
    return `${this.definitionId}/drilldown`;
  }

  public get state(): ReportStore {

    const key = this.stateKey;
    const rs = this.workspace.currentTenant.reportState;
    if (!rs[key]) {
      rs[key] = new ReportStore();
    }

    return rs[key];
  }

  public get flip() {

    // this is to flip the UI icons in RTL
    return this.workspace.ws.isRtl ? 'horizontal' : null;
  }

  get title(): string {
    const title = this.workspace.currentTenant.getMultilingualValueImmediate(this.originalDefinition, 'Title');
    const drilldown = this.translate.instant('Drilldown');
    return `${title} - ${drilldown}`;
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

  public get canExport(): boolean {
    return true;
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

  public get disableRefresh(): boolean {
    return this.badDefinition;
  }

  public onRefresh() {
    this.refresh$.next();
  }

  public get skip(): number {
    return this.state.skip;
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
    this.refresh$.next();
  }

  get canPreviousPage(): boolean {
    return this.state.skip > 0;
  }

  onNextPage() {
    const s = this.state;
    s.skip = s.skip + ReportResultsComponent.DEFAULT_PAGE_SIZE;
    this.refresh$.next();
  }

  get canNextPage(): boolean {
    return this.to < this.total;
  }

  public get refresh(): Observable<void> {
    return this.refresh$;
  }

  public get export(): Observable<string> {
    return this.export$;
  }

  public onOrderByChange() {
    this.urlStateChanged();
  }
}
