import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { ResourceLookup, ResourceLookupForSave } from '~/app/data/entities/resource-lookup';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 'b-resource-lookups-details',
  templateUrl: './resource-lookups-details.component.html',
  styles: []
})
export class ResourceLookupsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private resourceLookupsApi = this.api.resourceLookupsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.resourceLookupsApi = this.api.resourceLookupsApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.details.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (!definitionId || !this.workspace.current.definitions.ResourceLookups[definitionId]) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }
  // UI Binding

  create = () => {
    const result = new ResourceLookupForSave();
    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: ResourceLookup): void => {
    if (!!model && !!model.Id) {
      this.resourceLookupsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: ResourceLookup): void => {
    if (!!model && !!model.Id) {
      this.resourceLookupsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: ResourceLookup) => !!model && !model.IsActive;
  public showDeactivate = (model: ResourceLookup) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: ResourceLookup) => this.ws.canDo(this.definitionId, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: ResourceLookup) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.ResourceLookups[definitionId];
    if (!definition) {
      this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }
}
