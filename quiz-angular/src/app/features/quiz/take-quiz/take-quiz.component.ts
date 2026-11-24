import { Component, OnInit, computed, inject, signal, ChangeDetectionStrategy, HostListener } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { QuizService } from "../quiz.service";
import { QuestionAnswerSelection } from "../../../core/models/quiz.models";
import { LoadingSpinnerComponent } from "../../../shared/components/loading-spinner/loading-spinner.component";

@Component({
  selector: "app-take-quiz",
  standalone: true,
  imports: [LoadingSpinnerComponent],
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .take-wrap { max-width: 760px; margin: 0 auto; padding: var(--s-6) var(--s-4) var(--s-8); }
    .q-card {
      position: relative;
      background: linear-gradient(145deg, var(--surface), var(--bg-elev));
      border: 1px solid var(--border);
      border-radius: var(--r-xl);
      padding: var(--s-6);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }
    .q-card::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0; height: 3px;
      background: linear-gradient(90deg, var(--violet), var(--cyan));
    }
    .q-head {
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      gap: var(--s-3);
      margin-bottom: var(--s-4);
    }
    .q-head h1 { margin: 0; font-size: 22px; }
    .q-head .counter {
      font-family: var(--font-mono);
      color: var(--text-dim);
      font-variant-numeric: tabular-nums;
      font-size: 13px;
    }
    .progress-neon {
      height: 6px;
      border-radius: var(--r-pill);
      background: var(--surface-hi);
      overflow: hidden;
      margin-bottom: var(--s-5);
    }
    .progress-neon > span {
      display: block;
      height: 100%;
      background: linear-gradient(90deg, var(--violet), var(--cyan));
      border-radius: var(--r-pill);
      transition: width 480ms var(--ease-out);
    }
    .q-text {
      font-size: 20px;
      font-weight: 600;
      color: var(--text);
      line-height: 1.4;
      margin: 0 0 var(--s-5);
    }
    .ans-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--s-3);
    }
    @media (max-width: 560px) { .ans-grid { grid-template-columns: 1fr; } }
    .ans {
      display: flex;
      align-items: center;
      gap: var(--s-3);
      padding: var(--s-4);
      border-radius: var(--r-lg);
      background: var(--surface);
      border: 2px solid var(--border);
      color: var(--text);
      font-size: 15px;
      font-weight: 500;
      text-align: left;
      cursor: pointer;
      transition: transform var(--t-fast), background var(--t-mid), border-color var(--t-mid), box-shadow var(--t-mid);
    }
    .ans:hover { transform: translateY(-1px); }
    .ans:nth-child(4n+1) { border-color: color-mix(in oklab, var(--ans-red) 55%, var(--border)); }
    .ans:nth-child(4n+2) { border-color: color-mix(in oklab, var(--ans-blue) 55%, var(--border)); }
    .ans:nth-child(4n+3) { border-color: color-mix(in oklab, var(--ans-amber) 55%, var(--border)); }
    .ans:nth-child(4n+4) { border-color: color-mix(in oklab, var(--ans-green) 55%, var(--border)); }
    .ans.selected {
      background: color-mix(in oklab, var(--violet) 18%, var(--surface));
      border-color: var(--violet);
      box-shadow: var(--glow-violet);
    }
    .ans .key {
      width: 28px;
      height: 28px;
      border-radius: 6px;
      background: var(--surface-hi);
      color: var(--text-dim);
      display: grid;
      place-items: center;
      font-family: var(--font-mono);
      font-weight: 700;
      font-size: 13px;
      flex-shrink: 0;
    }
    .ans.selected .key {
      background: var(--violet);
      color: #fff;
    }
    .q-actions {
      display: flex;
      justify-content: space-between;
      gap: var(--s-3);
      margin-top: var(--s-5);
      padding-top: var(--s-4);
      border-top: 1px solid var(--border);
    }
    .error {
      padding: var(--s-4);
      border-radius: var(--r-md);
      background: color-mix(in oklab, var(--danger) 15%, var(--surface));
      border: 1px solid color-mix(in oklab, var(--danger) 40%, var(--border));
      color: var(--danger);
      max-width: 760px;
      margin: var(--s-6) auto;
    }
  `],
  template: `
    @if (quizService.activeQuiz(); as quiz) {
      <div class="take-wrap">
        <article class="q-card" aria-live="polite">
          <header class="q-head">
            <h1>{{ quiz.quizTitle }}</h1>
            <span class="counter">Q{{ currentIndex() + 1 }} / {{ quiz.totalQuestions }}</span>
          </header>

          <div class="progress-neon" role="progressbar"
               [attr.aria-valuenow]="progress()"
               aria-valuemin="0"
               aria-valuemax="100"
               [attr.aria-label]="'Question ' + (currentIndex() + 1) + ' of ' + quiz.totalQuestions">
            <span [style.width.%]="progress()"></span>
          </div>

          @if (currentQuestion(); as q) {
            <p class="q-text">{{ q.text }}</p>

            <div class="ans-grid" role="radiogroup" [attr.aria-label]="q.text">
              @for (answer of q.answers; track answer.answerId; let i = $index) {
                <button type="button"
                        class="ans"
                        [class.selected]="selections().get(q.questionId) === answer.answerId"
                        [attr.aria-checked]="selections().get(q.questionId) === answer.answerId"
                        role="radio"
                        (click)="selectAnswer(q.questionId, answer.answerId)">
                  <span class="key" aria-hidden="true">{{ keys[i] }}</span>
                  <span>{{ answer.text }}</span>
                </button>
              }
            </div>

            <div class="q-actions">
              <button class="btn-ghost"
                      [disabled]="currentIndex() === 0"
                      (click)="prev()">Previous</button>

              @if (isLastQuestion()) {
                <button class="btn-neon"
                        [disabled]="!canSubmit() || submitting()"
                        (click)="submit(quiz.attemptId)">
                  {{ submitting() ? 'Submitting…' : 'Submit quiz' }}
                </button>
              } @else {
                <button class="btn-neon"
                        [disabled]="!canGoNext()"
                        (click)="next()">Next</button>
              }
            </div>
          }
        </article>
      </div>
    } @else if (loading()) {
      <app-loading-spinner />
    } @else {
      <div class="error" role="alert">Quiz not found or failed to load.</div>
    }
  `,
})
export class TakeQuizComponent implements OnInit {
  protected readonly quizService = inject(QuizService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly keys = ['A', 'B', 'C', 'D', 'E', 'F'];

  protected readonly loading = signal(true);
  protected readonly currentIndex = signal(0);
  protected readonly selections = signal(new Map<number, number>());
  protected readonly submitting = signal(false);

  protected readonly currentQuestion = computed(() => {
    const quiz = this.quizService.activeQuiz();
    return quiz ? quiz.questions[this.currentIndex()] : null;
  });

  protected readonly progress = computed(() => {
    const quiz = this.quizService.activeQuiz();
    if (!quiz) return 0;
    return Math.round(((this.currentIndex() + 1) / quiz.totalQuestions) * 100);
  });

  protected readonly canGoNext = computed(() => {
    const q = this.currentQuestion();
    return q != null && this.selections().has(q.questionId);
  });

  protected readonly isLastQuestion = computed(() => {
    const quiz = this.quizService.activeQuiz();
    return quiz != null && this.currentIndex() === quiz.totalQuestions - 1;
  });

  protected readonly canSubmit = computed(() => {
    const quiz = this.quizService.activeQuiz();
    if (!quiz) return false;
    return quiz.questions.every((q) => this.selections().has(q.questionId));
  });

  ngOnInit(): void {
    const quizId = Number(this.route.snapshot.paramMap.get("quizId"));
    this.quizService.startQuiz(quizId).subscribe({
      next: () => this.loading.set(false),
      error: () => this.router.navigate(["/quizzes"]),
    });
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyboard(ev: KeyboardEvent): void {
    const q = this.currentQuestion();
    if (!q) return;

    const idx = this.keys.indexOf(ev.key.toUpperCase());
    if (idx >= 0 && idx < q.answers.length) {
      ev.preventDefault();
      this.selectAnswer(q.questionId, q.answers[idx].answerId);
      return;
    }

    if (ev.key === 'Enter') {
      if (this.isLastQuestion()) {
        const quiz = this.quizService.activeQuiz();
        if (quiz && this.canSubmit() && !this.submitting()) {
          ev.preventDefault();
          this.submit(quiz.attemptId);
        }
      } else if (this.canGoNext()) {
        ev.preventDefault();
        this.next();
      }
    }
  }

  selectAnswer(questionId: number, answerId: number): void {
    this.selections.update((m) => new Map(m).set(questionId, answerId));
  }

  next(): void {
    this.currentIndex.update((i) => i + 1);
  }

  prev(): void {
    this.currentIndex.update((i) => Math.max(0, i - 1));
  }

  submit(attemptId: number): void {
    const quiz = this.quizService.activeQuiz();
    if (!quiz) return;

    this.submitting.set(true);
    const payload: QuestionAnswerSelection[] = Array.from(
      this.selections().entries(),
    ).map(([questionId, selectedAnswerId]) => ({
      questionId,
      selectedAnswerId,
    }));

    this.quizService.submitQuiz(attemptId, payload).subscribe({
      next: (result) =>
        this.router.navigate(["/quizzes/attempts", result.attemptId, "result"]),
      error: () => this.submitting.set(false),
    });
  }
}
