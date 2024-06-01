import { Select, VStack } from '@chakra-ui/react';
import { DateField } from '@mui/x-date-pickers';

import { MuiThemeProvider } from '@/app/utils/mui-theme-provider';

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
  const [firstValue, secondValue] = useValues(logicalFilter.value);

  const { selectedOption, setSelectedOption } =
    useSelectedOption(logicalFilter);

  return (
    <VStack w="full">
      <Select
        flexShrink={0}
        size="sm"
        value={selectedOption}
        onChange={(e) => {
          const option = e.target.value as DateFilterOption;

          const { value, operator } = getValueAndOperator(
            [firstValue, secondValue],
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
        <DateField
          sx={{ width: '100%' }}
          value={firstValue}
          onChange={(v) => {
            if (!v?.isValid()) {
              return;
            }

            const { value, operator } = getValueAndOperator(
              [v, secondValue],
              selectedOption,
            );

            onChange({
              ...logicalFilter,
              value,
              operator,
            });
          }}
        />

        {selectedOption === 'Между' && (
          <DateField
            sx={{ width: '100%' }}
            value={secondValue}
            onChange={(v) => {
              if (!v?.isValid()) {
                return;
              }

              const { value, operator } = getValueAndOperator(
                [firstValue, v],
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
