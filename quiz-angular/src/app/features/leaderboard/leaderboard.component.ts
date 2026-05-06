import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LeaderboardService } from './leaderboard.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [LoadingSpinnerComponent, RouterLink],
  template: `
    <h1 class="mb-4">Leaderboard</h1>

    @if (leaderboardService.loading()) {
      <app-loading-spinner />
    } @else {
      <div class="row g-4">
        <div class="col-lg-6">
          <div class="card shadow-sm h-100">
            <div class="card-header">
              <h5 class="mb-0">Most Taken Quizzes</h5>
            </div>
            <div class="table-responsive">
              <table class="table table-hover mb-0">
                <thead>
                  <tr>
                    <th>Rank</th>
                    <th>Quiz</th>
                    <th class="text-end">Attempts</th>
                  </tr>
                </thead>
                <tbody>
                  @for (quiz of leaderboardService.topQuizzes(); track quiz.rank) {
                    <tr>
                      <td><span class="badge bg-secondary">{{ quiz.rank }}</span></td>
                      <td>{{ quiz.quizTitle }}</td>
                      <td class="text-end">{{ quiz.attemptCount }}</td>
                    </tr>
                  }
                  @if (leaderboardService.topQuizzes().length === 0) {
                    <tr><td colspan="3" class="text-center text-muted py-3">No data yet</td></tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="col-lg-6">
          <div class="card shadow-sm h-100">
            <div class="card-header">
              <h5 class="mb-0">Top Scorers</h5>
            </div>
            <div class="table-responsive">
              <table class="table table-hover mb-0">
                <thead>
                  <tr>
                    <th>Rank</th>
                    <th>User</th>
                    <th>Best On</th>
                    <th class="text-end">Score</th>
                  </tr>
                </thead>
                <tbody>
                  @for (user of leaderboardService.topUsers(); track user.rank) {
                    <tr>
                      <td><span class="badge bg-secondary">{{ user.rank }}</span></td>
                      <td>{{ user.userName }}</td>
                      <td class="text-muted small">{{ user.quizTitle }}</td>
                      <td class="text-end">{{ user.bestScorePercent }}%</td>
                    </tr>
                  }
                  @if (leaderboardService.topUsers().length === 0) {
                    <tr><td colspan="4" class="text-center text-muted py-3">No data yet</td></tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <div class="text-center mt-4">
        <a routerLink="/quizzes" class="btn btn-primary">Take a Quiz</a>
      </div>
    }
  `
})
export class LeaderboardComponent implements OnInit {
  protected readonly leaderboardService = inject(LeaderboardService);

  ngOnInit(): void {
    this.leaderboardService.loadLeaderboard();
  }
}
