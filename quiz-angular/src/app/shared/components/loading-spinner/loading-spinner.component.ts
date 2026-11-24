import { Component, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [`
    .ls-wrap { display: flex; justify-content: center; padding: var(--s-6) 0; }
    .ls-ring {
      width: 36px; height: 36px;
      border: 3px solid var(--surface-hi);
      border-top-color: var(--violet);
      border-radius: 50%;
      animation: ls-spin 0.8s linear infinite;
    }
    @keyframes ls-spin { to { transform: rotate(360deg); } }
    @media (prefers-reduced-motion: reduce) { .ls-ring { animation-duration: 3s; } }
  `],
  template: `
    <div class="ls-wrap">
      <div class="ls-ring" role="status" aria-label="Loading"></div>
    </div>
  `
})
export class LoadingSpinnerComponent {}
