import { TestBed } from "@angular/core/testing";
import { provideHttpClient } from "@angular/common/http";
import {
  provideHttpClientTesting,
  HttpTestingController,
} from "@angular/common/http/testing";
import { QuizService } from "./quiz.service";
import {
  QuizListViewModel,
  TakeQuizViewModel,
  QuizResultViewModel,
  QuestionAnswerSelection,
} from "../../core/models/quiz.models";

const mockQuizzes: QuizListViewModel[] = [
  {
    id: 1,
    title: "Test Quiz",
    description: "A description",
    questionCount: 3,
    attemptCount: 5,
    createdAt: "2025-01-01T00:00:00Z",
  },
];

const mockTakeQuiz: TakeQuizViewModel = {
  attemptId: 10,
  quizId: 1,
  quizTitle: "Test Quiz",
  totalQuestions: 2,
  questions: [
    {
      questionId: 1,
      text: "Q1?",
      displayOrder: 1,
      answers: [
        { answerId: 1, text: "A" },
        { answerId: 2, text: "B" },
      ],
    },
    {
      questionId: 2,
      text: "Q2?",
      displayOrder: 2,
      answers: [
        { answerId: 3, text: "C" },
        { answerId: 4, text: "D" },
      ],
    },
  ],
};

const mockResult: QuizResultViewModel = {
  attemptId: 10,
  quizTitle: "Test Quiz",
  score: 2,
  totalQuestions: 2,
  percentage: 100,
  grade: "Excellent",
  answers: [],
};

describe("QuizService", () => {
  let service: QuizService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [QuizService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(QuizService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  describe("loadQuizzes()", () => {
    it("should load quizzes and update signal", () => {
      service.loadQuizzes();

      const req = httpMock.expectOne(
        (r) => r.url.includes("/api/quizzes") && r.method === "GET",
      );
      req.flush(mockQuizzes);

      expect(service.quizzes()).toEqual(mockQuizzes);
      expect(service.loading()).toBeFalse();
    });

    it("should set error and stop loading on failure", () => {
      service.loadQuizzes();

      httpMock
        .expectOne((r) => r.url.includes("/api/quizzes") && r.method === "GET")
        .flush("error", { status: 500, statusText: "Server Error" });

      expect(service.quizzes()).toEqual([]);
      expect(service.loading()).toBeFalse();
      expect(service.error()).toBeTruthy();
    });

    it("should set loading to true during request", () => {
      service.loadQuizzes();
      expect(service.loading()).toBeTrue();
      httpMock
        .expectOne((r) => r.url.includes("/api/quizzes") && r.method === "GET")
        .flush([]);
    });
  });

  describe("startQuiz()", () => {
    it("should POST to start endpoint and update activeQuiz signal", () => {
      let result: TakeQuizViewModel | undefined;
      service.startQuiz(1).subscribe((r) => {
        result = r;
      });

      const req = httpMock.expectOne((r) =>
        r.url.includes("/api/quizzes/1/start"),
      );
      expect(req.request.method).toBe("POST");
      req.flush(mockTakeQuiz);

      expect(result).toEqual(mockTakeQuiz);
      expect(service.activeQuiz()).toEqual(mockTakeQuiz);
    });
  });

  describe("submitQuiz()", () => {
    it("should POST selections and update result signal, clear activeQuiz", () => {
      // Seed active quiz
      service.startQuiz(1).subscribe();
      httpMock
        .expectOne((r) => r.url.includes("/api/quizzes/1/start"))
        .flush(mockTakeQuiz);

      const selections: QuestionAnswerSelection[] = [
        { questionId: 1, selectedAnswerId: 1 },
        { questionId: 2, selectedAnswerId: 3 },
      ];

      let submitted: QuizResultViewModel | undefined;
      service.submitQuiz(10, selections).subscribe((r) => {
        submitted = r;
      });

      const req = httpMock.expectOne((r) =>
        r.url.includes("/api/quizzes/attempts/10/submit"),
      );
      expect(req.request.method).toBe("POST");
      req.flush(mockResult);

      expect(submitted).toEqual(mockResult);
      expect(service.quizResult()).toEqual(mockResult);
      expect(service.activeQuiz()).toBeNull();
    });
  });

  describe("getResult()", () => {
    it("should GET result and update signal", () => {
      let fetched: QuizResultViewModel | undefined;
      service.getResult(10).subscribe((r) => {
        fetched = r;
      });

      const req = httpMock.expectOne((r) =>
        r.url.includes("/api/quizzes/attempts/10/result"),
      );
      expect(req.request.method).toBe("GET");
      req.flush(mockResult);

      expect(fetched).toEqual(mockResult);
      expect(service.quizResult()).toEqual(mockResult);
    });
  });
});
