import { Component, OnInit, computed, inject, signal } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { QuizService } from "../quiz.service";
import { QuestionAnswerSelection } from "../../../core/models/quiz.models";
import { LoadingSpinnerComponent } from "../../../shared/components/loading-spinner/loading-spinner.component";

@Component({
  selector: "app-take-quiz",
  standalone: true,
  imports: [LoadingSpinnerComponent],
  template: `
    @if (quizService.activeQuiz(); as quiz) {
      <div class="row justify-content-center">
        <div class="col-lg-8">
          <div class="card shadow-sm">
            <div class="card-header">
              <h4 class="mb-1">{{ quiz.quizTitle }}</h4>
              <div class="progress" style="height: 6px;">
                <div
                  class="progress-bar"
                  [style.width.%]="progress()"
                  role="progressbar"
                  [attr.aria-valuenow]="progress()"
                  aria-valuemin="0"
                  aria-valuemax="100"
                ></div>
              </div>
              <small class="text-muted">
                Question {{ currentIndex() + 1 }} of {{ quiz.totalQuestions }}
              </small>
            </div>

            @if (currentQuestion(); as q) {
              <div class="card-body p-4">
                <p class="fs-5 fw-semibold mb-4">{{ q.text }}</p>

                <div class="d-grid gap-2">
                  @for (answer of q.answers; track answer.answerId) {
                    <button
                      class="btn text-start"
                      [class.btn-primary]="
                        selections().get(q.questionId) === answer.answerId
                      "
                      [class.btn-outline-secondary]="
                        selections().get(q.questionId) !== answer.answerId
                      "
                      (click)="selectAnswer(q.questionId, answer.answerId)"
                    >
                      {{ answer.text }}
                    </button>
                  }
                </div>
              </div>

              <div class="card-footer d-flex justify-content-between">
                <button
                  class="btn btn-outline-secondary"
                  [disabled]="currentIndex() === 0"
                  (click)="prev()"
                >
                  Previous
                </button>

                @if (isLastQuestion()) {
                  <button
                    class="btn btn-success"
                    [disabled]="!canSubmit() || submitting()"
                    (click)="submit(quiz.attemptId, quiz.totalQuestions)"
                  >
                    @if (submitting()) {
                      <span
                        class="spinner-border spinner-border-sm me-2"
                      ></span>
                    }
                    Submit Quiz
                  </button>
                } @else {
                  <button
                    class="btn btn-primary"
                    [disabled]="!canGoNext()"
                    (click)="next()"
                  >
                    Next
                  </button>
                }
              </div>
            }
          </div>
        </div>
      </div>
    } @else if (loading()) {
      <app-loading-spinner />
    } @else {
      <div class="alert alert-danger">Quiz not found or failed to load.</div>
    }
  `,
})
export class TakeQuizComponent implements OnInit {
  protected readonly quizService = inject(QuizService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

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

  selectAnswer(questionId: number, answerId: number): void {
    this.selections.update((m) => new Map(m).set(questionId, answerId));
  }

  next(): void {
    this.currentIndex.update((i) => i + 1);
  }

  prev(): void {
    this.currentIndex.update((i) => Math.max(0, i - 1));
  }

  submit(attemptId: number, totalQuestions: number): void {
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
