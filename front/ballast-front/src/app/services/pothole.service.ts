import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IPotholeCreateDto, IPotholeDto } from '../interfaces/pothole.interface';
import { BaseService } from './base.service';

@Injectable({
  providedIn: 'root'
})
export class PotholeService extends BaseService {
  constructor(private readonly http: HttpClient) {
    super();
  }

  getAll(): Observable<IPotholeDto[]> {
    return this.http.get<IPotholeDto[]>(`${this.apiUrl}api/potholes`);
  }

  getById(id: string): Observable<IPotholeDto> {
    return this.http.get<IPotholeDto>(`${this.apiUrl}api/potholes/${id}`);
  }

  create(payload: IPotholeCreateDto): Observable<IPotholeDto> {
    return this.http.post<IPotholeDto>(`${this.apiUrl}api/potholes`, payload);
  }

  update(id: string, payload: Partial<IPotholeDto>): Observable<IPotholeDto> {
    return this.http.put<IPotholeDto>(`${this.apiUrl}api/potholes/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}api/potholes/${id}`);
  }
}
