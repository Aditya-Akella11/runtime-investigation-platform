import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule],
  template: `
    <section class="container">
      <h2>Applications</h2>
      <form (ngSubmit)="createApplication()" #form="ngForm">
        <input name="name" [(ngModel)]="name" placeholder="Application name" required />
        <input name="description" [(ngModel)]="description" placeholder="Description" />
        <button type="submit">Create</button>
      </form>

      <ul>
        <li *ngFor="let app of applications">
          <strong>{{ app.name }}</strong> - {{ app.description }}
        </li>
      </ul>
    </section>
  `,
  styles: [
    '.container { display: flex; flex-direction: column; gap: 1rem; padding: 1rem; }',
    'form { display: flex; gap: 0.5rem; }',
    'input { padding: 0.5rem; }',
    'button { padding: 0.5rem 1rem; }'
  ]
})
export class ApplicationsComponent implements OnInit {
  private http = inject(HttpClient);
  applications: Array<{ id: string; name: string; description?: string; createdAt: string }> = [];
  name = '';
  description = '';

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.http.get<Array<{ id: string; name: string; description?: string; createdAt: string }>>('http://localhost:5000/api/applications').subscribe({
      next: (data) => this.applications = data,
      error: () => this.applications = []
    });
  }

  createApplication(): void {
    if (!this.name.trim()) {
      return;
    }

    this.http.post<{ id: string; name: string; description?: string; createdAt: string }>('http://localhost:5000/api/applications', {
      name: this.name,
      description: this.description
    }).subscribe({
      next: () => {
        this.name = '';
        this.description = '';
        this.loadApplications();
      }
    });
  }
}
