import { HttpClient, HttpErrorResponse, HttpHeaders, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, takeUntil, tap } from 'rxjs/operators';
import { ActivateArguments } from './dto/activate-arguments';
import { DtoForSaveKeyBase } from './dto/dto-for-save-key-base';
import { GetArguments } from './dto/get-arguments';
import { GetByIdArguments } from './dto/get-by-id-arguments';
import { GetResponse, EntitiesResponse } from './dto/get-response';
import { MeasurementUnit } from './dto/measurement-unit';
import { TemplateArguments } from './dto/template-arguments';
import { ImportArguments } from './dto/import-arguments';
import { ImportResult } from './dto/import-result';
import { ExportArguments } from './dto/export-arguments';
import { GetByIdResponse } from './dto/get-by-id-response';
import { SaveArguments } from './dto/save-arguments';
import { appconfig } from './appconfig';
import { Agent } from './dto/agent';
import { Role } from './dto/role';
import { View } from './dto/view';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  public saveInProgress = false;

  // Will abstract away standard API calls for CRUD operations
  constructor(public http: HttpClient, public translate: TranslateService) {
  }

  public measurementUnitsApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<MeasurementUnit>('measurement-units', cancellationToken$),
      deactivate: this.deactivateFactory<MeasurementUnit>('measurement-units', cancellationToken$)
    };
  }

  public agentsApi(agentType: string, cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<Agent>(`agents/${agentType}`, cancellationToken$),
      deactivate: this.deactivateFactory<Agent>(`agents/${agentType}`, cancellationToken$)
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

  public localUsersApi(cancellationToken$: Observable<void>) {
    return {
      activate: this.activateFactory<View>('local-users', cancellationToken$),
      deactivate: this.deactivateFactory<View>('local-users', cancellationToken$)
    };
  }

  public crudFactory<TDto extends DtoForSaveKeyBase = DtoForSaveKeyBase, TDtoForSave extends DtoForSaveKeyBase = DtoForSaveKeyBase>(
    endpoint: string, cancellationToken$: Observable<void>) {
    return {
      get: (args: GetArguments) => {
        const paramsArray = this.stringifyGetArguments(args);
        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.get<GetResponse<TDto>>(url).pipe(
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

        const obs$ = this.http.get<GetByIdResponse<TDto>>(url).pipe(
          catchError(error => {
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$)
        );

        return obs$;
      },

      save: (entities: TDtoForSave[], args: SaveArguments) => {
        this.saveInProgress = true;
        args = args || {};
        const paramsArray: string[] = [];

        if (!!args.expand) {
          paramsArray.push(`expand=${encodeURIComponent(args.expand)}`);
        }

        paramsArray.push(`returnEntities=${!!args.returnEntities}`);

        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}?${params}`;

        const obs$ = this.http.post<EntitiesResponse<TDto>>(url, entities, {
          headers: new HttpHeaders({ 'Content-Type': 'application/json' })
        }).pipe(
          tap(() => this.saveInProgress = false),
          catchError((error) => {
            this.saveInProgress = false;
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.saveInProgress = false)
        );

        return obs$;
      },

      delete: (ids: (number | string)[]) => {
        this.saveInProgress = true;
        const url = appconfig.apiAddress + `api/${endpoint}`;
        const obs$ = this.http.request('DELETE', url, { body: ids }).pipe(
          tap(() => this.saveInProgress = false),
          catchError((error) => {
            this.saveInProgress = false;
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.saveInProgress = false)
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

        this.saveInProgress = true;
        const params: string = paramsArray.join('&');
        const url = appconfig.apiAddress + `api/${endpoint}/import?${params}`;
        const obs$ = this.http.post<ImportResult>(url, formData).pipe(
          tap(() => this.saveInProgress = false),
          catchError((error) => {
            this.saveInProgress = false;
            const friendlyError = this.friendly(error);
            return throwError(friendlyError);
          }),
          takeUntil(cancellationToken$),
          finalize(() => this.saveInProgress = false)
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

  private activateFactory<TDto extends DtoForSaveKeyBase>(endpoint: string, cancellationToken$: Observable<void>) {
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

      this.saveInProgress = true;
      const obs$ = this.http.put<EntitiesResponse<TDto>>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.saveInProgress = false),
        catchError(error => {
          this.saveInProgress = false;
          const friendlyError = this.friendly(error);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.saveInProgress = false)
      );

      return obs$;
    };
  }

  private deactivateFactory<TDto extends DtoForSaveKeyBase>(endpoint: string, cancellationToken$: Observable<void>) {
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

      this.saveInProgress = true;
      const obs$ = this.http.put<EntitiesResponse<TDto>>(url, ids, {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
      }).pipe(
        tap(() => this.saveInProgress = false),
        catchError(error => {
          this.saveInProgress = false;
          const friendlyError = this.friendly(error);
          return throwError(friendlyError);
        }),
        takeUntil(cancellationToken$),
        finalize(() => this.saveInProgress = false)
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

    if (!!args.orderBy) {
      paramsArray.push(`orderBy=${args.orderBy}`);
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

    return paramsArray;
  }

  // Function to turn status codes into friendly localized human-readable errors
  friendly(error: any) {
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
