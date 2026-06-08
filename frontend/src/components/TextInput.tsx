import { forwardRef, InputHTMLAttributes } from 'react';
import { ValidationMessage } from './ValidationMessage';

interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

// a labelled text input. forwardRef so react-hook-form's register() can attach
// its ref. the label is tied to the input by id, and the error is linked with
// aria-describedby + aria-invalid for accessibility.
export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(
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

TextInput.displayName = 'TextInput';
