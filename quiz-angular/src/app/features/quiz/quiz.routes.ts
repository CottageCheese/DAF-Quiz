import { Routes } from '@angular/router';
import { QuizListComponent } from './quiz-list/quiz-list.component';
import { TakeQuizComponent } from './take-quiz/take-quiz.component';
import { QuizResultComponent } from './quiz-result/quiz-result.component';

export const QUIZ_ROUTES: Routes = [
  { path: '', component: QuizListComponent },
  { path: ':quizId/take', component: TakeQuizComponent },
  { path: 'attempts/:attemptId/result', component: QuizResultComponent }
];
