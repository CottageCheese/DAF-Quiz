import { environment } from '../../environments/environment';

export function api(path: string): string {
  return `${environment.apiBase}${path}`;
}
