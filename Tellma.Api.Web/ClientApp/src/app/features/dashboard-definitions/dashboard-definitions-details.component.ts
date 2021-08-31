// tslint:disable:member-ordering
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, TemplateRef, ViewChild } from '@angular/core';
import { NgControl } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateService } from '@ngx-translate/core';
import {
  DashboardDefinitionForClient,
  DashboardDefinitionWidgetForClient,
  ReportDefinitionForClient
} from '~/app/data/dto/definitions-for-client';
import { ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import {
  DashboardDefinition,
  DashboardDefinitionForSave,
  DashboardDefinitionRoleForSave,
  DashboardDefinitionWidget,
  DashboardDefinitionWidgetForSave
} from '~/app/data/entities/dashboard-definition';
import { metadata_ReportDefinition } from '~/app/data/entities/report-definition';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { areServerErrors, highlightInvalid, validationErrors } from '~/app/shared/form-group-base/form-group-base.component';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { defaultHeight, defaultWidth, overlapY, rearrange } from '../dashboard/dashboard-util';


@Component({
  selector: 't-dashboard-definitions-details',
  templateUrl: './dashboard-definitions-details.component.html',
  styles: []
})
export class DashboardDefinitionsDetailsComponent extends DetailsBaseComponent {

  public expand = 'Widgets.ReportDefinition,Roles.Role';

  @ViewChild('widgetConfigModal', { static: true })
  widgetConfigModal: TemplateRef<any>;

  create = () => {
    const result: DashboardDefinitionForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Title = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Title2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Title3 = this.initialText;
    }

    result.AutoRefreshPeriodInMinutes = 5; // Default 5 minutes
    result.Widgets = [];
    result.Roles = [];

    return result;
  }

  clone: (item: DashboardDefinition) => DashboardDefinition = (item: DashboardDefinition) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as DashboardDefinition;
      delete clone.Id;

      if (!!clone.Widgets) {
        clone.Widgets.forEach(e => delete e.Id);
      }

      if (!!clone.Roles) {
        clone.Roles.forEach(e => delete e.Id);
      }
      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService,
    private modalService: NgbModal) {
    super();
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public isInactive: (model: DashboardDefinition) => string = (_: DashboardDefinition) => null;

  public onDefinitionChange(model: DashboardDefinition, prop?: string) {

    if (!!prop) {
      // Non-critical change, no need to refresh
      this.getForClient(model)[prop] = model[prop];
    } else {
      // Critical change: trigger a refresh
      this._currentModelModified = true;
    }
  }

  /**
   * This contains a fresh mapping of the model since the last time a critical change was made
   */
  public modelForClient: DashboardDefinitionForClient;

  /**
   * The last model that was copied into immutable model
   */
  private _currentModel: DashboardDefinition;

  /**
   * Set to true when the model changes in a way that requires refreshing the report-results.component screen
   */
  private _currentModelModified = false;

  public getForClient(model: DashboardDefinition): DashboardDefinitionForClient {
    if (!model) {
      return null;
    }

    if (this._currentModel !== model || this._currentModelModified) {
      this._currentModelModified = false;
      this._currentModel = model;

      // The mapping is trivial since the two data structures are identical
      this.modelForClient = { ...model } as DashboardDefinitionForClient;
      if (model.Widgets) {
        this.modelForClient.Widgets = model.Widgets.map(this.map);
        this.modelForClient.Widgets.forEach(w => {
          w.OffsetX = Math.min(w.OffsetX, 1000);
          w.OffsetY = Math.min(w.OffsetY, 1000);
          w.Width = Math.min(w.Width, 16);
          w.Height = Math.min(w.Height, 16);
        });
      }
    }

    return this.modelForClient;
  }

  private _isEdit = false;

  public watchIsEdit(isEdit: boolean): boolean {
    // this is a hack to trigger window resize when isEdit changes
    if (this._isEdit !== isEdit) {
      this._isEdit = isEdit;
      window.dispatchEvent(new Event('resize')); // So charts would resize
    }

    return true;
  }

  //////////////////////////// Definition Editor and Sections

  public collapseDefinition = false;
  private _sections: { [key: string]: boolean } = {
    Title: false,
    Widgets: true,
    MainMenu: false
  };

  public onToggleSection(key: string): void {
    this._sections[key] = !this._sections[key];
  }

  showSection(key: string): boolean {
    return this._sections[key];
  }

  public onToggleDefinition(): void {
    this.collapseDefinition = !this.collapseDefinition;
    window.dispatchEvent(new Event('resize')); // So the chart would resize
  }

  public flipIcon(isExpanded: boolean): string {
    return this.workspace.ws.isRtl && !isExpanded ? 'horizontal' : null;
  }

  public rotateIcon(isExpanded: boolean): number {
    return isExpanded ? 90 : 0;
  }

  //////////////////////////// Errors

  public invalid(control: NgControl, serverErrors: string[]): boolean {
    return highlightInvalid(control, serverErrors);
  }

  public errors(control: NgControl, serverErrors: string[]): (() => string)[] {
    return validationErrors(control, serverErrors, this.translate);
  }

  public weakEntityErrors(model: DashboardDefinitionWidgetForSave | DashboardDefinitionRoleForSave) {
    return !!model.serverErrors &&
      Object.keys(model.serverErrors).some(key => areServerErrors(model.serverErrors[key]));
  }

  public titleSectionErrors(model: DashboardDefinitionForSave) {
    return !!model.serverErrors && (areServerErrors(model.serverErrors.Id) ||
      areServerErrors(model.serverErrors.Code) ||
      areServerErrors(model.serverErrors.Title) ||
      areServerErrors(model.serverErrors.Title2) ||
      areServerErrors(model.serverErrors.Title3));
  }

  public widgetsSectionErrors(model: DashboardDefinitionForSave) {
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.AutoRefreshPeriodInMinutes))) ||
      (!!model.Widgets && model.Widgets.some(e => this.weakEntityErrors(e)));
  }

  public mainMenuSectionErrors(model: DashboardDefinitionForSave) {
    return (!!model.serverErrors && (
      areServerErrors(model.serverErrors.MainMenuSection) ||
      areServerErrors(model.serverErrors.MainMenuIcon) ||
      areServerErrors(model.serverErrors.MainMenuSortKey))) ||
      (!!model.Roles && model.Roles.some(e => this.weakEntityErrors(e)));
  }

  public savePreprocessing(model: DashboardDefinition) {

    if (!model.Roles || model.Roles.length === 0) {
      delete model.MainMenuIcon;
      delete model.MainMenuSection;
      delete model.MainMenuSortKey;
    }
  }

  /////////////////////////// Main Menu

  public get allMainMenuSections(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.workspace, this.translate).properties.MainMenuSection as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public get allMainMenuIcons(): SelectorChoice[] {
    const desc = metadata_ReportDefinition(this.workspace, this.translate).properties.MainMenuIcon as ChoicePropDescriptor;
    return getChoices(desc);
  }

  public onIconClick(model: DashboardDefinition, icon: SelectorChoice): void {
    model.MainMenuIcon = icon.value;
    this.onDefinitionChange(model, 'MainMenuSortKey');
  }

  //////////////////////////// Widgets

  modelRef: DashboardDefinition;
  widgetToEdit: DashboardDefinitionWidget;
  widgetToEditHasChanged = false;
  widgetShowAdvancedOptions = false;

  public getWidgets(model: DashboardDefinition): DashboardDefinitionWidget[] {
    model.Widgets = model.Widgets || [];
    return model.Widgets;
  }

  public onCreateWidget(model: DashboardDefinition, offsetX?: number, offsetY?: number) {
    this.onConfigureWidget(model.Widgets.length, model, offsetX, offsetY);
  }

  private chartReportDef(reportDef: ReportDefinitionForClient): boolean {
    return !!reportDef && reportDef.Type === 'Summary' && reportDef.Chart && reportDef.DefaultsToChart;
  }

  public onConfigureWidget(index: number, model: DashboardDefinitionForSave, offsetX?: number, offsetY?: number) {
    this.widgetToEditHasChanged = false;
    const original = model.Widgets[index];
    const edited = { ...original || this.createWidget(model, offsetX, offsetY) } as DashboardDefinitionWidgetForSave;
    this.widgetToEdit = edited;
    this.modelRef = model;

    this.modalService.open(this.widgetConfigModal, { windowClass: 't-dark-theme t-wider-modal' }).result.then(() => {
      if (this.widgetToEditHasChanged) {

        // The widget changed size, so we have to re-render the chart inside it
        const chartAdjustmentRequired = !!original &&
          (original.Width !== edited.Width ||
            original.Height !== edited.Height) &&
          !!original.ReportDefinitionId &&
          original.ReportDefinitionId === edited.ReportDefinitionId &&
          this.chartReportDef(this.ws.definitions.Reports[edited.ReportDefinitionId]);

        // The widget has moved or changed size, we must rearrange the other widgets to prevent overlap
        const rearrangeRequired = !original || // Created anew
          original.OffsetX !== edited.OffsetX ||
          original.OffsetY !== edited.OffsetY ||
          original.Width !== edited.Width ||
          original.Height !== edited.Height;

        model.Widgets[index] = edited;

        if (rearrangeRequired) {
          this.rearrange(edited, model);
        }

        if (chartAdjustmentRequired) {
          window.dispatchEvent(new Event('resize')); // So the chart would resize
        }

        this.onDefinitionChange(model);
      }
    }, (_: any) => { });
  }

  public onDeleteWidget(index: number, model: DashboardDefinitionForSave) {
    model.Widgets.splice(index, 1);
    this.onDefinitionChange(model);
  }

  public onMoveWidget(index: number, model: DashboardDefinitionForSave, offsetX: number, offsetY: number) {
    const widget = model.Widgets[index];
    widget.OffsetX = offsetX;
    widget.OffsetY = offsetY;

    this.rearrange(widget, model);
    this.onDefinitionChange(model);
  }

  public onResizeWidget(index: number, model: DashboardDefinitionForSave, width: number, height: number) {
    const widget = model.Widgets[index];
    widget.Width = width;
    widget.Height = height;

    if (this.chartReportDef(this.ws.definitions.Reports[widget.ReportDefinitionId])) {
      window.dispatchEvent(new Event('resize')); // So the chart would resize
    }

    this.rearrange(widget, model);
    this.onDefinitionChange(model);
  }

  private rearrange(widget: DashboardDefinitionWidgetForSave, model: DashboardDefinitionForSave) {
    // Rearranges the widgets in order to avoid overlaps
    const originalWidgets = model.Widgets.filter(w => w !== widget);
    const mappedWidgets = originalWidgets.map(this.map);
    const modifiedWidget = this.map(widget);

    rearrange(modifiedWidget, mappedWidgets);

    for (let i = 0; i < mappedWidgets.length; i++) {
      const mappedWidget = mappedWidgets[i];
      const originalWidget = originalWidgets[i];

      originalWidget.OffsetY += (mappedWidget.changeY || 0);
    }
  }

  private map = (w: DashboardDefinitionWidgetForSave): DashboardDefinitionWidgetForClient =>
    ({ ...w } as DashboardDefinitionWidgetForClient)

  private overlap = (x: number, y: number, w: number, h: number, widget: DashboardDefinitionWidgetForSave): boolean => {
    return overlapY(x, y, w, h, widget.OffsetX, widget.OffsetY, widget.Width, widget.Height) > 0;
  }

  private createWidget(model: DashboardDefinitionForSave, offsetX?: number, offsetY?: number): DashboardDefinitionWidgetForSave {

    const width = defaultWidth;
    const height = defaultHeight;

    // This first section calculates an appropriate default tile position for the new widget
    if (offsetX === undefined || offsetY === undefined) {
      offsetX = 0;
      offsetY = 0;
      let distance = 0;
      let found = false;
      while (true) {
        offsetX = distance;
        offsetY = 0;
        while (offsetY <= distance - 1) {
          if (!model.Widgets.some(widget => this.overlap(offsetX, offsetY, width, height, widget))) {
            // Empty spot found
            found = true;
            break;
          }

          offsetY++;
        }

        if (!found) {
          offsetX = 0;
          offsetY = distance;
          while (offsetX <= distance) {
            if (!model.Widgets.some(widget => this.overlap(offsetX, offsetY, width, height, widget))) {
              // Empty spot found
              found = true;
              break;
            }

            offsetX++;
          }
        }

        if (found) {
          break;
        }

        distance++;
      }
    }

    // This first section calculates an appropriate default tile position for the new widget
    return {
      OffsetX: offsetX,
      OffsetY: offsetY,
      Width: width,
      Height: height,
    };
  }

  public canApplyWidget(widget: DashboardDefinitionWidgetForSave): boolean {
    return !!widget.ReportDefinitionId;
  }

  public rowDrop(event: CdkDragDrop<any[]>, model: DashboardDefinitionForSave) {
    moveItemInArray(model.Widgets, event.previousIndex, event.currentIndex);
    if (event.previousIndex !== event.currentIndex) {
      this.onDefinitionChange(model);
    }
  }

  ////////////////// Roles

  public onDeleteRow(row: any, collection: any[]) {
    const index = collection.indexOf(row);
    if (index >= 0) {
      collection.splice(index, 1);
    }
  }

  public onInsertRow(collection: any[], create?: () => any) {
    const item = !!create ? create() : { Id: 0 };
    collection.push(item);
  }
}
