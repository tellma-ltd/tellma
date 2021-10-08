import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, takeUntil, tap, map } from 'rxjs/operators';
import { ActivateArguments } from './dto/activate-arguments';
import { EntityForSave } from './entities/base/entity-for-save';
import { FactArguments, GetArguments } from './dto/get-arguments';
import { GetByIdArguments } from './dto/get-by-id-arguments';
import { GetResponse } from './dto/get-response';
import { EntitiesResponse } from './dto/entities-response';
import { Unit } from './entities/unit';
import { TemplateArguments } from './dto/template-arguments';
import { ImportArguments } from './dto/import-arguments';
import { ImportResult } from './dto/import-result';
import { ExportForImportArguments } from './dto/export-for-import-arguments';
import { GetByIdResponse } from './dto/get-by-id-response';
import { SaveArguments } from './dto/save-arguments';
import { appsettings } from './global-resolver.guard';
import { Agent } from './entities/agent';
import { Role } from './entities/role';
import { SettingsForClient } from './dto/settings-for-client';
import { Versioned } from './dto/versioned';
import { PermissionsForClient } from './dto/permissions-for-client';
import { SaveSettingsResponse } from './dto/save-settings-response';
import { UserSettingsForClient } from './dto/user-settings-for-client';
import { GlobalSettingsForClient } from './dto/global-settings';
import { GetEntityResponse } from './dto/get-entity-response';
import { DefinitionsForClient } from './dto/definitions-for-client';
import { Currency } from './entities/currency';
import { Lookup } from './entities/lookup';
import { Resource } from './entities/resource';
import { User } from './entities/user';
import { AccountClassification } from './entities/account-classification';
import { Account } from './entities/account';
import { GetChildrenArguments } from './dto/get-children-arguments';
import { GetAggregateArguments } from './dto/get-aggregate-arguments';
import { GetAggregateResponse } from './dto/get-aggregate-response';
import { Center } from './entities/center';
import { friendlify, isSpecified } from './util';
import { EntryType } from './entities/entry-type';
import { Document } from './entities/document';
import { SignArguments } from './dto/sign-arguments';
import { AssignArguments } from './dto/assign-arguments';
import { MyUserForSave } from './dto/my-user';
import { AccountType } from './entities/account-type';
import { AdminUser } from './entities/admin-user';
import { AdminSettingsForClient } from './dto/admin-settings-for-client';
import { AdminUserSettingsForClient } from './dto/admin-user-settings-for-client';
import { MyAdminUserForSave } from './dto/my-admin-user';
import { AdminPermissionsForClient } from './dto/admin-permissions-for-client';
import { CompaniesForClient } from './dto/companies-for-client';
import { IdentityServerUser } from './entities/identity-server-user';
import { ResetPasswordArgs } from './dto/reset-password-args';
import { ActionArguments } from './dto/action-arguments';
import { PrintEntitiesArguments, PrintEntityByIdArguments, PrintArguments } from './dto/print-arguments';
import { PrintPreviewResponse } from './dto/printing-preview-response';
import { PrintingPreviewTemplate } from './dto/printing-preview-template';
import { GetByIdsArguments } from './dto/get-by-ids-arguments';
import { StatementArguments } from './dto/statement-arguments';
import { StatementResponse } from './dto/statement-response';
import { UpdateStateArguments } from './dto/update-state-arguments';
import { NotificationSummary } from './dto/server-notification-summary';
import { LineForSave } from './entities/line';
import {
  ReconciliationGetUnreconciledArguments,
  ReconciliationGetUnreconciledResponse,
  ReconciliationGetReconciledResponse,
  ReconciliationGetReconciledArguments,
  ReconciliationSavePayload
} from './dto/reconciliation';
import { ExternalEntryForSave } from './entities/external-entry';
import { Entity } from './entities/base/entity';
import { SelectExpandArguments } from './dto/select-expand-arguments';
import { GetFactResponse } from './dto/get-fact-response';
import { UpdateAssignmentArguments } from './dto/update-assignment-arguments';
import { IdentityServerClient } from './entities/identity-server-client';
import { ResetClientSecretArguments } from './dto/reset-client-secret-args';
import { ReportArguments } from './workspace.service';


@Injectable({
  providedIn: 'root'
})
export class ApiService {

  public showRotator = false;

  // Will abstract away standard API calls for CRUD operations
  constructor(public http: HttpClient, public trx: TranslateService) { }

  // Notifications

  public notificationsRecap() {
    // This call occurs automatically when the computer becomes online again,
    // So it should be silent, ie. doesn't update user activity
    const url = appsettings.apiAddress + 'api/notifications/recap?silent=true';
    const obs$ = this.http.get<NotificationSummary>(url).pipe(
      catchError(error => {
        const friendlyError = friendlify(error, this.trx);
        return throwError(friendlyError);
      }));

    return obs$;
  }

