import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of } from 'rxjs';
import { User } from '../models/user.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private baseApiUrl = environment.baseApiUrl;
  private readonly USER_STORAGE_KEY = 'user';

  constructor(private http: HttpClient) {
    const storedUser = this.getUserFromStorage();
    this.currentUserSubject = new BehaviorSubject<User | null>(storedUser);
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Get user data from localStorage
   */
  private getUserFromStorage(): User | null {
    try {
      const storedUser = localStorage.getItem(this.USER_STORAGE_KEY);
      return storedUser ? JSON.parse(storedUser) : null;
    } catch (error) {
      console.error('Error parsing user from storage:', error);
      this.clearUserFromStorage();
      return null;
    }
  }

  /**
   * Store user data in localStorage
   */
  private setUserInStorage(user: User): void {
    try {
      localStorage.setItem(this.USER_STORAGE_KEY, JSON.stringify(user));
    } catch (error) {
      console.error('Error storing user in storage:', error);
    }
  }

  /**
   * Clear user data from localStorage
   */
  private clearUserFromStorage(): void {
    localStorage.removeItem(this.USER_STORAGE_KEY);
  }

  /**
   * Get current user's token
   */
  public getCurrentUserToken(): string | null {
    const user = this.currentUserValue;
    return user?.token || null;
  }

  /**
   * Check if user is authenticated
   */
  public isAuthenticated(): boolean {
    const user = this.currentUserValue;
    return !!(user && user.token);
  }

  /**
   * Update current user and storage
   */
  public updateCurrentUser(user: User | null): void {
    if (user) {
      this.setUserInStorage(user);
    } else {
      this.clearUserFromStorage();
    }
    this.currentUserSubject.next(user);
  }

  // Mock login for development purposes
  mockLogin(username: string = 'demo_user', password: string = 'password'): Observable<User> {
    const mockUser: User = {
      id: 1,
      firstname: 'Demo',
      lastname: 'User',
      username: username,
      email: 'demo@example.ca',
      token: 'mock-jwt-token-123'
    };
    
    // Simulate API delay
    return of(mockUser).pipe(
      tap(user => this.updateCurrentUser(user))
    );
  }

  login(username: string, password: string): Observable<any> {
    // For development, use mock login
    // In production, this would be the actual API call
    //return this.mockLogin(username, password);
    
    // API call that can return either User or MFA response
    return this.http.post<any>(`${this.baseApiUrl}/users/authenticate`, { username, password })
      .pipe(tap(response => {
        // If it's a complete user login (no MFA required)
        if (response.token && !response.mfaRequired) {
          this.updateCurrentUser(response);
        }
        // If MFA is required, don't update current user yet
        // The response will be handled by the login component
      }));
  }

  /**
   * Verify MFA code and complete login process
   */
  verifyMfaLogin(request: any): Observable<User> {
    return this.http.post<User>(`${this.baseApiUrl}/mfa/verify`, request)
      .pipe(tap(user => this.updateCurrentUser(user)));
  }

  logout() {
    this.updateCurrentUser(null);
  }

  signup(signupData: any): Observable<any> {
    return this.http.post<any>(`${this.baseApiUrl}/users/signup`, signupData)
      .pipe(tap(response => {
        if (response.user && response.token) {
          // Store user and token for automatic login after signup
          const user: User = {
            ...response.user,
            token: response.token
          };
          this.updateCurrentUser(user);
        }
      }));
  }

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseApiUrl}/users`);
  }

  getById(id: number): Observable<User> {
    return this.http.get<User>(`${this.baseApiUrl}/users/${id}`);
  }

  getByUsername(username: string): Observable<User> {
    return this.http.get<User>(`${this.baseApiUrl}/users/username/${username}`);
  }

  register(user: User): Observable<User> {
    return this.http.post<User>(`${this.baseApiUrl}/users`, user);
  }

  update(user: User): Observable<User> {
    return this.http.put<User>(`${this.baseApiUrl}/users/${user.id}`, user);
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.baseApiUrl}/users/${id}`);
  }
}
