
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  password: string;
}

export interface Workout {
  id: number;
  title: string;
  date: string;
  durationMinutes: number;
  calories: number;
  notes: string | null;
  sportTypeId: number;
  sportTypeName: string;
  photoUrl: string | null;
  authorId: string;
}

export interface WorkoutCreateRequest {
  title: string;
  date: string;
  durationMinutes: number;
  calories: number;
  notes: string | null;
  sportTypeId: number;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
  totalMinutes: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);

  private readonly apiUrl = 'http://localhost:5245/api';

  register(data: RegisterRequest): Observable<{ id: number; username: string }> {
    return this.http.post<{ id: number; username: string }>(
      `${this.apiUrl}/auth/register`,
      data
    );
  }

  login(data: LoginRequest): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(
      `${this.apiUrl}/auth/login`,
      data
    );
  }

  getWorkouts(
    page = 1,
    pageSize = 10
  ): Observable<PagedResponse<Workout>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResponse<Workout>>(
      `${this.apiUrl}/workouts`,
      { params }
    );
  }

  getWorkout(id: number): Observable<Workout> {
    return this.http.get<Workout>(`${this.apiUrl}/workouts/${id}`);
  }

  createWorkout(data: WorkoutCreateRequest): Observable<Workout> {
    return this.http.post<Workout>(`${this.apiUrl}/workouts`, data);
  }

  updateWorkout(
    id: number,
    data: WorkoutCreateRequest
  ): Observable<Workout> {
    return this.http.put<Workout>(
      `${this.apiUrl}/workouts/${id}`,
      data
    );
  }

  deleteWorkout(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/workouts/${id}`);
  }

  uploadPhoto(id: number, data: FormData): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/workouts/${id}/photo`,
      data
    );
  }
}
