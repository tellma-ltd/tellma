import { CdkDragDrop, CdkDragMove, CdkDragRelease, CdkDragStart } from '@angular/cdk/drag-drop';
import { DOCUMENT } from '@angular/common';
import {
  AfterViewInit,
  ApplicationRef, Component, ElementRef, EventEmitter, Input, NgZone, OnChanges,
  OnDestroy, OnInit, Output, SimpleChanges, ViewChild
} from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import { concat, interval, Observable, Subject, Subscription, timer } from 'rxjs';
import { concatAll, first, tap } from 'rxjs/operators';
import {
  DashboardDefinitionForClient,
  DashboardDefinitionWidgetForClient,
  ReportDefinitionForClient
} from '~/app/data/dto/definitions-for-client';
import { FriendlyError, isSpecified } from '~/app/data/util';
import { ReportStatus, ReportStore, WorkspaceService } from '~/app/data/workspace.service';
import { ReportView } from '../report-results/report-results.component';
import { cleanupWidgetPreviews, maxOffset, maxSize, overlapY, rearrange, tileHeight, tileWidth } from './dashboard-util';

@Component({
  selector: 't-dashboard',
  templateUrl: './dashboard.component.html',
  styles: []
})
export class DashboardComponent implements OnInit, OnChanges, AfterViewInit, OnDestroy {

  private _subscriptions: Subscription;
  private definitionId: number;

  public rendered = false;

  @Input()
  mode: 'screen' | 'preview' = 'screen';

  @Input()
  disableDrilldown = false;

  @Input()
  isEdit = false; // TODO switch to false

  @Input()
  previewDefinition: DashboardDefinitionForClient; // Used in preview mode

  @ViewChild('dashboard', { static: false })
  dashboard: ElementRef<HTMLDivElement>;

  @Output()
  public addWidget = new EventEmitter<{ offsetX: number, offsetY: number }>();

  @Output()
  public editWidget = new EventEmitter<{ index: number }>();

  @Output()
  public deleteWidget = new EventEmitter<{ index: number }>();

  @Output()
  public moveWidget = new EventEmitter<{ index: number, offsetX: number, offsetY: number }>();

