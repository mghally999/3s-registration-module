interface ValidationMessageProps {
  id?: string;
  message?: string;
}

// the inline error shown beside a field. role="alert" so screen readers
// announce it, and the id is wired to the input via aria-describedby.
export function ValidationMessage({ id, message }: ValidationMessageProps) {
  if (!message) {
    return null;
  }

  return (
    <p id={id} className="validation-message" role="alert">
      {message}
    </p>
  );
}
