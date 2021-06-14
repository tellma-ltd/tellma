import { Component, OnInit, Input } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { ResourceDefinitionForClient } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 't-resources-master',
  templateUrl: './resources-master.component.html',
  styles: []
})
export class ResourcesMasterComponent extends MasterBaseComponent implements OnInit {

  private resourcesApi = this.api.resourcesApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.resourcesApi = this.api.resourcesApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): number {
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

        const definitionId = +params.get('definitionId');

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get view(): string {
    return `resources/${this.definitionId}`;
  }

  public get c() {
    return this.ws.Resource;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get definition(): ResourceDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Resources[this.definitionId] : null;
  }

  public get found(): boolean {
    return !this.definitionId || !!this.definition;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.resourcesApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.resourcesApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.view, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural') :
      this.translate.instant('Resources');
  }

  public get summary(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitleSingular') :
      this.translate.instant('Resource');
  }

  public get Image_isVisible(): boolean {
    return !!this.definition.ImageVisibility;
  }
}
