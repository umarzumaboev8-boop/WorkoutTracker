
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize, timeout } from 'rxjs';
import { ApiService, Workout } from '../../services/api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  private api = inject(ApiService);
  private cdr = inject(ChangeDetectorRef);

  workouts: Workout[] = [];
  loading = false;
  error = '';

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    this.error = '';
    this.cdr.markForCheck();

    this.api.getWorkouts(1, 100)
      .pipe(
        timeout({ first: 10000 }),
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: response => {
          this.workouts = response.items ?? [];
          this.cdr.markForCheck();
        },
        error: err => {
          console.error('Dashboard API error:', err);
          this.error = `Ошибка загрузки: ${err.status || 'нет соединения'}`;
          this.cdr.markForCheck();
        }
      });
  }

  get totalWorkouts(): number {
    return this.workouts.length;
  }

  get totalCalories(): number {
    return this.workouts.reduce((sum, w) => sum + w.calories, 0);
  }

  get totalMinutes(): number {
    return this.workouts.reduce((sum, w) => sum + w.durationMinutes, 0);
  }

  get totalHours(): string {
    return (this.totalMinutes / 60).toFixed(1);
  }

  get averageCalories(): number {
    return this.totalWorkouts
      ? Math.round(this.totalCalories / this.totalWorkouts)
      : 0;
  }

  get sportStats() {
    const groups = new Map<string, number>();

    this.workouts.forEach(w => {
      const name = w.sportTypeName || 'Other';
      groups.set(name, (groups.get(name) || 0) + 1);
    });

    return Array.from(groups.entries())
      .map(([name, count]) => ({
        name,
        count,
        percentage: Math.round(count / this.totalWorkouts * 100)
      }))
      .sort((a, b) => b.count - a.count);
  }

  get recentWorkouts(): Workout[] {
    return [...this.workouts]
      .sort((a, b) =>
        new Date(b.date).getTime() - new Date(a.date).getTime()
      )
      .slice(0, 5);
  }
}
