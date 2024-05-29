import { useContext } from 'react';

import { CurrentTimezoneContext } from '@/app/utils/current-timezone-provider/current-timezone-provider';

export const useCurrentTimezone = () => {
  const { timezone, setTimezone } = useContext(CurrentTimezoneContext);

  return { timezone, setTimezone };
};
