import { Component, OnInit, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { metadata_Agent } from '~/app/data/entities/agent';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 'b-agents-master',
  templateUrl: './agents-master.component.html'
})
export class AgentsMasterComponent extends MasterBaseComponent implements OnInit {

  private agentsApi = this.api.agentsApi('', this.notifyDestruct$); // for intellisense

  private _definitionId: string;

  @Input()
  filterDefault: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.agentsApi = this.api.agentsApi(t, this.notifyDestruct$);
    }
  }

  public get definitionId(): string {
    return this._definitionId;
  }


  public tableColumnPaths: string[];
  public filterDefinition: any;
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

        if (!definitionId || !this.workspace.current.definitions.Agents[definitionId]) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get viewId(): string {
    return `agents/${this.definitionId}`;
  }

  public get c() {
    return this.workspace.current.Agent;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.agentsApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.viewId, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.Agents[definitionId];
    if (!definition) {
      return '???';
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }

  public get summary(): string {
    const definitionId = this.definitionId;
    const definition = this.workspace.current.definitions.Agents[definitionId];
    if (!definition) {
      return '???';
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitleSingular');
  }
}
