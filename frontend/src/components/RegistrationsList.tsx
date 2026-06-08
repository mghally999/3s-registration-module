import { FormEvent, useCallback, useEffect, useState } from 'react';
import { api } from '../api/client';
import { RegistrationSummary } from '../api/types';

const PAGE_SIZE = 10;

// A read-only view of everything saved in the database, backed by the paginated
// GET /api/registrations endpoint, with search by email/mobile.
export function RegistrationsList() {
  const [items, setItems] = useState<RegistrationSummary[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async (p: number, term: string) => {
    setLoading(true);
    setError(null);
    try {
      const result = await api.searchRegistrations(p, PAGE_SIZE, term);
      setItems(result.items);
      setPage(result.page);
      setTotalPages(Math.max(1, result.totalPages));
      setTotalCount(result.totalCount);
    } catch {
      setError('Could not load registrations. Is the API running?');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load(1, '');
  }, [load]);

  const onSearch = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    load(1, search.trim());
  };

  return (
    <section className="list-card" aria-label="Submitted registrations">
      <h1>Submissions</h1>
      <p className="form-lead">
        Every registration saved in the database — {totalCount} total.
      </p>

      <form className="list-search" onSubmit={onSearch} role="search">
        <input
          type="search"
          aria-label="Search by email or mobile number"
          placeholder="Search by email or mobile…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button type="submit" className="btn btn-add list-btn">
          Search
        </button>
      </form>

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
            {loading ? (
              <tr>
                <td colSpan={5} className="table-empty">
                  Loading…
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={5} className="table-empty">
                  No registrations yet. Submit one from the form tab.
                </td>
              </tr>
            ) : (
              items.map((r) => (
                <tr key={r.id}>
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
          className="btn btn-add list-btn"
          disabled={page <= 1 || loading}
          onClick={() => load(page - 1, search.trim())}
        >
          Previous
        </button>
        <span>
          Page {page} of {totalPages}
        </span>
        <button
          type="button"
          className="btn btn-add list-btn"
          disabled={page >= totalPages || loading}
          onClick={() => load(page + 1, search.trim())}
        >
          Next
        </button>
      </div>
    </section>
  );
}
