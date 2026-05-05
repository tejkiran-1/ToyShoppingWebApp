import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
    email: string = '';
    password: string = '';
    isLoading: boolean = false;
    errorMessage: string = '';

    constructor( private authService: AuthService, private router: Router) {
        console.log('✅ LoginComponent initialized');
    }

    onLogin(): void {
        // 1️⃣ Validation
        if (!this.email || !this.password) {
            this.errorMessage = 'Email and password are required!';
            return;
        }

        // 2️⃣ Show loading state
        this.isLoading = true;
        this.errorMessage = '';

        // 3️⃣ Call AuthService.login()
        this.authService.login(this.email, this.password).subscribe({
            next: (response) => {
            console.log('✅ Login successful!', response);
            
            // 4️⃣ Get user role from response
            const userRole = response.user?.role;
            console.log('User role from response:', userRole);
            
            // 5️⃣ Redirect based on role
            if (userRole === 'Admin') {
                console.log('🔴 Redirecting to admin dashboard...');
                this.router.navigate(['/admin']);
            } else if (userRole === 'Customer') {
                console.log('🔵 Redirecting to customer dashboard...');
                this.router.navigate(['/customer']);
            } else {
                console.warn('⚠️ Unknown role:', userRole);
                this.errorMessage = 'Unable to determine user role!';
                this.isLoading = false;
            }
            },
            error: (error) => {
                console.error('❌ Login failed:', error);
                this.errorMessage = error.error?.message || 'Login failed. Please try again.';
                this.isLoading = false;
            }
        });
    }
}