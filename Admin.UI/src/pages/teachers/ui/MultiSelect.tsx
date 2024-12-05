import {
  Button,
  Checkbox,
  Menu,
  MenuButton,
  MenuItem,
  MenuList,
} from '@chakra-ui/react';
import { useEffect, useMemo, useState } from 'react';
import { Controller } from 'react-hook-form';

import type { Teacher } from '../types';
import type { RenderProps } from '@/widgets/form';
import type { FC } from 'react';

const CustomMenu: FC<{
  value?: string;
  options: { value: string; label: string }[];
  onChange: (v: string) => void;
}> = ({ value, onChange, options }) => {
  const [selectedValues, setSelectedValues] = useState<string[]>([]);

  const labelMap = useMemo(
    () =>
      options.reduce(
        (acc, v) => {
          acc[v.value] = v.label;
          return acc;
        },
        {} as Record<string, string>,
      ),
    [options],
  );

  useEffect(() => {
    if (!value) {
      return;
    }

    if (value == selectedValues.join(', ')) {
      return;
    }

    setSelectedValues(value.split(', '));
  }, [value, selectedValues, setSelectedValues]);

  const handleCheckboxChange = (value: string) => {
    setSelectedValues((prev) =>
      prev.includes(value) ? prev.filter((v) => v !== value) : [...prev, value],
    );
  };

  return (
    <Menu>
      <MenuButton as={Button} width="100%">
        {selectedValues.length > 0
          ? selectedValues.map((v) => labelMap[v]).join(', ')
          : 'Выберите роли'}
      </MenuButton>
      <MenuList>
        {options.map((option) => (
          <MenuItem key={option.value}>
            <Checkbox
              isChecked={selectedValues.includes(option.value)}
              onChange={() => {
                handleCheckboxChange(option.value);

                const newVal = (
                  selectedValues.includes(option.value)
                    ? selectedValues.filter((v) => v !== option.value)
                    : [...selectedValues, option.value]
                ).join(', ');

                console.log(newVal);

                onChange(newVal);
              }}
            >
              {option.label}
            </Checkbox>
          </MenuItem>
        ))}
      </MenuList>
    </Menu>
  );
};

export const MultiSelect: FC<
  RenderProps<Teacher, 'permissions'> & {
    options: { value: string; label: string }[];
  }
> = ({ control, name, options }) => {
  const [selectedValues, setSelectedValues] = useState<string[]>([]);

  const handleCheckboxChange = (value: string) => {
    setSelectedValues((prev) =>
      prev.includes(value) ? prev.filter((v) => v !== value) : [...prev, value],
    );
  };

  return (
    <Controller
      control={control}
      name={name}
      render={({ field: { value, onChange } }) => (
        <CustomMenu value={value} onChange={onChange} options={options} />
      )}
    />
  );
};
