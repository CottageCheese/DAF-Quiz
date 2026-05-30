import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { HistoryService } from './history.service';
import { GradeBadgeComponent } from '../../shared/components/grade-badge/grade-badge.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [GradeBadgeComponent, LoadingSpinnerComponent, RouterLink, DatePipe],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1>My Quiz History</h1>
      <a routerLink="/quizzes" class="btn btn-primary">Browse Quizzes</a>
    </div>

    @if (historyService.loading()) {
      <app-loading-spinner />
    } @else if (historyService.history().length === 0) {
      <div class="alert alert-info">
        No completed quizzes yet. <a routerLink="/quizzes">Take a quiz!</a>
      </div>
    } @else {
      <div class="card shadow-sm">
        <div class="table-responsive" tabindex="0">
          <table class="table table-hover mb-0">
            <thead>
              <tr>
                <th>Quiz</th>
                <th class="text-center">Score</th>
                <th class="text-center">Percentage</th>
                <th class="text-center">Grade</th>
                <th class="text-end">Completed</th>
              </tr>
            </thead>
            <tbody>
              @for (attempt of historyService.history(); track attempt.attemptId) {
                <tr>
                  <td>{{ attempt.quizTitle }}</td>
                  <td class="text-center">{{ attempt.score }} / {{ attempt.totalQuestions }}</td>
                  <td class="text-center">{{ attempt.percentage }}%</td>
                  <td class="text-center">
                    <app-grade-badge [grade]="attempt.grade" />
                  </td>
                  <td class="text-end text-muted small">
                    {{ attempt.completedAt | date: 'd MMM yyyy, HH:mm' }}
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }
  `
})
export class HistoryComponent implements OnInit {
  protected readonly historyService = inject(HistoryService);

  ngOnInit(): void {
    this.historyService.loadHistory();
  }
}
