import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpRequest, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

// Kindly provided by https://bit.ly/3hcg7GR
@Injectable()
export class BlobErrorHttpInterceptor implements HttpInterceptor {
    public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError(err => {
                if (err instanceof HttpErrorResponse && err.error instanceof Blob) {
                    if (err.error.type === 'text/plain' || err.error.type === 'application/json') {
                        return new Promise<any>((_, reject) => {
                            const reader = new FileReader();
                            reader.onload = (e: Event) => {
                                try {
                                    let errorMsg = (e.target as any).result;
                                    if (err.error.type === 'application/json') {
                                        errorMsg = JSON.parse(errorMsg);
                                    }

                                    reject(new HttpErrorResponse({
                                        error: errorMsg,
                                        headers: err.headers,
                                        status: err.status,
                                        statusText: err.statusText,
                                        url: err.url
                                    }));
                                } catch (e) {
                                    reject(err);
                                }
                            };
                            reader.onerror = (e) => {
                                reject(err);
                            };
                            reader.readAsText(err.error);
                        });
                    }
                }

                return throwError(err);
            })
        );
    }
}
