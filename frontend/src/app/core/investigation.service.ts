import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InvestigationItem {
  id: string;
  applicationId: string;
  title: string;
  description: string;
  status: string;
  createdAtUtc: string;
}

export interface ProbeItem {
  id: string;
  investigationId: string;
  type: string;
  target: string;
  expression?: string;
  status: string;
  createdAt: string;
  expiresAt: string;
}

export interface ProbeResultItem {
  id: string;
  probeId: string;
  correlationId: string;
  methodName: string;
  arguments: Record<string, string>;
  returnValue?: string;
  durationMs: number;
  capturedAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class InvestigationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api';

  getInvestigations(): Observable<InvestigationItem[]> {
    return this.http.get<InvestigationItem[]>(`${this.baseUrl}/investigations`);
  }

  getInvestigation(id: string): Observable<InvestigationItem> {
    return this.http.get<InvestigationItem>(`${this.baseUrl}/investigations/${id}`);
  }

  createInvestigation(payload: { applicationId: string; title: string; description: string }): Observable<InvestigationItem> {
    return this.http.post<InvestigationItem>(`${this.baseUrl}/investigations`, payload);
  }

  getProbes(investigationId: string): Observable<ProbeItem[]> {
    return this.http.get<ProbeItem[]>(`${this.baseUrl}/investigations/${investigationId}/probes`);
  }

  addProbe(investigationId: string, payload: { probeType?: string; targetClass: string; targetMethod: string; expression?: string; durationMinutes?: number }): Observable<ProbeItem> {
    return this.http.post<ProbeItem>(`${this.baseUrl}/investigations/${investigationId}/probes`, payload);
  }

  activateProbe(probeId: string, application = 'PaymentApi'): Observable<ProbeItem> {
    return this.http.post<ProbeItem>(`${this.baseUrl}/probes/${probeId}/activate?application=${application}`, {});
  }

  deactivateProbe(probeId: string, reason = 'Operator deactivated'): Observable<ProbeItem> {
    return this.http.post<ProbeItem>(`${this.baseUrl}/probes/${probeId}/deactivate?reason=${encodeURIComponent(reason)}`, {});
  }

  getProbeResults(probeId: string, limit = 100): Observable<ProbeResultItem[]> {
    return this.http.get<ProbeResultItem[]>(`${this.baseUrl}/probes/${probeId}/results?limit=${limit}`);
  }
}
