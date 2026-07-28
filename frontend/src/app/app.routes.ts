import { Routes } from '@angular/router';
import { ShellComponent } from './features/shell/shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: ShellComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'applications',
        loadComponent: () => import('./features/applications/applications.component').then(m => m.ApplicationsComponent)
      },
      {
        path: 'investigations',
        loadComponent: () => import('./features/investigations/investigations.component').then(m => m.InvestigationsComponent)
      },
      {
        path: 'runtime-probes',
        loadComponent: () => import('./features/runtime-probes/runtime-probes.component').then(m => m.RuntimeProbesComponent)
      },
      {
        path: 'evidence',
        loadComponent: () => import('./features/evidence/evidence.component').then(m => m.EvidenceComponent)
      },
      {
        path: 'audit',
        loadComponent: () => import('./features/audit/audit.component').then(m => m.AuditComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
