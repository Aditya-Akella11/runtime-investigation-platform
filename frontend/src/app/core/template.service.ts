import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ProbeTemplateDto {
  type: string;
  targetClass: string;
  targetMethod: string;
  expression?: string;
  condition?: string;
  durationMinutes: number;
}

export interface InvestigationTemplateDto {
  id: string;
  name: string;
  description: string;
  category: string;
  probeTemplates: ProbeTemplateDto[];
}

export interface TemplateInstantiationResult {
  investigation: any;
  probes: any[];
}

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api';

  getTemplates(): Observable<InvestigationTemplateDto[]> {
    return this.http.get<InvestigationTemplateDto[]>(`${this.baseUrl}/templates`);
  }

  getTemplate(id: string): Observable<InvestigationTemplateDto> {
    return this.http.get<InvestigationTemplateDto>(`${this.baseUrl}/templates/${id}`);
  }

  createFromTemplate(command: {
    templateId: string;
    applicationId: string;
    title?: string;
    description?: string;
  }): Observable<TemplateInstantiationResult> {
    return this.http.post<TemplateInstantiationResult>(`${this.baseUrl}/templates/instantiate`, command);
  }
}
