import { Component, OnInit } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 't-markup-templates-master',
  templateUrl: './markup-templates-master.component.html',
  styles: []
})
export class MarkupTemplatesMasterComponent extends MasterBaseComponent {

  private markupTemplatesApi = this.api.markupTemplatesApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.markupTemplatesApi = this.api.markupTemplatesApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.MarkupTemplate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // public onActivate = (ids: (number | string)[]): Observable<any> => {
  //   const obs$ = this.markupTemplatesApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
  //     tap(res => addToWorkspace(res, this.workspace))
  //   );

  //   // The master template handles any errors
  //   return obs$;
  // }

  // public onDeactivate = (ids: (number | string)[]): Observable<any> => {
  //   const obs$ = this.markupTemplatesApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
  //     tap(res => addToWorkspace(res, this.workspace))
  //   );

  //   // The master template handles any errors
  //   return obs$;
  // }

  // public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('markup-templates', 'IsActive', null);

  // public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
  //   this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
