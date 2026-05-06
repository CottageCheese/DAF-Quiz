import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, EMPTY, Observable } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { QuizListViewModel, TakeQuizViewModel, QuizResultViewModel, QuestionAnswerSelection } from '../../core/models/quiz.models';
import { api } from '../../core/api';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private readonly http = inject(HttpClient);

  private readonly _quizzes$ = new BehaviorSubject<QuizListViewModel[]>([]);
  private readonly _activeQuiz$ = new BehaviorSubject<TakeQuizViewModel | null>(null);
  private readonly _quizResult$ = new BehaviorSubject<QuizResultViewModel | null>(null);
  private readonly _loading$ = new BehaviorSubject<boolean>(false);
  private readonly _error$ = new BehaviorSubject<string | null>(null);

  readonly quizzes$ = this._quizzes$.asObservable();
  readonly activeQuiz$ = this._activeQuiz$.asObservable();
  readonly quizResult$ = this._quizResult$.asObservable();
  readonly loading$ = this._loading$.asObservable();
  readonly error$ = this._error$.asObservable();

  readonly quizzes = toSignal(this._quizzes$, { initialValue: [] });
  readonly activeQuiz = toSignal(this._activeQuiz$, { initialValue: null });
  readonly quizResult = toSignal(this._quizResult$, { initialValue: null });
  readonly loading = toSignal(this._loading$, { initialValue: false });
  readonly error = toSignal(this._error$, { initialValue: null });

  loadQuizzes(): void {
    this._loading$.next(true);
    this._error$.next(null);
    this.http.get<QuizListViewModel[]>(api('/api/quizzes')).pipe(
      tap(q => this._quizzes$.next(q)),
      catchError(() => {
        this._error$.next('Failed to load quizzes. Please try again.');
        return EMPTY;
      }),
      finalize(() => this._loading$.next(false))
    ).subscribe();
  }

  startQuiz(quizId: number): Observable<TakeQuizViewModel> {
    return this.http.post<TakeQuizViewModel>(api(`/api/quizzes/${quizId}/start`), {}).pipe(
      tap(quiz => this._activeQuiz$.next(quiz))
    );
  }

  submitQuiz(attemptId: number, selections: QuestionAnswerSelection[]): Observable<QuizResultViewModel> {
    return this.http.post<QuizResultViewModel>(
      api(`/api/quizzes/attempts/${attemptId}/submit`),
      selections
    ).pipe(
      tap(result => {
        this._quizResult$.next(result);
        this._activeQuiz$.next(null);
      })
    );
  }

  getResult(attemptId: number): Observable<QuizResultViewModel> {
    return this.http.get<QuizResultViewModel>(api(`/api/quizzes/attempts/${attemptId}/result`)).pipe(
      tap(result => this._quizResult$.next(result))
    );
  }
}
