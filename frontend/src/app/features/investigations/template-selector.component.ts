import { Component, OnInit, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  TemplateService,
  InvestigationTemplateDto
} from '../../core/template.service';

export interface TemplateSelectorResult {
  templateId: string;
  applicationId: string;
  title: string;
  description: string;
}

@Component({
  selector: 'app-template-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="modal-overlay" (click)="onOverlayClick($event)">
      <div class="modal-panel" role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <div class="modal-header">
          <h2 id="modal-title">Create from Template</h2>
          <button class="close-btn" (click)="cancel()" aria-label="Close">&times;</button>
        </div>

        <!-- Loading state -->
        <div class="loading-state" *ngIf="loading">
          <div class="spinner"></div>
          <p>Loading templates&hellip;</p>
        </div>

        <!-- Error state -->
        <div class="error-state" *ngIf="error">
          <p>{{ error }}</p>
          <button class="btn btn-outline" (click)="loadTemplates()">Retry</button>
        </div>

        <!-- Template list + form -->
        <ng-container *ngIf="!loading && !error">
          <!-- Step 1: pick template -->
          <div class="step" *ngIf="step === 1">
            <p class="step-hint">Select a built-in template to pre-populate your investigation with recommended probes.</p>
            <div class="template-grid">
              <div
                *ngFor="let t of templates"
                class="template-card"
                [class.selected]="selectedTemplate?.id === t.id"
                (click)="selectTemplate(t)"
                role="button"
                tabindex="0"
                (keyup.enter)="selectTemplate(t)"
              >
                <div class="template-card-header">
                  <span class="template-name">{{ t.name }}</span>
                  <span class="template-category">{{ t.category }}</span>
                </div>
                <p class="template-desc">{{ t.description }}</p>
                <div class="probe-summary">
                  <span class="probe-count">{{ t.probeTemplates.length }} probe{{ t.probeTemplates.length !== 1 ? 's' : '' }}</span>
                  <span *ngFor="let p of t.probeTemplates.slice(0, 2)" class="probe-chip">{{ p.type }}</span>
                  <span *ngIf="t.probeTemplates.length > 2" class="probe-chip more">+{{ t.probeTemplates.length - 2 }} more</span>
                </div>
              </div>
            </div>

            <div class="modal-footer">
              <button class="btn btn-outline" (click)="cancel()">Cancel</button>
              <button class="btn btn-primary" [disabled]="!selectedTemplate" (click)="step = 2">
                Next &rarr;
              </button>
            </div>
          </div>

          <!-- Step 2: fill in details -->
          <div class="step" *ngIf="step === 2">
            <div class="selected-preview" *ngIf="selectedTemplate">
              <span class="template-badge">{{ selectedTemplate.name }}</span>
              <button class="link-btn" (click)="step = 1">Change template</button>
            </div>

            <div class="form-group">
              <label for="ts-app-id">Application ID <span class="required">*</span></label>
              <input
                id="ts-app-id"
                type="text"
                [(ngModel)]="form.applicationId"
                placeholder="e.g. PaymentApi"
                required
              />
            </div>

            <div class="form-group">
              <label for="ts-title">Investigation Title</label>
              <input
                id="ts-title"
                type="text"
                [(ngModel)]="form.title"
                placeholder="Leave blank to use template name"
              />
            </div>

            <div class="form-group">
              <label for="ts-desc">Description</label>
              <textarea
                id="ts-desc"
                [(ngModel)]="form.description"
                rows="3"
                placeholder="Optional description"
              ></textarea>
            </div>

            <div class="modal-footer">
              <button class="btn btn-outline" (click)="step = 1">&larr; Back</button>
              <button
                class="btn btn-primary"
                [disabled]="!form.applicationId || submitting"
                (click)="submit()"
              >
                <span *ngIf="submitting" class="spinner-inline"></span>
                {{ submitting ? 'Creating…' : 'Create Investigation' }}
              </button>
            </div>
          </div>
        </ng-container>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center; z-index: 1000;
    }
    .modal-panel {
      background: #fff; border-radius: 10px; width: 90%; max-width: 680px;
      max-height: 90vh; overflow-y: auto; box-shadow: 0 20px 60px rgba(0,0,0,0.25);
    }
    .modal-header {
      display: flex; justify-content: space-between; align-items: center;
      padding: 1.25rem 1.5rem; border-bottom: 1px solid #e2e8f0;
    }
    .modal-header h2 { margin: 0; font-size: 1.25rem; color: #0f172a; }
    .close-btn { background: none; border: none; font-size: 1.5rem; cursor: pointer; color: #94a3b8; line-height: 1; padding: 0 0.25rem; }
    .close-btn:hover { color: #475569; }

    .step { padding: 1.25rem 1.5rem; }
    .step-hint { color: #64748b; margin: 0 0 1rem 0; font-size: 0.875rem; }

    .template-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; }
    .template-card {
      border: 2px solid #e2e8f0; border-radius: 8px; padding: 1rem;
      cursor: pointer; transition: all 0.15s ease;
    }
    .template-card:hover { border-color: #93c5fd; background: #f0f9ff; }
    .template-card.selected { border-color: #2563eb; background: #eff6ff; }
    .template-card-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.5rem; }
    .template-name { font-weight: 600; color: #0f172a; font-size: 0.9375rem; }
    .template-category { background: #dbeafe; color: #1d4ed8; border-radius: 4px; padding: 0.125rem 0.4rem; font-size: 0.7rem; font-weight: 600; text-transform: uppercase; }
    .template-desc { color: #64748b; font-size: 0.8125rem; margin: 0 0 0.75rem 0; }
    .probe-summary { display: flex; align-items: center; gap: 0.375rem; flex-wrap: wrap; }
    .probe-count { font-size: 0.75rem; color: #64748b; margin-right: 0.25rem; }
    .probe-chip { background: #f1f5f9; border: 1px solid #cbd5e1; color: #475569; font-size: 0.7rem; padding: 0.1rem 0.4rem; border-radius: 3px; }
    .probe-chip.more { background: #e0f2fe; color: #0369a1; border-color: #bae6fd; }

    .form-group { margin-bottom: 1rem; }
    .form-group label { display: block; font-size: 0.75rem; font-weight: 600; color: #475569; margin-bottom: 0.25rem; text-transform: uppercase; letter-spacing: 0.05em; }
    .required { color: #dc2626; }
    .form-group input, .form-group textarea {
      width: 100%; padding: 0.5rem 0.75rem; border: 1px solid #cbd5e1;
      border-radius: 4px; font-size: 0.875rem; box-sizing: border-box;
      transition: border-color 0.15s;
    }
    .form-group input:focus, .form-group textarea:focus { outline: none; border-color: #2563eb; box-shadow: 0 0 0 3px rgba(37,99,235,0.1); }
    .form-group textarea { resize: vertical; }

    .selected-preview { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1.25rem; }
    .template-badge { background: #eff6ff; border: 1px solid #bfdbfe; color: #1d4ed8; padding: 0.3rem 0.7rem; border-radius: 4px; font-size: 0.8125rem; font-weight: 600; }
    .link-btn { background: none; border: none; color: #2563eb; cursor: pointer; font-size: 0.8125rem; text-decoration: underline; }

    .modal-footer { display: flex; justify-content: flex-end; gap: 0.75rem; margin-top: 1.5rem; padding-top: 1rem; border-top: 1px solid #f1f5f9; }
    .btn { padding: 0.5rem 1rem; border-radius: 4px; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; transition: all 0.15s; }
    .btn-primary { background: #2563eb; color: white; }
    .btn-primary:hover:not(:disabled) { background: #1d4ed8; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-outline { background: transparent; border: 1px solid #cbd5e1; color: #475569; }
    .btn-outline:hover { border-color: #94a3b8; }

    .loading-state { display: flex; flex-direction: column; align-items: center; padding: 3rem; gap: 1rem; color: #64748b; }
    .spinner { width: 32px; height: 32px; border: 3px solid #e2e8f0; border-top-color: #2563eb; border-radius: 50%; animation: spin 0.75s linear infinite; }
    .spinner-inline { display: inline-block; width: 14px; height: 14px; border: 2px solid rgba(255,255,255,0.4); border-top-color: white; border-radius: 50%; animation: spin 0.75s linear infinite; margin-right: 0.5rem; vertical-align: middle; }
    @keyframes spin { to { transform: rotate(360deg); } }

    .error-state { padding: 2rem; text-align: center; color: #991b1b; }
  `]
})
export class TemplateSelectorComponent implements OnInit {
  @Output() selected = new EventEmitter<TemplateSelectorResult>();
  @Output() cancelled = new EventEmitter<void>();

  private readonly templateService = inject(TemplateService);

  templates: InvestigationTemplateDto[] = [];
  selectedTemplate: InvestigationTemplateDto | null = null;
  step = 1;
  loading = false;
  error: string | null = null;
  submitting = false;

  form = {
    applicationId: '',
    title: '',
    description: ''
  };

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.loading = true;
    this.error = null;
    this.templateService.getTemplates().subscribe({
      next: ts => {
        this.templates = ts;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load templates. Please check your connection and try again.';
        this.loading = false;
      }
    });
  }

  selectTemplate(t: InvestigationTemplateDto): void {
    this.selectedTemplate = t;
  }

  submit(): void {
    if (!this.selectedTemplate || !this.form.applicationId) return;
    this.submitting = true;
    this.selected.emit({
      templateId: this.selectedTemplate.id,
      applicationId: this.form.applicationId,
      title: this.form.title || this.selectedTemplate.name,
      description: this.form.description
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }

  onOverlayClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-overlay')) {
      this.cancel();
    }
  }
}
