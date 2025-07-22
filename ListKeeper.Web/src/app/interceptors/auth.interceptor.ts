import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(
        private userService: UserService,
        private router: Router
    ) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Get the current user and token
        const currentUser = this.userService.currentUserValue;
        const isLoggedIn = currentUser && currentUser.token;

        // Check if this is an API request (not external URLs)
        const isApiUrl = request.url.includes('/api/');

        if (isLoggedIn && isApiUrl) {
            // Clone the request and add the Authorization header with Bearer token
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${currentUser.token}`
                }
            });
        }

        return next.handle(request).pipe(
            catchError((error: HttpErrorResponse) => {
                // If we get a 401 Unauthorized response, logout and redirect to login
                if (error.status === 401) {
                    this.userService.logout();
                    this.router.navigate(['']);
                }
                return throwError(() => error);
            })
        );
    }
}
