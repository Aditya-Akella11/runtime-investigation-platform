import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { InvestigationService, InvestigationItem, ProbeItem, ProbeResultItem } from '../../core/investigation.service';

@Component({
  selector: 'app-investigation-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-container" *ngIf="investigation; else loadingTpl">
      <header class="header">
        <a routerLink="/investigations" class="back-link">&larr; Back to Investigations</a>
        <div class="header-content">
          <h1>{{ investigation.title }}</h1>
          <span class="status-badge" [attr.data-status]="investigation.status">{{ investigation.status }}</span>
        </div>
        <p class="description">{{ investigation.description }}</p>
        <div class="meta">
          <span>Application: <strong>{{ investigation.applicationId }}</strong></span>
          <span>Created: {{ investigation.createdAtUtc | date:'short' }}</span>
        </div>
      </header>

      <div class="content-grid">
        <!-- Probes section -->
        <section class="card probes-section">
          <div class="card-header">
            <h2>Active Probes ({{ probes.length }})</h2>
            <button class="btn btn-secondary" (click)="showAddProbeForm = !showAddProbeForm">
              {{ showAddProbeForm ? 'Cancel' : '+ Add Probe' }}
            </button>
          </div>

          <!-- Add probe form -->
          <div class="add-probe-form" *ngIf="showAddProbeForm">
            <h3>Configure Runtime Probe</h3>
            <div class="form-row">
              <div class="form-group">
                <label>Target Class</label>
                <input type="text" [(ngModel)]="newProbe.targetClass" placeholder="e.g. PaymentService" required />
              </div>
              <div class="form-group">
                <label>Target Method</label>
                <input type="text" [(ngModel)]="newProbe.targetMethod" placeholder="e.g. ProcessPayment" required />
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Filter Condition (optional)</label>
                <input type="text" [(ngModel)]="newProbe.expression" placeholder="e.g. amount > 100" />
              </div>
              <div class="form-group">
                <label>Duration (minutes)</label>
                <input type="number" [(ngModel)]="newProbe.durationMinutes" min="1" max="120" />
              </div>
            </div>
            <div class="form-actions">
              <button class="btn btn-primary" (click)="createProbe()" [disabled]="!newProbe.targetClass || !newProbe.targetMethod">
                Save Probe
              </button>
            </div>
          </div>

          <!-- Probes table -->
          <table class="data-table" *ngIf="probes.length > 0; else noProbesTpl">
            <thead>
              <tr>
                <th>Target</th>
                <th>Type</th>
                <th>Status</th>
                <th>Expires</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let probe of probes">
                <td>
                  <strong>{{ probe.target }}</strong>
                  <div *ngIf="probe.expression" class="sub-text">Condition: {{ probe.expression }}</div>
                </td>
                <td><span class="type-badge">{{ probe.type }}</span></td>
                <td><span class="status-badge" [attr.data-status]="probe.status">{{ probe.status }}</span></td>
                <td>{{ probe.expiresAt | date:'shortTime' }}</td>
                <td class="actions-cell">
                  <button *ngIf="probe.status === 'Pending' || probe.status === 'Draft'" class="btn btn-sm btn-success" (click)="activateProbe(probe.id)">
                    Activate
                  </button>
                  <button *ngIf="probe.status === 'Active'" class="btn btn-sm btn-danger" (click)="deactivateProbe(probe.id)">
                    Deactivate
                  </button>
                  <button class="btn btn-sm btn-outline" (click)="viewResults(probe.id)">
                    Results
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <ng-template #noProbesTpl>
            <p class="empty-state">No probes configured for this investigation yet.</p>
          </ng-template>
        </section>

        <!-- Live Evidence/Results section -->
        <section class="card results-section" *ngIf="selectedProbeId">
          <div class="card-header">
            <h2>Probe Evidence ({{ results.length }})</h2>
            <button class="btn btn-sm btn-outline" (click)="refreshResults()">Refresh</button>
          </div>
          <div class="results-list" *ngIf="results.length > 0; else noResultsTpl">
            <div class="result-item" *ngFor="let res of results">
              <div class="result-header">
                <span class="result-method">{{ res.methodName }}</span>
                <span class="result-time">{{ res.capturedAtUtc | date:'mediumTime' }}</span>
              </div>
              <div class="result-args">
                <pre>{{ res.arguments | json }}</pre>
              </div>
              <div *ngIf="res.returnValue" class="result-return">
                <strong>Returned:</strong> {{ res.returnValue }}
              </div>
            </div>
          </div>
          <ng-template #noResultsTpl>
            <p class="empty-state">No execution evidence captured yet.</p>
          </ng-template>
        </section>
      </div>
    </div>

    <ng-template #loadingTpl>
      <div class="loading-state">
        <p>Loading investigation...</p>
      </div>
    </ng-template>
  `,
  styles: [`
    .page-container { padding: 1.5rem; max-width: 1200px; margin: 0 auto; }
    .back-link { color: #64748b; text-decoration: none; font-size: 0.875rem; display: inline-block; margin-bottom: 0.5rem; }
    .header-content { display: flex; align-items: center; gap: 1rem; }
    .header h1 { margin: 0; font-size: 1.75rem; color: #0f172a; }
    .description { color: #475569; margin: 0.5rem 0; }
    .meta { display: flex; gap: 2rem; font-size: 0.875rem; color: #64748b; margin-top: 0.5rem; }
    .content-grid { display: grid; grid-template-columns: 1fr; gap: 1.5rem; margin-top: 1.5rem; }
    .card { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 1.25rem; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
    .card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
    .card-header h2 { margin: 0; font-size: 1.25rem; color: #1e293b; }
    .add-probe-form { background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 6px; padding: 1rem; margin-bottom: 1.5rem; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 0.75rem; }
    .form-group label { display: block; font-size: 0.75rem; font-weight: 600; color: #475569; margin-bottom: 0.25rem; text-transform: uppercase; }
    .form-group input { width: 100%; padding: 0.5rem; border: 1px solid #cbd5e1; border-radius: 4px; font-size: 0.875rem; box-sizing: border-box; }
    .form-actions { display: flex; justify-content: flex-end; margin-top: 0.5rem; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
    .data-table th, .data-table td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #f1f5f9; }
    .data-table th { background: #f8fafc; color: #475569; font-weight: 600; }
    .sub-text { font-size: 0.75rem; color: #64748b; margin-top: 0.25rem; }
    .status-badge { padding: 0.25rem 0.6rem; border-radius: 9999px; font-size: 0.75rem; font-weight: 600; text-transform: uppercase; }
    .status-badge[data-status="Active"] { background: #dcfce7; color: #166534; }
    .status-badge[data-status="Pending"], .status-badge[data-status="Draft"] { background: #fef9c3; color: #854d0e; }
    .status-badge[data-status="Removed"], .status-badge[data-status="Expired"] { background: #f1f5f9; color: #475569; }
    .status-badge[data-status="Failed"] { background: #fee2e2; color: #991b1b; }
    .type-badge { background: #e0f2fe; color: #0369a1; padding: 0.2rem 0.5rem; border-radius: 4px; font-size: 0.75rem; }
    .btn { padding: 0.4rem 0.8rem; border-radius: 4px; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; }
    .btn-primary { background: #2563eb; color: white; }
    .btn-secondary { background: #f1f5f9; color: #1e293b; border: 1px solid #cbd5e1; }
    .btn-outline { background: transparent; border: 1px solid #cbd5e1; color: #475569; }
    .btn-success { background: #16a34a; color: white; }
    .btn-danger { background: #dc2626; color: white; }
    .btn-sm { padding: 0.25rem 0.5rem; font-size: 0.75rem; margin-right: 0.4rem; }
    .results-list { display: flex; flex-direction: column; gap: 0.75rem; max-height: 400px; overflow-y: auto; }
    .result-item { background: #f8fafc; border-left: 3px solid #3b82f6; padding: 0.75rem; border-radius: 4px; }
    .result-header { display: flex; justify-content: space-between; font-size: 0.8125rem; font-weight: 600; }
    .result-args pre { margin: 0.5rem 0 0 0; font-size: 0.75rem; background: #0f172a; color: #38bdf8; padding: 0.5rem; border-radius: 4px; }
    .empty-state { color: #94a3b8; font-style: italic; text-align: center; padding: 2rem; }
    .loading-state { text-align: center; padding: 4rem; color: #64748b; }
  `]
})
export class InvestigationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly investigationService = inject(InvestigationService);

  investigation: InvestigationItem | null = null;
  probes: ProbeItem[] = [];
  selectedProbeId: string | null = null;
  results: ProbeResultItem[] = [];
  showAddProbeForm = false;

  newProbe = {
    probeType: 'Log',
    targetClass: '',
    targetMethod: '',
    expression: '',
    durationMinutes: 30
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadInvestigation(id);
    }
  }

  loadInvestigation(id: string): void {
    this.investigationService.getInvestigation(id).subscribe({
      next: inv => {
        this.investigation = inv;
        this.loadProbes(id);
      },
      error: () => {
        // Fallback placeholder for development / mock
        this.investigation = {
          id,
          applicationId: 'PaymentApi',
          title: 'Payment Performance Investigation',
          description: 'Investigating high latency spikes on checkout flow',
          status: 'Active',
          createdAtUtc: new Date().toISOString()
        };
        this.loadProbes(id);
      }
    });
  }

  loadProbes(investigationId: string): void {
    this.investigationService.getProbes(investigationId).subscribe({
      next: p => this.probes = p,
      error: () => this.probes = []
    });
  }

  createProbe(): void {
    if (!this.investigation) return;
    this.investigationService.addProbe(this.investigation.id, this.newProbe).subscribe({
      next: probe => {
        this.probes = [probe, ...this.probes];
        this.showAddProbeForm = false;
        this.newProbe.targetClass = '';
        this.newProbe.targetMethod = '';
        this.newProbe.expression = '';
      },
      error: err => alert('Failed to create probe: ' + (err.error?.error || err.message))
    });
  }

  activateProbe(probeId: string): void {
    this.investigationService.activateProbe(probeId).subscribe({
      next: updated => {
        const idx = this.probes.findIndex(p => p.id === probeId);
        if (idx >= 0) this.probes[idx] = updated;
      },
      error: err => alert('Activation failed: ' + (err.error?.error || err.message))
    });
  }

  deactivateProbe(probeId: string): void {
    this.investigationService.deactivateProbe(probeId).subscribe({
      next: updated => {
        const idx = this.probes.findIndex(p => p.id === probeId);
        if (idx >= 0) this.probes[idx] = updated;
      },
      error: err => alert('Deactivation failed: ' + (err.error?.error || err.message))
    });
  }

  viewResults(probeId: string): void {
    this.selectedProbeId = probeId;
    this.refreshResults();
  }

  refreshResults(): void {
    if (!this.selectedProbeId) return;
    this.investigationService.getProbeResults(this.selectedProbeId).subscribe({
      next: res => this.results = res,
      error: () => this.results = []
    });
  }
}
