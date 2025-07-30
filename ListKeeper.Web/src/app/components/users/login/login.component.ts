import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { first } from 'rxjs/operators';
import { CommonModule } from '@angular/common';

declare var window: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  error = '';
  isLoading = false;
  showMfaInput = false;
  mfaToken = '';
  mfaForm!: FormGroup;

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.mfaForm = this.formBuilder.group({
      mfaCode: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  navigateToSignup(): void {
    this.router.navigate(['/signup']);
  }

  // convenience getter for easy access to form fields
  get f() { return this.loginForm.controls; }

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.error = '';
    this.isLoading = true;
    
    this.userService.login(this.f['username'].value, this.f['password'].value)
      .pipe(first())
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          
          // Check if MFA is required
          if (response.mfaRequired) {
            this.mfaToken = response.mfaToken;
            this.showMfaInput = true;
          } else {
            // Direct login success, navigate to notes
            this.router.navigate(['/notes']);
          }
        },
        error: error => {
          this.isLoading = false;
          this.error = 'Login failed. Please check your credentials.';
        }
      });
  }

  onMfaSubmit() {
    if (this.mfaForm.invalid) {
      return;
    }

    this.error = '';
    this.isLoading = true;

    const mfaVerificationRequest = {
      emailOrUsername: this.f['username'].value,
      password: this.f['password'].value,
      mfaCode: this.mfaForm.get('mfaCode')?.value,
      mfaToken: this.mfaToken
    };

    // Use the MFA service to verify and complete login
    this.verifyMfaAndLogin(mfaVerificationRequest);
  }

  private verifyMfaAndLogin(request: any) {
    // Make API call to verify MFA and get complete user data
    this.userService.verifyMfaLogin(request)
      .pipe(first())
      .subscribe({
        next: (user) => {
          this.isLoading = false;
          // Navigate to notes page after successful MFA verification
          this.router.navigate(['/notes']);
        },
        error: (error) => {
          this.isLoading = false;
          this.error = 'Invalid MFA code. Please try again.';
        }
      });
  }
}
