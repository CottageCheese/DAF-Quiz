import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { HistoryService } from './history.service';
import { UserAttemptHistoryViewModel } from '../../core/models/quiz.models';

const mockHistory: UserAttemptHistoryViewModel[] = [
  {
    attemptId: 5,
    quizTitle: 'Angular Basics',
    score: 8,
    totalQuestions: 10,
    percentage: 80,
    grade: 'Good',
    completedAt: '2025-06-01T14:30:00Z'
  }
];

describe('HistoryService', () => {
  let service: HistoryService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        HistoryService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(HistoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should load history and update signal', () => {
    service.loadHistory();

    const req = httpMock.expectOne(r => r.url.includes('/api/quizzes/my-history'));
    expect(req.request.method).toBe('GET');
    req.flush(mockHistory);

    expect(service.history()).toEqual(mockHistory);
    expect(service.loading()).toBeFalse();
  });

  it('should set loading to true during request', () => {
    service.loadHistory();
    expect(service.loading()).toBeTrue();
    httpMock.expectOne(r => r.url.includes('/api/quizzes/my-history')).flush([]);
  });

  it('should set empty array and stop loading on error', () => {
    service.loadHistory();
    httpMock.expectOne(r => r.url.includes('/api/quizzes/my-history'))
      .flush('error', { status: 500, statusText: 'Server Error' });

    expect(service.history()).toEqual([]);
    expect(service.loading()).toBeFalse();
  });
});
