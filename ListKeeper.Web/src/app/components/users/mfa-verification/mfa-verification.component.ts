import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MfaService } from '../../../services/mfa.service';

@Component({
  selector: 'app-mfa-verification',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './mfa-verification.component.html',
  styleUrls: ['./mfa-verification.component.css']
})
export class MfaVerificationComponent {
  @Input() username!: string;
  @Output() verificationComplete = new EventEmitter<boolean>();
  @Output() useBackupCode = new EventEmitter<void>();

  verificationForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  attemptsRemaining = 3;
  showBackupCodeOption = false;

  constructor(
    private fb: FormBuilder,
    private mfaService: MfaService
  ) {
    this.verificationForm = this.fb.group({
      verificationCode: ['', [
        Validators.required,
        Validators.pattern(/^\d{6}$/)
      ]]
    });
  }

  ngOnInit() {
    // Show backup code option after first failed attempt
    setTimeout(() => {
      this.showBackupCodeOption = true;
    }, 10000); // Show after 10 seconds
  }

  async onSubmit() {
    if (this.verificationForm.valid && !this.isLoading) {
      this.isLoading = true;
      this.errorMessage = '';

      const verificationCode = this.verificationForm.get('verificationCode')?.value;

      try {
        const result = await this.mfaService.verifyMfa(verificationCode).toPromise();
        
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
      this.errorMessage = 'Too many failed attempts. Please try again later or use a backup code.';
      this.showBackupCodeOption = true;
    } else {
      this.errorMessage = `Invalid verification code. ${this.attemptsRemaining} attempts remaining.`;
    }
    
    // Clear the form
    this.verificationForm.get('verificationCode')?.setValue('');
    
    // Show backup code option after 2 failed attempts
    if (this.attemptsRemaining <= 1) {
      this.showBackupCodeOption = true;
    }
  }

  onUseBackupCode() {
    this.useBackupCode.emit();
  }

  onCancel() {
    this.verificationComplete.emit(false);
  }

  // Helper method to format input as user types
  onCodeInput(event: any) {
    let value = event.target.value.replace(/\D/g, ''); // Remove non-digits
    if (value.length > 6) {
      value = value.slice(0, 6);
    }
    this.verificationForm.get('verificationCode')?.setValue(value);
  }

  get verificationCode() {
    return this.verificationForm.get('verificationCode');
  }
}
