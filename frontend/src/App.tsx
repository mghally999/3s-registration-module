import { RegistrationForm } from './components/RegistrationForm';

// Two-pane shell: a formal brand/intro panel on the left and the form on the
// right. Collapses to a single stacked column under 900px (see styles.css). The
// brand headline is a styled <p>, not a heading, so the form keeps the single
// <h1> on the page for a clean document outline.
export default function App() {
  return (
    <div className="app-shell">
      <aside className="brand-pane" aria-label="3S GROUP — Secured Smart Systems">
        <div className="brand-inner">
          <div className="brand-mark" aria-hidden="true">
            3S
          </div>
          <p className="brand-org">3S GROUP · Secured Smart Systems</p>
          <p className="brand-headline">Official citizen registration</p>
          <p className="brand-sub">
            Create your profile and registered addresses through a secure,
            validated government portal. Every entry is checked on this device and
            again on our servers before it is stored.
          </p>
          <ul className="brand-points">
            <li>Validated on your device and on the server</li>
            <li>Every request audited and correlation-logged</li>
            <li>Privacy-first — sensitive data is never exposed</li>
          </ul>
          <p className="brand-foot">© 3S GROUP · Secured Smart Systems</p>
        </div>
      </aside>

      <main className="page form-pane">
        <RegistrationForm />
      </main>
    </div>
  );
}
