import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

interface LoginRequest {
  username: string;
  password: string;
}

interface LoginResponse {
  token: string;
  expiresIn: number;
}

interface DecodedToken {
  role?: string;
  email?: string;
  [key: string]: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5082/api/auth';
  
  // BehaviorSubject keeps track of login state
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();
  
  // Track current user role
  private userRoleSubject = new BehaviorSubject<string | null>(this.getStoredRole());
  public userRole$ = this.userRoleSubject.asObservable();

  constructor(private http: HttpClient) {
    console.log('AuthService initialized');
  }

  // Login: Call backend API
  login(username: string, password: string): Observable<LoginResponse> {
    const request: LoginRequest = { username, password };
    
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        // Store token in localStorage
        localStorage.setItem('token', response.token);
        
        // Extract and store role
        const role = this.extractRoleFromToken(response.token);
        localStorage.setItem('userRole', role || '');
        
        // Update observables (notify all subscribers)
        this.isLoggedInSubject.next(true);
        this.userRoleSubject.next(role);
        
        console.log('✅ Login successful. Role:', role);
      })
    );
  }

  // Register: Call backend API
  register(username: string, email: string, password: string): Observable<any> {
    const request = { username, email, password };
    return this.http.post(`${this.apiUrl}/register`, request);
  }

  // Logout: Clear token and update state
  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userRole');
    
    this.isLoggedInSubject.next(false);
    this.userRoleSubject.next(null);
    
    console.log('✅ Logged out');
  }

  // Check if user is logged in
  isLoggedIn(): boolean {
    return this.hasToken();
  }

  // Get current user role
  getUserRole(): string | null {
    return this.getStoredRole();
  }

  // Get stored token
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // Helper: Check if token exists
  private hasToken(): boolean {
    return !!localStorage.getItem('token');
  }

  // Helper: Get stored role from localStorage
  private getStoredRole(): string | null {
    return localStorage.getItem('userRole');
  }

  // Helper: Extract role from JWT token
  private extractRoleFromToken(token: string): string | null {
    try {
      // JWT format: header.payload.signature
      // Split and get payload (second part)
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      
      // Decode payload (base64)
      const decoded = JSON.parse(atob(parts[1])) as DecodedToken;
      
      // Return role (backend stores it as 'role')
      return decoded.role || null;
    } catch (error) {
      console.error('❌ Error decoding token:', error);
      return null;
    }
  }
}