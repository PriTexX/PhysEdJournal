import { FormControl, HStack, IconButton, Select } from '@chakra-ui/react';
import { IconX } from '@tabler/icons-react';
import { Controller } from 'react-hook-form';

import type { ModifyTableForm } from './modify-table-modal';
import type { FC } from 'react';
import type { Control } from 'react-hook-form';

interface ColumnItemProps {
  control: Control<ModifyTableForm>;
  canRemove: boolean;
  onRemove: VoidFunction;
  onUpdate: (newValue: string) => void;
  controllerName: `columns.${number}.value`;
  options: string[];
  isInvalid: boolean;
}

export const ColumnItem: FC<ColumnItemProps> = ({
  control,
  onRemove,
  controllerName,
  options,
  canRemove,
  onUpdate,
  isInvalid,
}) => {
  return (
    <HStack w="100%">
      <Controller
        control={control}
        name={controllerName}
        render={({ field }) => (
          <FormControl isInvalid={isInvalid}>
            <Select {...field} onChange={(e) => onUpdate(e.target.value)}>
              {options.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </Select>
          </FormControl>
        )}
      />

      <IconButton
        onClick={onRemove}
        isDisabled={!canRemove}
        aria-label="remove-cell"
      >
        <IconX size={20} />
      </IconButton>
    </HStack>
  );
};
