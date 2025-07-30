import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MfaService } from '../../../services/mfa.service';

@Component({
  selector: 'app-backup-code-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './backup-code-verification.component.html',
  styleUrls: ['./backup-code-verification.component.css']
})
export class BackupCodeVerificationComponent {
  @Input() username!: string;
  @Output() verificationComplete = new EventEmitter<boolean>();
  @Output() backToMfa = new EventEmitter<void>();

  backupForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  attemptsRemaining = 5;

  constructor(
    private fb: FormBuilder,
    private mfaService: MfaService
  ) {
    this.backupForm = this.fb.group({
      backupCode: ['', [
        Validators.required,
        Validators.pattern(/^[A-Z0-9]{8}$/)
      ]]
    });
  }

  async onSubmit() {
    if (this.backupForm.valid && !this.isLoading) {
      this.isLoading = true;
      this.errorMessage = '';

      const backupCode = this.backupForm.get('backupCode')?.value;

      try {
        const result = await this.mfaService.verifyBackupCode(backupCode).toPromise();
        
        if (result?.success) {
          this.verificationComplete.emit(true);
        } else {
          this.handleVerificationError();
        }
      } catch (error) {
        this.handleVerificationError();
      } finally {
        this.isLoading = false;
      }
    }
  }

  private handleVerificationError() {
    this.attemptsRemaining--;
    
    if (this.attemptsRemaining <= 0) {
      this.errorMessage = 'Too many failed attempts. Please contact support for assistance.';
    } else {
      this.errorMessage = `Invalid backup code. ${this.attemptsRemaining} attempts remaining.`;
    }
    
    // Clear the form
    this.backupForm.get('backupCode')?.setValue('');
  }

  onBackToMfa() {
    this.backToMfa.emit();
  }

  onCancel() {
    this.verificationComplete.emit(false);
  }

  // Helper method to format input as user types
  onCodeInput(event: any) {
    let value = event.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    if (value.length > 8) {
      value = value.slice(0, 8);
    }
    this.backupForm.get('backupCode')?.setValue(value);
  }

  get backupCode() {
    return this.backupForm.get('backupCode');
  }
}
