import { Component, OnInit, Input } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { AccountDefinitionForClient } from '~/app/data/dto/definitions-for-client';
@Component({
  selector: 'b-accounts-master',
  templateUrl: './accounts-master.component.html',
  styles: []
})
export class AccountsMasterComponent extends MasterBaseComponent implements OnInit {

  private accountsApi = this.api.accountsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this._definitionId = t;
      this.accountsApi = this.api.accountsApi(t, this.notifyDestruct$);
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

        if (!this.definition(definitionId)) {
          this.router.navigate(['page-not-found'], { relativeTo: this.route.parent, replaceUrl: true });
        }

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  public definition(definitionId: string): AccountDefinitionForClient {
    return this.workspace.current.definitions.Accounts[definitionId];
  }

  public get c() {
    return this.workspace.current.Account;
  }

  public get ws() {
    return this.workspace.current;
  }

  get viewId(): string {
    return `accounts/${this.definitionId}`;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountsApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeprecate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.accountsApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeprecateItem = (_: (number | string)[]) => this.ws.canDo(this.viewId, 'IsDeprecated', null);

  public activateDeprecateTooltip = (ids: (number | string)[]) => this.canActivateDeprecateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const definition = this.definition(this.definitionId);
    if (!definition) {
      return '???';
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitlePlural');
  }

  public get summary(): string {
    const definition = this.definition(this.definitionId);
    if (!definition) {
      return '???';
    }

    return this.ws.getMultilingualValueImmediate(definition, 'TitleSingular');
  }
}
