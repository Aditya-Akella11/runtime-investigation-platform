import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { interval, Subject, takeUntil } from 'rxjs';
import { DemoWorkflowService, Evidence } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="panel">
      <h2>Evidence</h2>
      <p class="hint">Live refresh every 5 seconds.</p>
      <article *ngFor="let item of evidence">
        <strong>{{ item.targetClass }}.{{ item.targetMethod }}</strong>
        <span>{{ item.application }} · {{ item.capturedAt }}</span>
        <pre>{{ item.payload }}</pre>
      </article>
    </section>
  `,
  styles: [`
    .panel { display: grid; gap: 1rem; color: #e5e7eb; }
    .hint { color: #cbd5e1; margin: 0; }
    article { padding: 1rem; border-radius: 1rem; background: rgba(255,255,255,.05); }
    span { display: block; color: #cbd5e1; margin-top: .4rem; }
    pre { white-space: pre-wrap; color: #f8fafc; }
  `]
})
export class EvidenceComponent implements OnInit, OnDestroy {
  private readonly workflow = inject(DemoWorkflowService);
  private readonly destroy$ = new Subject<void>();
  evidence: Evidence[] = [];

  ngOnInit(): void {
    this.refresh();
    interval(5000).pipe(takeUntil(this.destroy$)).subscribe(() => this.refresh());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  refresh(): void {
    this.workflow.getEvidence().subscribe(items => this.evidence = items);
  }
}
