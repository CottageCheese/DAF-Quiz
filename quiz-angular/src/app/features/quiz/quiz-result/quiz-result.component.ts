import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy } from "@angular/core";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { QuizService } from "../quiz.service";
import { QuizResultViewModel } from "../../../core/models/quiz.models";
import { LoadingSpinnerComponent } from "../../../shared/components/loading-spinner/loading-spinner.component";

@Component({
  selector: "app-quiz-result",
  standalone: true,
  imports: [LoadingSpinnerComponent, RouterLink],
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .result-wrap { max-width: 860px; margin: 0 auto; padding: var(--s-6) var(--s-4) var(--s-8); }
    .result-hero {
      position: relative;
      background: linear-gradient(145deg, var(--surface), var(--bg-elev));
      border: 1px solid var(--border);
      border-radius: var(--r-xl);
      padding: var(--s-7) var(--s-6);
      box-shadow: var(--shadow-card);
      text-align: center;
      overflow: hidden;
      margin-bottom: var(--s-5);
    }
    .result-hero::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0; height: 3px;
      background: linear-gradient(90deg, var(--violet), var(--cyan));
    }
    .result-hero h1 {
      margin: 0 0 var(--s-5);
      font-size: 28px;
      background: linear-gradient(135deg, var(--violet), var(--cyan));
      -webkit-background-clip: text; background-clip: text;
      -webkit-text-fill-color: transparent; color: transparent;
    }
    .score-ring { display: inline-grid; place-items: center; position: relative; width: 180px; height: 180px; margin-bottom: var(--s-4); }
    .score-ring svg { transform: rotate(-90deg); width: 100%; height: 100%; }
    .score-ring .track { stroke: var(--surface-hi); }
    .score-ring .bar { transition: stroke-dashoffset 900ms var(--ease-out); }
    .score-ring.is-success .bar { stroke: var(--success); }
    .score-ring.is-warn    .bar { stroke: var(--warn); }
    .score-ring.is-danger  .bar { stroke: var(--danger); }
    .score-inner { position: absolute; inset: 0; display: grid; place-items: center; }
    .score-pct { font-family: var(--font-display); font-weight: 700; font-size: 38px; font-variant-numeric: tabular-nums; color: var(--text); }
    .score-sub { font-size: 13px; color: var(--text-dim); font-family: var(--font-mono); }
    .grade-pill {
      display: inline-block;
      padding: 6px 16px;
      border-radius: var(--r-pill);
      font-family: var(--font-display);
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      font-size: 12px;
      margin-bottom: var(--s-4);
    }
    .grade-pill.is-success { background: color-mix(in oklab, var(--success) 20%, var(--surface)); color: var(--success); border: 1px solid color-mix(in oklab, var(--success) 40%, var(--border)); }
    .grade-pill.is-warn    { background: color-mix(in oklab, var(--warn) 22%, var(--surface));    color: var(--warn);    border: 1px solid color-mix(in oklab, var(--warn) 40%, var(--border)); }
    .grade-pill.is-danger  { background: color-mix(in oklab, var(--danger) 22%, var(--surface));  color: var(--danger);  border: 1px solid color-mix(in oklab, var(--danger) 40%, var(--border)); }
    .stat-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: var(--s-3); margin: var(--s-5) 0; }
    .stat {
      padding: var(--s-4);
      border-radius: var(--r-lg);
      background: var(--surface-hi);
      border: 1px solid var(--border);
    }
    .stat .label { font-size: 11px; letter-spacing: 0.1em; text-transform: uppercase; color: var(--text-dim); }
    .stat .value { font-family: var(--font-mono); font-weight: 700; font-size: 22px; font-variant-numeric: tabular-nums; margin-top: 4px; }
    .stat.ok .value   { color: var(--success); }
    .stat.fail .value { color: var(--danger); }
    .cta-row { display: flex; justify-content: center; gap: var(--s-3); flex-wrap: wrap; margin-top: var(--s-4); }
    .breakdown {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--r-lg);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }
    .breakdown h2 { margin: 0; padding: var(--s-5); border-bottom: 1px solid var(--border); font-size: 16px; }
    .answer-row {
      display: grid;
      grid-template-columns: 32px 1fr auto;
      gap: var(--s-3);
      align-items: start;
      padding: var(--s-4) var(--s-5);
      border-bottom: 1px solid var(--border);
    }
    .answer-row:last-child { border-bottom: 0; }
    .answer-row .num { font-family: var(--font-mono); color: var(--text-dim); font-variant-numeric: tabular-nums; padding-top: 2px; }
    .answer-row .q { color: var(--text); font-weight: 500; }
    .answer-row .ab { margin-top: 6px; font-size: 13px; color: var(--text-dim); display: flex; flex-direction: column; gap: 2px; }
    .answer-row .ab .you.is-wrong { color: var(--danger); }
    .answer-row .ab .right { color: var(--success); }
    .answer-row .badge-res {
      padding: 4px 10px;
      border-radius: var(--r-pill);
      font-size: 11px;
      font-weight: 700;
      letter-spacing: 0.06em;
      text-transform: uppercase;
      align-self: start;
    }
    .badge-res.ok   { background: color-mix(in oklab, var(--success) 20%, var(--surface)); color: var(--success); }
    .badge-res.fail { background: color-mix(in oklab, var(--danger) 22%, var(--surface));  color: var(--danger); }
    .error { padding: var(--s-4); border-radius: var(--r-md); background: color-mix(in oklab, var(--danger) 15%, var(--surface)); border: 1px solid color-mix(in oklab, var(--danger) 40%, var(--border)); color: var(--danger); max-width: 760px; margin: var(--s-6) auto; }
    @media (prefers-reduced-motion: reduce) {
      .score-ring .bar { transition: none; }
    }
  `],
  template: `
    @if (result(); as r) {
      <div class="result-wrap">
        <section class="result-hero" aria-live="polite">
          <h1>{{ r.quizTitle }}</h1>

          <div class="score-ring"
               [class.is-success]="r.percentage >= 70"
               [class.is-warn]="r.percentage >= 50 && r.percentage < 70"
               [class.is-danger]="r.percentage < 50"
               role="img"
               [attr.aria-label]="'Scored ' + r.percentage + ' percent'">
            <svg viewBox="0 0 120 120" aria-hidden="true">
              <circle class="track" cx="60" cy="60" r="52" fill="none" stroke-width="10"/>
              <circle class="bar" cx="60" cy="60" r="52" fill="none" stroke-width="10"
                      stroke-linecap="round"
                      [attr.stroke-dasharray]="circumference"
                      [attr.stroke-dashoffset]="dashOffset()"/>
            </svg>
            <div class="score-inner">
              <span class="score-pct">{{ r.percentage }}%</span>
              <span class="score-sub">{{ r.score }} / {{ r.totalQuestions }}</span>
            </div>
          </div>

          <div>
            <span class="grade-pill"
                  [class.is-success]="r.percentage >= 70"
                  [class.is-warn]="r.percentage >= 50 && r.percentage < 70"
                  [class.is-danger]="r.percentage < 50">
              Grade {{ r.grade }}
            </span>
          </div>

          <div class="stat-grid">
            <div class="stat ok">
              <div class="label">Correct</div>
              <div class="value">{{ r.score }}</div>
            </div>
            <div class="stat fail">
              <div class="label">Wrong</div>
              <div class="value">{{ r.totalQuestions - r.score }}</div>
            </div>
          </div>

          <div class="cta-row">
            <a routerLink="/quizzes" class="btn-neon">Take another</a>
            <a routerLink="/leaderboard" class="btn-ghost">Leaderboard</a>
            <a routerLink="/history" class="btn-ghost">My history</a>
          </div>
        </section>

        <section class="breakdown" aria-labelledby="bd-h">
          <h2 id="bd-h">Answer breakdown</h2>
          @for (a of r.answers; track $index) {
            <div class="answer-row">
              <span class="num">{{ $index + 1 }}</span>
              <div>
                <div class="q">{{ a.questionText }}</div>
                <div class="ab">
                  <span class="you" [class.is-wrong]="!a.isCorrect">Your answer: {{ a.selectedAnswerText }}</span>
                  @if (!a.isCorrect) {
                    <span class="right">Correct: {{ a.correctAnswerText }}</span>
                  }
                </div>
              </div>
              <span class="badge-res" [class.ok]="a.isCorrect" [class.fail]="!a.isCorrect">
                {{ a.isCorrect ? 'Correct' : 'Wrong' }}
              </span>
            </div>
          }
        </section>
      </div>
    } @else if (loading()) {
      <app-loading-spinner />
    } @else {
      <div class="error" role="alert">
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

  protected readonly circumference = 2 * Math.PI * 52;
  protected readonly dashOffset = computed(() => {
    const r = this.result();
    if (!r) return this.circumference;
    return this.circumference * (1 - r.percentage / 100);
  });

  ngOnInit(): void {
    const existing = this.quizService.quizResult();
    const attemptId = Number(this.route.snapshot.paramMap.get("attemptId"));

    if (existing && existing.attemptId === attemptId) {
      this.result.set(existing);
      this.loading.set(false);
      return;
    }

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
