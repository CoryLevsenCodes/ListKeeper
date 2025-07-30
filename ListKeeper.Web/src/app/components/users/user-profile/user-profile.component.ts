import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { MfaService, MfaStatusResponse } from '../../../services/mfa.service';

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

        <!-- MFA Section -->
        <div class="profile-section">
          <h3>🔐 Multi-Factor Authentication</h3>
          
          <div *ngIf="mfaLoading" class="mfa-loading">
            <p>Loading MFA status...</p>
          </div>

          <div *ngIf="!mfaLoading && mfaStatus" class="mfa-content">
            <!-- MFA Enabled State -->
            <div *ngIf="mfaStatus.isEnabled" class="mfa-enabled">
              <div class="mfa-status-badge enabled">
                ✅ MFA is Enabled
              </div>
              <p>Your account is protected with multi-factor authentication.</p>
              
              <div class="mfa-actions">
                <button class="btn-secondary" (click)="regenerateBackupCodes()">
                  🔄 Regenerate Backup Codes
                </button>
                <button class="btn-danger" (click)="disableMfa()">
                  ❌ Disable MFA
                </button>
              </div>

              <div class="mfa-info">
                <p><strong>Backup codes remaining:</strong> {{ mfaStatus.backupCodesRemaining || 0 }}</p>
                <p><strong>Setup date:</strong> {{ mfaStatus.setupDate || 'Unknown' }}</p>
              </div>
            </div>

            <!-- MFA Disabled State -->
            <div *ngIf="!mfaStatus.isEnabled" class="mfa-disabled">
              <div class="mfa-status-badge disabled">
                ❌ MFA is Disabled
              </div>
              <p>Enhance your account security by enabling multi-factor authentication.</p>
              
              <div class="mfa-benefits">
                <h4>Benefits of enabling MFA:</h4>
                <ul>
                  <li>🛡️ Enhanced security protection</li>
                  <li>🚫 Prevent unauthorized access</li>
                  <li>📱 Easy setup with authenticator apps</li>
                  <li>🔐 Backup codes for emergency access</li>
                </ul>
              </div>

              <button class="btn-primary btn-large" (click)="enableMfa()">
                🔐 Enable MFA
              </button>
            </div>
          </div>

          <div *ngIf="mfaError" class="error-message">
            <p>{{ mfaError }}</p>
            <button class="btn-secondary" (click)="retryLoadMfaStatus()">🔄 Retry</button>
          </div>
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

    .mfa-loading {
      text-align: center;
      padding: 20px;
      color: #666;
    }

    .mfa-status-badge {
      padding: 8px 16px;
      border-radius: 20px;
      font-weight: bold;
      margin-bottom: 15px;
      display: inline-block;
    }

    .mfa-status-badge.enabled {
      background: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }

    .mfa-status-badge.disabled {
      background: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }

    .mfa-actions {
      display: flex;
      gap: 10px;
      margin: 20px 0;
      flex-wrap: wrap;
    }

    .mfa-info {
      background: #f8f9fa;
      padding: 15px;
      border-radius: 6px;
      margin-top: 15px;
    }

    .mfa-info p {
      margin: 5px 0;
      font-size: 14px;
    }

    .mfa-benefits {
      margin: 15px 0;
    }

    .mfa-benefits h4 {
      color: #333;
      margin-bottom: 10px;
    }

    .mfa-benefits ul {
      list-style: none;
      padding-left: 0;
    }

    .mfa-benefits li {
      padding: 5px 0;
      font-size: 14px;
    }

    .btn-large {
      padding: 15px 30px;
      font-size: 16px;
      margin-top: 20px;
    }

    .btn-danger {
      background: #dc3545;
      color: white;
      padding: 10px 20px;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 500;
    }

    .btn-danger:hover {
      background: #c82333;
    }

    .error-message {
      background: #f8d7da;
      color: #721c24;
      padding: 15px;
      border-radius: 6px;
      border: 1px solid #f5c6cb;
      margin-top: 15px;
    }
  `]
})
export class UserProfileComponent implements OnInit {
  currentUser: any = null;
  isLoading = true;
  mfaStatus: MfaStatusResponse | null = null;
  mfaLoading = false;
  mfaError: string | null = null;

  constructor(
    private router: Router,
    private userService: UserService,
    private mfaService: MfaService
  ) {}

  ngOnInit() {
    this.loadUserData();
    this.loadMfaStatus();
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

  private loadMfaStatus() {
    this.mfaLoading = true;
    this.mfaError = null;
    
    this.mfaService.getMfaStatus().subscribe({
      next: (status) => {
        this.mfaStatus = status;
        this.mfaLoading = false;
      },
      error: (error) => {
        console.error('Error loading MFA status:', error);
        this.mfaError = 'Failed to load MFA status. Please try again.';
        this.mfaLoading = false;
      }
    });
  }

  enableMfa() {
    this.router.navigate(['/profile/mfa-setup']);
  }

  disableMfa() {
    if (confirm('Are you sure you want to disable Multi-Factor Authentication? This will make your account less secure.')) {
      // Prompt for current password and MFA code
      const password = prompt('Please enter your current password to confirm:');
      if (!password) {
        return;
      }

      const mfaCode = prompt('Please enter your current 6-digit MFA code to verify:');
      if (!mfaCode || mfaCode.length !== 6) {
        alert('Please enter a valid 6-digit MFA code.');
        return;
      }

      const disableRequest = {
        currentPassword: password,
        verificationCode: mfaCode,
        isBackupCode: false
      };

      this.mfaService.disableMfa(disableRequest).subscribe({
        next: (response) => {
          alert('MFA has been disabled successfully. Your account is now less secure.');
          // Refresh MFA status to show updated state
          this.loadMfaStatus();
        },
        error: (error) => {
          console.error('Error disabling MFA:', error);
          this.mfaError = 'Failed to disable MFA. Please check your password and verification code.';
        }
      });
    }
  }

  regenerateBackupCodes() {
    if (confirm('Generate new backup codes? Your current backup codes will become invalid.')) {
      this.mfaService.regenerateBackupCodes().subscribe({
        next: (response) => {
          // Refresh MFA status to show updated backup codes count
          this.loadMfaStatus();
          // Navigate to backup codes display page
          this.router.navigate(['/profile/backup-codes'], {
            state: { backupCodes: response.backupCodes, showMessage: true }
          });
        },
        error: (error) => {
          console.error('Error regenerating backup codes:', error);
          this.mfaError = 'Failed to regenerate backup codes. Please try again.';
        }
      });
    }
  }

  retryLoadMfaStatus() {
    this.loadMfaStatus();
  }

  goBack() {
    this.router.navigate(['/notes']);
  }
}
