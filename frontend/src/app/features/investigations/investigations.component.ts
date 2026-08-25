import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DemoWorkflowService, Investigation } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="panel">
      <div class="header-row">
        <div>
          <h2>Investigations</h2>
          <p class="hint">Active production investigations tracking anomalies, hypotheses, and probe evidence.</p>
        </div>
      </div>

      <div class="templates-box">
        <span class="label">Quick Templates:</span>
        <button type="button" class="btn-chip" (click)="applyTemplate('Payment Gateway Timeout Spikes', 'Payment API', 'Production', 'Investigate intermittent 504 Gateway Timeouts during peak load.')">Payment Gateway Timeouts</button>
        <button type="button" class="btn-chip" (click)="applyTemplate('Duplicate Billing on Retry', 'Payment API', 'Production', 'Investigate duplicate charge incidents when clients retry failed requests.')">Duplicate Charges</button>
        <button type="button" class="btn-chip" (click)="applyTemplate('Null Reference in Order Processing', 'Payment API', 'Staging', 'Investigate unhandled exceptions in PaymentService during refund calls.')">Null Reference Exception</button>
      </div>

      <form (ngSubmit)="create()" class="form">
        <input [(ngModel)]="title" name="title" placeholder="Investigation Title" required />
        <input [(ngModel)]="application" name="application" placeholder="Application Name" required />
        <input [(ngModel)]="environment" name="environment" placeholder="Environment (e.g. Production)" required />
        <input [(ngModel)]="description" name="description" placeholder="Description / hypothesis" />
        <button type="submit">Create investigation</button>
      </form>

      <div class="cards">
        <article *ngFor="let investigation of investigations" class="inv-card">
          <div class="card-header">
            <strong>{{ investigation.title }}</strong>
            <span class="badge">{{ investigation.status }}</span>
          </div>
          <span class="app-env">{{ investigation.application }} · {{ investigation.environment }}</span>
          <p class="desc">{{ investigation.description || 'No description provided.' }}</p>
          <div class="card-actions">
            <button type="button" class="btn-primary" (click)="selectAndNavigate(investigation.id)">Launch Probe &rarr;</button>
          </div>
        </article>
      </div>

      <div *ngIf="investigations.length === 0" class="empty-state">
        No active investigations. Use the form or quick templates above to start one.
      </div>
    </section>
  `,
  styles: [`
    .panel, .form, .cards { display: grid; gap: 1rem; }
    .panel { color: #e5e7eb; }
    .hint { color: #cbd5e1; margin: 0.25rem 0 0; }
    .templates-box { display: flex; align-items: center; flex-wrap: wrap; gap: 0.5rem; padding: 0.75rem 1rem; background: rgba(255,255,255,0.03); border-radius: 0.75rem; border: 1px solid rgba(255,255,255,0.06); }
    .templates-box .label { font-size: 0.85rem; color: #94a3b8; font-weight: 600; margin-right: 0.25rem; }
    .btn-chip { background: rgba(56,189,248,0.15); color: #7dd3fc; border: 1px solid rgba(56,189,248,0.3); padding: 0.35rem 0.75rem; border-radius: 999px; font-size: 0.8rem; cursor: pointer; font-weight: 600; }
    .btn-chip:hover { background: rgba(56,189,248,0.25); }
    .form { grid-template-columns: repeat(4, minmax(0, 1fr)) auto; gap: 0.75rem; }
    input, button { padding: 0.75rem 1rem; border-radius: 0.75rem; border: 0; }
    input { background: rgba(15,23,42,0.6); border: 1px solid rgba(255,255,255,0.1); color: #f8fafc; }
    button[type="submit"] { background: #38bdf8; color: #0f172a; font-weight: 700; cursor: pointer; }
    .cards { grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; }
    .inv-card { padding: 1.25rem; border-radius: 1rem; background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.08); display: flex; flex-direction: column; justify-content: space-between; }
    .card-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 0.5rem; }
    .card-header strong { font-size: 1.05rem; color: #f8fafc; }
    .badge { background: rgba(34,197,94,0.2); color: #4ade80; padding: 0.2rem 0.6rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; }
    .app-env { display: block; color: #94a3b8; font-size: 0.85rem; margin-top: 0.35rem; }
    .desc { color: #cbd5e1; font-size: 0.9rem; margin: 0.75rem 0; flex-grow: 1; }
    .card-actions { margin-top: 0.5rem; }
    .btn-primary { width: 100%; background: #f8fafc; color: #0f172a; font-weight: 700; cursor: pointer; }
    .empty-state { text-align: center; padding: 2rem; color: #94a3b8; }
    @media (max-width: 1000px) { .form { grid-template-columns: 1fr; } }
  `]
})
export class InvestigationsComponent implements OnInit {
  private readonly workflow = inject(DemoWorkflowService);
  private readonly router = inject(Router);

  investigations: Investigation[] = [];
  title = 'Payment failures & duplicate charges';
  application = 'Payment API';
  environment = 'Production';
  description = 'Investigate gateway failures and duplicate charges.';

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.workflow.getInvestigations().subscribe(items => this.investigations = items);
  }

  applyTemplate(title: string, application: string, environment: string, description: string): void {
    this.title = title;
    this.application = application;
    this.environment = environment;
    this.description = description;
  }

  create(): void {
    if (!this.title.trim()) return;

    this.workflow.createInvestigation({
      title: this.title,
      application: this.application,
      environment: this.environment,
      description: this.description
    }).subscribe(created => {
      this.workflow.setSelectedInvestigationId(created.id);
      this.refresh();
    });
  }

  selectAndNavigate(id: string): void {
    this.workflow.setSelectedInvestigationId(id);
    this.router.navigate(['/runtime-probes']);
  }
}
