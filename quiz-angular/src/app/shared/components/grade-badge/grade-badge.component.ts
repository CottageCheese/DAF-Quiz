import { Component, computed, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-grade-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .g-pill {
      display: inline-block;
      padding: 4px 10px;
      border-radius: var(--r-pill);
      font-family: var(--font-display);
      font-weight: 700;
      font-size: 12px;
      letter-spacing: 0.06em;
      text-transform: uppercase;
      border: 1px solid var(--border);
    }
    .g-pill.is-success { background: color-mix(in oklab, var(--success) 20%, var(--surface)); color: var(--success); border-color: color-mix(in oklab, var(--success) 40%, var(--border)); }
    .g-pill.is-primary { background: color-mix(in oklab, var(--violet) 20%, var(--surface));  color: var(--violet-hi); border-color: color-mix(in oklab, var(--violet) 40%, var(--border)); }
    .g-pill.is-warn    { background: color-mix(in oklab, var(--warn) 22%, var(--surface));    color: var(--warn); border-color: color-mix(in oklab, var(--warn) 40%, var(--border)); }
    .g-pill.is-danger  { background: color-mix(in oklab, var(--danger) 22%, var(--surface));  color: var(--danger); border-color: color-mix(in oklab, var(--danger) 40%, var(--border)); }
  `],
  template: `<span class="g-pill" [class.is-success]="tone() === 'success'" [class.is-primary]="tone() === 'primary'" [class.is-warn]="tone() === 'warn'" [class.is-danger]="tone() === 'danger'">{{ grade() }}</span>`
})
export class GradeBadgeComponent {
  readonly grade = input.required<string>();

  readonly tone = computed<'success' | 'primary' | 'warn' | 'danger'>(() => {
    switch (this.grade()) {
      case 'Excellent': return 'success';
      case 'Good': return 'primary';
      case 'Pass': return 'warn';
      default: return 'danger';
    }
  });
}
