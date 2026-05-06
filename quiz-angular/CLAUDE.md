# quiz-angular — CLAUDE.md

Angular 19 SPA frontend for the DAF-Quiz application. Communicates exclusively with the ASP.NET Core API (`QuizProject.Api`) on port 7001. No server-side rendering, no lazy-loaded modules, no NgRx.

---

## Tech Stack

| Concern | Library / Version |
|---|---|
| Framework | Angular 19 (standalone components) |
| Language | TypeScript ~5.6 |
| Reactivity | RxJS ~7.8 + Angular Signals |
| Styling | Bootstrap 5.3 (CSS only — no `bootstrap.js`) |
| Testing | Karma + Jasmine |
| Build | Angular CLI 19 |

---

## Project Structure

```
src/app/
├── core/
│   ├── api.ts                        # api() URL helper — prepends environment.apiBase
│   ├── auth/
│   │   ├── auth.service.ts           # Login, register, logout, token refresh
│   │   └── token-storage.service.ts  # localStorage: access/refresh tokens + expiry
│   ├── guards/
│   │   ├── auth.guard.ts             # Redirects unauthenticated → /login
│   │   └── already-logged-in.guard.ts # Redirects authenticated away from /login, /register
│   ├── interceptors/
│   │   └── auth.interceptor.ts       # Injects Bearer token; proactive + reactive refresh
│   └── models/
│       ├── auth.models.ts            # AuthResponse, LoginRequest, RegisterRequest, AuthUser, JwtPayload
│       ├── quiz.models.ts            # QuizListViewModel, TakeQuizViewModel, QuizResultViewModel, etc.
│       └── leaderboard.models.ts     # TopQuizViewModel, TopUserViewModel
├── features/
│   ├── auth/
│   │   ├── login/login.component.ts
│   │   └── register/register.component.ts
│   ├── quiz/
│   │   ├── quiz-list/quiz-list.component.ts
│   │   ├── take-quiz/take-quiz.component.ts
│   │   ├── quiz-result/quiz-result.component.ts
│   │   ├── quiz.service.ts
│   │   └── quiz.routes.ts            # Sub-routes for the quiz feature
│   ├── history/
│   │   ├── history.component.ts
│   │   └── history.service.ts
│   └── leaderboard/
│       ├── leaderboard.component.ts
│       └── leaderboard.service.ts
└── shared/
    └── components/
        ├── navbar/navbar.component.ts
        ├── grade-badge/grade-badge.component.ts  # Input: grade string → coloured Bootstrap badge
        └── loading-spinner/loading-spinner.component.ts
```

All components use **inline templates** (no `.html` files).

---

## Architecture & Design Patterns

- **Standalone components** — no `NgModule`. All components declare their own `imports`.
- **Functional guards** — `CanActivateFn` functions, not classes.
- **Functional interceptor** — `HttpInterceptorFn`, registered in `app.config.ts`.
- **Service-based state** — no NgRx/store. Each service owns its state as private `BehaviorSubject`s.
- **Signal exposure** — BehaviorSubjects are converted to signals via `toSignal()` for template consumption.
- **Computed signals** — derived state lives in `computed()` (e.g. `isLoggedIn`, `canSubmit`, `progress`).
- **Component-local signals** — loading, error, and UI state use `signal()` directly in components.

---

## Data Flow

```
HTTP Response
  → service BehaviorSubject.next(value)
    → toSignal(bs$)               // read in templates as signal()
      → component template        // auto-updates via Angular change detection
```

Example — quiz list:
1. `QuizListComponent.ngOnInit()` calls `quizService.loadQuizzes()`
2. `QuizService` POSTs to API, calls `_quizzes$.next(data)`
3. Template reads `quizService.quizzes()` (signal)

---

## State Management

No centralised store. State lives in services:

```typescript
// In a service:
private _items$ = new BehaviorSubject<Item[]>([]);
readonly items = toSignal(this._items$, { initialValue: [] });

loadItems(): void {
  this.http.get<Item[]>(api('/api/items')).subscribe(data => this._items$.next(data));
}
```

Each service also exposes `loading` and `error` signals the same way.

Component-local state (transient UI state):
```typescript
loading = signal(false);
error = signal<string | null>(null);
currentIndex = signal(0);
```

Derived state:
```typescript
canSubmit = computed(() => this.selections().size === this.quiz()?.totalQuestions);
```

---

## API Integration

### URL helper (`core/api.ts`)

```typescript
export function api(path: string): string {
  return `${environment.apiBase}${path}`;
}
```

`environment.apiBase` defaults to `https://localhost:7001` in development.

### Endpoints

```
POST   /api/auth/login
POST   /api/auth/register
POST   /api/auth/refresh
POST   /api/auth/revoke

GET    /api/quizzes
POST   /api/quizzes/{quizId}/start
POST   /api/quizzes/attempts/{attemptId}/submit
GET    /api/quizzes/attempts/{attemptId}/result
GET    /api/quizzes/my-history

GET    /api/leaderboard/top-quizzes
GET    /api/leaderboard/top-users
```

### Auth Interceptor (`core/interceptors/auth.interceptor.ts`)

Two-phase token strategy:
1. **Proactive** — if `TokenStorageService.isAccessTokenExpired()` (with 30 s buffer), refresh before sending request
2. **Reactive** — if response returns 401, refresh and retry the original request once

Auth endpoints (`/api/auth/`) are excluded from interception to prevent loops.
Concurrent refresh requests are deduplicated via `shareReplay(1)`.

---

## Routing

Defined in `app.routes.ts` and `features/quiz/quiz.routes.ts`.

