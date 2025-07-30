import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { NoteListComponent } from './components/notes/note-list/note-list.component';
import { SignupComponent } from './components/users/signup/signup.component';
import { LoginComponent } from './components/users/login/login.component';
import { UserProfileComponent } from './components/users/user-profile/user-profile.component';
import { MfaSetupComponent } from './components/users/mfa-setup/mfa-setup.component';
import { MfaVerificationComponent } from './components/users/mfa-verification/mfa-verification.component';
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
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

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
