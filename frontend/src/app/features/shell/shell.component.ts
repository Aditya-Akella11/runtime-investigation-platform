import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  template: `
    <div class="shell">
      <aside class="sidebar">
        <div class="brand">Runtime Investigation Platform</div>
        <nav>
          <a routerLink="/">Dashboard</a>
          <a routerLink="/applications">Applications</a>
          <a routerLink="/investigations">Investigations</a>
          <a routerLink="/runtime-probes">Runtime Probes</a>
          <a routerLink="/evidence">Evidence</a>
          <a routerLink="/audit">Audit</a>
          <a routerLink="/login">Sign in</a>
        </nav>
      </aside>
      <main class="content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .shell { min-height: 100vh; display: grid; grid-template-columns: 280px 1fr; background: linear-gradient(135deg, #0f172a, #111827 45%, #1f2937); color: #e5e7eb; }
    .sidebar { padding: 1.5rem; border-right: 1px solid rgba(255,255,255,.08); display: flex; flex-direction: column; gap: 1.5rem; }
    .brand { font-size: 1.2rem; font-weight: 700; letter-spacing: .05em; }
    nav { display: flex; flex-direction: column; gap: .75rem; }
    a { color: #d1d5db; text-decoration: none; padding: .6rem .8rem; border-radius: .75rem; background: rgba(255,255,255,.04); }
    a:hover { background: rgba(255,255,255,.09); }
    .content { padding: 2rem; }
    @media (max-width: 900px) { .shell { grid-template-columns: 1fr; } .sidebar { border-right: 0; border-bottom: 1px solid rgba(255,255,255,.08); } }
  `]
})
export class ShellComponent {}
