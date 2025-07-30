import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user.model';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-user-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-status.component.html',
  styleUrls: ['./user-status.component.css']
})
export class UserStatusComponent implements OnInit {
  user: User | null = null;
  currentRoute: string = '';

  constructor(
    private userService: UserService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.userService.currentUser.subscribe(user => {
      this.user = user;
    });

    // Track current route
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.currentRoute = event.url;
    });

    // Set initial route
    this.currentRoute = this.router.url;
  }

  // Check if we should show the login link
  shouldShowLoginLink(): boolean {
    // Don't show login link on home page or login page
    return this.currentRoute !== '/' && this.currentRoute !== '/login';
  }

  login() {
    this.router.navigate(['/login']);
    return false;
  }

  goToProfile() {
    this.router.navigate(['/profile']);
    return false;
  }

  logout() {
    this.userService.logout();
    this.router.navigate(['']);
    return false;
  }
}
