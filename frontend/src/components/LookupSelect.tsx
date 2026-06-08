import { forwardRef, SelectHTMLAttributes } from 'react';
import { ValidationMessage } from './ValidationMessage';

export interface LookupOption {
  value: number;
  label: string;
}

interface LookupSelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  error?: string;
  options: LookupOption[];
  placeholder?: string;
  // shown when the select is disabled because a prerequisite is not chosen yet
  // (e.g. city before governorate).
  emptyHint?: string;
}

// a labelled select used for the governorate and city lookups. the city select
// is driven by the selected governorate; when there are no options it is
// disabled and shows a hint.
export const LookupSelect = forwardRef<HTMLSelectElement, LookupSelectProps>(
  ({ label, error, options, placeholder, emptyHint, id, name, required, disabled, ...rest }, ref) => {
    const selectId = id ?? name ?? label;
    const errorId = error ? `${selectId}-error` : undefined;
    const isEmpty = options.length === 0;

    return (
      <div className="field">
        <label htmlFor={selectId}>
          {label}
          {required ? <span aria-hidden="true"> *</span> : null}
        </label>
        <select
          id={selectId}
          name={name}
          ref={ref}
          disabled={disabled || isEmpty}
          aria-invalid={error ? true : undefined}
          aria-describedby={errorId}
          aria-required={required}
          {...rest}
        >
          <option value="">{placeholder ?? 'Select...'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        {isEmpty && emptyHint ? <p className="field-hint">{emptyHint}</p> : null}
        <ValidationMessage id={errorId} message={error} />
      </div>
    );
  },
);

LookupSelect.displayName = 'LookupSelect';
