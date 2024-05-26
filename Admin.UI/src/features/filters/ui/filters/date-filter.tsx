import { Select, VStack } from '@chakra-ui/react';
import { DateTimeField } from '@mui/x-date-pickers';

import { MuiThemeProvider } from '@/app/utils/mui-theme-provider';
import { applyTimezone, clearTimezone } from '@/shared/utils/dates';
import { useCurrentTimezone } from '@/shared/utils/timezones/use-current-timezone';

import { filterOptionToOperatorMap } from '../../utils/date/date-operators';
import { getValueAndOperator } from '../../utils/date/get-value-and-operator';
import { useSelectedOption } from '../../utils/date/use-selected-option';
import { useValues } from '../../utils/date/use-values';

import type { DateFilterOption } from '../../utils/date/date-operators';
import type { XFilterProps } from '../../utils/types';

export const DateFilter: React.FC<XFilterProps> = ({
  logicalFilter,
  onChange,
}) => {
  const { timezone } = useCurrentTimezone();

  const values = useValues(logicalFilter.value);

  const [firstValue, secondValue] = values.map((v) =>
    applyTimezone(v, timezone),
  );

  const { selectedOption, setSelectedOption } =
    useSelectedOption(logicalFilter);

  const inputLabel = timezone === '+00:00' ? 'UTC' : `UTC${timezone}`;

  return (
    <VStack w="full">
      <Select
        flexShrink={0}
        size="sm"
        value={selectedOption}
        onChange={(e) => {
          const option = e.target.value as DateFilterOption;

          const { value, operator } = getValueAndOperator(
            [
              clearTimezone(firstValue, timezone),
              clearTimezone(secondValue, timezone),
            ],
            option,
          );

          onChange({
            ...logicalFilter,
            operator,
            value: value,
          });

          setSelectedOption(option);
        }}
      >
        {Object.keys(filterOptionToOperatorMap).map((filter) => (
          <option key={filter} value={filter}>
            {filter}
          </option>
        ))}
      </Select>

      <MuiThemeProvider>
        <DateTimeField
          sx={{ width: '100%' }}
          label={inputLabel}
          value={firstValue}
          onChange={(v) => {
            if (!v?.isValid()) {
              return;
            }

            const { value, operator } = getValueAndOperator(
              [
                clearTimezone(v, timezone),
                clearTimezone(secondValue, timezone),
              ],
              selectedOption,
            );

            onChange({
              ...logicalFilter,
              value,
              operator,
            });
          }}
        />

        {selectedOption === 'Between' && (
          <DateTimeField
            sx={{ width: '100%' }}
            label={inputLabel}
            value={secondValue}
            onChange={(v) => {
              if (!v?.isValid()) {
                return;
              }

              const { value, operator } = getValueAndOperator(
                [
                  clearTimezone(firstValue, timezone),
                  clearTimezone(v, timezone),
                ],
                selectedOption,
              );

              onChange({
                ...logicalFilter,
                value,
                operator,
              });
            }}
          />
        )}
      </MuiThemeProvider>
    </VStack>
  );
};
