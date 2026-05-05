import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-toy-management',
  standalone: true,
  templateUrl: './toy-management.component.html',
  styleUrls: ['./toy-management.component.css']
})
export class ToyManagementComponent {
  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/admin']);
  }
}