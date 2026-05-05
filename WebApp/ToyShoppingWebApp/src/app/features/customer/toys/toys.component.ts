import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToyService, ToyDto } from '../../../core/services/toy.service';
import { UrlShortenerService, ShortenUrlResponse } from '../../../core/services/url-shortener.service';

@Component({
  selector: 'app-toys',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './toys.component.html',
  styleUrls: ['./toys.component.css']
})
export class ToysComponent implements OnInit {
  toys: ToyDto[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  // URL Shortener properties
  urlToShorten: string = '';
  shortenedUrl: string = '';
  isShorteningUrl: boolean = false;
  shortenUrlError: string = '';
  shortenUrlSuccess: string = '';

  constructor(
    private router: Router,
    private toyService: ToyService,
    private urlShortenerService: UrlShortenerService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.loadToys();
  }

  loadToys(): void {
    this.isLoading = true;
    this.errorMessage = '';
    console.log('📦 Loading toys...');
    this.toyService.getAllToys().subscribe({
      next: (data) => {
        console.log('✅ Toys loaded successfully:', data);
        this.toys = data;
        this.isLoading = false;
        if (data.length === 0) {
          this.errorMessage = 'No toys available at the moment.';
        }
      },
      error: (error) => {
        console.error('❌ Error loading toys:', error);
        this.isLoading = false;
        if (error.status === 0) {
          this.errorMessage = 'Cannot connect to server. Is the backend running at http://localhost:5082?';
        } else if (error.status === 401) {
          this.errorMessage = 'Unauthorized. Please log in again.';
        } else if (error.status === 403) {
          this.errorMessage = 'You do not have permission to view toys.';
        } else if (error.status === 404) {
          this.errorMessage = 'Toys endpoint not found.';
        } else {
          this.errorMessage = `Error loading toys: ${error.message || error.status}`;
        }
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/customer']);
  }

  addToCart(toy: ToyDto): void {
    console.log('Added to cart:', toy);
    // TODO: Implement add to cart functionality
  }

  shortenUrl(): void {
    if (!this.urlToShorten.trim()) {
      this.shortenUrlError = 'Please enter a URL to shorten';
      return;
    }

    this.isShorteningUrl = true;
    this.shortenUrlError = '';
    this.shortenUrlSuccess = '';
    this.shortenedUrl = '';

    console.log('🔗 Starting to shorten URL:', this.urlToShorten);

    this.urlShortenerService.shortenUrl(this.urlToShorten).subscribe({
      next: (response: ShortenUrlResponse) => {
        console.log('📥 Response received:', response);
        
        // Wrap state updates in ngZone.run() to trigger change detection
        this.ngZone.run(() => {
          this.shortenedUrl = response.shortUrl;
          this.shortenUrlSuccess = `✅ Short URL created successfully!`;
          this.urlToShorten = '';
          this.isShorteningUrl = false;
          this.shortenUrlError = '';
          
          console.log('✅ State updated - shortenedUrl:', this.shortenedUrl);
          console.log('✅ State updated - isShorteningUrl:', this.isShorteningUrl);
          
          this.cdr.markForCheck();
        });
      },
      error: (error: any) => {
        console.error('❌ Error shortening URL:', error);
        
        this.ngZone.run(() => {
          this.isShorteningUrl = false;
          
          let errorMsg = 'Failed to shorten URL. Please try again.';
          if (error.error?.message) {
            errorMsg = error.error.message;
          } else if (error.error?.title) {
            errorMsg = error.error.title;
          } else if (error.statusText) {
            errorMsg = `${error.status}: ${error.statusText}`;
          }
          
          this.shortenUrlError = errorMsg;
          
          console.log('❌ Error message set:', this.shortenUrlError);
          
          this.cdr.markForCheck();
        });
      }
    });
  }

  copyToClipboard(): void {
    if (this.shortenedUrl) {
      navigator.clipboard.writeText(this.shortenedUrl).then(() => {
        console.log('✅ Copied to clipboard:', this.shortenedUrl);
        this.ngZone.run(() => {
          this.shortenUrlSuccess = '✅ Copied to clipboard!';
          this.cdr.markForCheck();
          
          setTimeout(() => {
            this.ngZone.run(() => {
              this.shortenUrlSuccess = '';
              this.cdr.markForCheck();
            });
          }, 3000);
        });
      }).catch(err => {
        console.error('❌ Failed to copy:', err);
        this.ngZone.run(() => {
          this.shortenUrlError = 'Failed to copy to clipboard';
          this.cdr.markForCheck();
        });
      });
    }
  }
}