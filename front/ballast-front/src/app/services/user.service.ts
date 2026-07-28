import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { IAuthResponse, ILoginRequest, IRegisterRequest, IUserDto } from '../interfaces/user.interface';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseService {
  private readonly currentUserSubject = new BehaviorSubject<IUserDto | null>(null);
  private readonly authTokenSubject = new BehaviorSubject<string | null>(null);

  readonly currentUser$ = this.currentUserSubject.asObservable();
  readonly authToken$ = this.authTokenSubject.asObservable();

  constructor(private readonly http: HttpClient) {
    super();
  }

  register(payload: IRegisterRequest): Observable<IAuthResponse> {
    return this.http.post<IAuthResponse>(`${this.apiUrl}api/users/register`, payload);
  }

  login(payload: ILoginRequest): Observable<IAuthResponse> {
    return this.http.post<IAuthResponse>(`${this.apiUrl}api/users/login`, payload);
  }

  saveAuthResponse(response: IAuthResponse, fallbackEmail?: string): void {
    const user = this.buildUserFromResponse(response, fallbackEmail);
    this.currentUserSubject.next(user);
    this.authTokenSubject.next(response.token);
    localStorage.setItem('authToken', response.token);
    localStorage.setItem('currentUser', JSON.stringify(user));
  }

  private buildUserFromResponse(response: IAuthResponse, fallbackEmail?: string): IUserDto {
    const id = response.user?.id ?? response.Id ?? response.id ?? '';
    const email = response.user?.email ?? response.email ?? fallbackEmail ?? '';
    const displayName = response.user?.displayName ?? response.displayName ?? email?.split('@')[0] ?? 'Logged user';

    return {
      id,
      email,
      displayName,
      createdDate: response.user?.createdDate ?? new Date().toISOString()
    };
  }

  clearAuth(): void {
    this.currentUserSubject.next(null);
    this.authTokenSubject.next(null);
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
  }

  getCurrentUser(): IUserDto | null {
    return this.currentUserSubject.getValue();
  }

  getAuthToken(): string | null {
    return this.authTokenSubject.getValue();
  }

  getAll(): Observable<IUserDto[]> {
    return this.http.get<IUserDto[]>(`${this.apiUrl}api/users`);
  }

  getById(id: string): Observable<IUserDto> {
    return this.http.get<IUserDto>(`${this.apiUrl}api/users/${id}`);
  }

  update(id: string, payload: Partial<IUserDto>): Observable<IUserDto> {
    return this.http.put<IUserDto>(`${this.apiUrl}api/users/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}api/users/${id}`);
  }
}
