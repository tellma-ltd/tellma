import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';


export class RootHttpInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    //// Prefixes every 
    //if (config.apiAddress) {
    //  let address = config.apiAddress;
    //  if (!address.endsWith('/')) {
    //    address = address + '/';
    //  }

    //  req = req.clone({
    //    url : `${address}${req.url}`
    //  });
    //}

    // TODO add authorization header
    // TODO add cache versions and intercept responses
    // TODO intercept 401 responses and log the user out

    return next.handle(req);
  }
}
