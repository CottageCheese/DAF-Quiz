import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { QuizService } from '../quiz.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-quiz-list',
  standalone: true,
  imports: [LoadingSpinnerComponent],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1>Available Quizzes</h1>
    </div>

    @if (quizService.loading()) {
      <app-loading-spinner />
    } @else if (quizService.error()) {
      <div class="alert alert-danger">{{ quizService.error() }}</div>
    } @else if (quizService.quizzes().length === 0) {
      <div class="alert alert-info">No quizzes available yet.</div>
    } @else {
      <div class="row g-4">
        @for (quiz of quizService.quizzes(); track quiz.id) {
          <div class="col-md-6 col-lg-4">
            <div class="card h-100 shadow-sm">
              <div class="card-body d-flex flex-column">
                <h2 class="card-title h5">{{ quiz.title }}</h2>
                @if (quiz.description) {
                  <p class="card-text text-muted flex-grow-1">{{ quiz.description }}</p>
                } @else {
                  <div class="flex-grow-1"></div>
                }
                <div class="d-flex justify-content-between text-muted small mb-3">
                  <span>{{ quiz.questionCount }} question{{ quiz.questionCount !== 1 ? 's' : '' }}</span>
                  <span>{{ quiz.attemptCount }} attempt{{ quiz.attemptCount !== 1 ? 's' : '' }}</span>
                </div>
                <button
                  class="btn btn-primary w-100"
                  [disabled]="startingId() === quiz.id"
                  (click)="startQuiz(quiz.id)"
                >
                  @if (startingId() === quiz.id) {
                    <span class="spinner-border spinner-border-sm me-2"></span>
                  }
                  Take Quiz
                </button>
              </div>
            </div>
          </div>
        }
      </div>
    }
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
