import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="profile-container">
      <div class="profile-header">
        <h2>👤 User Profile</h2>
        <button class="btn-secondary" (click)="goBack()">← Back to Notes</button>
      </div>

      <div class="profile-content" *ngIf="!isLoading && currentUser">
        <!-- User Information Section -->
        <div class="profile-section">
          <h3>📋 Account Information</h3>
          <div class="user-info">
            <div class="info-row">
              <label>Email:</label>
              <span>{{ currentUser.email }}</span>
            </div>
            <div class="info-row">
              <label>Username:</label>
              <span>{{ currentUser.username }}</span>
            </div>
            <div class="info-row">
              <label>First Name:</label>
              <span>{{ currentUser.firstname }}</span>
            </div>
            <div class="info-row">
              <label>Last Name:</label>
              <span>{{ currentUser.lastname }}</span>
            </div>
          </div>
        </div>

        <!-- MFA Section Placeholder -->
        <div class="profile-section">
          <h3>🔐 Multi-Factor Authentication</h3>
          <p>MFA setup will be available here once backend integration is complete.</p>
          <button class="btn-primary" disabled>Enable MFA (Coming Soon)</button>
        </div>
      </div>

      <div *ngIf="isLoading" class="loading">
        <p>Loading profile...</p>
      </div>

      <div *ngIf="!currentUser && !isLoading" class="error">
        <p>Unable to load profile. Please log in again.</p>
      </div>
    </div>
  `,
  styles: [`
    .profile-container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
    }

    .profile-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
      padding-bottom: 20px;
      border-bottom: 2px solid #f0f0f0;
    }

    .profile-section {
      background: white;
      border-radius: 12px;
      padding: 25px;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
      margin-bottom: 20px;
    }

    .user-info {
      display: grid;
      gap: 15px;
    }

    .info-row {
      display: flex;
      gap: 10px;
    }

    .info-row label {
      font-weight: bold;
      min-width: 120px;
    }

    .btn-secondary, .btn-primary {
      padding: 10px 20px;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 500;
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .btn-primary {
      background: #007bff;
      color: white;
    }

    .btn-primary:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .loading, .error {
      text-align: center;
      padding: 40px;
    }
  `]
})
export class UserProfileComponent implements OnInit {
  currentUser: any = null;
  isLoading = true;

  constructor(
    private router: Router,
    private userService: UserService
  ) {}

  ngOnInit() {
    this.loadUserData();
  }

  private loadUserData() {
    // Use the same user service as other components
    this.userService.currentUser.subscribe({
      next: (user) => {
        this.currentUser = user;
        this.isLoading = false;
        
        if (!user) {
          // If no user, redirect to login
          this.router.navigate(['/login']);
        }
      },
      error: (error) => {
        console.error('Error loading user:', error);
        this.isLoading = false;
        this.router.navigate(['/login']);
      }
    });
  }

  goBack() {
    this.router.navigate(['/notes']);
  }
}
