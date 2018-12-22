import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkspaceService } from './workspace.service';

export class ApplicationHttpInterceptor implements HttpInterceptor {

  constructor(private workspace: WorkspaceService) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    const tenantId = this.workspace.ws.tenantId;
    if (!!tenantId) {
      req = req.clone({
        setHeaders: { 'Tenant-Id': tenantId.toString() } // TODO: implement properly
      });
    }

    return next.handle(req);
  }
}
