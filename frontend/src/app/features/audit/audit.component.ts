import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subject, takeUntil } from 'rxjs';
import { AuditEntry, DemoWorkflowService } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="panel">
      <h2>Timeline</h2>
      <p class="hint">Live refresh every 5 seconds. This shows the full investigation path.</p>
      <article *ngFor="let item of audit" [class.highlight]="isMilestone(item.eventType)">
        <strong>{{ item.eventType }}</strong>
        <span>{{ item.message }}</span>
        <small>{{ item.createdAt }}</small>
      </article>
    </section>
  `,
  styles: [`
    .panel { display: grid; gap: 1rem; color: #e5e7eb; }
    .hint { color: #cbd5e1; margin: 0; }
    article { padding: 1rem; border-radius: 1rem; background: rgba(255,255,255,.05); }
    .highlight { background: rgba(56,189,248,.16); border: 1px solid rgba(125,211,252,.2); }
    span, small { display: block; color: #cbd5e1; margin-top: .35rem; }
  `]
})
export class AuditComponent implements OnInit, OnDestroy {
  private readonly workflow = inject(DemoWorkflowService);
  private readonly destroy$ = new Subject<void>();
  audit: AuditEntry[] = [];

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

  isMilestone(eventType: string): boolean {
    return ['InvestigationCreated', 'ProbeCreated', 'ProbeDispatched', 'ProbeRemoved'].includes(eventType);
  }
}