```
''                                    → redirect → /quizzes
/login                                → LoginComponent          [alreadyLoggedInGuard]
/register                             → RegisterComponent       [alreadyLoggedInGuard]
/leaderboard                          → LeaderboardComponent
/quizzes                              → (quiz sub-routes)       [authGuard]
  /quizzes                            → QuizListComponent
  /quizzes/:quizId/take               → TakeQuizComponent
  /quizzes/attempts/:attemptId/result → QuizResultComponent
/history                              → HistoryComponent        [authGuard]
/**                                   → redirect → /quizzes
```

`withComponentInputBinding()` is NOT used on quiz routes — components read route params via `ActivatedRoute.snapshot`.

---

## Testing Strategy

**Framework:** Karma + Jasmine
**Run:** `npm test`

9 spec files exist (services, guards, interceptor, one component):

| File | Covers |
|---|---|
| `core/auth/auth.service.spec.ts` | Login, register, refresh, deduplication |
| `core/auth/token-storage.service.spec.ts` | localStorage get/set/clear/expiry |
| `core/guards/auth.guard.spec.ts` | Redirect when unauthenticated |
| `core/guards/already-logged-in.guard.spec.ts` | Redirect when authenticated |
| `core/interceptors/auth.interceptor.spec.ts` | Token injection, 401 retry, refresh |
| `features/history/history.service.spec.ts` | History loading |
| `features/quiz/quiz.service.spec.ts` | Quiz CRUD operations |
| `features/quiz/take-quiz/take-quiz.component.spec.ts` | Quiz UI state |
| `shared/components/grade-badge/grade-badge.component.spec.ts` | Grade → CSS class |

**Patterns:**
- `TestBed.configureTestingModule()` for dependency injection
- `jasmine.createSpyObj()` for service mocks
- `HttpClientTestingModule` + `HttpTestingController` for HTTP
- Signal inputs set via `fixture.componentRef.setInput()`

**No E2E tests** (no Cypress/Playwright configuration).

---

## Key Files Explained

| File | Purpose |
|---|---|
| `core/api.ts` | Single source of truth for API base URL |
| `core/auth/auth.service.ts` | Central auth state; login/logout/refresh; exposes `isLoggedIn` signal and `currentUser` signal |
| `core/auth/token-storage.service.ts` | Reads/writes JWT tokens from localStorage; `isAccessTokenExpired()` includes 30 s buffer |
| `core/interceptors/auth.interceptor.ts` | Proactive + reactive token refresh; never intercepts `/api/auth/` |
| `features/quiz/quiz.service.ts` | Quiz state hub; holds active quiz, result, and quiz list as signals |
| `app.config.ts` | Bootstraps `HttpClient` with interceptor, router with routes |
| `app.routes.ts` | Top-level route definitions with guards |
| `src/environments/environment.ts` | Dev API base: `https://localhost:7001` |

---

## How to Run & Develop

```bash
# Install dependencies
npm install

# Development server (http://localhost:4200)
# Requires QuizProject.Api running on https://localhost:7001
npm start

# Run unit tests (Karma/Jasmine)
npm test

# Production build
npm run build
```

The dev server proxies nothing — the Angular app calls the API directly. CORS must be permissive on the API during development.

---

## How to Add a New Feature

Follow the pattern used by `history` and `leaderboard`:

### 1. Add model(s) to `core/models/`
```typescript
// core/models/my-feature.models.ts
export interface MyItemViewModel {
  id: number;
  title: string;
}
```

### 2. Create a service in `features/my-feature/`
```typescript
@Injectable({ providedIn: 'root' })
export class MyFeatureService {
  private http = inject(HttpClient);
  private _items$ = new BehaviorSubject<MyItemViewModel[]>([]);
  private _loading$ = new BehaviorSubject(false);

  readonly items = toSignal(this._items$, { initialValue: [] });
  readonly loading = toSignal(this._loading$, { initialValue: false });

  loadItems(): void {
    this._loading$.next(true);
    this.http.get<MyItemViewModel[]>(api('/api/my-feature'))
      .pipe(finalize(() => this._loading$.next(false)))
      .subscribe(data => this._items$.next(data));
  }
}
```

### 3. Create a component (inline template)
```typescript
@Component({
  selector: 'app-my-feature',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent],
  template: `
    @if (service.loading()) {
      <app-loading-spinner />
    } @else {
      <!-- render service.items() -->
    }
  `
})
export class MyFeatureComponent {
  service = inject(MyFeatureService);
  ngOnInit() { this.service.loadItems(); }
}
```

### 4. Add a route in `app.routes.ts`
```typescript
{ path: 'my-feature', component: MyFeatureComponent, canActivate: [authGuard] }
```

### 5. Add nav link in `NavbarComponent` if needed

---

## Known Issues / Technical Debt

- **Production API URL placeholder** — `environment.prod.ts` has `https://your-api.example.com`; must be set before any production build
- **Dual API surface on services** — each service exposes both `_items$` (BehaviorSubject) and `items` (signal); only signals should be read in components
- **Sparse test coverage** — 9 spec files for ~20 components/services; most feature components have no tests
- **No E2E tests** — no Cypress or Playwright setup
- **Route params via snapshot** — `TakeQuizComponent` and `QuizResultComponent` read `ActivatedRoute.snapshot.params` rather than using `withComponentInputBinding()`; means no live param updates
- **No pagination** — history and leaderboard endpoints return all records; could become a problem at scale
- **No retry on quiz start failure** — `QuizListComponent` sets `startingId` to `null` on error but does not offer a retry path
- **Bootstrap JS not loaded** — collapse/dropdown/modal behaviour requires `bootstrap.bundle.js`; only CSS is imported. Navbar collapse is handled manually with a custom `menuOpen` signal
