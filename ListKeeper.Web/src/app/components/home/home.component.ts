import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  currentUser: User | null = null;

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if user is logged in and redirect to notes if they are
    this.userService.currentUser.subscribe(user => {
      this.currentUser = user;
      if (user) {
        // If user is logged in, redirect to notes page
        this.router.navigate(['/notes']);
      }
    });
  }

  navigateToSignup(): void {
    this.router.navigate(['/signup']);
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }
}
