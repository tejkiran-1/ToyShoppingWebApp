import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    // Check 1: Is user logged in?
    if (!this.authService.isLoggedIn()) {
      console.warn('❌ User not logged in. Redirecting to /login');
      this.router.navigate(['/login']);
      return false;
    }

    // Check 2: Does route require a specific role?
    const requiredRole = route.data['requiredRole'];
    
    if (requiredRole) {
      const userRole = this.authService.getUserRole();
      
      // Check if user has required role
      if (userRole !== requiredRole) {
        console.warn(`❌ User role (${userRole}) doesn't match required role (${requiredRole})`);
        this.router.navigate(['/unauthorized']);
        return false;
      }
    }

    console.log('✅ User authorized. Granting access.');
    return true;
  }
}