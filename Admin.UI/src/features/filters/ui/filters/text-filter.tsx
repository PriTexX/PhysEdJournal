import { Box, Input, Select, Tag, TagLabel, VStack } from '@chakra-ui/react';
import { useEffect } from 'react';

import {
  DEFAULT_OPERATOR,
  readableFilterOperations,
  textFilterOperations,
} from '../../utils/text/text-filter-operators';
import { useValue } from '../../utils/text/use-value';

import type { TextFilterOperation } from '../../utils/text/text-filter-operators';
import type { XFilterProps } from '../../utils/types';

export const TextFilter: React.FC<XFilterProps> = ({
  logicalFilter,
  onChange,
}) => {
  const { arrayValue, inputValue, setInputValue, valueToEmit } =
    useValue(logicalFilter);

  useEffect(() => {
    if (textFilterOperations.includes(logicalFilter.operator as never)) {
      onChange({
        field: logicalFilter.field,
        value: valueToEmit,
        operator: logicalFilter.operator,
      });
    } else {
      setInputValue('');
    }
  }, [
    valueToEmit,
    logicalFilter.operator,
    onChange,
    logicalFilter.field,
    setInputValue,
  ]);

  return (
    <VStack w="full">
      <Select
        flexShrink={0}
        size="sm"
        value={logicalFilter.operator}
        onChange={(e) =>
          onChange({
            ...logicalFilter,
            operator: e.target.value as TextFilterOperation,
          })
        }
      >
        {textFilterOperations.map((filter) => (
          <option key={filter} value={filter}>
            {readableFilterOperations[filter]}
          </option>
        ))}
      </Select>
      <Input
        borderRadius="md"
        size="sm"
        autoComplete="off"
        value={inputValue}
        onChange={(e) => {
          setInputValue(e.target.value);

          onChange({
            ...logicalFilter,

            operator: textFilterOperations.includes(
              logicalFilter.operator as never,
            )
              ? logicalFilter.operator
              : DEFAULT_OPERATOR,

            value: valueToEmit,
          });
        }}
      />

      {logicalFilter.operator === 'in' && (
        <Box
          display="flex"
          w="100%"
          gap={2}
          flexWrap="wrap"
          justifyContent="flex-start"
        >
          {arrayValue.map((v, index) => (
            <Tag>
              <TagLabel key={index}>{v}</TagLabel>
            </Tag>
          ))}
        </Box>
      )}
    </VStack>
  );
};
