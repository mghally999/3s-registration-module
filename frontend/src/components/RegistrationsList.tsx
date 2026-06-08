import { useCallback, useEffect, useRef, useState } from 'react';
import { api } from '../api/client';
import { PagedResult, RegistrationSummary } from '../api/types';
import { RegistrationDetailModal } from './RegistrationDetailModal';

const PAGE_SIZE = 10;
const DEBOUNCE_MS = 250;

// A read-only view of everything saved in the database, backed by the paginated
// GET /api/registrations endpoint. The filter is live (no button): it debounces
// keystrokes and memoizes results so the UI stays snappy.
export function RegistrationsList() {
  const [items, setItems] = useState<RegistrationSummary[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Memoization cache keyed by `${page}|${term}`. Revisiting a query you've
  // already run (typing then backspacing, or paging back and forth) is an O(1)
  // hash-map hit with zero network round-trips.
  const cache = useRef<Map<string, PagedResult<RegistrationSummary>>>(new Map());

  const apply = (r: PagedResult<RegistrationSummary>) => {
    setItems(r.items);
    setPage(r.page);
    setTotalPages(Math.max(1, r.totalPages));
    setTotalCount(r.totalCount);
  };

  const load = useCallback(async (p: number, term: string) => {
    const key = `${p}|${term.toLowerCase()}`;
    const hit = cache.current.get(key);
    if (hit) {
      apply(hit);
      setError(null);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await api.searchRegistrations(p, PAGE_SIZE, term);
      cache.current.set(key, result);
      apply(result);
    } catch {
      setError('Could not load registrations. Is the API running?');
    } finally {
      setLoading(false);
    }
  }, []);

  // Live, debounced filtering — a single timer that resets on every keystroke,
  // so we query at most once per quiet period instead of once per character.
  // Also runs on mount (empty term => full list).
  useEffect(() => {
    const id = setTimeout(() => load(1, search.trim()), DEBOUNCE_MS);
    return () => clearTimeout(id);
  }, [search, load]);

  return (
    <section className="list-card" aria-label="Submitted registrations">
      <h1>Submissions</h1>
      <p className="form-lead">
        Every registration saved in the database — {totalCount} total.
      </p>

      <div className="list-search">
        <svg
          className="list-search-icon"
          viewBox="0 0 24 24"
          width="18"
          height="18"
          aria-hidden="true"
        >
          <circle cx="11" cy="11" r="7" fill="none" stroke="currentColor" strokeWidth="2" />
          <line x1="16.5" y1="16.5" x2="21" y2="21" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
        </svg>
        <input
          type="search"
          aria-label="Filter registrations by email or mobile number"
          placeholder="Type to filter by email or mobile…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {loading ? <span className="list-search-spinner" aria-hidden="true" /> : null}
      </div>

      {error ? (
        <p className="banner banner-error" role="alert">
          {error}
        </p>
      ) : null}

      <div className="table-wrap">
        <table className="reg-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Mobile</th>
              <th>Addresses</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            {loading && items.length === 0 ? (
              <tr>
                <td colSpan={5} className="table-empty">
                  Loading…
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={5} className="table-empty">
                  {search ? 'No matches for that filter.' : 'No registrations yet.'}
                </td>
              </tr>
            ) : (
              items.map((r) => (
                <tr
                  key={r.id}
                  className="row-clickable"
                  tabIndex={0}
                  role="button"
                  aria-label={`View ${r.fullName}`}
                  onClick={() => setSelectedId(r.id)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      setSelectedId(r.id);
                    }
                  }}
                >
                  <td>{r.fullName}</td>
                  <td>{r.email}</td>
                  <td>{r.mobileNumber}</td>
                  <td>{r.addressCount}</td>
                  <td>{new Date(r.createdAtUtc).toLocaleString()}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="list-pager">
        <button
          type="button"
          className="btn-page"
          disabled={page <= 1 || loading}
          onClick={() => load(page - 1, search.trim())}
        >
          ‹ Previous
        </button>
        <span className="list-pager-info">
          Page {page} of {totalPages}
        </span>
        <button
          type="button"
          className="btn-page"
          disabled={page >= totalPages || loading}
          onClick={() => load(page + 1, search.trim())}
        >
          Next ›
        </button>
      </div>

      {selectedId ? (
        <RegistrationDetailModal id={selectedId} onClose={() => setSelectedId(null)} />
      ) : null}
    </section>
  );
}
