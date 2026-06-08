import { useEffect, useRef, useState } from 'react';
import { api } from '../api/client';
import { RegistrationDetail } from '../api/types';

interface RegistrationDetailModalProps {
  id: string;
  onClose: () => void;
}

// A dialog that loads and shows the full detail of one registration (the data
// the user entered) when a row in the list is clicked. Closes on Escape, on a
// backdrop click, or via the close button; focus moves to the close button on
// open for keyboard users.
export function RegistrationDetailModal({ id, onClose }: RegistrationDetailModalProps) {
  const [data, setData] = useState<RegistrationDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const closeRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    let active = true;
    api
      .getRegistration(id)
      .then((d) => active && setData(d))
      .catch(() => active && setError('Could not load this registration.'));
    return () => {
      active = false;
    };
  }, [id]);

  useEffect(() => {
    closeRef.current?.focus();
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [onClose]);

  const fullName = data
    ? [data.firstName, data.middleName, data.lastName].filter(Boolean).join(' ')
    : '';

  return (
    <div className="modal-overlay" role="presentation" onClick={onClose}>
      <div
        className="modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-title"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal-head">
          <h2 id="modal-title">Registration details</h2>
          <button ref={closeRef} type="button" className="modal-close" aria-label="Close" onClick={onClose}>
            ×
          </button>
        </div>

        {error ? (
          <p className="banner banner-error" role="alert">
            {error}
          </p>
        ) : !data ? (
          <p className="modal-loading">Loading…</p>
        ) : (
          <div className="modal-body">
            <dl className="detail-grid">
              <div>
                <dt>Name</dt>
                <dd>{fullName}</dd>
              </div>
              <div>
                <dt>Birth date</dt>
                <dd>{data.birthDate}</dd>
              </div>
              <div>
                <dt>Mobile</dt>
                <dd>{data.mobileNumber}</dd>
              </div>
              <div>
                <dt>Email</dt>
                <dd>{data.email}</dd>
              </div>
              <div>
                <dt>Created</dt>
                <dd>{new Date(data.createdAtUtc).toLocaleString()}</dd>
              </div>
              <div>
                <dt>ID</dt>
                <dd className="detail-mono">{data.id}</dd>
              </div>
            </dl>

            <h3 className="detail-sub">Addresses ({data.addresses.length})</h3>
            <div className="detail-addresses">
              {data.addresses.map((a) => (
                <div key={a.id} className="detail-address">
                  <div className="detail-address-head">
                    <span>
                      {a.governorateName} · {a.cityName}
                    </span>
                    {a.isPrimary ? <span className="badge-primary">Primary</span> : null}
                  </div>
                  <div className="detail-address-line">
                    {a.street}, Bldg {a.buildingNumber}, Flat {a.flatNumber}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
