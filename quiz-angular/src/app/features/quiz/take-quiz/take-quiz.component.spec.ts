import { ComponentFixture, TestBed } from '@angular/core/testing';
import { convertToParamMap, ActivatedRoute, Router } from '@angular/router';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { TakeQuizComponent } from './take-quiz.component';
import { QuizService } from '../quiz.service';
import { TakeQuizViewModel, QuizResultViewModel } from '../../../core/models/quiz.models';

const mockQuiz: TakeQuizViewModel = {
  attemptId: 1,
  quizId: 42,
  quizTitle: 'Test Quiz',
  totalQuestions: 2,
  questions: [
    {
      questionId: 1, text: 'Q1?', displayOrder: 1,
      answers: [{ answerId: 1, text: 'A' }, { answerId: 2, text: 'B' }]
    },
    {
      questionId: 2, text: 'Q2?', displayOrder: 2,
      answers: [{ answerId: 3, text: 'C' }, { answerId: 4, text: 'D' }]
    }
  ]
};

const mockResult: QuizResultViewModel = {
  attemptId: 1, quizTitle: 'Test Quiz', score: 2, totalQuestions: 2,
  percentage: 100, grade: 'Excellent', answers: []
};

describe('TakeQuizComponent', () => {
  let component: TakeQuizComponent;
  let fixture: ComponentFixture<TakeQuizComponent>;
  let mockQuizService: jasmine.SpyObj<QuizService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockQuizService = jasmine.createSpyObj('QuizService', ['startQuiz', 'submitQuiz'], {
      activeQuiz: signal(mockQuiz),
      loading: signal(false),
      quizResult: signal(null)
    });
    mockQuizService.startQuiz.and.returnValue(of(mockQuiz));
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [TakeQuizComponent],
      providers: [
        { provide: QuizService, useValue: mockQuizService },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ quizId: '42' }) } }
        },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TakeQuizComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should call startQuiz on init', () => {
    expect(mockQuizService.startQuiz).toHaveBeenCalledWith(42);
  });

  it('should start at question index 0', () => {
    expect(component['currentIndex']()).toBe(0);
    expect(component['currentQuestion']()?.questionId).toBe(1);
  });

  it('should not allow next before selecting an answer', () => {
    expect(component['canGoNext']()).toBeFalse();
  });

  it('should allow next after selecting an answer', () => {
    component['selectAnswer'](1, 1);
    expect(component['canGoNext']()).toBeTrue();
  });

  it('should advance to next question', () => {
    component['selectAnswer'](1, 1);
    component['next']();
    expect(component['currentIndex']()).toBe(1);
    expect(component['currentQuestion']()?.questionId).toBe(2);
  });

  it('should not go below index 0 on prev', () => {
    component['prev']();
    expect(component['currentIndex']()).toBe(0);
  });

  it('should detect last question correctly', () => {
    expect(component['isLastQuestion']()).toBeFalse();
    component['selectAnswer'](1, 1);
    component['next']();
    expect(component['isLastQuestion']()).toBeTrue();
  });

  it('should not allow submit until all questions answered', () => {
    expect(component['canSubmit']()).toBeFalse();
    component['selectAnswer'](1, 1);
    expect(component['canSubmit']()).toBeFalse();
    component['selectAnswer'](2, 3);
    expect(component['canSubmit']()).toBeTrue();
  });

  it('should submit and navigate to result page', () => {
    mockQuizService.submitQuiz.and.returnValue(of(mockResult));
    component['selectAnswer'](1, 1);
    component['selectAnswer'](2, 3);
    component['submit'](1, 2);
    expect(mockQuizService.submitQuiz).toHaveBeenCalledWith(1, jasmine.any(Array));
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/quizzes/attempts', 1, 'result']);
  });

  it('should navigate away on startQuiz error', async () => {
    mockQuizService.startQuiz.and.returnValue(throwError(() => new Error('Not found')));
    component.ngOnInit();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/quizzes']);
  });
});
