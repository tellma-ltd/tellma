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
        setHeaders: { 'Tenant-Id': tenantId.toString() },
        setParams: { 't': encodeURIComponent(new Date().getTime().toString()) }
        // ^ adding current time to the query params to prevent
        // the browser from using cached GET responses
      });
    }

    const culture = this.workspace.ws.culture;
    if (!!culture) {
      req = req.clone({
        setParams: { 'ui-culture': culture }
      });
    }


    // TODO add authorization header
    // TODO add cache versions and intercept responses
    // TODO intercept 401 responses and log the user out
    // TODO add culture to the query url
    // TODO add time stamp to prevent

    return next.handle(req);
  }
}
