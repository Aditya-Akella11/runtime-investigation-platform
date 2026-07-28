import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="hero">
      <p class="eyebrow">Phase 1 Demo</p>
      <h1>Investigate runtime failures in a controlled production-like environment.</h1>
      <p class="lede">Create an investigation, deploy a probe to the demo payment service, watch evidence arrive, and remove the probe when you are done.</p>
      <div class="actions">
        <a routerLink="/investigations">Start an investigation</a>
        <a routerLink="/runtime-probes" class="secondary">Create a probe</a>
      </div>
      <div class="stats">
        <div><strong>Demo API</strong><span>Payment, orders, refunds</span></div>
        <div><strong>Mock Agent</strong><span>Activation and evidence</span></div>
        <div><strong>Audit Trail</strong><span>Every action recorded</span></div>
      </div>
    </section>
  `,
  styles: [`
    .hero { max-width: 960px; padding: 2rem; border-radius: 2rem; background: rgba(15,23,42,.8); border: 1px solid rgba(255,255,255,.08); box-shadow: 0 20px 80px rgba(0,0,0,.35); }
    .eyebrow { text-transform: uppercase; letter-spacing: .18em; color: #93c5fd; font-size: .75rem; }
    h1 { font-size: clamp(2.2rem, 4vw, 4.5rem); line-height: 1; margin: .6rem 0 1rem; }
    .lede { max-width: 60ch; color: #cbd5e1; font-size: 1.05rem; }
    .actions { display: flex; gap: .75rem; margin-top: 1.5rem; flex-wrap: wrap; }
    a { text-decoration: none; color: #0f172a; background: #f8fafc; padding: .8rem 1.1rem; border-radius: 999px; font-weight: 700; }
    .secondary { background: transparent; color: #e5e7eb; border: 1px solid rgba(255,255,255,.15); }
    .stats { margin-top: 2rem; display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 1rem; }
    .stats div { padding: 1rem; border-radius: 1rem; background: rgba(255,255,255,.05); }
    .stats strong, .stats span { display: block; }
    .stats span { color: #cbd5e1; margin-top: .35rem; }
    @media (max-width: 800px) { .stats { grid-template-columns: 1fr; } }
  `]
})
export class DashboardComponent {}
