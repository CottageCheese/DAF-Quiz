import { Component, OnInit, inject, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { HistoryService } from './history.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

type Filter = 'all' | 'passed' | 'failed';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [LoadingSpinnerComponent, RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .hist-hero { text-align: center; padding: var(--s-6) 0 var(--s-5); }
    .hist-hero .eyebrow {
      display: inline-block; color: var(--violet-hi);
      font-family: var(--font-display); font-weight: 600;
      font-size: 11px; letter-spacing: 0.15em; text-transform: uppercase;
      margin-bottom: var(--s-2);
    }
    .hist-hero h1 {
      margin: 0; font-size: 36px; font-weight: 700;
      background: linear-gradient(135deg, var(--violet), var(--cyan));
      -webkit-background-clip: text; background-clip: text;
      -webkit-text-fill-color: transparent; color: transparent;
    }
    .hist-hero p { margin: var(--s-2) auto 0; color: var(--text-dim); max-width: 44ch; }
    .filter-row { display: flex; gap: var(--s-2); justify-content: center; flex-wrap: wrap; margin-bottom: var(--s-5); }
    .filter-chip {
      padding: 8px 16px;
      border-radius: var(--r-pill);
      background: var(--surface);
      border: 1px solid var(--border);
      color: var(--text-dim);
      font-size: 13px;
      font-weight: 500;
      cursor: pointer;
      transition: background var(--t-mid), border-color var(--t-mid), color var(--t-mid);
    }
    .filter-chip:hover { color: var(--text); }
    .filter-chip.active {
      background: color-mix(in oklab, var(--violet) 18%, var(--surface));
      border-color: var(--violet);
      color: var(--violet-hi);
    }
    .hist-list {
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--r-lg);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }
    .hist-row {
      display: grid;
      grid-template-columns: 1fr auto auto auto;
      gap: var(--s-4);
      align-items: center;
      padding: var(--s-4) var(--s-5);
      border-bottom: 1px solid var(--border);
      color: var(--text);
      text-decoration: none;
      transition: background var(--t-mid);
    }
    .hist-row:last-child { border-bottom: 0; }
    .hist-row:hover,
    .hist-row:focus-visible {
      background: var(--surface-hi);
      outline: none;
    }
    .hist-title { font-weight: 600; font-size: 15px; }
    .hist-date { color: var(--text-dim); font-size: 12px; font-family: var(--font-mono); margin-top: 2px; }
    .hist-score {
      font-family: var(--font-mono);
      font-weight: 700;
      font-variant-numeric: tabular-nums;
      padding: 4px 10px;
      border-radius: var(--r-pill);
      font-size: 13px;
      background: var(--surface-hi);
      border: 1px solid var(--border);
      color: var(--text-dim);
    }
    .hist-pct {
      font-family: var(--font-mono);
      font-weight: 700;
      font-size: 14px;
      font-variant-numeric: tabular-nums;
      min-width: 56px;
      text-align: right;
    }
    .hist-pct.ok   { color: var(--success); }
    .hist-pct.warn { color: var(--warn); }
    .hist-pct.bad  { color: var(--danger); }
    .hist-grade {
      min-width: 40px;
      text-align: center;
      padding: 4px 10px;
      border-radius: var(--r-pill);
      font-family: var(--font-display);
      font-weight: 700;
      font-size: 13px;
    }
    .hist-grade.ok   { background: color-mix(in oklab, var(--success) 20%, var(--surface)); color: var(--success); }
    .hist-grade.warn { background: color-mix(in oklab, var(--warn) 22%, var(--surface));    color: var(--warn); }
    .hist-grade.bad  { background: color-mix(in oklab, var(--danger) 22%, var(--surface));  color: var(--danger); }
    .empty {
      border: 1px dashed var(--border);
      border-radius: var(--r-lg);
      padding: var(--s-7);
      text-align: center;
      color: var(--text-dim);
    }
    @media (max-width: 560px) {
      .hist-row { grid-template-columns: 1fr auto; gap: var(--s-3); }
      .hist-score, .hist-grade { display: none; }
    }
  `],
  template: `
    <div class="page">
      <section class="hist-hero">
        <span class="eyebrow">Your history</span>
        <h1>Your runs on the board</h1>
        <p>Every quiz you've taken, scored and dated.</p>
      </section>

      @if (historyService.loading()) {
        <app-loading-spinner />
      } @else if (historyService.history().length === 0) {
        <div class="empty" role="status">
          <h2>No runs yet</h2>
          <p>Take your first quiz to start tracking your scores.</p>
          <a routerLink="/quizzes" class="btn-neon">Browse quizzes</a>
        </div>
      } @else {
        <div class="filter-row" role="tablist" aria-label="Filter quizzes">
          <button type="button" role="tab" class="filter-chip" [class.active]="filter() === 'all'" [attr.aria-selected]="filter() === 'all'" (click)="filter.set('all')">
            All ({{ historyService.history().length }})
          </button>
          <button type="button" role="tab" class="filter-chip" [class.active]="filter() === 'passed'" [attr.aria-selected]="filter() === 'passed'" (click)="filter.set('passed')">
            Passed ({{ passedCount() }})
          </button>
          <button type="button" role="tab" class="filter-chip" [class.active]="filter() === 'failed'" [attr.aria-selected]="filter() === 'failed'" (click)="filter.set('failed')">
            Failed ({{ failedCount() }})
          </button>
        </div>

        <div class="hist-list">
          @for (a of filtered(); track a.attemptId) {
            <a class="hist-row" [routerLink]="['/quizzes/attempts', a.attemptId, 'result']">
              <div>
                <div class="hist-title">{{ a.quizTitle }}</div>
                <div class="hist-date">{{ a.completedAt | date: 'd MMM yyyy, HH:mm' }}</div>
              </div>
              <span class="hist-score">{{ a.score }} / {{ a.totalQuestions }}</span>
              <span class="hist-pct" [class.ok]="a.percentage >= 70" [class.warn]="a.percentage >= 50 && a.percentage < 70" [class.bad]="a.percentage < 50">{{ a.percentage }}%</span>
              <span class="hist-grade" [class.ok]="a.percentage >= 70" [class.warn]="a.percentage >= 50 && a.percentage < 70" [class.bad]="a.percentage < 50">{{ a.grade }}</span>
            </a>
          }
        </div>
      }
    </div>
  `
})
export class HistoryComponent implements OnInit {
  protected readonly historyService = inject(HistoryService);
  protected readonly filter = signal<Filter>('all');

  protected readonly passedCount = computed(() =>
    this.historyService.history().filter(a => a.percentage >= 50).length);
  protected readonly failedCount = computed(() =>
    this.historyService.history().filter(a => a.percentage < 50).length);

  protected readonly filtered = computed(() => {
    const list = this.historyService.history();
    switch (this.filter()) {
      case 'passed': return list.filter(a => a.percentage >= 50);
      case 'failed': return list.filter(a => a.percentage < 50);
      default: return list;
    }
  });

  ngOnInit(): void {
    this.historyService.loadHistory();
  }
}
