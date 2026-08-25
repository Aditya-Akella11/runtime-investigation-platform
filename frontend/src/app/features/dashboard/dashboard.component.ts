import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="hero">
      <p class="eyebrow">Runtime Investigation Platform · Phase 2</p>
      <h1>Safe runtime instrumentation for production services.</h1>
      <p class="lede">
        Add temporary runtime probes to live applications without source code modifications or deployments. Capture targeted evidence, isolate root causes, and automatically clean up instrumentation upon expiration.
      </p>
      
      <div class="actions">
        <a routerLink="/investigations" class="btn-primary">Start Investigation</a>
        <a routerLink="/runtime-probes" class="btn-secondary">Deploy Probes</a>
        <a routerLink="/audit" class="btn-secondary">View Timeline</a>
      </div>

      <div class="stats">
        <div>
          <strong>.NET Runtime Agent</strong>
          <span>Dynamic log & method entry/exit probes with local safety boundaries</span>
        </div>
        <div>
          <strong>Controlled Demo API</strong>
          <span>Live Payment API reproducing timeouts, concurrency, & retry anomalies</span>
        </div>
        <div>
          <strong>Immutable Audit Trail</strong>
          <span>Append-only verification for every probe creation, activation, and removal</span>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .hero { max-width: 960px; padding: 2.5rem; border-radius: 2rem; background: rgba(15,23,42,0.85); border: 1px solid rgba(255,255,255,0.08); box-shadow: 0 20px 80px rgba(0,0,0,0.35); }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.18em; color: #38bdf8; font-size: 0.8rem; font-weight: 700; margin: 0 0 0.5rem; }
    h1 { font-size: clamp(2.2rem, 4vw, 4rem); line-height: 1.1; margin: 0.5rem 0 1rem; color: #f8fafc; font-weight: 800; }
    .lede { max-width: 65ch; color: #cbd5e1; font-size: 1.1rem; line-height: 1.6; margin-bottom: 2rem; }
    .actions { display: flex; gap: 0.75rem; flex-wrap: wrap; margin-bottom: 2.5rem; }
    a { text-decoration: none; padding: 0.85rem 1.4rem; border-radius: 999px; font-weight: 700; font-size: 0.95rem; display: inline-flex; align-items: center; }
    .btn-primary { background: #38bdf8; color: #0f172a; }
    .btn-secondary { background: transparent; color: #f8fafc; border: 1px solid rgba(255,255,255,0.2); }
    .btn-secondary:hover { background: rgba(255,255,255,0.05); }
    .stats { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; }
    .stats div { padding: 1.25rem; border-radius: 1rem; background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.06); }
    .stats strong { display: block; color: #f8fafc; font-size: 1.05rem; }
    .stats span { display: block; color: #94a3b8; font-size: 0.85rem; margin-top: 0.4rem; line-height: 1.4; }
    @media (max-width: 800px) { .stats { grid-template-columns: 1fr; } }
  `]
})
export class DashboardComponent {}
