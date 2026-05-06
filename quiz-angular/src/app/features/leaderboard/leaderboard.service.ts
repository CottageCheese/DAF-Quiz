import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, EMPTY, forkJoin } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { TopQuizViewModel, TopUserViewModel } from '../../core/models/leaderboard.models';
import { api } from '../../core/api';

@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly http = inject(HttpClient);

  private readonly _topQuizzes$ = new BehaviorSubject<TopQuizViewModel[]>([]);
  private readonly _topUsers$ = new BehaviorSubject<TopUserViewModel[]>([]);
  private readonly _loading$ = new BehaviorSubject<boolean>(false);

  readonly topQuizzes$ = this._topQuizzes$.asObservable();
  readonly topUsers$ = this._topUsers$.asObservable();
  readonly loading$ = this._loading$.asObservable();

  readonly topQuizzes = toSignal(this._topQuizzes$, { initialValue: [] });
  readonly topUsers = toSignal(this._topUsers$, { initialValue: [] });
  readonly loading = toSignal(this._loading$, { initialValue: false });

  loadLeaderboard(): void {
    this._loading$.next(true);
    forkJoin({
      quizzes: this.http.get<TopQuizViewModel[]>(api('/api/leaderboard/top-quizzes')),
      users: this.http.get<TopUserViewModel[]>(api('/api/leaderboard/top-users'))
    }).pipe(
      tap(({ quizzes, users }) => {
        this._topQuizzes$.next(quizzes);
        this._topUsers$.next(users);
      }),
      catchError(() => EMPTY),
      finalize(() => this._loading$.next(false))
    ).subscribe();
  }
}
