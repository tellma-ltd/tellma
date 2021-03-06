// tslint:disable:member-ordering
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

  private documentsApi = this.api.documentsApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  // caching
  private _selectDefaultDefinition: DocumentDefinitionForClient;
  private _selectDefaultResult: string;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.documentsApi = this.api.documentsApi(t, this.notifyDestruct$);
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
        this.definitionId = +params.get('definitionId');
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

  // State

  public onClose = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.close(ids, { returnEntities: true }).pipe(
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

  public onOpen = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.documentsApi.open(ids, { returnEntities: true }).pipe(
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

  public showClose = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === 0;
    });
  }

  public showCancel = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === 0;
    });
  }

  public showOpen = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === 1;
    });
  }

  public showUncancel = (ids: (number | string)[]): boolean => {
    return ids.some(id => {
      const doc = this.ws.get('Document', id) as Document;
      return !!doc && doc.State === -1;
    });
  }

  public hasStatePermission = (_: (number | string)[]) => this.ws.canDo(this.view, 'State', null);

  public stateTooltip = (ids: (number | string)[]) => this.hasStatePermission(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get selectDefault(): string {
    const def = this.definition;
    if (this._selectDefaultDefinition !== def) {
      this._selectDefaultDefinition = def;
      let result = 'State,Assignee,AssignedAt';
      if (!!def && !!def.PostingDateVisibility) {
        result = 'PostingDate,' + result;
      }
      if (!!def && !!def.MemoVisibility) {
        result += ',Memo';
      }

      this._selectDefaultResult = result;
    }

    return this._selectDefaultResult;
  }

  public get showDocumentPostingDate(): boolean {
    const def = this.definition;
    return !def || !!def.PostingDateVisibility;
  }

  public get showDocumentMemo(): boolean {
    const def = this.definition;
    return !def || !!def.MemoVisibility;
  }
}
