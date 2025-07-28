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

  constructor(private http: HttpClient) {
    const storedUser = localStorage.getItem('user');
    const parsedUser = storedUser ? JSON.parse(storedUser) : null;
    this.currentUserSubject = new BehaviorSubject<User | null>(parsedUser);
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
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
      tap(user => {
        localStorage.setItem('user', JSON.stringify(user));
        this.currentUserSubject.next(user);
      })
    );
  }

  login(username: string, password: string) {
    // For development, use mock login
    // In production, this would be the actual API call
    //return this.mockLogin(username, password);
    
    // Uncomment below for actual API integration:
    return this.http.post<User>(`${this.baseApiUrl}/users/authenticate`, { username, password })
      .pipe(tap(user => {
        localStorage.setItem('user', JSON.stringify(user));
        this.currentUserSubject.next(user);
        return user;
      }));
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
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

  signup(signupData: any): Observable<any> { 
  return this.http.post<any>(`${this.baseApiUrl}/users/signup`, signupData) 
    .pipe(tap(response => { 
      if (response.user && response.token) { 
        // Store user and token for automatic login after signup 
        const user: User = { 
          ...response.user, 
          token: response.token 
        }; 
        localStorage.setItem('user', JSON.stringify(user)); 
        this.currentUserSubject.next(user); 
      } 
    })); 
  } 
}
