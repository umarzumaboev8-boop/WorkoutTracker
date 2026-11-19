
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  ApiService,
  Workout,
  WorkoutCreateRequest
} from '../../services/api';

import { finalize } from 'rxjs';

@Component({
  selector: 'app-workouts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './workouts.html',
  styleUrl: './workouts.scss'
})
export class Workouts implements OnInit {

  private api = inject(ApiService);
  private cdr = inject(ChangeDetectorRef);

  workouts: Workout[] = [];

  loading = false;
  saving = false;
  uploadingId: number | null = null;

  error = '';
  success = '';

  page = 1;
  pageSize = 10;
  total = 0;

  showForm = false;
  editingId: number | null = null;

  sportTypes = [
    { id: 1, name: 'Boxing' },
    { id: 2, name: 'Running' },
    { id: 3, name: 'Swimming' },
    { id: 4, name: 'Gym' },
    { id: 5, name: 'Cycling' }
  ];

  form = this.emptyForm();

  ngOnInit(): void {
    this.loadWorkouts();
  }

  private emptyForm() {
    const localDate = new Date();

    localDate.setMinutes(
      localDate.getMinutes() - localDate.getTimezoneOffset()
    );

    return {
      title: '',
      date: localDate.toISOString().slice(0, 16),
      durationMinutes: 60,
      calories: 300,
      notes: '',
      sportTypeId: 1
    };
  }

  loadWorkouts(): void {
    this.loading = true;
    this.error = '';

    this.cdr.markForCheck();

    this.api.getWorkouts(this.page, this.pageSize)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: response => {
          this.workouts = response.items ?? [];
          this.total = response.total ?? 0;

          this.cdr.markForCheck();
        },

        error: (err: any) => {
          console.error('Workouts loading error:', err);

          this.error = `Не удалось загрузить тренировки. HTTP ${err.status || 'ошибка соединения'}`;

          this.cdr.markForCheck();
        }
      });
  }

  openCreateForm(): void {
    this.editingId = null;
    this.form = this.emptyForm();
    this.showForm = true;
    this.error = '';
    this.success = '';

    this.cdr.markForCheck();
  }

  editWorkout(workout: Workout): void {
    this.editingId = workout.id;

    const date = new Date(workout.date);

    date.setMinutes(
      date.getMinutes() - date.getTimezoneOffset()
    );

    this.form = {
      title: workout.title,
      date: date.toISOString().slice(0, 16),
      durationMinutes: workout.durationMinutes,
      calories: workout.calories,
      notes: workout.notes ?? '',
      sportTypeId: workout.sportTypeId
    };

    this.showForm = true;
    this.error = '';
    this.success = '';

    this.cdr.markForCheck();
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingId = null;
    this.form = this.emptyForm();
    this.error = '';

    this.cdr.markForCheck();
  }

  saveWorkout(): void {
    this.error = '';
    this.success = '';

    if (!this.form.title.trim()) {
      this.error = 'Введите название тренировки';
      this.cdr.markForCheck();
      return;
    }

    if (
      !this.form.date ||
      this.form.durationMinutes <= 0 ||
      this.form.calories < 0
    ) {
      this.error = 'Проверьте дату, продолжительность и калории';
      this.cdr.markForCheck();
      return;
    }

    const request: WorkoutCreateRequest = {
      title: this.form.title.trim(),
      date: new Date(this.form.date).toISOString(),
      durationMinutes: Number(this.form.durationMinutes),
      calories: Number(this.form.calories),
      notes: this.form.notes.trim() || null,
      sportTypeId: Number(this.form.sportTypeId)
    };

    this.saving = true;
    this.cdr.markForCheck();

    const operation = this.editingId === null
      ? this.api.createWorkout(request)
      : this.api.updateWorkout(this.editingId, request);

    operation
      .pipe(
        finalize(() => {
          this.saving = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.success = this.editingId === null
            ? 'Тренировка добавлена'
            : 'Тренировка обновлена';

          this.showForm = false;
          this.editingId = null;
          this.form = this.emptyForm();

          this.loadWorkouts();

          this.cdr.markForCheck();
        },

        error: (err: any) => {
          console.error('Save workout error:', err);

          this.error = `Не удалось сохранить тренировку. HTTP ${err.status || 'ошибка соединения'}`;

          this.cdr.markForCheck();
        }
      });
  }

  deleteWorkout(id: number): void {
    if (!window.confirm('Удалить эту тренировку?')) {
      return;
    }

    this.error = '';
    this.success = '';

    this.api.deleteWorkout(id).subscribe({
      next: () => {
        this.success = 'Тренировка удалена';

        if (this.workouts.length === 1 && this.page > 1) {
          this.page--;
        }

        this.loadWorkouts();

        this.cdr.markForCheck();
      },

      error: (err: any) => {
        console.error('Delete workout error:', err);

        this.error = `Не удалось удалить тренировку. HTTP ${err.status || 'ошибка соединения'}`;

        this.cdr.markForCheck();
      }
    });
  }

  uploadPhoto(event: Event, id: number): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.error = 'Максимальный размер фотографии — 5 МБ';
      input.value = '';
      this.cdr.markForCheck();
      return;
    }

    const allowed = [
      'image/jpeg',
      'image/png',
      'image/webp'
    ];

    if (!allowed.includes(file.type)) {
      this.error = 'Разрешены JPG, PNG и WEBP';
      input.value = '';
      this.cdr.markForCheck();
      return;
    }

    const data = new FormData();
    data.append('file', file);

    this.uploadingId = id;
    this.error = '';
    this.success = '';

    this.cdr.markForCheck();

    this.api.uploadPhoto(id, data)
      .pipe(
        finalize(() => {
          this.uploadingId = null;
          input.value = '';
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          this.success = 'Фотография загружена';

          this.loadWorkouts();

          this.cdr.markForCheck();
        },

        error: (err: any) => {
          console.error('Photo upload error:', err);

          this.error = `Ошибка загрузки фотографии. HTTP ${err.status || 'ошибка соединения'}`;

          this.cdr.markForCheck();
        }
      });
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadWorkouts();
    }
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.total) {
      this.page++;
      this.loadWorkouts();
    }
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }
}
