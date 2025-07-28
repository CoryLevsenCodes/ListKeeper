import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service'; 

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css']
})
export class SignupComponent {
  signupForm: FormGroup;
  isSubmitting = false;
  showSuccessMessage = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private userService: UserService
  ) {
    this.signupForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      agreeToTerms: [false, Validators.requiredTrue],
      wantsUpdates: [true], // Pre-checked because who doesn't want updates? 😉
      favoriteTimHortonsItem: ['Double-Double'], // Because we're Canadian!
      
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  onSubmit() { 
  if (this.signupForm.valid) { 
    this.isSubmitting = true; 
    this.errorMessage = ''; 
    
    // Prepare signup data 
    const signupData = { 
      firstName: this.signupForm.value.firstName, 
      lastName: this.signupForm.value.lastName, 
      email: this.signupForm.value.email, 
      username: this.signupForm.value.username, 
      password: this.signupForm.value.password, 
      confirmPassword: this.signupForm.value.confirmPassword, 
      agreeToTerms: this.signupForm.value.agreeToTerms, 
      wantsUpdates: this.signupForm.value.wantsUpdates, 
      favoriteTimHortonsItem: this.signupForm.value.favoriteTimHortonsItem 
    }; 
 
    // Call the signup service 
    this.userService.signup(signupData).subscribe({ 
      next: (response) => { 
        this.isSubmitting = false; 
        this.showSuccessMessage = true; 
        
        // Auto-redirect after showing success message 
        setTimeout(() => { 
          this.router.navigate(['/notes']); 
        }, 2000); 
      }, 
      error: (error) => { 
        this.isSubmitting = false; 
        console.error('Signup error:', error); 
        this.errorMessage = error.error?.message || 'Signup failed. Please try again.'; 
      } 
    }); 
  } else { 
    // Mark all fields as touched to show validation errors 
    Object.keys(this.signupForm.controls).forEach(key => { 
      this.signupForm.get(key)?.markAsTouched(); 
    }); 
  } 
}

  goBack() {
    this.router.navigate(['/notes']);
  }

  buyTimHortonsCoffee() {
    // This would typically open a payment modal or redirect to a payment page
    alert('Eh! Thanks for wanting to support us! 🇨🇦 Unfortunately, our Tim Hortons payment system is still in beta (like our hockey skills). But your kindness is appreciated! ☕');
  }
}
