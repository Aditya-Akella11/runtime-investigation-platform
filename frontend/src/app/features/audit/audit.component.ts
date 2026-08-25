import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { interval, Subject, takeUntil } from 'rxjs';
import { AuditEntry, DemoWorkflowService } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="panel">
      <div class="header-row">
        <div>
          <h2>Investigation Timeline</h2>
          <p class="hint">Live timeline recording every state change, dispatch, evidence capture, and probe removal.</p>
        </div>
        <div class="controls">
          <button type="button" class="btn-secondary" (click)="refresh()">Refresh</button>
          <button type="button" class="btn-danger" (click)="resetDemo()">Reset Demo</button>
        </div>
      </div>

      <div class="filter-bar">
        <input [(ngModel)]="filterText" placeholder="Filter by event type, ID, or text..." />
      </div>

      <div class="timeline-list">
        <article *ngFor="let item of filteredAudit" [class.highlight]="isMilestone(item.eventType)" class="timeline-item">
          <div class="badge" [ngClass]="getBadgeClass(item.eventType)">{{ item.eventType }}</div>
          <div class="content">
            <span class="message">{{ item.message }}</span>
            <small class="meta">Subject: {{ item.subjectId }} · {{ item.createdAt | date:'shortTime' }} ({{ item.createdAt | date:'mediumDate' }})</small>
          </div>
        </article>
        <div *ngIf="filteredAudit.length === 0" class="empty-state">
          No audit events found matching criteria.
        </div>
      </div>
    </section>
  `,
  styles: [`
    .panel { display: grid; gap: 1.25rem; color: #e5e7eb; }
    .header-row { display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap; gap: 1rem; }
    .hint { color: #cbd5e1; margin: 0.25rem 0 0; }
    .controls { display: flex; gap: 0.5rem; }
    .filter-bar input { width: 100%; padding: 0.75rem 1rem; border-radius: 0.75rem; border: 1px solid rgba(255,255,255,0.1); background: rgba(15,23,42,0.6); color: #f8fafc; }
    button { padding: 0.6rem 1rem; border-radius: 0.75rem; border: 0; font-weight: 600; cursor: pointer; }
    .btn-secondary { background: rgba(255,255,255,0.1); color: #f8fafc; }
    .btn-danger { background: rgba(239,68,68,0.2); color: #fca5a5; border: 1px solid rgba(239,68,68,0.3); }
    .timeline-list { display: grid; gap: 0.75rem; }
    .timeline-item { display: flex; gap: 1rem; align-items: flex-start; padding: 1rem; border-radius: 1rem; background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.06); }
    .highlight { background: rgba(56,189,248,0.12); border-color: rgba(125,211,252,0.25); }
    .badge { padding: 0.35rem 0.75rem; border-radius: 999px; font-size: 0.75rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; white-space: nowrap; }
    .badge-info { background: rgba(56,189,248,0.2); color: #38bdf8; }
    .badge-success { background: rgba(34,197,94,0.2); color: #4ade80; }
    .badge-warning { background: rgba(234,179,8,0.2); color: #fde047; }
    .badge-danger { background: rgba(239,68,68,0.2); color: #f87171; }
    .content { display: grid; gap: 0.25rem; }
    .message { color: #f1f5f9; font-weight: 500; }
    .meta { color: #94a3b8; font-size: 0.8rem; }
    .empty-state { padding: 2rem; text-align: center; color: #94a3b8; }
  `]
})
export class AuditComponent implements OnInit, OnDestroy {
  private readonly workflow = inject(DemoWorkflowService);
  private readonly destroy$ = new Subject<void>();
  audit: AuditEntry[] = [];
  filterText = '';

  ngOnInit(): void {
    this.refresh();
    interval(5000).pipe(takeUntil(this.destroy$)).subscribe(() => this.refresh());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  refresh(): void {
    this.workflow.getAudit().subscribe(items => this.audit = items);
  }

  resetDemo(): void {
    this.workflow.resetDemo().subscribe(() => this.refresh());
  }

  get filteredAudit(): AuditEntry[] {
    if (!this.filterText) return this.audit;
    const term = this.filterText.toLowerCase();
    return this.audit.filter(item =>
      item.eventType.toLowerCase().includes(term) ||
      item.message.toLowerCase().includes(term) ||
      item.subjectId.toLowerCase().includes(term)
    );
  }

  isMilestone(eventType: string): boolean {
    return ['InvestigationCreated', 'ProbeCreated', 'ProbeDispatched', 'ProbeDeployed', 'ProbeRemoved', 'ProbeExpired'].includes(eventType);
  }

  getBadgeClass(eventType: string): string {
    switch (eventType) {
      case 'InvestigationCreated':
      case 'ProbeDeployed':
        return 'badge-success';
      case 'ProbeDispatched':
      case 'ProbeCreated':
        return 'badge-info';
      case 'ProbeExpired':
      case 'EvidenceCaptured':
        return 'badge-warning';
      case 'ProbeRemoved':
        return 'badge-danger';
      default:
        return 'badge-info';
    }
  }
}
