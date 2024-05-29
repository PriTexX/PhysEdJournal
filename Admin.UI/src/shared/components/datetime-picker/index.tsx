import { DateTimePicker } from '@mui/x-date-pickers';
import { useController } from 'react-hook-form';

import { MuiThemeProvider } from '@/app/utils/mui-theme-provider';
import {
  applyTimezone,
  clearTimezone,
  fromISO,
  toISO,
} from '@/shared/utils/dates';

import './styles.css';

import { useState } from 'react';

import { useIsDisabled } from '@/shared/utils/form';
import { useCurrentTimezone } from '@/shared/utils/timezones/use-current-timezone';

import type { FieldRules } from '@/shared/types';
import type { Control, FieldValues, Path } from 'react-hook-form';

export interface DatetimePickerProps<T extends FieldValues> {
  control: Control<T>;
  name: Path<T>;
  readonly?: boolean;
  rules?: FieldRules<T>;
}

export const DatetimePicker = <T extends FieldValues>({
  control,
  name,
  readonly,
  rules,
}: DatetimePickerProps<T>) => {
  const {
    fieldState: { error },
    field: { value, onChange, ref },
  } = useController({ control, name, rules });

  const [dateParseError, setDateParseError] = useState(false);

  const isDisabled = useIsDisabled();

  const { timezone } = useCurrentTimezone();

  const label = timezone === '+00:00' ? 'UTC' : `UTC${timezone}`;

  return (
    <MuiThemeProvider>
      <div className={error || dateParseError ? 'error-datepicker' : undefined}>
        <DateTimePicker
          sx={{ marginTop: '0.5rem' }}
          label={label}
          disabled={isDisabled || readonly}
          value={applyTimezone(fromISO(value), timezone)}
          onChange={(date) => {
            onChange(toISO(clearTimezone(date, timezone)));
          }}
          ref={ref}
          onError={(v) => setDateParseError(Boolean(v))}
          slotProps={{
            textField: {
              error: Boolean(error) || dateParseError,
            },
          }}
        />
      </div>
    </MuiThemeProvider>
  );
};
