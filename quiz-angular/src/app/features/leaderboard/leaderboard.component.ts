import { Component, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LeaderboardService } from './leaderboard.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-leaderboard',
  standalone: true,
  imports: [LoadingSpinnerComponent, RouterLink],
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .lb-hero {
      text-align: center;
      padding: var(--s-6) 0 var(--s-5);
    }
    .lb-hero .eyebrow {
      display: inline-block;
      color: var(--violet-hi);
      font-family: var(--font-display);
      font-weight: 600;
      font-size: 11px;
      letter-spacing: 0.15em;
      text-transform: uppercase;
      margin-bottom: var(--s-2);
    }
    .lb-hero h1 {
      margin: 0;
      font-size: 36px;
      font-weight: 700;
      background: linear-gradient(135deg, var(--violet), var(--cyan));
      -webkit-background-clip: text;
      background-clip: text;
      -webkit-text-fill-color: transparent;
      color: transparent;
    }
    .lb-hero p {
      margin: var(--s-2) auto 0;
      color: var(--text-dim);
      max-width: 44ch;
    }
    .lb-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
      gap: var(--s-5);
    }
    .lb-card {
      position: relative;
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--r-lg);
      padding: var(--s-5);
      box-shadow: var(--shadow-card);
    }
    .lb-card::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0; height: 2px;
      background: linear-gradient(90deg, var(--violet), var(--cyan));
      border-radius: var(--r-lg) var(--r-lg) 0 0;
    }
    .lb-card h2 {
      margin: 0 0 var(--s-4);
      font-size: 18px;
      font-weight: 600;
    }
    .lb-row {
      display: grid;
      grid-template-columns: 40px 1fr auto;
      gap: var(--s-3);
      align-items: center;
      padding: var(--s-3) 0;
      border-bottom: 1px solid var(--border);
    }
    .lb-row:last-child { border-bottom: 0; }
    .rank-chip {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      font-weight: 700;
      font-family: var(--font-display);
      font-size: 13px;
      background: var(--surface-hi);
      color: var(--text-dim);
      border: 1px solid var(--border);
    }
    .rank-chip.r1 { background: linear-gradient(135deg, #FFD54A, #FBBF24); color: #0B1020; border-color: transparent; }
    .rank-chip.r2 { background: linear-gradient(135deg, #D9DDE8, #9AA4C0); color: #0B1020; border-color: transparent; }
    .rank-chip.r3 { background: linear-gradient(135deg, #D97706, #A16207); color: #FFF; border-color: transparent; }
    .lb-name { font-weight: 600; color: var(--text); }
    .lb-sub { color: var(--text-dim); font-size: 12px; }
    .lb-metric {
      padding: 4px 12px;
      border-radius: var(--r-pill);
      font-weight: 700;
      font-family: var(--font-mono);
      font-size: 13px;
      background: var(--surface-hi);
      color: var(--text-dim);
      border: 1px solid var(--border);
    }
    .lb-metric.is-success { color: var(--success); border-color: color-mix(in oklab, var(--success) 40%, var(--border)); }
    .lb-metric.is-warn    { color: var(--warn);    border-color: color-mix(in oklab, var(--warn) 40%, var(--border)); }
    .lb-metric.is-danger  { color: var(--danger);  border-color: color-mix(in oklab, var(--danger) 40%, var(--border)); }
    .lb-empty {
      color: var(--text-mute);
      text-align: center;
      padding: var(--s-5) 0;
      font-size: 14px;
    }
    .lb-cta { text-align: center; margin-top: var(--s-6); }
  `],
  template: `
    <div class="page">
      <section class="lb-hero">
        <span class="eyebrow">Leaderboard</span>
        <h1>Top players &amp; most-played quizzes</h1>
        <p>Who's rising to the top of TriviaRush? Here's today's board.</p>
      </section>

      @if (leaderboardService.loading()) {
        <app-loading-spinner />
      } @else {
        <div class="lb-grid">
          <section class="lb-card" aria-labelledby="lb-quizzes">
            <h2 id="lb-quizzes">Most played</h2>
            @for (quiz of leaderboardService.topQuizzes(); track quiz.rank) {
              <div class="lb-row">
                <span class="rank-chip" [class.r1]="quiz.rank === 1" [class.r2]="quiz.rank === 2" [class.r3]="quiz.rank === 3">{{ quiz.rank }}</span>
                <div>
                  <div class="lb-name">{{ quiz.quizTitle }}</div>
                  <div class="lb-sub">{{ quiz.attemptCount }} attempts</div>
                </div>
                <span class="lb-metric">{{ quiz.attemptCount }}</span>
              </div>
            }
            @if (leaderboardService.topQuizzes().length === 0) {
              <p class="lb-empty">No data yet</p>
            }
          </section>

          <section class="lb-card" aria-labelledby="lb-users">
            <h2 id="lb-users">Top scorers</h2>
            @for (user of leaderboardService.topUsers(); track user.rank) {
              <div class="lb-row">
                <span class="rank-chip" [class.r1]="user.rank === 1" [class.r2]="user.rank === 2" [class.r3]="user.rank === 3">{{ user.rank }}</span>
                <div>
                  <div class="lb-name">{{ user.userName }}</div>
                  <div class="lb-sub">Best on {{ user.quizTitle }}</div>
                </div>
                <span class="lb-metric" [class.is-success]="user.bestScorePercent >= 70" [class.is-warn]="user.bestScorePercent >= 50 && user.bestScorePercent < 70" [class.is-danger]="user.bestScorePercent < 50">{{ user.bestScorePercent }}%</span>
              </div>
            }
            @if (leaderboardService.topUsers().length === 0) {
              <p class="lb-empty">No data yet</p>
            }
          </section>
        </div>

        <div class="lb-cta">
          <a routerLink="/quizzes" class="btn-neon">Take a quiz</a>
        </div>
      }
    </div>
  `
})
export class LeaderboardComponent implements OnInit {
  protected readonly leaderboardService = inject(LeaderboardService);

  ngOnInit(): void {
    this.leaderboardService.loadLeaderboard();
  }
}
