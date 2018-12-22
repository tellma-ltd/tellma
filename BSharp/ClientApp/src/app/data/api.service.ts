import { Injectable } from '@angular/core';
import { GetArguments } from './dto/get-arguments';
import { GetByIdArguments } from './dto/get-by-id-arguments';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { takeUntil, catchError, tap, finalize, map } from 'rxjs/operators';
import { GetResponse } from './dto/get-response';
import { DtoForSaveKeyBase } from './dto/dto-for-save-key-base';
import { MeasurementUnit, MeasurementUnitForSave } from './dto/measurement-unit';
import { ActivateArguments } from './dto/activate-arguments';
import { TranslateService } from '@ngx-translate/core';

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

  public crudFactory<TDto extends DtoForSaveKeyBase = DtoForSaveKeyBase, TDtoForSave extends DtoForSaveKeyBase = DtoForSaveKeyBase>(
    endpoint: string, cancellationToken$: Observable<void>) {
    return {
      get: (args: GetArguments) => {
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
          paramsArray.push(`expand=${encodeURIComponent(args.expand)})`);
        }

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

      getById: (args: GetByIdArguments) => {
        // TODO
      }
    };
  }

  private activateFactory<TDto extends DtoForSaveKeyBase>(endpoint: string, cancellationToken$: Observable<void>) {
    return (args: ActivateArguments) => {
      const url = appconfig.apiAddress + `api/${endpoint}/activate`;
      const obs$ = this.http.post<TDto[]>(url, args, {
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
    return (args: ActivateArguments) => {
      const url = appconfig.apiAddress + `api/${endpoint}/deactivate`;
      const obs$ = this.http.post<TDto[]>(url, args, {
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
          return friendlyStructure(res.status, this.translate.instant(`Error_UnkownClientError`));
      }

    } else {
      console.error(error);
      return friendlyStructure(null, this.translate.instant(`Error_UnkownClientError`));
    }
  }
}
