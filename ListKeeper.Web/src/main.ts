import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { importProvidersFrom } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { AuthInterceptor } from './app/interceptors/auth.interceptor';

// Import routes directly instead of the module
import { HomeComponent } from './app/components/home/home.component';
import { NoteListComponent } from './app/components/notes/note-list/note-list.component';
import { SignupComponent } from './app/components/users/signup/signup.component';
import { LoginComponent } from './app/components/users/login/login.component';
import { UserProfileComponent } from './app/components/users/user-profile/user-profile.component';
import { MfaSetupComponent } from './app/components/users/mfa-setup/mfa-setup.component';
import { MfaVerificationComponent } from './app/components/users/mfa-verification/mfa-verification.component';
import { AuthGuard } from './app/guards/auth.guard';

const routes = [
  { 
    path: '', 
    component: HomeComponent,
    data: { debug: 'Root route - HomeComponent' }
  },
  { 
    path: 'notes', 
    component: NoteListComponent, 
    canActivate: [AuthGuard],
    data: { debug: 'Notes route - NoteListComponent' }
  },
  { 
    path: 'profile', 
    component: UserProfileComponent, 
    canActivate: [AuthGuard],
    data: { debug: 'Profile route - UserProfileComponent' }
  },
  { 
    path: 'profile/mfa-setup', 
    component: MfaSetupComponent, 
    canActivate: [AuthGuard],
    data: { debug: 'MFA Setup route - MfaSetupComponent' }
  },
  { 
    path: 'profile/mfa-verification', 
    component: MfaVerificationComponent, 
    canActivate: [AuthGuard],
    data: { debug: 'MFA Verification route - MfaVerificationComponent' }
  },
  { 
    path: 'signup', 
    component: SignupComponent,
    data: { debug: 'Signup route - SignupComponent' }
  },
  { 
    path: 'login', 
    component: LoginComponent,
    data: { debug: 'Login route - LoginComponent' }
  },
  { 
    path: '**', 
    redirectTo: '',
    data: { debug: 'Wildcard route - redirect to root' }
  }
];

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom(HttpClientModule),
    provideRouter(routes),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
}).catch(err => console.error(err));
