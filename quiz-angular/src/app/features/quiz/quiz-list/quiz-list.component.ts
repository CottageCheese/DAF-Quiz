import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { QuizService } from '../quiz.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-quiz-list',
  standalone: true,
  imports: [LoadingSpinnerComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .page-head h1 {
      background: linear-gradient(135deg, var(--violet), var(--cyan));
      -webkit-background-clip: text;
      background-clip: text;
      -webkit-text-fill-color: transparent;
      color: transparent;
    }
    .quiz-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: var(--s-5);
    }
    .quiz-card {
      position: relative;
      background: linear-gradient(145deg, var(--surface), var(--bg-elev));
      border: 1px solid var(--border);
      border-radius: var(--r-lg);
      padding: var(--s-5);
      box-shadow: var(--shadow-card);
      display: flex;
      flex-direction: column;
      transition: transform var(--t-mid) var(--ease-out), box-shadow var(--t-mid) var(--ease-out), border-color var(--t-mid) var(--ease-out);
      overflow: hidden;
    }
    .quiz-card::before {
      content: '';
      position: absolute;
      top: -40px; right: -40px;
      width: 120px; height: 120px;
      background: radial-gradient(circle, color-mix(in oklab, var(--violet) 40%, transparent) 0%, transparent 70%);
      pointer-events: none;
    }
    .quiz-card:hover,
    .quiz-card:focus-within {
      transform: translateY(-2px);
      border-color: color-mix(in oklab, var(--violet) 40%, var(--border));
      box-shadow: var(--glow-violet);
    }
    .quiz-card h2 {
      margin: 0 0 var(--s-2);
      font-size: 18px;
      font-weight: 600;
    }
    .quiz-card p {
      color: var(--text-dim);
      font-size: 14px;
      line-height: 1.55;
      margin: 0 0 var(--s-4);
      flex: 1 1 auto;
    }
    .meta {
      display: flex;
      gap: var(--s-2);
      flex-wrap: wrap;
      margin-bottom: var(--s-4);
    }
    .meta-chip {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 4px 10px;
      border-radius: var(--r-pill);
      background: var(--surface-hi);
      border: 1px solid var(--border);
      color: var(--text-dim);
      font-size: 12px;
      font-variant-numeric: tabular-nums;
    }
    .diff {
      padding: 4px 10px;
      border-radius: var(--r-pill);
      font-size: 11px;
      font-weight: 700;
      letter-spacing: 0.05em;
      text-transform: uppercase;
    }
    .diff.easy   { background: color-mix(in oklab, var(--success) 20%, var(--surface)); color: var(--success); }
    .diff.medium { background: color-mix(in oklab, var(--warn) 22%, var(--surface));    color: var(--warn); }
    .diff.hard   { background: color-mix(in oklab, var(--danger) 22%, var(--surface));  color: var(--danger); }
    .quiz-grid-empty {
      border: 1px dashed var(--border);
      border-radius: var(--r-lg);
      padding: var(--s-7);
      text-align: center;
      color: var(--text-dim);
    }
    .error {
      padding: var(--s-4);
      border-radius: var(--r-md);
      background: color-mix(in oklab, var(--danger) 15%, var(--surface));
      border: 1px solid color-mix(in oklab, var(--danger) 40%, var(--border));
      color: var(--danger);
    }
  `],
  template: `
    <div class="page">
      <header class="page-head">
        <div>
          <h1>Available quizzes</h1>
          <p class="sub">Pick a quiz and prove what you know.</p>
        </div>
      </header>

      @if (quizService.loading()) {
        <app-loading-spinner />
      } @else if (quizService.error()) {
        <div class="error" role="alert">{{ quizService.error() }}</div>
      } @else if (quizService.quizzes().length === 0) {
        <div class="quiz-grid-empty" role="status">
          <h2>No quizzes yet</h2>
          <p>Check back soon — new quizzes land weekly.</p>
        </div>
      } @else {
        <div class="quiz-grid">
          @for (quiz of quizService.quizzes(); track quiz.id) {
            <article class="quiz-card">
              <h2>{{ quiz.title }}</h2>
              @if (quiz.description) {
                <p>{{ quiz.description }}</p>
              } @else {
                <p>&nbsp;</p>
              }
              <div class="meta">
                <span class="meta-chip">{{ quiz.questionCount }} q</span>
                <span class="meta-chip">{{ quiz.attemptCount }} played</span>
                <span class="diff" [class.easy]="quiz.questionCount <= 5" [class.medium]="quiz.questionCount > 5 && quiz.questionCount <= 10" [class.hard]="quiz.questionCount > 10">
                  {{ quiz.questionCount <= 5 ? 'Easy' : quiz.questionCount <= 10 ? 'Medium' : 'Hard' }}
                </span>
              </div>
              <button
                class="btn-neon"
                [disabled]="startingId() === quiz.id"
                (click)="startQuiz(quiz.id)"
                [attr.aria-label]="'Start ' + quiz.title">
                @if (startingId() === quiz.id) {
                  Starting…
                } @else {
                  Start quiz
                }
              </button>
            </article>
          }
        </div>
      }
    </div>
  `
})
export class QuizListComponent implements OnInit {
  protected readonly quizService = inject(QuizService);
  private readonly router = inject(Router);

  protected readonly startingId = signal<number | null>(null);

  ngOnInit(): void {
    this.quizService.loadQuizzes();
  }

  startQuiz(quizId: number): void {
    this.startingId.set(quizId);
    this.quizService.startQuiz(quizId).subscribe({
      next: () => this.router.navigate(['/quizzes', quizId, 'take']),
      error: () => this.startingId.set(null)
    });
  }
}
