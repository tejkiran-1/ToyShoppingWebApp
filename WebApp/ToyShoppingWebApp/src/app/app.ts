import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <!-- This displays the current route's component -->
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent {
  title = 'ToyShoppingWebApp';
}