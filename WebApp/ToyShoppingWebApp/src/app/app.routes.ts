import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

// Lazy-loaded components (we'll create these next)
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { CustomerDashboardComponent } from './features/customer/dashboard/customer-dashboard.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { ToysComponent } from './features/customer/toys/toys.component';
import { OrdersComponent } from './features/customer/orders/orders.component';
import { ToyManagementComponent } from './features/admin/toy-management/toy-management.component';
import { OrderManagementComponent } from './features/admin/order-management/order-management.component';
import { UnauthorizedComponent } from './features/auth/unauthorized/unauthorized.component';

export const routes: Routes = [
  // 🟢 PUBLIC ROUTES (No guard needed)
  { 
    path: '', 
    redirectTo: 'login', 
    pathMatch: 'full' 
  },
  { 
    path: 'login', 
    component: LoginComponent 
  },
  { 
    path: 'register', 
    component: RegisterComponent 
  },
  { 
    path: 'unauthorized', 
    component: UnauthorizedComponent 
  },

  // 🔵 CUSTOMER ROUTES (Protected by AuthGuard + role check)
  { 
    path: 'customer', 
    component: CustomerDashboardComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: 'Customer' }
  },
  { 
    path: 'customer/toys', 
    component: ToysComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: 'Customer' }
  },
  { 
    path: 'customer/orders', 
    component: OrdersComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: 'Customer' }
  },

  // 🔴 ADMIN ROUTES (Protected by AuthGuard + role check)
  { 
    path: 'admin', 
    component: AdminDashboardComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: 'Admin' }
  },
  { 
    path: 'admin/toys', 
    component: ToyManagementComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: 'Admin' }
  },
  { 
    path: 'admin/orders', 
    component: OrderManagementComponent,
    canActivate: [AuthGuard],
    data: { requiredRole: 'Admin' }
  },

  // 🟠 CATCH-ALL (Unknown routes)
  { 
    path: '**', 
    redirectTo: 'login' 
  }
];