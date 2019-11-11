import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import {
  ResourceClassificationForSave, ResourceClassification, metadata_ResourceClassification
} from '~/app/data/entities/resource-classification';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 'b-resource-classifications-details',
  templateUrl: './resource-classifications-details.component.html'
})
export class ResourceClassificationsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private resourceClassificationsApi = this.api.resourceClassificationsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.resourceClassificationsApi = this.api.resourceClassificationsApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }

  public expand = 'Parent';

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

  get viewId(): string {
    return `resources/${this.definitionId}`;
  }

  create = () => {
    const result = new ResourceClassificationForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsLeaf = false;
    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: ResourceClassification): void => {
    if (!!model && !!model.Id) {
      this.resourceClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: ResourceClassification): void => {
    if (!!model && !!model.Id) {
      this.resourceClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: ResourceClassification) => !!model && !model.IsActive;
  public showDeactivate = (model: ResourceClassification) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: ResourceClassification) => this.ws.canDo(this.viewId, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: ResourceClassification) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const entityDesc = metadata_ResourceClassification(this.ws, this.translate, this.definitionId);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }
}
