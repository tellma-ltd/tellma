import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkspaceService } from './workspace.service';

export class RootHttpInterceptor implements HttpInterceptor {

  constructor(private workspace: WorkspaceService) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // Set the Tenant-Id in the headers
    // This piece of logic does not really belong to the root module and is
    // specific to the application module, but moving it there is not worth
    // the hassle now
    const tenantId = this.workspace.ws.tenantId;
    if (!!tenantId) {
      req = req.clone({
        setHeaders: { 'Tenant-Id': tenantId.toString() }
      });
    }


    // TODO add authorization header
    // TODO add cache versions and intercept responses
    // TODO intercept 401 responses and log the user out

    return next.handle(req);
  }
}