  @Output()
  public resizeWidget = new EventEmitter<{ index: number, width: number, height: number }>();

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService, private appRef: ApplicationRef,
    private router: Router, private route: ActivatedRoute, public modalService: NgbModal, private zone: NgZone) {
  }

  ngOnInit(): void {
    const wss = this.workspace;
    // Pick up state from the URL
    this._subscriptions = new Subscription();
    this._subscriptions.add(this.route.paramMap.subscribe((params: ParamMap) => {

      // This triggers changes on the screen
      if (this.isScreenMode) {

        const definitionId = +params.get('definitionId');

        if (!definitionId || !wss.currentTenant.definitions.Dashboards[definitionId]) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    }));

    this._subscriptions.add(wss.visibilityChanged$.subscribe(_ => {
      if (wss.visibility === 'visible') {
        this.startTimers();
      } else {
        this.stopTimers();
      }
    }));

    this.rendered = false;
    if (!!this.definition.Widgets && this.definition.Widgets.length <= 4) {
      this.render();
    }
  }

  ngAfterViewInit() {
    // Our famous trick, ensuring that the navigation to the dashboard screen
    // is lightening fast, and then rendering occurs asynchronously
    if (!this.rendered) {
      timer(1).subscribe(() => this.render());
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!!changes.mode && !changes.mode.isFirstChange()) {
      this.resetTimers();
    }
    if (!!changes.isEdit && !changes.isEdit.isFirstChange()) {
      this.resetTimers();
    }
  }
  render() {
    this.rendered = true;
  }

  ngOnDestroy(): void {
    this.stopTimers();
    if (this._subscriptions) {
      this._subscriptions.unsubscribe();
    }
  }

  get isScreenMode(): boolean {
    return this.mode === 'screen';
  }

  get isPreviewMode(): boolean {
    return this.mode === 'preview';
  }

  get definition(): DashboardDefinitionForClient {
    return this.isScreenMode ?
      this.workspace.currentTenant.definitions.Dashboards[this.definitionId] :
      this.previewDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  ///////////////// UI Bindings

  get title(): string {
    const title = this.ws.getMultilingualValueImmediate(this.definition, 'Title');
    return title;
  }

  public onFullScreen(): void {
    if (!!this.dashboard.nativeElement.requestFullscreen) {
      this.dashboard.nativeElement.requestFullscreen();
    }
  }

  public get canFullScreen(): boolean {
    return true; // TODO
  }

  public onRefresh(): void {
    if (!!this._refreshSubjects) {
      this._refreshSubjects.forEach(r => r.next());
      this.resetTimers();
    }
  }

  public get disableRefresh(): boolean {
    return false; // TODO
  }

  public get showEditDefinition(): boolean {
    return this.isScreenMode && this.ws.canDo('dashboard-definitions', 'Update', null);
  }

  public get showActionsDropdown(): boolean {
    return this.showEditDefinition;
  }

  public onEdit(): void {
    const wss = this.workspace;
    wss.isEdit = true;
    this.router.navigate(['../../dashboard-definitions', this.definitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete wss.isEdit;
        }
      })
      .catch(_ => delete wss.isEdit);
  }

  public get actionsDropdownPlacement() {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  ///////////////// Report Results

  public renderWidget(widget: DashboardDefinitionWidgetForClient): boolean {
    return !!widget && !!widget.ReportDefinitionId && widget.OffsetX >= 0 && widget.OffsetY >= 0 && widget.Width > 0 && widget.Height > 0;
  }

  private horizontal(widget: DashboardDefinitionWidgetForClient): number {
    return widget.OffsetX * tileWidth;
  }

  public left(widget: DashboardDefinitionWidgetForClient): number {
    return this.workspace.ws.isRtl ? null : this.horizontal(widget);
  }

  public right(widget: DashboardDefinitionWidgetForClient): number {
    return this.workspace.ws.isRtl ? this.horizontal(widget) : null;
  }

  public top(widget: DashboardDefinitionWidgetForClient): number {
    const offsetY = widget.OffsetY + (widget.changeY || 0);
    return offsetY * tileHeight;
  }

  public width(widget: DashboardDefinitionWidgetForClient): number {
    return (widget.Width + (widget.changeW || 0))  * tileWidth;
  }

  public height(widget: DashboardDefinitionWidgetForClient): number {
    return (widget.Height + (widget.changeH || 0)) * tileHeight;
  }

  public reportDefinition(e: DashboardDefinitionWidgetForClient): ReportDefinitionForClient {
    return this.ws.definitions.Reports[e.ReportDefinitionId];
  }

  public state(e: DashboardDefinitionWidgetForClient): ReportStore {
    // Use the same key used by the report screen
    // This way opening the report will be immediate
    const stateKey = e.ReportDefinitionId.toString();

    const rs = this.workspace.currentTenant.reportState;
    if (!rs[stateKey]) {
      rs[stateKey] = new ReportStore();
    }

    return rs[stateKey];
  }

  public reportView(e: DashboardDefinitionWidgetForClient): ReportView {
    const reportDef = this.reportDefinition(e);
    return !!reportDef && !!reportDef.Chart && reportDef.DefaultsToChart ? ReportView.chart : ReportView.pivot;
  }

  // tslint:disable:member-ordering
  private _defForRefreshes: DashboardDefinitionForClient;
  private _refreshSubjects: Subject<boolean>[] = [];
  private _autoRefreshSubscriptions: Subscription[] = [];

  public refreshSubject(index: number): Observable<boolean> {
    const def = this.definition;
    if (this._defForRefreshes !== def) {
      this._defForRefreshes = def;
      this._refreshSubjects = def.Widgets.map(_ => new Subject<boolean>());
      this.resetTimers();
    }

    return this._refreshSubjects[index];
  }

  private stopTimers() {
    this._autoRefreshSubscriptions.forEach(s => s.unsubscribe());
    this._autoRefreshSubscriptions = [];

    this._offlineRetrySubscriptions.forEach(s => {
      if (!!s) {
        s.unsubscribe();
      }
    });
    this._offlineRetrySubscriptions = [];
  }

  private startTimers(def?: DashboardDefinitionForClient) {
    if (this.workspace.visibility !== 'visible') {
      return; // Timers only work when the page is visible
    }
    if (this.isEdit) {
      return; // Timers do not work in edit mode
    }

    def = def || this.definition;
    // Schedule the auto-refresh for every widget
    for (let i = 0; i < def.Widgets.length; i++) {
      const widget = def.Widgets[i];
      const refresh$ = this._refreshSubjects[i];

      // Get the refresh period
      const refreshPeriod = (isSpecified(widget.AutoRefreshPeriodInMinutes) ?
        widget.AutoRefreshPeriodInMinutes : (def.AutoRefreshPeriodInMinutes || 0)) * 60 * 1000;

      if (refreshPeriod > 0) {
        // Get the refresh time
        const lastRefreshTimeString = this.state(widget).time;
        let nextRefreshIn: number; // How many milliseconds before we trigger the next refresh
        if (!!lastRefreshTimeString) {
          const lastRefreshTime = new Date(lastRefreshTimeString);
          nextRefreshIn = Math.min(Math.max(refreshPeriod - (new Date().getTime() - lastRefreshTime.getTime()), 0), refreshPeriod);
        } else {
          // No results were retrieved yet, the widget will automatically retrieve the results the first time, so assume it's now
          nextRefreshIn = refreshPeriod;
        }

        const random = Math.floor(Math.random() * 5000);
        const everyRefresh$ = timer(nextRefreshIn, refreshPeriod + random);
        this._autoRefreshSubscriptions.push(everyRefresh$
          .subscribe(_ => refresh$.next(true))); // Silent

        // const appIsStable$ = this.appRef.isStable.pipe(first(isStable => isStable === true));
        // const everyRefreshOnceAppIsStable$ = concat(appIsStable$, everyRefresh$);
        // this._autoRefreshSubscriptions.push(everyRefreshOnceAppIsStable$
        //   .subscribe(_ => this.zone.run(() => refresh$.next(true)))); // Silent
      }
    }
  }

  private resetTimers() {
    this.stopTimers();
    this.startTimers();
  }

  private _offlineRetrySubscriptions: Subscription[] = [];

  public onSilentRefreshError(error: FriendlyError, widget: DashboardDefinitionWidgetForClient, index: number) {
    // Offline error
    if (error.status === 0 || error.status === 504) {
      // Keep trying every 5 seconds
      const refreshSubject$ = this._refreshSubjects[index];
      if (!!refreshSubject$) {
        const existing = this._offlineRetrySubscriptions[index];
        if (!!existing) {
          existing.unsubscribe(); // Just in case
        }
        // If offline error keep retrying every 30 seconds as long as the error remains
        this._offlineRetrySubscriptions[index] = timer(30000).subscribe(() => {
          const s = this.state(widget);
          if (s.silentError) {
            refreshSubject$.next(true);
          }
        });
      }
    }
  }

  public showErrorIcon(widget: DashboardDefinitionWidgetForClient): boolean {
    return this.state(widget).silentError;
  }

  public errorMessage(widget: DashboardDefinitionWidgetForClient): string {
    return this.state(widget).errorMessage;
  }

  public get popoverPlacement(): string {
    return this.workspace.ws.isRtl ? 'bottom-left' : 'bottom-right';
  }

  public onRefreshWidget(index: number) {
    this.refreshSubject(index); // To update the _refreshSubjects array
    const refreshSubject$ = this._refreshSubjects[index];
    refreshSubject$.next();
  }

  public onExpandWidget(widget: DashboardDefinitionWidgetForClient) {
    this.router.navigate(['../../report', widget.ReportDefinitionId], { relativeTo: this.route });
  }

  public onExportWidget(index: number) {
    this.exportSubject(index); // To update the _exportSubjects array
    const exporrtSubject$ = this._exportSubjects[index];
    exporrtSubject$.next();
  }

  public get showEditReportDefinition(): boolean {
    return this.workspace.currentTenant.canUpdate('report-definitions', undefined);
  }

  public onEditReportDefinition(widget: DashboardDefinitionWidgetForClient) {
    const ws = this.workspace;
    ws.isEdit = true;
    this.router.navigate(['../../report-definitions', widget.ReportDefinitionId], { relativeTo: this.route })
      .then(success => {
        if (!success) {
          delete ws.isEdit;
        }
      })
      .catch(() => delete ws.isEdit);
  }

  // tslint:disable:member-ordering
  private _defForExports: DashboardDefinitionForClient;
  private _exportSubjects: Subject<void>[] = [];

  public exportSubject(index: number): Observable<void> {
    const def = this.definition;
    if (this._defForExports !== def) {
      this._defForExports = def;
      this._exportSubjects = def.Widgets.map(_ => new Subject<void>());
    }

    return this._exportSubjects[index];
  }

  public onDoubleClickSurface(e: MouseEvent) {
    if (this.isEdit && !this.isVerticalMode) {
      e.preventDefault(); // To prevent the surface from catching it
      e.stopPropagation(); // To prevent the surface from catching it

      // First calculate the clicked X and Y
      const dashboardDiv = this.dashboard.nativeElement;
      const rect = dashboardDiv.getBoundingClientRect();
      const offsetX = Math.max(Math.floor((e.clientX + dashboardDiv.scrollLeft - rect.left - 5) / tileWidth), 0); // 5 = outer margin
      const offsetY = Math.max(Math.floor((e.clientY + dashboardDiv.scrollTop - rect.top - 5) / tileHeight), 0);

      this.addWidget.emit({ offsetX, offsetY });
    }
  }

  public onDoubleClickWidget(e: MouseEvent, index: number) {
    if (this.isEdit && !this.isVerticalMode) {
      e.preventDefault(); // To prevent the surface from catching it
      e.stopPropagation(); // To prevent the surface from catching it

      this.onEditWidget(index);
    }
  }

  public onEditWidget(index: number) {
    if (this.isEdit) {
      this.editWidget.emit({ index });
    }
  }

  public onDeleteWidget(index: number) {
    if (this.isEdit) {
      this.deleteWidget.emit({ index });
    }
  }

  public onDragStarted(e: CdkDragStart, widget: DashboardDefinitionWidgetForClient) {
    // Set a very high z-index so it's visible over all the other widgets
    const draggedDiv = e.source.getRootElement();
    draggedDiv.style.setProperty('z-index', '10000');

    // Show the placeholder at the same place and size as the dragged widget
    this.showPlaceholder = true;
    this._placeholderOffsetX = widget.OffsetX;
    this._placeholderOffsetY = widget.OffsetY;
    this._placeholderWidth = widget.Width;
    this._placeholderHeight = widget.Height;
  }

  public onDragMoved(e: CdkDragMove<any>, widget: DashboardDefinitionWidgetForClient) {
    const rtl = this.workspace.ws.isRtl ? -1 : 1;

    // Adjust the placeholder position
    this._placeholderOffsetX = Math.min(Math.max(widget.OffsetX + Math.round(rtl * e.distance.x / tileWidth), 0), maxOffset);
    this._placeholderOffsetY = Math.min(Math.max(widget.OffsetY + Math.round(e.distance.y / tileHeight), 0), maxOffset);

    // Rearrange the other tiles
    if (this._placeholderOffsetX === widget.OffsetX && this._placeholderOffsetY === widget.OffsetY) {
      cleanupWidgetPreviews(this.definition.Widgets);
    } else {
      const clone = { ...widget } as DashboardDefinitionWidgetForClient;
      clone.OffsetX = this._placeholderOffsetX;
      clone.OffsetY = this._placeholderOffsetY;

      rearrange(clone, this.definition.Widgets.filter(w => w !== widget));
    }
  }

  public onDragEnded(e: CdkDragRelease, index: number) {
    // Clear the drag translation transform
    e.source.reset();

    // Clear the z-index specified on DragStarted
    const draggedDiv = e.source.getRootElement();
    draggedDiv.style.removeProperty('z-index');

    // Remove the placeholder
    this.showPlaceholder = false;

    // // Return everything
    // cleanupWidgetPreviews(this.definition.Widgets);

    this.moveWidget.emit({ index, offsetX: this._placeholderOffsetX, offsetY: this._placeholderOffsetY });
  }

  public showPlaceholder = false;
  public _placeholderOffsetX: number;
  public _placeholderOffsetY: number;
  public _placeholderWidth: number;
  public _placeholderHeight: number;

  public get placeholderLeft(): number {
    return this.workspace.ws.isRtl ? null : this._placeholderOffsetX * tileWidth;
  }

  public get placeholderRight(): number {
    return this.workspace.ws.isRtl ? this._placeholderOffsetX * tileWidth : null;
  }

  public get placeholderTop(): number {
    return this._placeholderOffsetY * tileHeight;
  }

  public get placeholderWidth(): number {
    return this._placeholderWidth * tileWidth;
  }

  public get placeholderHeight(): number {
    return this._placeholderHeight * tileHeight;
  }

  public trackByReportId(i: number, widget: DashboardDefinitionWidgetForClient) {
    return widget.ReportDefinitionId || widget;
  }

  // Resize Handle
  public resizeHandleSize = 16;
  public widgetPadding = 5;

  public topResizeHandle(widget: DashboardDefinitionWidgetForClient): number {
    return this.top(widget) + widget.Height  * tileHeight - this.resizeHandleSize - this.widgetPadding;
  }

  private horizontalResizeHandle(widget: DashboardDefinitionWidgetForClient): number {
    return this.horizontal(widget) + widget.Width  * tileWidth - this.resizeHandleSize - this.widgetPadding;
  }

  public leftResizeHandle(widget: DashboardDefinitionWidgetForClient): number {
    return this.workspace.ws.isRtl ? 0 : this.horizontalResizeHandle(widget);
  }

  public rightResizeHandle(widget: DashboardDefinitionWidgetForClient): number {
    return this.workspace.ws.isRtl ? this.horizontalResizeHandle(widget) : 0;
  }

  public onResizeStarted(e: CdkDragStart, widget: DashboardDefinitionWidgetForClient) {
    // Set a very high z-index so it's over everything else
    const draggedDiv = e.source.getRootElement();
    draggedDiv.style.setProperty('z-index', '10000');

    // Show the placeholder at the same place and size as the dragged widget
    this.showPlaceholder = true;
    this._placeholderOffsetX = widget.OffsetX;
    this._placeholderOffsetY = widget.OffsetY;
    this._placeholderWidth = widget.Width;
    this._placeholderHeight = widget.Height;
  }

  public onResizeMoved(e: CdkDragMove<any>, widget: DashboardDefinitionWidgetForClient) {
    const rtl = this.workspace.ws.isRtl ? -1 : 1;
    widget.changeW = Math.max(rtl * e.distance.x / tileWidth, 1 - widget.Width - 0.25);
    widget.changeH = Math.max(e.distance.y / tileHeight, 1 - widget.Height - 0.25);

    this._placeholderWidth = Math.min(widget.Width + Math.round(0.5 + widget.changeW - (10 / tileWidth)), maxSize);
    this._placeholderHeight = Math.min(widget.Height + Math.round(0.5 + widget.changeH - (10 / tileHeight)), maxSize);

    // Rearrange the other tiles
    if (this._placeholderWidth === widget.Width && this._placeholderHeight === widget.Height) {
      cleanupWidgetPreviews(this.definition.Widgets);
    } else {
      const clone = { ...widget } as DashboardDefinitionWidgetForClient;
      clone.Width = this._placeholderWidth;
      clone.Height = this._placeholderHeight;

      rearrange(clone, this.definition.Widgets.filter(w => w !== widget));
    }
  }

  public onResizeEnded(e: CdkDragRelease, widget: DashboardDefinitionWidgetForClient, index: number) {
    // Clear the drag translation transform
    e.source.reset();

    // Clear the z-index specified on ResizeStarted
    const draggedDiv = e.source.getRootElement();
    draggedDiv.style.removeProperty('z-index');

    delete widget.changeH;
    delete widget.changeW;

    // Remove the placeholder
    this.showPlaceholder = false;

    // Signal the dashboard
    this.resizeWidget.emit({ index, width: this._placeholderWidth, height: this._placeholderHeight });
  }

  public isPivot(widget: DashboardDefinitionWidgetForClient): boolean {
    const reportDef = this.ws.definitions.Reports[widget.ReportDefinitionId];
    return !!reportDef && reportDef.Type === 'Summary' && (!reportDef.Chart || !reportDef.DefaultsToChart);
  }

  get isMdScreen(): boolean {
    return this.workspace.mediumDevice;
  }

  get isVerticalMode(): boolean {
    return !this.workspace.mediumDevice;
  }
}
