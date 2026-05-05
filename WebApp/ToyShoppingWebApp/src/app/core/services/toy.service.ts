import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ToyDto {
  id: number;
  name: string;
  description: string;
  price: number;
  stock: number;
  categoryName: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalItems: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToyService {
  private apiUrl = 'http://localhost:5082/api/toys';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    return headers;
  }

  // Get all toys
  getAllToys(): Observable<ToyDto[]> {
    console.log('🔄 Fetching toys from:', this.apiUrl);
    return this.http.get<ToyDto[]>(`${this.apiUrl}`, {
      headers: this.getHeaders()
    });
  }

  // Get toys by category with pagination
  getToysByCategory(categoryId: number, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<ToyDto>> {
    return this.http.get<PaginatedResponse<ToyDto>>(`${this.apiUrl}/category/${categoryId}`, {
      params: { page: page.toString(), pageSize: pageSize.toString() },
      headers: this.getHeaders()
    });
  }

  // Get toy by ID
  getToyById(id: number): Observable<ToyDto> {
    return this.http.get<ToyDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }
}
