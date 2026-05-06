import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GradeBadgeComponent } from './grade-badge.component';
import { By } from '@angular/platform-browser';

describe('GradeBadgeComponent', () => {
  let fixture: ComponentFixture<GradeBadgeComponent>;

  const create = (grade: string) => {
    fixture = TestBed.createComponent(GradeBadgeComponent);
    fixture.componentRef.setInput('grade', grade);
    fixture.detectChanges();
    return fixture.debugElement.query(By.css('.badge'));
  };

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [GradeBadgeComponent] });
  });

  it('should show Excellent with bg-success', () => {
    const badge = create('Excellent');
    expect(badge.nativeElement.textContent.trim()).toBe('Excellent');
    expect(badge.nativeElement.classList).toContain('bg-success');
  });

  it('should show Good with bg-primary', () => {
    const badge = create('Good');
    expect(badge.nativeElement.classList).toContain('bg-primary');
  });

  it('should show Pass with bg-warning', () => {
    const badge = create('Pass');
    expect(badge.nativeElement.classList).toContain('bg-warning');
  });

  it('should show Needs Improvement with bg-danger', () => {
    const badge = create('Needs Improvement');
    expect(badge.nativeElement.classList).toContain('bg-danger');
  });
});
