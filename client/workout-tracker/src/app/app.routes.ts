
import { Routes } from '@angular/router';
import { Auth } from './pages/auth/auth';
import { Workouts } from './pages/workouts/workouts';
import { Dashboard } from './pages/dashboard/dashboard';
import { authGuard } from './services/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'auth', pathMatch: 'full' },
  { path: 'auth', component: Auth },
  { path: 'workouts', component: Workouts, canActivate: [authGuard] },
  { path: 'dashboard', component: Dashboard, canActivate: [authGuard] },
  { path: '**', redirectTo: 'auth' }
];
