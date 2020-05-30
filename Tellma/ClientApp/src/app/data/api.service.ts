import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, throwError, of } from 'rxjs';
import { catchError, finalize, takeUntil, tap, map } from 'rxjs/operators';
import { ActivateArguments } from './dto/activate-arguments';
import { EntityForSave } from './entities/base/entity-for-save';
import { GetArguments } from './dto/get-arguments';
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
import { Settings } from './entities/settings';
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
import { CustomClassification } from './entities/custom-classification';
import { Account } from './entities/account';
import { GetChildrenArguments } from './dto/get-children-arguments';
import { GetAggregateArguments } from './dto/get-aggregate-arguments';
import { GetAggregateResponse } from './dto/get-aggregate-response';
import { Center } from './entities/center';
import { friendlify } from './util';
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
import { GenerateMarkupByFilterArguments, GenerateMarkupByIdArguments, GenerateMarkupArguments } from './dto/generate-markup-arguments';
import { MarkupPreviewResponse } from './dto/markup-preview-response';
import { MarkupPreviewTemplate } from './dto/markup-preview-template';
import { ExportSelectedArguments } from './dto/export-selected-arguments';
import { SelectExpandArguments } from './dto/select-expand-arguments';
import { GetByIdsArguments } from './dto/get-by-ids-arguments';


@Injectable({
  providedIn: 'root'
})
export class ApiService {

  public showRotator = false;

  // Will abstract away standard API calls for CRUD operations
  constructor(public http: HttpClient, public trx: TranslateService) { }

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
      invite: (id: number | string) => {
        this.showRotator = true;
        const url = appsettings.apiAddress + `api/admin-users/invite?id=${id}`;
        const obs$ = this.http.put(url, null).pipe(
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

  public agentsApi(definitionId: string, cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Agent>(`agents/${definitionId}`, cancellationToken$),
      deactivate: this.deactivateFactory<Agent>(`agents/${definitionId}`, cancellationToken$)
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

  public lookupsApi(definitionId: string, cancellationToken$: Observable<void>) {
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

  public resourcesApi(definitionId: string, cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Resource>(`resources/${definitionId}`, cancellationToken$),
      deactivate: this.deactivateFactory<Resource>(`resources/${definitionId}`, cancellationToken$)
    };
  }

  public customClassificationsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<CustomClassification>('custom-classifications', cancellationToken$),
      deactivate: this.deactivateFactory<CustomClassification>('custom-classifications', cancellationToken$)
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

  public markupTemplatesApi(cancellationToken$: Observable<void>) {
    return {
      preview: (entity: MarkupPreviewTemplate, args: GenerateMarkupArguments) => {
        const paramsArray = this.stringifyArguments(args);
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/markup-templates/preview?${params}`;
        const obs$ = this.http.put<MarkupPreviewResponse>(url, entity).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
      previewByFilter: (entity: MarkupPreviewTemplate, args: GenerateMarkupByFilterArguments) => {
        const paramsArray = this.stringifyArguments(args);
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/markup-templates/preview-by-filter?${params}`;
        const obs$ = this.http.put<MarkupPreviewResponse>(url, entity).pipe(
          catchError((error) => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },
      previewById: (id: string | number, entity: MarkupPreviewTemplate, args: GenerateMarkupByIdArguments) => {
        const paramsArray = this.stringifyArguments(args);
        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/markup-templates/preview-by-id/${id}?${params}`;
        const obs$ = this.http.put<MarkupPreviewResponse>(url, entity).pipe(
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

  public documentsApi(definitionId: string, cancellationToken$: Observable<void>) {
    return {
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
      post: this.updateStateFactory(definitionId, 'post', cancellationToken$),
      unpost: this.updateStateFactory(definitionId, 'unpost', cancellationToken$),
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
      printById: (docId: string | number, templateId: number, args: GenerateMarkupByIdArguments) => {
        const paramsArray = [`culture=${encodeURIComponent(args.culture)}`];
        const params: string = paramsArray.join('&');

        const url = appsettings.apiAddress + `api/documents/${definitionId}/${docId}/print/${templateId}?${params}`;

        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
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
        const url = appsettings.apiAddress + `api/users/client?unobtrusive=true`;
        const obs$ = this.http.get<Versioned<UserSettingsForClient>>(url).pipe(
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
      invite: (id: number | string) => {
        this.showRotator = true;
        const url = appsettings.apiAddress + `api/users/invite?id=${id}`;
        const obs$ = this.http.put(url, null).pipe(
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

  public settingsApi(cancellationToken$: Observable<void>) {
    return {
      get: (args: GetByIdArguments) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/settings?${params}`;

        const obs$ = this.http.get<GetEntityResponse<Settings>>(url).pipe(
          catchError(error => {
            const friendlyError = friendlify(error, this.trx);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getForClient: () => {
        const url = appsettings.apiAddress + `api/settings/client?unobtrusive=true`;
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
        const url = appsettings.apiAddress + `api/settings/ping`;
        const obs$ = this.http.get(url).pipe(
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      save: (entity: Settings, args: SaveArguments) => {
        this.showRotator = true;
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/settings?${params}`;

        const obs$ = this.http.post<SaveSettingsResponse>(url, entity, {
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

  public permissionsApi(cancellationToken$: Observable<void>) {
    return {
      getForClient: () => {
        const url = appsettings.apiAddress + `api/permissions/client?unobtrusive=true`;
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
        const url = appsettings.apiAddress + `api/definitions/client?unobtrusive=true`;
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

  public crudFactory<TEntity extends EntityForSave, TEntityForSave extends EntityForSave = EntityForSave>(
    endpoint: string, cancellationToken$: Observable<void>) {
    return {
      get: (args: GetArguments, extras?: { [key: string]: any }) => {
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

      getByIds: (ids: (number | string)[], args: GetByIdsArguments, extras?: { [key: string]: any }) => {
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

      getAggregate: (args: GetAggregateArguments, extras?: { [key: string]: any }) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.select) {
          paramsArray.push(`select=${encodeURIComponent(args.select)}`);
        }

        if (!!args.filter) {
          paramsArray.push(`filter=${encodeURIComponent(args.filter)}`);
        }

        this.addExtras(paramsArray, extras);

        const params: string = paramsArray.join('&');
        const url = appsettings.apiAddress + `api/${endpoint}/aggregate?${params}`;

        const obs$ = this.http.get<GetAggregateResponse>(url).pipe(
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

        paramsArray.push(`returnEntities=${!!args.returnEntities}`);

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

      // exportSelected: (args: ExportSelectedArguments) => {
      //   const paramsArray = this.stringifyGetArguments(args);

      //   const params: string = paramsArray.join('&');
      //   const url = appsettings.apiAddress + `api/${endpoint}/export-selected?${params}`;
      //   const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
      //     catchError((error) => {
      //       const friendlyError = friendlify(error, this.trx);
      //       return throwError(friendlyError);
      //     }),
      //     takeUntil(cancellationToken$),
      //   );
      //   return obs$;
      // }
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

  private updateStateFactory(definitionId: string, transition: string, cancellationToken$: Observable<void>) {
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

    if (!!args.unobtrusive) {
      paramsArray.push(`unobtrusive=${args.unobtrusive}`);
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

    if (!!args.returnEntities) {
      paramsArray.push(`returnEntities=${args.returnEntities}`);
    }

    return paramsArray;
  }

  stringifyArguments(args: { [key: string]: any }) {
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
