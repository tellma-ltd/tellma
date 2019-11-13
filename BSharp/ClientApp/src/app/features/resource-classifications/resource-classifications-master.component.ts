import { Component, Input, OnInit } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';
import { ParamMap, ActivatedRoute, Router } from '@angular/router';
import { metadata_ResourceClassification } from '~/app/data/entities/resource-classification';

@Component({
  selector: 'b-resource-classifications-master',
  templateUrl: './resource-classifications-master.component.html'
})
export class ResourceClassificationsMasterComponent extends MasterBaseComponent implements OnInit {

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

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private router: Router,
    private route: ActivatedRoute, private translate: TranslateService) {
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

  public get c() {
    return this.workspace.current.ResourceClassification;
  }

  public get ws() {
    return this.workspace.current;
  }
  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.resourceClassificationsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.resourceClassificationsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.viewId, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const entityDesc = metadata_ResourceClassification(this.ws, this.translate, this.definitionId);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }
}
