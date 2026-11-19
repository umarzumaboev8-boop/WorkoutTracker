
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth.html',
  styleUrl: './auth.scss'
})
export class Auth {
  private api = inject(ApiService);
  private router = inject(Router);

  isRegister = false;
  username = '';
  password = '';
  error = '';
  loading = false;

  submit(): void {
    this.error = '';

    if (!this.username.trim() || !this.password) {
      this.error = 'Введите имя пользователя и пароль';
      return;
    }

    this.loading = true;

    const request = {
      username: this.username.trim(),
      password: this.password
    };

    if (this.isRegister) {
      this.api.register(request).subscribe({
        next: () => {
          this.isRegister = false;
          this.password = '';
          this.error = 'Аккаунт создан. Теперь войдите.';
          this.loading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.handleError(err);
        }
      });

      return;
    }

    this.api.login(request).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err: HttpErrorResponse) => {
        this.handleError(err);
      }
    });
  }

  private handleError(err: HttpErrorResponse): void {
    console.error('Authentication error:', err);

    this.error =
      err.error?.message ||
      err.error?.title ||
      'Ошибка авторизации';

    this.loading = false;
  }
}
