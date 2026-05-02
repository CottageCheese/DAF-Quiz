export interface QuizListViewModel {
  id: number;
  title: string;
  description: string | null;
  questionCount: number;
  attemptCount: number;
  createdAt: string;
}

export interface QuizAnswerViewModel {
  answerId: number;
  text: string;
}

export interface QuizQuestionViewModel {
  questionId: number;
  text: string;
  displayOrder: number;
  answers: QuizAnswerViewModel[];
}

export interface TakeQuizViewModel {
  attemptId: number;
  quizId: number;
  quizTitle: string;
  totalQuestions: number;
  questions: QuizQuestionViewModel[];
}

export interface QuestionAnswerSelection {
  questionId: number;
  selectedAnswerId: number;
}

export interface ResultAnswerViewModel {
  questionText: string;
  selectedAnswerText: string;
  correctAnswerText: string;
  isCorrect: boolean;
}

export interface QuizResultViewModel {
  attemptId: number;
  quizTitle: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  grade: string;
  answers: ResultAnswerViewModel[];
}

export interface UserAttemptHistoryViewModel {
  attemptId: number;
  quizTitle: string;
  score: number;
  totalQuestions: number;
  percentage: number;
  grade: string;
  completedAt: string;
}
