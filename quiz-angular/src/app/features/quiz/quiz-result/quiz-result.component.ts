import { Component, OnInit, inject, signal } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { QuizService } from "../quiz.service";
import { QuizResultViewModel } from "../../../core/models/quiz.models";
import { GradeBadgeComponent } from "../../../shared/components/grade-badge/grade-badge.component";
import { LoadingSpinnerComponent } from "../../../shared/components/loading-spinner/loading-spinner.component";

@Component({
  selector: "app-quiz-result",
  standalone: true,
  imports: [GradeBadgeComponent, LoadingSpinnerComponent, RouterLink],
  template: `
    @if (result(); as r) {
      <div class="row justify-content-center">
        <div class="col-lg-8">
          <div class="card shadow-sm mb-4">
            <div class="card-body text-center p-5">
              <h1 class="mb-4 h2">{{ r.quizTitle }}</h1>

              <div
                class="score-circle mb-4"
                [class.text-success]="r.percentage >= 70"
                [class.text-warning]="r.percentage >= 50 && r.percentage < 70"
                [class.text-danger]="r.percentage < 50"
              >
                <span class="score-number"
                  >{{ r.score }}/{{ r.totalQuestions }}</span
                >
                <span class="score-label">{{ r.percentage }}%</span>
              </div>

              <div class="mb-4">
                <app-grade-badge [grade]="r.grade" />
              </div>

              <div class="d-flex justify-content-center gap-3">
                <a routerLink="/quizzes" class="btn btn-primary"
                  >Take Another Quiz</a
                >
                <a routerLink="/leaderboard" class="btn btn-outline-secondary"
                  >Leaderboard</a
                >
                <a routerLink="/history" class="btn btn-outline-secondary"
                  >My History</a
                >
              </div>
            </div>
          </div>

          <div class="card shadow-sm">
            <div class="card-header">
              <h2 class="mb-0 h5">Answer Breakdown</h2>
            </div>
            <div class="table-responsive" tabindex="0">
              <table class="table table-hover mb-0">
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Question</th>
                    <th>Your Answer</th>
                    <th>Correct Answer</th>
                    <th>Result</th>
                  </tr>
                </thead>
                <tbody>
                  @for (answer of r.answers; track $index) {
                    <tr>
                      <td>{{ $index + 1 }}</td>
                      <td>{{ answer.questionText }}</td>
                      <td [class.text-danger]="!answer.isCorrect">
                        {{ answer.selectedAnswerText }}
                      </td>
                      <td class="text-success">
                        {{ answer.correctAnswerText }}
                      </td>
                      <td>
                        @if (answer.isCorrect) {
                          <span class="badge bg-success">Correct</span>
                        } @else {
                          <span class="badge bg-danger">Wrong</span>
                        }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    } @else if (loading()) {
      <app-loading-spinner />
    } @else {
      <div class="alert alert-danger">
        Result not found. <a routerLink="/quizzes">Back to quizzes</a>
      </div>
    }
  `,
})
export class QuizResultComponent implements OnInit {
  private readonly quizService = inject(QuizService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly loading = signal(true);
  protected readonly result = signal<QuizResultViewModel | null>(null);

  ngOnInit(): void {
    // Check if result already in service state (just submitted)
    const existing = this.quizService.quizResult();
    const attemptId = Number(this.route.snapshot.paramMap.get("attemptId"));

    if (existing && existing.attemptId === attemptId) {
      this.result.set(existing);
      this.loading.set(false);
      return;
    }

    // Otherwise fetch from API (direct navigation / page refresh)
    this.quizService.getResult(attemptId).subscribe({
      next: (r) => {
        this.result.set(r);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
