import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-order-management',
  standalone: true,
  templateUrl: './order-management.component.html',
  styleUrls: ['./order-management.component.css']
})
export class OrderManagementComponent {
  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/admin']);
  }
}