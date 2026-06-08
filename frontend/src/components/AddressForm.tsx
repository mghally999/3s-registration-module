import { useEffect, useRef } from 'react';
import { useFormContext } from 'react-hook-form';
import { Governorate } from '../api/types';
import { useCities } from '../hooks/useLookups';
import { RegistrationFormValues } from '../validation/registrationSchema';
import { LookupSelect } from './LookupSelect';
import { TextInput } from './TextInput';

interface AddressFormProps {
  index: number;
  governorates: Governorate[];
  canRemove: boolean;
  onRemove: () => void;
}

// one address block. the city dropdown depends on the governorate chosen in
// this same block, and switching governorate clears the previously picked city
// so you can never submit a city that belongs to a different governorate.
export function AddressForm({ index, governorates, canRemove, onRemove }: AddressFormProps) {
  const {
    register,
    watch,
    setValue,
    getValues,
    formState: { errors },
  } = useFormContext<RegistrationFormValues>();

  const governorateId = Number(watch(`addresses.${index}.governorateId`)) || 0;
  const { cities } = useCities(governorateId);

  // when the governorate changes (after the first render), reset this row's
  // city so a stale, now-invalid city is not left selected.
  const previousGovernorate = useRef(governorateId);
  useEffect(() => {
    if (previousGovernorate.current !== governorateId) {
      previousGovernorate.current = governorateId;
      setValue(`addresses.${index}.cityId`, 0, { shouldValidate: false });
    }
  }, [governorateId, index, setValue]);

  const addressErrors = errors.addresses?.[index];

  // keep "primary" exclusive across all addresses: ticking this one unticks the
  // rest.
  const handlePrimaryChange = (checked: boolean) => {
    if (!checked) {
      return;
    }
    const all = getValues('addresses') ?? [];
    all.forEach((_, i) => {
      setValue(`addresses.${i}.isPrimary`, i === index, { shouldValidate: true });
    });
  };

  return (
    <fieldset className="address-card">
      <legend>Address {index + 1}</legend>

      <div className="grid-2">
        <LookupSelect
          label="Governorate"
          required
          placeholder="Select governorate"
          options={governorates.map((g) => ({ value: g.id, label: g.name }))}
          error={addressErrors?.governorateId?.message}
          {...register(`addresses.${index}.governorateId`)}
        />

        <LookupSelect
          label="City"
          required
          placeholder="Select city"
          emptyHint="Choose a governorate first."
          options={cities.map((c) => ({ value: c.id, label: c.name }))}
          error={addressErrors?.cityId?.message}
          {...register(`addresses.${index}.cityId`)}
        />
      </div>

      <TextInput
        label="Street"
        required
        error={addressErrors?.street?.message}
        {...register(`addresses.${index}.street`)}
      />

      <div className="grid-2">
        <TextInput
          label="Building number"
          required
          placeholder="e.g. 12A"
          error={addressErrors?.buildingNumber?.message}
          {...register(`addresses.${index}.buildingNumber`)}
        />

        <TextInput
          label="Flat number"
          required
          placeholder="e.g. 10/2"
          error={addressErrors?.flatNumber?.message}
          {...register(`addresses.${index}.flatNumber`)}
        />
      </div>

      <div className="field field-inline">
        <label>
          <input
            type="checkbox"
            {...register(`addresses.${index}.isPrimary`, {
              onChange: (e) => handlePrimaryChange(e.target.checked),
            })}
          />
          Primary address
        </label>
      </div>

      {canRemove ? (
        <button type="button" className="btn btn-remove" onClick={onRemove}>
          Remove address {index + 1}
        </button>
      ) : null}
    </fieldset>
  );
}
