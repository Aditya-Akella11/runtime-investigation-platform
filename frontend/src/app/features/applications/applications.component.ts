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
          <ng-container *ngIf="editingId !== app.id; else editForm">
            <strong>{{ app.name }}</strong> - {{ app.description }}
            <button type="button" (click)="startEdit(app)">Edit</button>
            <button type="button" (click)="deleteApplication(app.id)">Delete</button>
          </ng-container>
          <ng-template #editForm>
            <input [(ngModel)]="editName" [ngModelOptions]="{standalone: true}" required />
            <input [(ngModel)]="editDescription" [ngModelOptions]="{standalone: true}" />
            <button type="button" (click)="saveEdit(app.id)">Save</button>
            <button type="button" (click)="cancelEdit()">Cancel</button>
          </ng-template>
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
  private readonly apiUrl = 'http://localhost:5000/api/applications';
  applications: Array<{ id: string; name: string; description?: string; createdAt: string }> = [];
  name = '';
  description = '';
  editingId: string | null = null;
  editName = '';
  editDescription = '';

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.http.get<Array<{ id: string; name: string; description?: string; createdAt: string }>>(this.apiUrl).subscribe({
      next: (data) => this.applications = data,
      error: () => this.applications = []
    });
  }

  createApplication(): void {
    if (!this.name.trim()) {
      return;
    }

    this.http.post<{ id: string; name: string; description?: string; createdAt: string }>(this.apiUrl, {
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

  startEdit(app: { id: string; name: string; description?: string }): void {
    this.editingId = app.id; this.editName = app.name; this.editDescription = app.description ?? '';
  }

  cancelEdit(): void { this.editingId = null; }

  saveEdit(id: string): void {
    if (!this.editName.trim()) return;
    this.http.put(`${this.apiUrl}/${id}`, { name: this.editName, description: this.editDescription }).subscribe(() => { this.cancelEdit(); this.loadApplications(); });
  }

  deleteApplication(id: string): void {
    this.http.delete(`${this.apiUrl}/${id}`).subscribe(() => this.loadApplications());
  }
}