  // Admin

  public adminUsersApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<AdminUser>('admin-users', cancellationToken$),
      deactivate: this.deactivateFactory<AdminUser>('admin-users', cancellationToken$),
      getForClient: () => {
        const url = appsettings.apiAddress + `api/admin-users/client`;
        const obs$ = this.http.get<Versioned<AdminUserSettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveForClient: (key: string, value: string) => {
        const body = { key, value };
        const url = appsettings.apiAddress + `api/admin-users/client`;
        const obs$ = this.http.post<Versioned<AdminUserSettingsForClient>>(url, body, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      invite: (ids: (string | number)[], args: ActionArguments, extras?: { [key: string]: any }) => {
        args = args || {};

        const paramsArray: string[] = this.stringifyActionArguments(args);
        this.addExtras(paramsArray, extras);
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/admin-users/invite?${params}`;

        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<AdminUser>>(url, ids, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      getMyUser: () => {
        const url = appsettings.apiAddress + `api/admin-users/me`;
        const obs$ = this.http.get<GetByIdResponse<AdminUser>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveMyUser: (entity: MyAdminUserForSave) => {
        this.showRotator = true;
        const url = appsettings.apiAddress + `api/admin-users/me`;

        const obs$ = this.http.post<GetByIdResponse<AdminUser>>(url, entity, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      }
    };
  }

  public identityServerClientsApi(cancellationToken$: Observable<void>) {
    return {
      resetSecret: (args: ResetClientSecretArguments) => {
        args = args || {};

        const paramsArray: string[] = this.stringifyActionArguments(args);
        paramsArray.push(`id=${encodeURIComponent(args.id)}`);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/identity-server-clients/reset-secret?${params}`;

        console.log(url);

        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<IdentityServerClient>>(url, null).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      }
    };
  }

  public identityServerUsersApi(cancellationToken$: Observable<void>) {
    return {
      resetPassword: (args: ResetPasswordArgs) => {
        args = args || {};
        const url = appsettings.apiAddress + `api/identity-server-users/reset-password`;
        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<IdentityServerUser>>(url, args, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      }
    };
  }

  public adminSettingsApi(cancellationToken$: Observable<void>) {
    // TODO: Keep or remove?
    return {
      // get: (args: GetByIdArguments) => {
      //   args = args || {};
      //   const paramsArray: string[] = [];

      //   if (!!args.expand) {
      //     paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
      //   }

      //   const params: string = paramsArray.join('&');
      //   const url = appsettings.apiAddress + `api/admin-settings?${params}`;

      //   const obs$ = this.http.get<GetEntityResponse<AdminSettings>>(url).pipe(
      //     catchError(error => {
      //       const friendlyError = friendlify(error, this.trx);
      //       return throwError(friendlyError);
      //     }),
      //     takeUntil(cancellationToken$)
      //   );

      //   return obs$;
      // },

      getForClient: () => {
        const url = appsettings.apiAddress + `api/admin-settings/client`;
        const obs$ = this.http.get<Versioned<AdminSettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      ping: () => {
        const url = appsettings.apiAddress + `api/admin-settings/ping`;
        const obs$ = this.http.get(url).pipe(
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      // save: (entity: AdminSettings, args: SaveArguments) => {
      //   this.showRotator = true;
      //   args = args || {};
      //   const paramsArray: string[] = [];

      //   if (!!args.expand) {
      //     paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
      //   }

      //   const params: string = paramsArray.join('&');
      //   const url = appsettings.apiAddress + `api/admin-settings?${params}`;

      //   const obs$ = this.http.post<SaveAdminSettingsResponse>(url, entity, {
      //     headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      //   }).pipe(
      //     tap(() => this.showRotator = false),
      //     catchError((error) => {
      //       this.showRotator = false;
      //       const friendlyError = friendlify(error, this.trx);
      //       return throwError(friendlyError);
      //     }),
      //     takeUntil(cancellationToken$),
      //     finalize(() => this.showRotator = false)
      //   );

      //   return obs$;
      // }
    };
  }

  public adminPermissionsApi(cancellationToken$: Observable<void>) {
    return {
      getForClient: () => {
        const url = appsettings.apiAddress + `api/admin-permissions/client`;
        const obs$ = this.http.get<Versioned<AdminPermissionsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  // Application

  public unitsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Unit>('units', cancellationToken$),
      deactivate: this.deactivateFactory<Unit>('units', cancellationToken$)
    };
  }

  public agentsApi(definitionId: number, cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Agent>(`agents/${definitionId}`, cancellationToken$),
      deactivate: this.deactivateFactory<Agent>(`agents/${definitionId}`, cancellationToken$),
      getAttachment: (agentId: string | number, attachmentId: string | number) => {
        const url = appsettings.apiAddress + `api/agents/${definitionId}/${agentId}/attachments/${attachmentId}`;
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
    };
  }

  public rolesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Role>('roles', cancellationToken$),
      deactivate: this.deactivateFactory<Role>('roles', cancellationToken$)
    };
  }

  public accountTypesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<AccountType>(`account-types`, cancellationToken$),
      deactivate: this.deactivateFactory<AccountType>(`account-types`, cancellationToken$)
    };
  }

  public lookupsApi(definitionId: number, cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Lookup>(`lookups/${definitionId}`, cancellationToken$),
      deactivate: this.deactivateFactory<Lookup>(`lookups/${definitionId}`, cancellationToken$)
    };
  }

  public currenciesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Currency>('currencies', cancellationToken$),
      deactivate: this.deactivateFactory<Currency>('currencies', cancellationToken$)
    };
  }

  public resourcesApi(definitionId: number, cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Resource>(`resources/${definitionId}`, cancellationToken$),
      deactivate: this.deactivateFactory<Resource>(`resources/${definitionId}`, cancellationToken$)
    };
  }

  public accountClassificationsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<AccountClassification>('account-classifications', cancellationToken$),
      deactivate: this.deactivateFactory<AccountClassification>('account-classifications', cancellationToken$)
    };
  }

  public entryTypesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<EntryType>(`entry-types`, cancellationToken$),
      deactivate: this.deactivateFactory<EntryType>(`entry-types`, cancellationToken$)
    };
  }

  public inboxApi(cancellationToken$: Observable<void>) {
    return {
      check: (now: string) => {
        const url = appsettings.apiAddress + `api/inbox/check`;
        const obs$ = this.http.put(url, JSON.stringify(now), {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      }
    };
  }

  public printingTemplatesApi(cancellationToken$: Observable<void>) {
    return {
      print: (templateId: number, args: PrintArguments, custom?: ReportArguments) => {
        const paramsArray = this.stringifyArguments(args).concat(this.stringifyArguments(custom));
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/printing-templates/print/${templateId}?${params}`;

        const obs$ = this.http.get(url, { observe: 'response', responseType: 'blob' }).pipe(
          map(res => ({ blob: res.body, name: res.headers.get('x-filename') })),
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
      preview: (entity: PrintingPreviewTemplate, args: PrintArguments, custom?: ReportArguments) => {
        const paramsArray = this.stringifyArguments(args).concat(this.stringifyArguments(custom));
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/printing-templates/preview?${params}`;
        const obs$ = this.http.put<PrintPreviewResponse>(url, entity).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
      previewByFilter: (entity: PrintingPreviewTemplate, args: PrintEntitiesArguments, custom?: ReportArguments) => {
        const paramsArray = this.stringifyArguments(args).concat(this.stringifyArguments(custom));
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/printing-templates/preview-by-filter?${params}`;
        const obs$ = this.http.put<PrintPreviewResponse>(url, entity).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
      previewById: (id: string | number, entity: PrintingPreviewTemplate, args: PrintEntityByIdArguments, custom?: ReportArguments) => {
        const paramsArray = this.stringifyArguments(args).concat(this.stringifyArguments(custom));
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/printing-templates/preview-by-id/${id}?${params}`;
        const obs$ = this.http.put<PrintPreviewResponse>(url, entity).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      }
    };
  }

  public documentsApi(definitionId: number, cancellationToken$: Observable<void>) {
    return {
      updateAssignment: (args: UpdateAssignmentArguments, extras?: { [key: string]: any }) => {
        const paramsArray = this.stringifyActionArguments(args);
        this.addExtras(paramsArray, extras);

        paramsArray.push(`id=${encodeURIComponent(args.id)}`);

        if (!!args.comment) {
          paramsArray.push(`comment=${encodeURIComponent(args.comment)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/documents/${definitionId}/update-assignment?${params}`;

        this.showRotator = true;
        const obs$ = this.http.put<GetByIdResponse<Document>>(url, null, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      assign: (ids: (string | number)[], args: AssignArguments, extras?: { [key: string]: any }) => {
        const paramsArray = this.stringifyActionArguments(args);
        this.addExtras(paramsArray, extras);

        paramsArray.push(`assigneeId=${encodeURIComponent(args.assigneeId)}`);

        if (!!args.comment) {
          paramsArray.push(`comment=${encodeURIComponent(args.comment)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/documents/${definitionId}/assign?${params}`;

        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<Document>>(url, ids, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      sign: (ids: (string | number)[], args: SignArguments, extras?: { [key: string]: any }) => {

        const paramsArray = this.stringifyActionArguments(args);
        this.addExtras(paramsArray, extras);

        paramsArray.push(`toState=${encodeURIComponent(args.toState)}`);

        if (!!args.reasonId) {
          paramsArray.push(`reasonId=${encodeURIComponent(args.reasonId)}`);
        }

        if (!!args.reasonDetails) {
          paramsArray.push(`reasonDetails=${encodeURIComponent(args.reasonDetails)}`);
        }

        if (!!args.onBehalfOfUserId) {
          paramsArray.push(`onBehalfOfUserId=${encodeURIComponent(args.onBehalfOfUserId)}`);
        }

        if (!!args.ruleType) {
          paramsArray.push(`ruleType=${encodeURIComponent(args.ruleType)}`);
        }

        if (!!args.roleId) {
          paramsArray.push(`roleId=${encodeURIComponent(args.roleId)}`);
        }

        if (!!args.signedAt) {
          paramsArray.push(`signedAt=${encodeURIComponent(args.signedAt)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/documents/${definitionId}/sign-lines?${params}`;

        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<Document>>(url, ids, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      unsign: (ids: (string | number)[], args: ActionArguments, extras?: { [key: string]: any }) => {

        const paramsArray = this.stringifyActionArguments(args);
        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/documents/${definitionId}/unsign-lines?${params}`;

        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<Document>>(url, ids, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      close: this.updateStateFactory(definitionId, 'close', cancellationToken$),
      open: this.updateStateFactory(definitionId, 'open', cancellationToken$),
      cancel: this.updateStateFactory(definitionId, 'cancel', cancellationToken$),
      uncancel: this.updateStateFactory(definitionId, 'uncancel', cancellationToken$),
      getAttachment: (docId: string | number, attachmentId: string | number) => {
        const url = appsettings.apiAddress + `api/documents/${definitionId}/${docId}/attachments/${attachmentId}`;
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
      autoGenerate: (lineDefId: number, args: { [key: string]: any }) => {

        const paramsArray: string[] = [];
        for (const key of Object.keys(args)) {
          const val = args[key];
          if (isSpecified(val)) {
            paramsArray.push(`${key}=${encodeURIComponent(val.toString())}`);
          }
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/documents/${definitionId}/generate-lines/${lineDefId}?${params}`;

        this.showRotator = true;
        const obs$ = this.http.get<EntitiesResponse<LineForSave>>(url).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      }
    };
  }

  public detailsEntriesApi(cancellationToken$: Observable<void>) {
    return {
      statement: (args: StatementArguments) => {
        const paramsArray = [];
        for (const key of Object.keys(args)) {
          paramsArray.push(`${key}=${encodeURIComponent(args[key])}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/details-entries/statement?${params}`;

        const obs$ = this.http.get<StatementResponse>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );

        return obs$;
      }
    };
  }

  public exchangeRatesApi(cancellationToken$: Observable<void>) {
    return {
      convertToFunctional: (date: string, currencyId: string, amount: number) => {
        const paramsArray: string[] = [];

        paramsArray.push(`date=${encodeURIComponent(date)}`);
        paramsArray.push(`currencyId=${encodeURIComponent(currencyId)}`);
        paramsArray.push(`amount=${amount}`);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/exchange-rates/convert-to-functional?${params}`;
        const obs$ = this.http.get<number>(url).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      }
    };
  }

  public centersApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Center>('centers', cancellationToken$),
      deactivate: this.deactivateFactory<Center>('centers', cancellationToken$)
    };
  }

  public accountsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Account>(`accounts`, cancellationToken$),
      deactivate: this.deactivateFactory<Account>(`accounts`, cancellationToken$)
    };
  }

  public usersApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<User>('users', cancellationToken$),
      deactivate: this.deactivateFactory<User>('users', cancellationToken$),
      getForClient: () => {
        const url = appsettings.apiAddress + `api/users/client?silent=true`;
        const obs$ = this.http.get<Versioned<UserSettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveUserSetting: (key: string, value: string) => {
        const body = { key, value };
        const url = appsettings.apiAddress + `api/users/client`;
        const obs$ = this.http.post<Versioned<UserSettingsForClient>>(url, body, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveUserPreferredLanguage: (preferredLanguage: string) => {
        const url = appsettings.apiAddress + `api/users/client/preferred-language?preferredLanguage=${preferredLanguage}`;
        const obs$ = this.http.post<Versioned<UserSettingsForClient>>(url, {}).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveUserPreferredCalendar: (preferredCalendar: string) => {
        const url = appsettings.apiAddress + `api/users/client/preferred-calendar?preferredCalendar=${preferredCalendar}`;
        const obs$ = this.http.post<Versioned<UserSettingsForClient>>(url, {}).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      invite: (ids: (string | number)[], args: ActionArguments, extras?: { [key: string]: any }) => {
        args = args || {};

        const paramsArray: string[] = this.stringifyActionArguments(args);
        this.addExtras(paramsArray, extras);
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/users/invite?${params}`;

        this.showRotator = true;
        const obs$ = this.http.put<EntitiesResponse<User>>(url, ids, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      testEmail: (email: string) => {
        const url = appsettings.apiAddress + `api/users/test-email?email=${encodeURIComponent(email)}`;

        this.showRotator = true;
        const obs$ = this.http.put<{ Message: string }>(url, null).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      testPhone: (phone: string) => {
        const url = appsettings.apiAddress + `api/users/test-phone?phone=${encodeURIComponent(phone)}`;

        this.showRotator = true;
        const obs$ = this.http.put<{ Message: string }>(url, null).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
      getMyUser: () => {
        const url = appsettings.apiAddress + `api/users/me`;
        const obs$ = this.http.get<GetByIdResponse<User>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveMyUser: (entity: MyUserForSave) => {
        this.showRotator = true;
        const url = appsettings.apiAddress + `api/users/me`;

        const obs$ = this.http.post<GetByIdResponse<User>>(url, entity, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      }
    };
  }

  public companiesApi(cancellationToken$: Observable<void>) {
    return {
      getForClient: () => {
        const url = appsettings.apiAddress + `api/companies/client`;
        const obs$ = this.http.get<CompaniesForClient>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      }
    };
  }

  public globalSettingsApi(cancellationToken$: Observable<void>) {
    return {
      getForClient: () => {
        const url = appsettings.apiAddress + `api/global-settings/client`;
        const obs$ = this.http.get<Versioned<GlobalSettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      ping: () => {
        const url = appsettings.apiAddress + `api/global-settings/ping`;
        const obs$ = this.http.get(url).pipe(
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  public pingApi(cancellationToken$: Observable<void>) {
    return {
      ping: () => {
        const url = appsettings.apiAddress + `api/ping`;
        const obs$ = this.http.get<void>(url).pipe(
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  public reconciliationApi(cancellationToken$: Observable<void>) {
    return {
      getUnreconciled: (args: ReconciliationGetUnreconciledArguments) => {
        const paramsArray: string[] = this.stringifyArguments(args);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/reconciliation/unreconciled?${params}`;

        const obs$ = this.http.get<ReconciliationGetUnreconciledResponse>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getReconciled: (args: ReconciliationGetReconciledArguments) => {
        const paramsArray: string[] = this.stringifyArguments(args);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/reconciliation/reconciled?${params}`;

        const obs$ = this.http.get<ReconciliationGetReconciledResponse>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      saveAndGetUnreconciled: (payload: ReconciliationSavePayload, args: ReconciliationGetUnreconciledArguments) => {
        this.showRotator = true;
        const paramsArray: string[] = this.stringifyArguments(args);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/reconciliation/unreconciled?${params}`;

        const obs$ = this.http.post<ReconciliationGetUnreconciledResponse>(url, payload, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      saveAndGetReconciled: (payload: ReconciliationSavePayload, args: ReconciliationGetReconciledArguments) => {
        this.showRotator = true;
        const paramsArray: string[] = this.stringifyArguments(args);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/reconciliation/reconciled?${params}`;

        const obs$ = this.http.post<ReconciliationGetReconciledResponse>(url, payload, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      import: (file: File) => {
        // args = args || {};

        const paramsArray: string[] = [];

        // if (!!args.mode) {
        //   paramsArray.push(`mode=${args.mode}`);
        // }

        // if (!!args.key) {
        //   paramsArray.push(`key=${args.key}`);
        // }

        const formData = new FormData();
        formData.append(file.name, file, file.name);

        this.showRotator = true;
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/reconciliation/import?${params}`;
        const obs$ = this.http.post<ExternalEntryForSave[]>(url, formData).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },
    };
  }

  public generalSettingsApi(cancellationToken$: Observable<void>) {
    return {

      getForClient: () => {
        const url = appsettings.apiAddress + `api/general-settings/client?silent=true`;
        const obs$ = this.http.get<Versioned<SettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      ping: () => {
        const url = appsettings.apiAddress + `api/general-settings/ping`;
        const obs$ = this.http.get(url).pipe(
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  public permissionsApi(cancellationToken$: Observable<void>) {
    return {
      getForClient: () => {
        const url = appsettings.apiAddress + `api/permissions/client?silent=true`;
        const obs$ = this.http.get<Versioned<PermissionsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  public definitionsApi(cancellationToken$: Observable<void>) {
    return {
      getForClient: () => {
        const url = appsettings.apiAddress + `api/definitions/client?silent=true`;
        const obs$ = this.http.get<Versioned<DefinitionsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  // Definitions update state
  public lookupDefinitionsApi(cancellationToken$: Observable<void>) {
    return {
      updateState: this.updateDefinitionStateFactory('lookup-definitions', cancellationToken$)
    };
  }

  public resourceDefinitionsApi(cancellationToken$: Observable<void>) {
    return {
      updateState: this.updateDefinitionStateFactory('resource-definitions', cancellationToken$)
    };
  }

  public agentDefinitionsApi(cancellationToken$: Observable<void>) {
    return {
      updateState: this.updateDefinitionStateFactory('agent-definitions', cancellationToken$)
    };
  }

  public documentDefinitionsApi(cancellationToken$: Observable<void>) {
    return {
      updateState: this.updateDefinitionStateFactory('document-definitions', cancellationToken$)
    };
  }

  public settingsFactory<TSettings extends Entity, TSettingsForSave extends Entity>(
    endpoint: string, cancellationToken$: Observable<void>) {
    return {
      get: (args: SelectExpandArguments) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.select) {
          paramsArray.push(`select=${encodeURIComponent(args.select)}`);
        }

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.get<GetEntityResponse<TSettings>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      save: (entity: TSettingsForSave, args: SaveArguments) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.select) {
          paramsArray.push(`select=${encodeURIComponent(args.select)}`);
        }

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        if (args.returnEntities || !isSpecified(args.returnEntities)) {
          paramsArray.push(`returnEntities=true`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}?${params}`;

        this.showRotator = true;
        const obs$ = this.http.post<SaveSettingsResponse<TSettings>>(url, entity, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      }
    };
  }

  public crudFactory<TEntity extends EntityForSave, TEntityForSave extends EntityForSave = EntityForSave>(
    endpoint: string, cancellationToken$: Observable<void>) {
    return {
      getEntities: (args: GetArguments, extras?: { [key: string]: any }) => {
        const paramsArray = this.stringifyGetArguments(args);
        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.get<GetResponse<TEntity>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getByIds: (args: GetByIdsArguments, extras?: { [key: string]: any }) => {
        const paramsArray = this.stringifyGetArguments(args);
        this.addExtras(paramsArray, extras);

        if (!!args.i) {
          args.i.forEach(id => {
            paramsArray.push(`i=${encodeURIComponent(id)}`);
          });
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/by-ids?${params}`;

        const obs$ = this.http.get<EntitiesResponse<TEntity>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getById: (id: number | string, args: GetByIdArguments, extras?: { [key: string]: any }) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        if (!!args.select) {
          paramsArray.push(`select=${encodeURIComponent(args.select)}`);
        }

        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/${id}?${params}`;

        const obs$ = this.http.get<GetByIdResponse<TEntity>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getFact: (args: FactArguments, extras?: { [key: string]: any }) => {
        const paramsArray = this.stringifyGetArguments(args);
        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/fact?${params}`;

        const obs$ = this.http.get<GetFactResponse>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getAggregate: (args: GetAggregateArguments, extras?: { [key: string]: any }) => {
        args = args || {};
        const paramsArray: string[] = [];
        const headers: { [key: string]: string } = {};

        if (!!args.select) {
          if (args.select.length > 512) {
            headers['X-Select'] = args.select;
            paramsArray.push(`select_hash=${this.hashCode(args.select)}`);
          } else {
            paramsArray.push(`select=${encodeURIComponent(args.select)}`);
          }
        }

        if (!!args.filter) {
          paramsArray.push(`filter=${encodeURIComponent(args.filter)}`);
        }

        if (!!args.having) {
          paramsArray.push(`having=${encodeURIComponent(args.having)}`);
        }

        if (!!args.silent) {
          paramsArray.push(`silent=${!!args.silent}`);
        }

        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/aggregate?${params}`;

        const obs$ = this.http.get<GetAggregateResponse>(url, { headers }).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getChildrenOf: (args: GetChildrenArguments, extras?: { [key: string]: any }) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        if (!!args.select) {
          paramsArray.push(`select=${encodeURIComponent(args.select)}`);
        }

        if (!!args.filter) {
          paramsArray.push(`filter=${encodeURIComponent(args.filter)}`);
        }

        paramsArray.push(`roots=${!!args.roots}`);

        if (!!args.i) {
          args.i.forEach(id => {
            paramsArray.push(`i=${encodeURIComponent(id)}`);
          });
        }

        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/children-of?${params}`;

        const obs$ = this.http.get<EntitiesResponse<TEntity>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      save: (entities: TEntityForSave[], args: SaveArguments, extras?: { [key: string]: any }) => {
        this.showRotator = true;
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        if (!!args.select) {
          paramsArray.push(`select=${encodeURIComponent(args.select)}`);
        }
        if (args.returnEntities || !isSpecified(args.returnEntities)) {
          paramsArray.push(`returnEntities=true`);
        }

        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.post<EntitiesResponse<TEntity>>(url, entities, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      deleteId: (id: number | string) => {
        this.showRotator = true;

        const url = appsettings.apiAddress + `api/${endpoint}` + '/' + encodeURIComponent(id);
        const obs$ = this.http.delete(url).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      delete: (ids: (number | string)[]) => {
        this.showRotator = true;

        const url = appsettings.apiAddress + `api/${endpoint}?` + ids.map(id => `i=${encodeURIComponent(id)}`).join('&');
        const obs$ = this.http.delete(url).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      deleteWithDescendants: (ids: (number | string)[]) => {
        this.showRotator = true;
        const url = appsettings.apiAddress + `api/${endpoint}/with-descendants?` + ids.map(id => `i=${encodeURIComponent(id)}`).join('&');
        const obs$ = this.http.delete(url).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      template: (args: TemplateArguments) => {
        args = args || {};

        const paramsArray: string[] = [];

        if (!!args.format) {
          paramsArray.push(`format=${args.format}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/template?${params}`;
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },

      import: (args: ImportArguments, file: File) => {
        args = args || {};

        const paramsArray: string[] = [];

        if (!!args.mode) {
          paramsArray.push(`mode=${args.mode}`);
        }

        if (!!args.key) {
          paramsArray.push(`key=${args.key}`);
        }

        const formData = new FormData();
        formData.append(file.name, file, file.name);

        this.showRotator = true;
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/import?${params}`;
        const obs$ = this.http.post<ImportResult>(url, formData).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      export: (args: ExportForImportArguments) => {
        const paramsArray = this.stringifyGetArguments(args);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/export?${params}`;
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },

      exportByIds: (ids: (string | number)[]) => {

        const url = appsettings.apiAddress + `api/${endpoint}/export-by-ids?` + ids.map(id => `i=${encodeURIComponent(id)}`).join('&');
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },

      printEntities: (templateId: number, args: PrintEntitiesArguments) => {
        const paramsArray: string[] = [
        ];

        if (!!args.filter) {
          paramsArray.push(`filter=${encodeURIComponent(args.filter)}`);
        }

        if (!!args.orderby) {
          paramsArray.push(`orderby=${encodeURIComponent(args.orderby)}`);
        }

        if (!!args.skip || args.skip === 0) {
          paramsArray.push(`skip=${args.skip}`);
        }

        if (!!args.top || args.top === 0) {
          paramsArray.push(`top=${args.top}`);
        }

        if (!!args.i) {
          args.i.forEach(id => {
            paramsArray.push(`i=${encodeURIComponent(id)}`);
          });
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/print-entities/${templateId}?${params}`;

        const obs$ = this.http.get(url, { observe: 'response', responseType: 'blob' }).pipe(
          map(res => ({ blob: res.body, name: res.headers.get('x-filename') })),
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },

      printEntity: (id: string | number, templateId: number, args: PrintEntityByIdArguments) => {
        const paramsArray = [`culture=${encodeURIComponent(args.culture)}`];
        const params: string = paramsArray.join('&');

        const url = appsettings.apiAddress + `api/${endpoint}/${id}/print-entity/${templateId}?${params}`;

        const obs$ = this.http.get(url, { observe: 'response', responseType: 'blob' }).pipe(
          map(res => ({ blob: res.body, name: res.headers.get('x-filename') })),
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
    };
  }

  // We refactored this out to support the t-image component
  public getImage(endpoint: string, imageId: string, cancellationToken$: Observable<void>) {

    // Note: cache=true instructs the HTTP interceptor to not add cache-busting parameters
    const url = appsettings.apiAddress + `api/${endpoint}?imageId=${imageId}`;
    const obs$ = this.http.get(url, { responseType: 'blob', observe: 'response' }).pipe(
      map(res => {
        return { image: res.body, imageId: res.headers.get('x-image-id') };
      }),
      catchError(error => {
        const friendlyError = friendlify(error, this.trx);
        return throwError(friendlyError);
      }),
      takeUntil(cancellationToken$)
    );

    return obs$;
  }

  private updateStateFactory(definitionId: number, transition: string, cancellationToken$: Observable<void>) {
    return (ids: (string | number)[], args: ActionArguments, extras?: { [key: string]: any }) => {

      const paramsArray = this.stringifyActionArguments(args);
      this.addExtras(paramsArray, extras);

      const params: string = paramsArray.join('&');
      const url = appsettings.apiAddress + `api/documents/${definitionId}/${transition}?${params}`;

      this.showRotator = true;
      const obs$ = this.http.put<EntitiesResponse<Document>>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.showRotator = false),
        catchError(error => {
          this.showRotator = false;
          const friendlyError = friendlify(error, this.trx);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.showRotator = false)
      );

      return obs$;
    };
  }

  private updateDefinitionStateFactory<TDefinition>(endpoint: string, cancellationToken$: Observable<void>) {
    return (ids: (string | number)[], args: UpdateStateArguments, extras?: { [key: string]: any }) => {

      const paramsArray = this.stringifyActionArguments(args);
      this.addExtras(paramsArray, extras);

      paramsArray.push(`state=${encodeURIComponent(args.state)}`);

      const params: string = paramsArray.join('&');
      const url = appsettings.apiAddress + `api/${endpoint}/update-state?${params}`;

      this.showRotator = true;
      const obs$ = this.http.put<EntitiesResponse<TDefinition>>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.showRotator = false),
        catchError(error => {
          this.showRotator = false;
          const friendlyError = friendlify(error, this.trx);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.showRotator = false)
      );

      return obs$;
    };
  }

  private activateFactory<TDto extends EntityForSave>(endpoint: string, cancellationToken$: Observable<void>) {
    return (ids: (string | number)[], args: ActivateArguments, extras?: { [key: string]: any }) => {
      args = args || {};

      const paramsArray: string[] = this.stringifyActionArguments(args);
      this.addExtras(paramsArray, extras);
      const params: string = paramsArray.join('&');
      const url = appsettings.apiAddress + `api/${endpoint}/activate?${params}`;

      this.showRotator = true;
      const obs$ = this.http.put<EntitiesResponse<TDto>>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.showRotator = false),
        catchError(error => {
          this.showRotator = false;
          const friendlyError = friendlify(error, this.trx);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.showRotator = false)
      );

      return obs$;
    };
  }

  private deactivateFactory<TDto extends EntityForSave>(endpoint: string, cancellationToken$: Observable<void>) {
    return (ids: (string | number)[], args: ActivateArguments, extras?: { [key: string]: any }) => {
      args = args || {};

      const paramsArray: string[] = this.stringifyActionArguments(args);
      this.addExtras(paramsArray, extras);
      const params: string = paramsArray.join('&');
      const url = appsettings.apiAddress + `api/${endpoint}/deactivate?${params}`;

      this.showRotator = true;
      const obs$ = this.http.put<EntitiesResponse<TDto>>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.showRotator = false),
        catchError(error => {
          this.showRotator = false;
          const friendlyError = friendlify(error, this.trx);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.showRotator = false)
      );

      return obs$;
    };
  }

  private hashCode(s: string) {
    const l = s.length;
    let h = 0;
    let i = 0;
    if (l > 0) {
      while (i < l) {
        // tslint:disable-next-line:no-bitwise
        h = (h << 5) - h + s.charCodeAt(i++) | 0;
      }
    }
    return h;
  }

  stringifyGetArguments(args: GetArguments): string[] {
    args = args || {};
    const top = args.top || 50;
    const skip = args.skip || 0;

    const paramsArray: string[] = [
      `top=${top}`,
      `skip=${skip}`
    ];

    if (!!args.search) {
      paramsArray.push(`search=${encodeURIComponent(args.search)}`);
    }

    if (!!args.orderby) {
      paramsArray.push(`orderBy=${args.orderby}`);
    }

    if (!!args.filter) {
      paramsArray.push(`filter=${encodeURIComponent(args.filter)}`);
    }

    if (!!args.expand) {
      paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
    }

    if (!!args.select) {
      paramsArray.push(`select=${encodeURIComponent(args.select)}`);
    }

    if (!!args.countEntities) {
      paramsArray.push(`countEntities=true`);
    }

    if (!!args.silent) {
      paramsArray.push(`silent=${!!args.silent}`);
    }

    return paramsArray;
  }

  stringifyActionArguments(args: ActionArguments): string[] {
    args = args || {};

    const paramsArray: string[] = [
    ];

    if (!!args.expand) {
      paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
    }

    if (!!args.select) {
      paramsArray.push(`select=${encodeURIComponent(args.select)}`);
    }

    if (!!args.returnEntities || !isSpecified(args.returnEntities)) {
      paramsArray.push(`returnEntities=true`);
    }

    return paramsArray;
  }

  stringifyArguments(args: { [key: string]: any }): string[] {
    if (!args) {
      return [];
    }

    const paramsArray: string[] = [];
    const keys = Object.keys(args);
    for (const key of keys) {
      if (args[key] !== null && args[key] !== undefined) {
        paramsArray.push(`${key}=${encodeURIComponent(args[key].toString())}`);
      }
    }

    return paramsArray;
  }

  addExtras(paramsArray: string[], extras: { [key: string]: any }) {
    if (!!extras) {
      Object.keys(extras).forEach(key => {
        const value = extras[key];
        if (value !== undefined && value !== null) {
          const valueString = value.toString();
          paramsArray.push(`${key}=${encodeURIComponent(valueString)}`);
        }
      });
    }
  }
}
