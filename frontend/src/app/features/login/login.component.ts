import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section>
      <h2>Sign in</h2>
      <form (ngSubmit)="login()">
        <input name="username" [(ngModel)]="username" required placeholder="Username" />
        <input name="password" [(ngModel)]="password" type="password" required placeholder="Password" />
        <button type="submit">Sign in</button>
      </form>
      <p>{{ error }}</p>
    </section>
  `,
  styles: ['section, form { display: flex; flex-direction: column; gap: .75rem; max-width: 24rem; }', 'input, button { padding: .6rem; }']
})
export class LoginComponent {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  username = '';
  password = '';
  error = '';

  login(): void {
    this.http.post<{ accessToken: string }>('http://localhost:5000/api/auth/token', {
      username: this.username,
      password: this.password
    }).subscribe({
      next: response => {
        sessionStorage.setItem('accessToken', response.accessToken);
        this.router.navigateByUrl('/applications');
      },
      error: () => this.error = 'Sign-in failed.'
    });
  }
}
