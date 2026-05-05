import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, finalize } from 'rxjs/operators';

export interface ShortenUrlResponse {
  shortCode: string;
  shortUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class UrlShortenerService {
  private apiUrl = 'http://localhost:5082/api';

  constructor(private http: HttpClient) { }

  shortenUrl(longUrl: string): Observable<ShortenUrlResponse> {
    console.log('🌐 Service: Sending POST request to:', `${this.apiUrl}/shorten`);
    console.log('🌐 Service: Payload:', { longUrl });
    
    return this.http.post<ShortenUrlResponse>(`${this.apiUrl}/shorten`, { longUrl }).pipe(
      tap((response) => {
        console.log('🌐 Service: Response received:', response);
        console.log('🌐 Service: Short URL:', response.shortUrl);
        console.log('🌐 Service: Short Code:', response.shortCode);
      }),
      finalize(() => {
        console.log('🌐 Service: Request completed');
      })
    );
  }

  getLongUrl(shortUrl: string): Observable<string> {
    return this.http.get<string>(`${this.apiUrl}/${shortUrl}`);
  }
}
