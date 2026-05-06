import { Component, computed, input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-grade-badge',
  standalone: true,
  imports: [NgClass],
  template: `<span class="badge" [ngClass]="badgeClass()">{{ grade() }}</span>`
})
export class GradeBadgeComponent {
  readonly grade = input.required<string>();

  readonly badgeClass = computed(() => ({
    'bg-success': this.grade() === 'Excellent',
    'bg-primary': this.grade() === 'Good',
    'bg-warning text-dark': this.grade() === 'Pass',
    'bg-danger': this.grade() === 'Needs Improvement'
  }));
}
