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
  selector: 't-lookup-definitions-master',
  templateUrl: './lookup-definitions-master.component.html',
  styles: []
})
export class LookupDefinitionsMasterComponent extends MasterBaseComponent {

  // private lookupsDefinitionsApi = this.api.lookupDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    // this.lookupsDefinitionsApi = this.api.lookupDefinitionsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.LookupDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // public onActivate = (ids: (number | string)[]): Observable<any> => {
  //   const obs$ = this.lookupsDefinitionsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
  //     tap(res => addToWorkspace(res, this.workspace))
  //   );

  //   // The master template handles any errors
  //   return obs$;
  // }

  // public onDeactivate = (ids: (number | string)[]): Observable<any> => {
  //   const obs$ = this.lookupsDefinitionsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
  //     tap(res => addToWorkspace(res, this.workspace))
  //   );

  //   // The master template handles any errors
  //   return obs$;
  // }

  // public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('lookup-definitions', 'IsActive', null);

  // public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
  //   this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
