import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, EMPTY } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { UserAttemptHistoryViewModel } from '../../core/models/quiz.models';
import { api } from '../../core/api';

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private readonly http = inject(HttpClient);

  private readonly _history$ = new BehaviorSubject<UserAttemptHistoryViewModel[]>([]);
  private readonly _loading$ = new BehaviorSubject<boolean>(false);

  readonly history$ = this._history$.asObservable();
  readonly loading$ = this._loading$.asObservable();

  readonly history = toSignal(this._history$, { initialValue: [] });
  readonly loading = toSignal(this._loading$, { initialValue: false });

  loadHistory(): void {
    this._loading$.next(true);
    this.http.get<UserAttemptHistoryViewModel[]>(api('/api/quizzes/my-history')).pipe(
      tap(h => this._history$.next(h)),
      catchError(() => {
        this._history$.next([]);
        return EMPTY;
      }),
      finalize(() => this._loading$.next(false))
    ).subscribe();
  }
}
