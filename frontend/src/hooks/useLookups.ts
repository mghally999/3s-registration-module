import { useEffect, useRef, useState } from 'react';
import { api } from '../api/client';
import { City, Governorate } from '../api/types';

// loads the governorate list once on mount.
export function useGovernorates() {
  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    api
      .getGovernorates()
      .then((data) => {
        if (active) setGovernorates(data);
      })
      .catch(() => {
        if (active) setError('Could not load governorates.');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  return { governorates, loading, error };
}

// loads the cities for the selected governorate. results are memoized per
// governorate id so switching back and forth does not refetch. passing 0 (no
// governorate chosen yet) clears the list.
export function useCities(governorateId: number) {
  const [cities, setCities] = useState<City[]>([]);
  const [loading, setLoading] = useState(false);
  const cache = useRef<Map<number, City[]>>(new Map());

  useEffect(() => {
    if (!governorateId) {
      setCities([]);
      return;
    }

    const cached = cache.current.get(governorateId);
    if (cached) {
      setCities(cached);
      return;
    }

    let active = true;
    setLoading(true);
    api
      .getCities(governorateId)
      .then((data) => {
        cache.current.set(governorateId, data);
        if (active) setCities(data);
      })
      .catch(() => {
        if (active) setCities([]);
      })
      .finally(() => {
        if (active) setLoading(false);
      });

    return () => {
      active = false;
    };
  }, [governorateId]);

  return { cities, loading };
}
