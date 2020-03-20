import { Component, OnInit, Input } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DocumentDefinitionForClient } from '~/app/data/dto/definitions-for-client';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { Observable } from 'rxjs';
import { Document } from '~/app/data/entities/document';

@Component({
  selector: 't-documents-master',
  templateUrl: './documents-master.component.html',
  styles: []
})
export class DocumentsMasterComponent extends MasterBaseComponent implements OnInit {

  private documentsApi = this.api.documentsApi('', this.notifyDestruct$); // for intellisense
  private _definitionId: string;

  // For caching
  private _filterDefinitionArg: any[];
  private _filterDefinitionDocDef: DocumentDefinitionForClient;
  private _filterDefinitionResult: any;

  @Input()
  public set definitionId(t: string) {
    if (this._definitionId !== t) {
      this.documentsApi = this.api.documentsApi(t, this.notifyDestruct$);
      this._definitionId = t;
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

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get view(): string {
    return `documents/${this.definitionId}`;
  }

  public get c() {
    return this.ws.Document;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get definition(): DocumentDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Documents[this.definitionId] : null;
  }

  public get found(): boolean {
    return !this.definitionId || !!this.definition;
  }

  public get masterCrumb(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural') :
      this.translate.instant('Documents');
  }

  public get summary(): string {
    return !!this.definition ?
      this.ws.getMultilingualValueImmediate(this.definition, 'TitleSingular') :
      this.translate.instant('Document');
  }

  public get defaultSelect(): string {
    // This shows the State column only when there is a workflow
    const def = this.definition;
    const stateColumn = !!def && def.CanReachState4 ? ',State' : '';
    return `PostingDate,PostingState${stateColumn},Memo`;
  }

  // Posting State

  public onPost = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.post(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onCancel = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.cancel(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onUnpost = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.unpost(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onUncancel = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.uncancel(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public showPost = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.PostingState === 0;
    });
  }

  public showCancel = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.PostingState === 0;
    });
  }

  public showUnpost = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.PostingState === 1;
    });
  }

  public showUncancel = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.PostingState === -1;
    });
  }

  public hasPostingStatePermission = (_: (number | string)[]) => this.ws.canDo(this.view, 'PostingState', null);

  public postingStateTooltip = (ids: (number | string)[]) => this.hasPostingStatePermission(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public adjustFilterDefinition(filterDefinition: any): any {
    const def = this.definition;
    if (this._filterDefinitionArg !== filterDefinition ||
      this._filterDefinitionDocDef !== def) {

      this._filterDefinitionArg = filterDefinition;
      this._filterDefinitionDocDef = def;

      const result = { ...filterDefinition };
      if (def.CanReachState4) {
        result.State = result.State.filter((e: any) => e.state === 0 || def['CanReachState' + Math.abs(e.state)]);
      } else {
        delete result.State;
      }

      this._filterDefinitionResult = result;
    }

    return this._filterDefinitionResult;
  }
}
