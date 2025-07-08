import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { NoteListComponent } from './components/notes/note-list/note-list.component';
import { SignupComponent } from './components/users/signup/signup.component';
import { LoginComponent } from './components/users/login/login.component';
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
