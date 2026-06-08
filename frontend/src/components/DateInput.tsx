import { forwardRef, InputHTMLAttributes } from 'react';
import { ValidationMessage } from './ValidationMessage';

interface DateInputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string;
  error?: string;
}

// a labelled native date input. same accessibility wiring as TextInput.
export const DateInput = forwardRef<HTMLInputElement, DateInputProps>(
  ({ label, error, id, name, required, ...rest }, ref) => {
    const inputId = id ?? name ?? label;
    const errorId = error ? `${inputId}-error` : undefined;

    return (
      <div className="field">
        <label htmlFor={inputId}>
          {label}
          {required ? <span aria-hidden="true"> *</span> : null}
        </label>
        <input
          id={inputId}
          name={name}
          type="date"
          ref={ref}
          aria-invalid={error ? true : undefined}
          aria-describedby={errorId}
          aria-required={required}
          {...rest}
        />
        <ValidationMessage id={errorId} message={error} />
      </div>
    );
  },
);

DateInput.displayName = 'DateInput';
