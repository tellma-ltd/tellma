import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ChoicePropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 't-contract-definitions-master',
  templateUrl: './contract-definitions-master.component.html',
  styles: []
})
export class ContractDefinitionsMasterComponent extends MasterBaseComponent {

  // private contractsDefinitionsApi = this.api.contractDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    // this.contractsDefinitionsApi = this.api.contractDefinitionsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.ContractDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // public onActivate = (ids: (number | string)[]): Observable<any> => {
  //   const obs$ = this.contractsDefinitionsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
  //     tap(res => addToWorkspace(res, this.workspace))
  //   );

  //   // The master template handles any errors
  //   return obs$;
  // }

  // public onDeactivate = (ids: (number | string)[]): Observable<any> => {
  //   const obs$ = this.contractsDefinitionsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
  //     tap(res => addToWorkspace(res, this.workspace))
  //   );

  //   // The master template handles any errors
  //   return obs$;
  // }

  // public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('contract-definitions', 'IsActive', null);

  // public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
  //   this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
