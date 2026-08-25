import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DemoWorkflowService, RuntimeProbe } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="panel">
      <div class="header-row">
        <div>
          <h2>Runtime Probes</h2>
          <p class="hint">Targeted temporary probes deployed dynamically to running services without redeployment.</p>
          <div *ngIf="investigationId" class="active-context">
            <span>Linked Investigation: <strong>{{ investigationId }}</strong></span>
            <a routerLink="/investigations" class="change-link">Change</a>
          </div>
        </div>
      </div>

      <div class="templates-box">
        <span class="label">Probe Presets:</span>
        <button type="button" class="btn-chip" (click)="applyPreset('Log', 'PaymentService', 'ProcessPayment', 'Amount > 1000', 15)">PaymentService.ProcessPayment (Log)</button>
        <button type="button" class="btn-chip" (click)="applyPreset('Exceptions', 'PaymentService', 'ProcessPayment', 'Exception != null', 30)">PaymentService (Exceptions)</button>
        <button type="button" class="btn-chip" (click)="applyPreset('MethodEntry', 'PaymentService', 'ProcessPayment', 'CustomerId != null', 15)">PaymentService (Method Entry)</button>
      </div>

      <form (ngSubmit)="create()" class="form">
        <input [(ngModel)]="investigationId" name="investigationId" placeholder="Investigation ID" required />
        <input [(ngModel)]="application" name="application" placeholder="Payment API" required />
        <select [(ngModel)]="probeType" name="probeType" class="select-input">
          <option value="Log">Log Probe</option>
          <option value="Exceptions">Exceptions Probe</option>
          <option value="MethodEntry">Method Entry Probe</option>
          <option value="MethodExit">Method Exit Probe</option>
        </select>
        <input [(ngModel)]="targetClass" name="targetClass" placeholder="PaymentService" required />
        <input [(ngModel)]="targetMethod" name="targetMethod" placeholder="ProcessPayment" required />
        <input [(ngModel)]="condition" name="condition" placeholder="Condition (e.g. Amount > 1000)" />
        <input [(ngModel)]="durationMinutes" name="durationMinutes" type="number" min="1" max="120" placeholder="Duration (min)" />
        <button type="submit" class="btn-create">Create Probe</button>
      </form>

      <div class="cards">
        <article *ngFor="let probe of probes" class="probe-card">
          <div class="card-header">
            <strong>{{ probe.targetClass }}.{{ probe.targetMethod }}</strong>
            <span class="badge" [ngClass]="getBadgeClass(probe.status)">{{ probe.status }}</span>
          </div>
          <span class="type-meta">{{ probe.application }} · {{ probe.probeType }} · Expires in {{ probe.durationMinutes }}m</span>
          <p class="condition-text"><strong>Condition:</strong> {{ probe.condition || 'None (All Invocations)' }}</p>
          
          <div class="actions">
            <button *ngIf="probe.status === 'Draft' || probe.status === 'Pending'" type="button" class="btn-deploy" (click)="deploy(probe.id)">Deploy Probe</button>
            <button *ngIf="probe.status === 'Active'" type="button" class="btn-remove" (click)="remove(probe.id)">Remove Probe</button>
            <button *ngIf="probe.status === 'Active'" type="button" class="btn-expire" (click)="expire(probe.id)">Simulate Expiry</button>
            <a *ngIf="probe.status === 'Active'" routerLink="/evidence" class="btn-view-evidence">View Evidence &rarr;</a>
          </div>
        </article>
      </div>

      <div *ngIf="probes.length === 0" class="empty-state">
        No probes created yet. Configure a probe above to attach temporary instrumentation.
      </div>
    </section>
  `,
  styles: [`
    .panel, .form, .cards { display: grid; gap: 1rem; }
    .panel { color: #e5e7eb; }
    .hint { color: #cbd5e1; margin: 0.25rem 0 0; }
    .active-context { margin-top: 0.5rem; font-size: 0.85rem; color: #38bdf8; display: flex; gap: 0.5rem; align-items: center; }
    .change-link { color: #94a3b8; text-decoration: underline; }
    .templates-box { display: flex; align-items: center; flex-wrap: wrap; gap: 0.5rem; padding: 0.75rem 1rem; background: rgba(255,255,255,0.03); border-radius: 0.75rem; border: 1px solid rgba(255,255,255,0.06); }
    .templates-box .label { font-size: 0.85rem; color: #94a3b8; font-weight: 600; margin-right: 0.25rem; }
    .btn-chip { background: rgba(56,189,248,0.15); color: #7dd3fc; border: 1px solid rgba(56,189,248,0.3); padding: 0.35rem 0.75rem; border-radius: 999px; font-size: 0.8rem; cursor: pointer; font-weight: 600; }
    .form { grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 0.75rem; }
    input, select, button { padding: 0.75rem 1rem; border-radius: 0.75rem; border: 0; }
    input, select { background: rgba(15,23,42,0.6); border: 1px solid rgba(255,255,255,0.1); color: #f8fafc; }
    .btn-create { background: #38bdf8; color: #0f172a; font-weight: 700; cursor: pointer; }
    .cards { grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; }
    .probe-card { padding: 1.25rem; border-radius: 1rem; background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.08); display: flex; flex-direction: column; justify-content: space-between; }
    .card-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 0.5rem; }
    .card-header strong { font-size: 1.05rem; color: #f8fafc; }
    .badge { padding: 0.2rem 0.6rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; }
    .badge-active { background: rgba(34,197,94,0.2); color: #4ade80; }
    .badge-draft { background: rgba(148,163,184,0.2); color: #cbd5e1; }
    .badge-removed { background: rgba(239,68,68,0.2); color: #f87171; }
    .badge-expired { background: rgba(234,179,8,0.2); color: #fde047; }
    .type-meta { display: block; color: #94a3b8; font-size: 0.85rem; margin-top: 0.35rem; }
    .condition-text { color: #cbd5e1; font-size: 0.9rem; margin: 0.75rem 0; flex-grow: 1; }
    .actions { display: flex; gap: 0.5rem; margin-top: 0.75rem; flex-wrap: wrap; }
    .btn-deploy { background: #38bdf8; color: #0f172a; font-weight: 700; cursor: pointer; padding: 0.5rem 0.8rem; }
    .btn-remove { background: rgba(239,68,68,0.2); color: #fca5a5; border: 1px solid rgba(239,68,68,0.3); font-weight: 600; cursor: pointer; padding: 0.5rem 0.8rem; }
    .btn-expire { background: rgba(234,179,8,0.2); color: #fde047; border: 1px solid rgba(234,179,8,0.3); font-weight: 600; cursor: pointer; padding: 0.5rem 0.8rem; }
    .btn-view-evidence { text-decoration: none; background: rgba(255,255,255,0.1); color: #f8fafc; font-weight: 600; padding: 0.5rem 0.8rem; border-radius: 0.75rem; font-size: 0.85rem; display: inline-flex; align-items: center; }
    .empty-state { text-align: center; padding: 2rem; color: #94a3b8; }
    @media (max-width: 1000px) { .form { grid-template-columns: 1fr; } }
  `]
})
export class RuntimeProbesComponent implements OnInit {
  private readonly workflow = inject(DemoWorkflowService);
  probes: RuntimeProbe[] = [];
  investigationId = '';
  application = 'Payment API';
  probeType = 'Log';
  targetClass = 'PaymentService';
  targetMethod = 'ProcessPayment';
  condition = 'Amount > 1000';
  durationMinutes = 15;

  ngOnInit(): void {
    this.investigationId = this.workflow.getSelectedInvestigationId();
    this.refresh();
  }

  refresh(): void {
    this.workflow.getProbes().subscribe(items => this.probes = items);
  }

  applyPreset(type: string, targetClass: string, targetMethod: string, condition: string, duration: number): void {
    this.probeType = type;
    this.targetClass = targetClass;
    this.targetMethod = targetMethod;
    this.condition = condition;
    this.durationMinutes = duration;
  }

  create(): void {
    this.workflow.createProbe({
      investigationId: this.investigationId || 'inv-default',
      application: this.application,
      probeType: this.probeType,
      targetClass: this.targetClass,
      targetMethod: this.targetMethod,
      condition: this.condition,
      durationMinutes: Number(this.durationMinutes)
    }).subscribe(() => this.refresh());
  }

  deploy(id: string): void {
    this.workflow.deployProbe(id).subscribe(() => this.refresh());
  }

  remove(id: string): void {
    this.workflow.removeProbe(id).subscribe(() => this.refresh());
  }

  expire(id: string): void {
    this.workflow.expireProbe(id).subscribe(() => this.refresh());
  }

  getBadgeClass(status: string): string {
    switch (status) {
      case 'Active': return 'badge-active';
      case 'Removed': return 'badge-removed';
      case 'Expired': return 'badge-expired';
      default: return 'badge-draft';
    }
  }
}
