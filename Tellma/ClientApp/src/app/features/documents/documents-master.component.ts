// tslint:disable:member-ordering
import { Component, OnInit, Input, TemplateRef, ViewChild } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DocumentDefinitionForClient, DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { tap, catchError, finalize } from 'rxjs/operators';
import { addToWorkspace, printBlob } from '~/app/data/util';
import { Observable, of, Subscription } from 'rxjs';
import { Document } from '~/app/data/entities/document';
import { MasterComponent } from '~/app/shared/master/master.component';
import { PrintingTemplate } from './documents-details.component';
import { GenerateMarkupByFilterArguments } from '~/app/data/dto/generate-markup-arguments';
import { SettingsForClient } from '~/app/data/dto/settings-for-client';
import { Placement } from '@ng-bootstrap/ng-bootstrap';

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

  @ViewChild(MasterComponent, { static: false })
  master: MasterComponent;

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

  // Printing Stuff

  public get actionsDropdownPlacement(): Placement {
    return this.workspace.ws.isRtl ? 'bottom-right' : 'bottom-left';
  }

  public get showPrint(): boolean {
    return this.printingTemplates.length > 0;
  }

  private _printingTemplatesSettings: SettingsForClient;
  private _printingTemplatesDefinitions: DefinitionsForClient;
  private _printingTemplatesResult: PrintingTemplate[];

  public get printingTemplates(): PrintingTemplate[] {
    const ws = this.ws;
    if (this._printingTemplatesDefinitions !== ws.definitions ||
      this._printingTemplatesSettings !== ws.settings) {
      this._printingTemplatesDefinitions = ws.definitions;
      this._printingTemplatesSettings = ws.settings;
      const result: PrintingTemplate[] = [];

      const settings = ws.settings;
      const def = this.definition;
      for (const template of def.MarkupTemplates.filter(e => e.Usage === 'QueryByFilter')) {
        const langCount = (template.SupportsPrimaryLanguage ? 1 : 0)
          + (template.SupportsSecondaryLanguage && !!settings.SecondaryLanguageId ? 1 : 0)
          + (template.SupportsTernaryLanguage && !!settings.TernaryLanguageId ? 1 : 0);

        if (template.SupportsPrimaryLanguage) {
          const postfix = langCount > 1 ? ` (${settings.PrimaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.PrimaryLanguageId
          });
        }

        if (template.SupportsSecondaryLanguage && !!settings.SecondaryLanguageId) {
          const postfix = langCount > 1 ? ` (${settings.SecondaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.SecondaryLanguageId
          });
        }

        if (template.SupportsTernaryLanguage && !!settings.TernaryLanguageId) {
          const postfix = langCount > 1 ? ` (${settings.TernaryLanguageSymbol})` : ``;
          result.push({
            name: () => `${ws.getMultilingualValueImmediate(template, 'Name')}${postfix}`,
            templateId: template.MarkupTemplateId,
            culture: settings.TernaryLanguageId
          });
        }
      }

      this._printingTemplatesResult = result;
    }

    return this._printingTemplatesResult;
  }

  private printingSubscription: Subscription;

  public onPrint(template: PrintingTemplate) {
    const checkedIds = this.master.checkedIds;
    if (!!checkedIds && checkedIds.length > 0) {
      // Print

      // Cancel any existing printing query
      if (!!this.printingSubscription) {
        this.printingSubscription.unsubscribe();
      }

      const args: GenerateMarkupByFilterArguments = {
        i: checkedIds,
        culture: template.culture
      };

      // New printing query
      this.printingSubscription = this.documentsApi
        .printByFilter(template.templateId, args)
        .pipe(
          tap(blob => {
            this.printingSubscription = null;
            printBlob(blob);
          }),
          catchError(friendlyError => {
            this.printingSubscription = null;
            alert(friendlyError.error);
            return of();
          }),
          finalize(() => {
            this.printingSubscription = null;
          })
        ).subscribe();
    }
  }

  public get isPrinting(): boolean {
    return !!this.printingSubscription;
  }
}
