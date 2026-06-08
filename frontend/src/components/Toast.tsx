import { useEffect } from 'react';

export interface ToastState {
  type: 'success' | 'error';
  text: string;
}

interface ToastProps {
  toast: ToastState | null;
  onDismiss: () => void;
}

// A lightweight, self-dismissing corner toast. It is purely visual feedback:
// the accessible source of truth remains the in-form role="status"/"alert"
// banners, so the toast is aria-hidden to avoid duplicate screen-reader
// announcements, and its dismiss control is kept out of the tab order.
export function Toast({ toast, onDismiss }: ToastProps) {
  useEffect(() => {
    if (!toast) {
      return;
    }
    const timer = setTimeout(onDismiss, 5000);
    return () => clearTimeout(timer);
    // re-arm only when a new toast appears; onDismiss is stable enough here.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [toast]);

  if (!toast) {
    return null;
  }

  return (
    <div className="toast-viewport" aria-hidden="true">
      <div className={`toast toast-${toast.type}`} role="presentation">
        <span className="toast-icon" aria-hidden="true" />
        <span className="toast-text">{toast.text}</span>
        <button type="button" className="toast-close" onClick={onDismiss} tabIndex={-1}>
          ×
        </button>
      </div>
    </div>
  );
}
