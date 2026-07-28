import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Investigation {
  id: string;
  title: string;
  application: string;
  environment: string;
  description: string;
  status: string;
  createdAt: string;
}

export interface RuntimeProbe {
  id: string;
  investigationId: string;
  application: string;
  probeType: string;
  targetClass: string;
  targetMethod: string;
  condition: string;
  durationMinutes: number;
  status: string;
  createdAt: string;
}

export interface Evidence {
  id: string;
  probeId: string;
  application: string;
  targetClass: string;
  targetMethod: string;
  payload: string;
  capturedAt: string;
}

export interface AuditEntry {
  id: string;
  eventType: string;
  subjectId: string;
  message: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class DemoWorkflowService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/demoWorkflow';
  private selectedInvestigationId = '';

  getInvestigations() {
    return this.http.get<Investigation[]>(`${this.baseUrl}/investigations`);
  }

  setSelectedInvestigationId(id: string) {
    this.selectedInvestigationId = id;
  }

  getSelectedInvestigationId() {
    return this.selectedInvestigationId;
  }

  createInvestigation(payload: { title: string; application: string; environment: string; description?: string }) {
    return this.http.post<Investigation>(`${this.baseUrl}/investigations`, payload);
  }

  getProbes() {
    return this.http.get<RuntimeProbe[]>(`${this.baseUrl}/probes`);
  }

  createProbe(payload: { investigationId: string; application: string; probeType: string; targetClass: string; targetMethod: string; condition: string; durationMinutes: number }) {
    return this.http.post<RuntimeProbe>(`${this.baseUrl}/probes`, payload);
  }

  deployProbe(id: string) {
    return this.http.post<RuntimeProbe>(`${this.baseUrl}/probes/${id}/deploy`, {});
  }

  removeProbe(id: string) {
    return this.http.post<RuntimeProbe>(`${this.baseUrl}/probes/${id}/remove`, {});
  }

  getEvidence() {
    return this.http.get<Evidence[]>(`${this.baseUrl}/evidence`);
  }

  getAudit() {
    return this.http.get<AuditEntry[]>(`${this.baseUrl}/audit`);
  }
}
