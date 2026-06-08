import { useState } from 'react';
import { RegistrationForm } from './components/RegistrationForm';
import { RegistrationsList } from './components/RegistrationsList';

type Tab = 'form' | 'list';

// A single, centered shell with two tabs: the registration form and a read-only
// list of everything that has been submitted.
export default function App() {
  const [tab, setTab] = useState<Tab>('form');

  return (
    <main className="page">
      <div className="shell">
        <nav className="tabs" aria-label="Sections">
          <button
            type="button"
            className={`tab ${tab === 'form' ? 'tab-active' : ''}`}
            aria-pressed={tab === 'form'}
            onClick={() => setTab('form')}
          >
            New registration
          </button>
          <button
            type="button"
            className={`tab ${tab === 'list' ? 'tab-active' : ''}`}
            aria-pressed={tab === 'list'}
            onClick={() => setTab('list')}
          >
            Submissions
          </button>
        </nav>

        {tab === 'form' ? <RegistrationForm /> : <RegistrationsList />}
      </div>
    </main>
  );
}
