import { HttpClient, HttpErrorResponse, HttpHeaders, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, takeUntil, tap, map } from 'rxjs/operators';
import { ActivateArguments } from './dto/activate-arguments';
import { EntityForSave } from './entities/base/entity-for-save';
import { GetArguments } from './dto/get-arguments';
import { GetByIdArguments } from './dto/get-by-id-arguments';
import { GetResponse, EntitiesResponse } from './dto/get-response';
import { MeasurementUnit } from './entities/measurement-unit';
import { TemplateArguments } from './dto/template-arguments';
import { ImportArguments } from './dto/import-arguments';
import { ImportResult } from './dto/import-result';
import { ExportArguments } from './dto/export-arguments';
import { GetByIdResponse } from './dto/get-by-id-response';
import { SaveArguments } from './dto/save-arguments';
import { appconfig } from './appconfig';
import { Agent } from './entities/agent';
import { Role } from './entities/role';
import { View } from './entities/view';
import { Settings, SettingsForClient } from './entities/settings';
import { DataWithVersion } from './dto/data-with-version';
import { PermissionsForClient } from './entities/permission';
import { SaveSettingsResponse } from './dto/save-settings-response';
import { UserSettingsForClient } from './entities/user';
import { GlobalSettingsForClient } from './dto/global-settings';
import { UserCompany } from './dto/user-company';
import { IfrsNote } from './entities/ifrs-note';
import { ProductCategory } from './entities/product-category';
import { GetEntityResponse } from './dto/get-entity-response';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  public showRotator = false;

  // Will abstract away standard API calls for CRUD operations
  constructor(public http: HttpClient, public translate: TranslateService) { }

  public measurementUnitsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<MeasurementUnit>('measurement-units', cancellationToken$),
      deactivate: this.deactivateFactory<MeasurementUnit>('measurement-units', cancellationToken$)
    };
  }

  public agentsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Agent>(`agents`, cancellationToken$),
      deactivate: this.deactivateFactory<Agent>(`agents`, cancellationToken$)
    };
  }

  public rolesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Role>('roles', cancellationToken$),
      deactivate: this.deactivateFactory<Role>('roles', cancellationToken$)
    };
  }

  public viewsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<View>('views', cancellationToken$),
      deactivate: this.deactivateFactory<View>('views', cancellationToken$)
    };
  }

  public ifrsNotesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<IfrsNote>('ifrs-notes', cancellationToken$),
      deactivate: this.deactivateFactory<IfrsNote>('ifrs-notes', cancellationToken$)
    };
  }

  public productCategoriesApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<ProductCategory>('product-categories', cancellationToken$),
      deactivate: this.deactivateFactory<ProductCategory>('product-categories', cancellationToken$)
    };
  }

  public usersApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<View>('users', cancellationToken$),
      deactivate: this.deactivateFactory<View>('users', cancellationToken$),
      getForClient: () => {
        const url = appconfig.apiAddress + `api/users/client`;
        const obs$ = this.http.get<DataWithVersion<UserSettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      saveForClient: (key: string, value: string) => {
        const keyParam = `key=${encodeURIComponent(key)}`;
        const valueParam = !!value ? `&value=${encodeURIComponent(value)}` : '';
        const url = appconfig.apiAddress + `api/users/client?` + keyParam + valueParam;
        const obs$ = this.http.post<DataWithVersion<UserSettingsForClient>>(url, null).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
      invite: (id: number | string) => {
        this.showRotator = true;
        const url = appconfig.apiAddress + `api/users/invite?id=${id}`;
        const obs$ = this.http.put(url, null).pipe(
          tap(() => this.showRotator = false),
          catchError(error => {
            this.showRotator = false;
            const friendlyError = this.friendly(error);
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
        const url = appconfig.apiAddress + `api/companies/client`;
        const obs$ = this.http.get<UserCompany[]>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
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
        const url = appconfig.apiAddress + `api/global-settings/client`;
        const obs$ = this.http.get<DataWithVersion<GlobalSettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      ping: () => {
        const url = appconfig.apiAddress + `api/global-settings/ping`;
        const obs$ = this.http.get(url).pipe(
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
        const url = appconfig.apiAddress + `api/settings?${params}`;

        const obs$ = this.http.get<GetEntityResponse<Settings>>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getForClient: () => {
        const url = appconfig.apiAddress + `api/settings/client`;
        const obs$ = this.http.get<DataWithVersion<SettingsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      ping: () => {
        const url = appconfig.apiAddress + `api/settings/ping`;
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
        const url = appconfig.apiAddress + `api/settings?${params}`;

        const obs$ = this.http.post<SaveSettingsResponse>(url, entity, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = this.friendly(error);
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
        const url = appconfig.apiAddress + `api/permissions/client`;
        const obs$ = this.http.get<DataWithVersion<PermissionsForClient>>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },
    };
  }

  public crudFactory<TDto extends EntityForSave = EntityForSave, TDtoForSave extends EntityForSave = EntityForSave>(
    endpoint: string, cancellationToken$: Observable<void>) {
    return {
      get: (args: GetArguments) => {
        const paramsArray = this.stringifyGetArguments(args);
        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.get<GetResponse>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      getById: (id: number | string, args: GetByIdArguments) => {
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}/${id}?${params}`;

        const obs$ = this.http.get<GetByIdResponse>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      save: (entities: TDtoForSave[], args: SaveArguments) => {
        this.showRotator = true;
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        paramsArray.push(`returnEntities=${!!args.returnEntities}`);

        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.post<EntitiesResponse>(url, entities, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      delete: (ids: (number | string)[]) => {
        this.showRotator = true;
        const url = appconfig.apiAddress + `api/${endpoint}`;
        const obs$ = this.http.request('DELETE', url, { body: ids }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      deleteWithDescendants: (ids: (number | string)[]) => {
        this.showRotator = true;
        const url = appconfig.apiAddress + `api/${endpoint}/with-descendants`;
        const obs$ = this.http.request('DELETE', url, { body: ids }).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = this.friendly(error);
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
        const url = appconfig.apiAddress + `api/${endpoint}/template?${params}`;
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      },

      import: (args: ImportArguments, files: any) => {
        args = args || {};

        const paramsArray: string[] = [];

        if (!!args.mode) {
          paramsArray.push(`mode=${args.mode}`);
        }

        const formData = new FormData();

        for (const file of files) {
          formData.append(file.name, file);
        }

        this.showRotator = true;
        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}/import?${params}`;
        const obs$ = this.http.post<ImportResult>(url, formData).pipe(
          tap(() => this.showRotator = false),
          catchError((error) => {
            this.showRotator = false;
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.showRotator = false)
        );

        return obs$;
      },

      export: (args: ExportArguments) => {
        const paramsArray = this.stringifyGetArguments(args);

        if (!!args.format) {
          paramsArray.push(`format=${args.format}`);
        }

        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}/export?${params}`;
        const obs$ = this.http.get(url, { responseType: 'blob' }).pipe(
          catchError((error) => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
        );
        return obs$;
      }
    };
  }

  // We refactored this out to support the b-image component
  public getImage(endpoint: string, imageId: string, cancellationToken$: Observable<void>) {

    // Note: cache=true instructs the HTTP interceptor to not add cache-busting parameters
    const url = appconfig.apiAddress + `api/${endpoint}?imageId=${imageId}`;
    const obs$ = this.http.get(url, { responseType: 'blob', observe : 'response' }).pipe(
      map(res => {
        return { image: res.body, imageId: res.headers.get('x-image-id')  };
      }),
      catchError(error => {
        const friendlyError = this.friendly(error);
        return throwError(friendlyError);
      }),
      takeUntil(cancellationToken$)
    );

    return obs$;
  }

  private activateFactory<TDto extends EntityForSave>(endpoint: string, cancellationToken$: Observable<void>) {
    return (ids: (string | number)[], args: ActivateArguments) => {
      args = args || {};

      const paramsArray: string[] = [];

      if (!!args.returnEntities) {
        paramsArray.push(`returnEntities=${args.returnEntities}`);
      }

      if (!!args.expand) {
        paramsArray.push(`expand=${args.expand}`);
      }

      const params: string = paramsArray.join('&');
      const url = appconfig.apiAddress + `api/${endpoint}/activate?${params}`;

      this.showRotator = true;
      const obs$ = this.http.put<EntitiesResponse>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.showRotator = false),
        catchError(error => {
          this.showRotator = false;
          const friendlyError = this.friendly(error);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.showRotator = false)
      );

      return obs$;
    };
  }

  private deactivateFactory<TDto extends EntityForSave>(endpoint: string, cancellationToken$: Observable<void>) {
    return (ids: (string | number)[], args: ActivateArguments) => {
      args = args || {};

      const paramsArray: string[] = [];

      if (!!args.returnEntities) {
        paramsArray.push(`returnEntities=${args.returnEntities}`);
      }

      if (!!args.expand) {
        paramsArray.push(`expand=${args.expand}`);
      }

      const params: string = paramsArray.join('&');
      const url = appconfig.apiAddress + `api/${endpoint}/deactivate?${params}`;

      this.showRotator = true;
      const obs$ = this.http.put<EntitiesResponse>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.showRotator = false),
        catchError(error => {
          this.showRotator = false;
          const friendlyError = this.friendly(error);
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
      paramsArray.push(`desc=${!!args.desc}`);
    }

    if (!!args.inactive) {
      paramsArray.push(`inactive=${args.inactive}`);
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

    return paramsArray;
  }

  // Function to turn status codes into friendly localized human-readable errors
  friendly(error: any): {
    status: number,
    error: any
  } {
    const friendlyStructure = (status: number, err: any) => {
      return {
        status: status,
        error: err
      };
    };

    // Translates HttpClient's errors into human-friendly errors
    if (error instanceof HttpErrorResponse) {
      const res = <HttpErrorResponse>error;

      switch (res.status) {
        case 0: // Offline
        case 504: // Service worker reports
          return friendlyStructure(res.status, this.translate.instant(`Error_UnableToReachServer`));

        case 400: // Bad Request
        case 422: // Unprocessible entity
          if (error.error instanceof Blob) {
            // TODO: Need a better solution to handle blobs
            return friendlyStructure(res.status, this.translate.instant(`Error_UnkownClientError`));
          } else {
            // These two status codes mean a friendly error is already coming from the server
            return friendlyStructure(res.status, res.error);
          }

        case 401:  // Unauthorized
          return friendlyStructure(res.status, this.translate.instant(`Error_LoginSessionExpired`));

        case 403:  // Forbidden
          return friendlyStructure(res.status, this.translate.instant(`Error_AccountDoesNotHaveSufficientPermissions`));

        case 404: // Not found
          return friendlyStructure(res.status, this.translate.instant(`Error_RecordNotFound`));

        case 500:  // Internal Server Error
          return friendlyStructure(res.status, this.translate.instant(`Error_UnhandledServerError`));

        default:  // Any other HTTP error
          return friendlyStructure(res.status, this.translate.instant(`Error_UnkownServerError`));
      }

    } else {
      console.error(error);
      return friendlyStructure(null, this.translate.instant(`Error_UnkownClientError`));
    }
  }

}
