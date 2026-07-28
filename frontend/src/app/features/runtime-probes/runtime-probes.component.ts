import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DemoWorkflowService, RuntimeProbe } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="panel">
      <h2>Runtime Probes</h2>
      <p class="hint">Selected investigation: {{ investigationId || 'none selected yet' }}</p>
      <form (ngSubmit)="create()" class="form">
        <input [(ngModel)]="investigationId" name="investigationId" placeholder="Investigation ID" />
        <input [(ngModel)]="application" name="application" placeholder="Payment API" />
        <input [(ngModel)]="probeType" name="probeType" placeholder="Log" />
        <input [(ngModel)]="targetClass" name="targetClass" placeholder="PaymentService" />
        <input [(ngModel)]="targetMethod" name="targetMethod" placeholder="ProcessPayment" />
        <input [(ngModel)]="condition" name="condition" placeholder="Amount > 1000" />
        <input [(ngModel)]="durationMinutes" name="durationMinutes" type="number" placeholder="15" />
        <button>Create probe</button>
      </form>
      <div class="cards">
        <article *ngFor="let probe of probes">
          <strong>{{ probe.targetClass }}.{{ probe.targetMethod }}</strong>
          <span>{{ probe.probeType }} · {{ probe.status }}</span>
          <p>{{ probe.condition }}</p>
          <div class="actions">
            <button type="button" (click)="deploy(probe.id)">Deploy</button>
            <button type="button" (click)="remove(probe.id)">Remove</button>
          </div>
        </article>
      </div>
    </section>
  `,
  styles: [`
    .panel, .form, .cards { display: grid; gap: 1rem; }
    .panel { color: #e5e7eb; }
    .hint { color: #cbd5e1; margin: 0; }
    .form { grid-template-columns: repeat(4, minmax(0, 1fr)); }
    input, button { padding: .8rem 1rem; border-radius: .8rem; border: 0; }
    button { background: #38bdf8; color: #0f172a; font-weight: 700; }
    .cards { grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); }
    article { padding: 1rem; border-radius: 1rem; background: rgba(255,255,255,.05); }
    span, p { display: block; color: #cbd5e1; }
    .actions { display: flex; gap: .5rem; margin-top: .75rem; }
    .actions button { padding: .5rem .8rem; }
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

  create(): void {
    this.workflow.createProbe({
      investigationId: this.investigationId,
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
}
