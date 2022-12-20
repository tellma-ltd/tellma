import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';
import { DocumentDefinition } from '~/app/data/entities/document-definition';

@Component({
  selector: 't-document-definitions-master',
  templateUrl: './document-definitions-master.component.html',
  styles: []
})
export class DocumentDefinitionsMasterComponent extends MasterBaseComponent {

  private documentDefinitionsApi = this.api.documentDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.documentDefinitionsApi = this.api.documentDefinitionsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.ws.DocumentDefinition;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  // State Update

  public onMakeHidden = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentDefinitionsApi.updateState(ids, { state: 'Hidden', returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public showMakeHidden = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const def = this.ws.get('DocumentDefinition', id) as DocumentDefinition;
      return !!def && def.State !== 'Hidden';
    });
  }

  public onMakeTesting = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentDefinitionsApi.updateState(ids, { state: 'Testing', returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public showMakeTesting = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const def = this.ws.get('DocumentDefinition', id) as DocumentDefinition;
      return !!def && def.State !== 'Testing';
    });
  }

  public onMakeVisible = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentDefinitionsApi.updateState(ids, { state: 'Visible', returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public showMakeVisible = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const def = this.ws.get('DocumentDefinition', id) as DocumentDefinition;
      return !!def && def.State !== 'Visible';
    });
  }

  public onMakeArchived = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentDefinitionsApi.updateState(ids, { state: 'Archived', returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public showMakeArchived = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const def = this.ws.get('DocumentDefinition', id) as DocumentDefinition;
      return !!def && def.State !== 'Archived';
    });
  }

  public hasStatePermission = (_: (number | string)[]) => this.ws.canDo('document-definitions', 'State', null);

  public stateTooltip = (ids: (number | string)[]) => this.hasStatePermission(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
