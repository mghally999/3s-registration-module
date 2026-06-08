import { useState } from 'react';
import { FormProvider, SubmitHandler, useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { api } from '../api/client';
import { ApiError, CreateRegistrationPayload } from '../api/types';
import { mapServerErrors } from '../api/errorMapping';
import { useGovernorates } from '../hooks/useLookups';
import {
  RegistrationFormValues,
  RegistrationParsed,
  registrationSchema,
} from '../validation/registrationSchema';
import { AddressForm } from './AddressForm';
import { DateInput } from './DateInput';
import { TextInput } from './TextInput';
import { Toast, ToastState } from './Toast';

const MAX_ADDRESSES = 5;

const emptyAddress = (isPrimary: boolean) => ({
  governorateId: 0,
  cityId: 0,
  street: '',
  buildingNumber: '',
  flatNumber: '',
  isPrimary,
});

const defaultValues: RegistrationFormValues = {
  firstName: '',
  middleName: '',
  lastName: '',
  birthDate: '',
  mobileNumber: '',
  email: '',
  addresses: [emptyAddress(true)],
};

export function RegistrationForm() {
  const { governorates, error: lookupError } = useGovernorates();
  const [createdId, setCreatedId] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [toast, setToast] = useState<ToastState | null>(null);

  const methods = useForm<RegistrationFormValues>({
    resolver: zodResolver(registrationSchema),
    mode: 'onChange',
    defaultValues,
  });

  const {
    register,
    handleSubmit,
    control,
    setError,
    formState: { errors, isSubmitting, isValid },
  } = methods;

  const { fields, append, remove } = useFieldArray({ control, name: 'addresses' });

  const onSubmit: SubmitHandler<RegistrationFormValues> = async (values) => {
    setFormError(null);
    setCreatedId(null);

    // values are already parsed/transformed by zod (numbers coerced, names
    // trimmed), so this is safe to send straight on.
    const parsed = values as unknown as RegistrationParsed;

    const payload: CreateRegistrationPayload = {
      firstName: parsed.firstName,
      middleName: parsed.middleName && parsed.middleName.length > 0 ? parsed.middleName : null,
      lastName: parsed.lastName,
      birthDate: parsed.birthDate,
      mobileNumber: parsed.mobileNumber,
      email: parsed.email,
      addresses: parsed.addresses.map((a) => ({
        governorateId: a.governorateId,
        cityId: a.cityId,
        street: a.street,
        buildingNumber: a.buildingNumber,
        flatNumber: a.flatNumber,
        isPrimary: a.isPrimary,
      })),
    };

    try {
      const result = await api.createRegistration(payload);
      setCreatedId(result.id);
      setToast({ type: 'success', text: 'Registration submitted successfully.' });
    } catch (err) {
      if (err instanceof ApiError) {
        const mapped = mapServerErrors(err.problem);
        // push each server error onto its field.
        mapped.forEach(({ path, message }) =>
          setError(path as keyof RegistrationFormValues, { type: 'server', message }),
        );
        if (mapped.length === 0) {
          setFormError(err.message);
        } else if (err.status === 409) {
          setFormError('This email or mobile number is already registered.');
        }
        setToast({ type: 'error', text: 'Please review the highlighted fields.' });
      } else {
        setFormError('Something went wrong. Please try again.');
        setToast({ type: 'error', text: 'Something went wrong. Please try again.' });
      }
    }
  };

  return (
    <FormProvider {...methods}>
      <Toast toast={toast} onDismiss={() => setToast(null)} />
      <form className="registration-form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <h1>Create your profile</h1>
        <p className="form-lead">
          Enter your personal details and at least one address. Fields marked
          <span aria-hidden="true"> *</span> are required, and every entry is
          validated before it is submitted.
        </p>

        {lookupError ? (
          <p className="banner banner-error" role="alert">
            {lookupError}
          </p>
        ) : null}

        <section className="personal-details">
          <h2>Personal details</h2>

          <div className="grid-2">
            <TextInput
              label="First name"
              required
              autoComplete="given-name"
              error={errors.firstName?.message}
              {...register('firstName')}
            />
            <TextInput
              label="Last name"
              required
              autoComplete="family-name"
              error={errors.lastName?.message}
              {...register('lastName')}
            />
          </div>

          <div className="grid-2">
            <TextInput
              label="Middle name"
              autoComplete="additional-name"
              error={errors.middleName?.message}
              {...register('middleName')}
            />
            <DateInput
              label="Birth date"
              required
              error={errors.birthDate?.message}
              {...register('birthDate')}
            />
          </div>

          <div className="grid-2">
            <TextInput
              label="Mobile number"
              required
              inputMode="tel"
              placeholder="+201006158123"
              autoComplete="tel"
              error={errors.mobileNumber?.message}
              {...register('mobileNumber')}
            />
            <TextInput
              label="Email"
              required
              type="email"
              autoComplete="email"
              error={errors.email?.message}
              {...register('email')}
            />
          </div>
        </section>

        <section className="addresses">
          <h2>Addresses</h2>
          {typeof errors.addresses?.message === 'string' ? (
            <p className="validation-message" role="alert">
              {errors.addresses.message}
            </p>
          ) : null}

          {fields.map((field, index) => (
            <AddressForm
              key={field.id}
              index={index}
              governorates={governorates}
              canRemove={fields.length > 1}
              onRemove={() => remove(index)}
            />
          ))}

          <button
            type="button"
            className="btn btn-add"
            onClick={() => append(emptyAddress(false))}
            disabled={fields.length >= MAX_ADDRESSES}
          >
            Add another address
          </button>
        </section>

        <div className="form-actions" aria-live="polite">
          {formError ? (
            <p className="banner banner-error" role="alert">
              {formError}
            </p>
          ) : null}
          {createdId ? (
            <p className="banner banner-success" role="status">
              Registration created. Your id is <strong>{createdId}</strong>.
            </p>
          ) : null}

          <button type="submit" className="btn btn-primary" disabled={isSubmitting || !isValid}>
            {isSubmitting ? 'Submitting...' : 'Submit registration'}
          </button>
        </div>
      </form>
    </FormProvider>
  );
}
