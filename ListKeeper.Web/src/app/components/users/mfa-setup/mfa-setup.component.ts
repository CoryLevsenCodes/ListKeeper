import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MfaService, MfaSetupResponse } from '../../../services/mfa.service';
import { UserService } from '../../../services/user.service';

@Component({
  selector: 'app-mfa-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './mfa-setup.component.html',
  styleUrls: ['./mfa-setup.component.css']
})
export class MfaSetupComponent implements OnInit {
  setupForm: FormGroup;
  currentStep = 1;
  maxSteps = 3;
  
  setupData: MfaSetupResponse | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  
  currentUser: any = null;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private mfaService: MfaService,
    private userService: UserService
  ) {
    this.setupForm = this.fb.group({
      verificationCode: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  ngOnInit() {
    this.loadCurrentUser();
    this.startMfaSetup();
  }

  private loadCurrentUser() {
    // Use UserService to get current user consistently
    const user = this.userService.currentUserValue;
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }
    
    this.currentUser = {
      email: user.email,
      username: user.username
    };
  }

  startMfaSetup() {
    if (!this.currentUser?.email) return;

    this.isLoading = true;
    this.mfaService.setupMfa({ email: this.currentUser.email }).subscribe({
      next: (response) => {
        this.setupData = response;
        this.isLoading = false;
        this.currentStep = 2; // Move to QR code step
      },
      error: (error) => {
        this.errorMessage = 'Failed to initialize MFA setup. Please try again.';
        this.isLoading = false;
        console.error('MFA setup error:', error);
      }
    });
  }

  nextStep() {
    if (this.currentStep < this.maxSteps) {
      this.currentStep++;
    }
  }

  previousStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  verifyAndEnable() {
    if (this.setupForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const verificationCode = this.setupForm.value.verificationCode;
      
      this.mfaService.enableMfa({ verificationCode }).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.successMessage = 'MFA enabled successfully!';
          this.currentStep = 3; // Move to success step
          
          // Update the current user's MFA status in UserService
          this.updateUserMfaStatus(true);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Invalid verification code. Please try again.';
          console.error('MFA enable error:', error);
        }
      });
    } else {
      this.setupForm.get('verificationCode')?.markAsTouched();
    }
  }

  downloadBackupCodes() {
    if (!this.setupData?.backupCodes) return;

    const codesText = this.setupData.backupCodes.join('\n');
    const blob = new Blob([codesText], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = 'listkeeper-backup-codes.txt';
    link.click();
    
    window.URL.revokeObjectURL(url);
  }

  copyBackupCodes() {
    if (!this.setupData?.backupCodes) return;

    const codesText = this.setupData.backupCodes.join('\n');
    navigator.clipboard.writeText(codesText).then(() => {
      this.successMessage = 'Backup codes copied to clipboard!';
      setTimeout(() => this.successMessage = '', 3000);
    }).catch(() => {
      this.errorMessage = 'Failed to copy backup codes. Please manually save them.';
    });
  }

  finishSetup() {
    this.router.navigate(['/profile'], { 
      queryParams: { mfaEnabled: 'true' } 
    });
  }

  cancel() {
    this.router.navigate(['/profile']);
  }

  // Helper method to get step title
  getStepTitle(): string {
    switch (this.currentStep) {
      case 1: return 'Initializing MFA Setup...';
      case 2: return 'Scan QR Code & Verify';
      case 3: return 'MFA Setup Complete!';
      default: return 'MFA Setup';
    }
  }

  // Helper method to check if step is complete
  isStepComplete(step: number): boolean {
    return this.currentStep > step;
  }

  // Helper method to check if step is current
  isStepCurrent(step: number): boolean {
    return this.currentStep === step;
  }

  /**
   * Update the current user's MFA status in UserService and localStorage
   */
  private updateUserMfaStatus(isMfaEnabled: boolean): void {
    const currentUser = this.userService.currentUserValue;
    if (currentUser) {
      // Update the user object with new MFA status
      const updatedUser = { ...currentUser, isMfaEnabled, mfaSetupDate: new Date() };
      
      // Update the current user in UserService
      // This will update both the BehaviorSubject and localStorage
      this.userService.updateCurrentUser(updatedUser);
    }
  }

  // Copy secret key to clipboard
  copySecretKey() {
    if (this.setupData?.secretKey) {
      navigator.clipboard.writeText(this.setupData.secretKey).then(() => {
        this.successMessage = 'Secret key copied to clipboard!';
        setTimeout(() => this.successMessage = '', 3000);
      }).catch(err => {
        console.error('Failed to copy:', err);
        this.errorMessage = 'Failed to copy secret key';
      });
    }
  }
}
