import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { ResourceForSave, Resource, metadata_Resource } from '~/app/data/entities/resource';
import { PropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 'b-resources-details',
  templateUrl: './resources-details.component.html',
  styles: []
})
export class ResourcesDetailsComponent extends DetailsBaseComponent implements OnInit {

  private resourcesApi = this.api.resourcesApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.resourcesApi = this.api.resourcesApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = `Currency,MassUnit,VolumeUnit,AreaUnit,LengthUnit,TimeUnit,
CountUnit,ResourceLookup1,ResourceLookup2,ResourceLookup3,ResourceLookup4`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService,
    private router: Router, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = params.get('definitionId');

        if (!definitionId || !this.workspace.current.definitions.Resources[definitionId]) {
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
    const result = new ResourceForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }
    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public get p(): { [prop: string]: PropDescriptor } {
    return metadata_Resource(this.ws, this.translate, this.definitionId).properties;
  }

  public onActivate = (model: Resource): void => {
    if (!!model && !!model.Id) {
      this.resourcesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Resource): void => {
    if (!!model && !!model.Id) {
      this.resourcesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Resource) => !!model && !model.IsActive;
  public showDeactivate = (model: Resource) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Resource) => this.ws.canDo(this.definitionId, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Resource) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.Resources[definitionId];
    if (!definition) {
      this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }
}
