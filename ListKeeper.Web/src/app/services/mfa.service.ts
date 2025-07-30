import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserService } from './user.service';

export interface MfaSetupRequest {
  email: string;
}

export interface MfaSetupResponse {
  secretKey: string;
  qrCodeImage: string;
  backupCodes: string[];
  instructions: string;
}

export interface MfaEnableRequest {
  verificationCode: string;
}

export interface MfaVerificationRequest {
  emailOrUsername: string;
  password: string;
  mfaCode: string;
  isBackupCode?: boolean;
}

export interface MfaRequiredResponse {
  mfaRequired: boolean;
  mfaToken: string;
  message: string;
}

export interface MfaDisableRequest {
  currentPassword: string;
  verificationCode: string;
  isBackupCode?: boolean;
}

export interface MfaStatusResponse {
  isEnabled: boolean;
  setupDate?: string;
  backupCodesRemaining: number;
}

@Injectable({
  providedIn: 'root'
})
export class MfaService {
  private apiUrl = `${environment.baseApiUrl}/mfa`;

  constructor(
    private http: HttpClient,
    private userService: UserService
  ) { }

  private getAuthHeaders(): HttpHeaders {
    const token = this.userService.getCurrentUserToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  /**
   * Initialize MFA setup - generates QR code and backup codes
   */
  setupMfa(request: MfaSetupRequest): Observable<MfaSetupResponse> {
    return this.http.post<MfaSetupResponse>(`${this.apiUrl}/setup`, request, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Enable MFA after verifying the setup code
   */
  enableMfa(request: MfaEnableRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/enable`, request, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Verify MFA code during login
   */
  verifyMfa(verificationCode: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify`, { verificationCode }, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Verify backup code during login
   */
  verifyBackupCode(backupCode: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-backup`, { backupCode }, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Disable MFA for a user
   */
  disableMfa(request: MfaDisableRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/disable`, request, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Get current MFA status
   */
  getMfaStatus(): Observable<MfaStatusResponse> {
    return this.http.get<MfaStatusResponse>(`${this.apiUrl}/status`, {
      headers: this.getAuthHeaders()
    });
  }

  /**
   * Generate new backup codes
   */
  regenerateBackupCodes(): Observable<any> {
    return this.http.post(`${this.apiUrl}/backup-codes/regenerate`, {}, {
      headers: this.getAuthHeaders()
    });
  }
}
