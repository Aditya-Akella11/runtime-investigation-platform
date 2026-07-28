import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DemoWorkflowService, Investigation } from '../../core/demo-workflow.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="panel">
      <h2>Investigations</h2>
      <form (ngSubmit)="create()" class="form">
        <input [(ngModel)]="title" name="title" placeholder="Payment failures" />
        <input [(ngModel)]="application" name="application" placeholder="Payment API" />
        <input [(ngModel)]="environment" name="environment" placeholder="Production" />
        <input [(ngModel)]="description" name="description" placeholder="Short description" />
        <button>Create investigation</button>
      </form>
      <div class="cards">
        <article *ngFor="let investigation of investigations">
          <strong>{{ investigation.title }}</strong>
          <span>{{ investigation.application }} · {{ investigation.environment }}</span>
          <p>{{ investigation.description }}</p>
          <small>{{ investigation.status }}</small>
          <button type="button" (click)="selectInvestigation(investigation.id)">Use for probe</button>
        </article>
      </div>
    </section>
  `,
  styles: [`
    .panel, .form, .cards { display: grid; gap: 1rem; }
    .panel { color: #e5e7eb; }
    .form { grid-template-columns: repeat(4, minmax(0, 1fr)); }
    input, button { padding: .8rem 1rem; border-radius: .8rem; border: 0; }
    button { background: #38bdf8; color: #0f172a; font-weight: 700; }
    .cards { grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); }
    article { padding: 1rem; border-radius: 1rem; background: rgba(255,255,255,.05); }
    article button { margin-top: .75rem; background: #f8fafc; color: #0f172a; }
    span, p, small { display: block; color: #cbd5e1; }
    @media (max-width: 1000px) { .form { grid-template-columns: 1fr; } }
  `]
})
export class InvestigationsComponent implements OnInit {
  private readonly workflow = inject(DemoWorkflowService);
  investigations: Investigation[] = [];
  title = 'Payment failures';
  application = 'Payment API';
  environment = 'Production';
  description = 'Investigate gateway failures and duplicate charges.';

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.workflow.getInvestigations().subscribe(items => this.investigations = items);
  }

  create(): void {
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

  selectInvestigation(id: string): void {
    this.workflow.setSelectedInvestigationId(id);
  }
}
