import { useColorMode } from '@chakra-ui/react';
import { useController } from 'react-hook-form';
import ReactJson from 'react-json-view';

import type { Control, FieldValues, Path } from 'react-hook-form';
import type { InteractionProps } from 'react-json-view';

import './styles.css';

import { useIsDisabled } from '@/shared/utils/form/use-is-disabled';

export interface JsonEditorProps<T extends FieldValues> {
  control: Control<T>;
  name: Path<T>;
}

const useJsonFromValue = (value: unknown): Record<string, unknown> => {
  if (!value) {
    return {};
  }

  if (typeof value === 'object') {
    return value as Record<string, unknown>;
  }

  if (typeof value === 'string') {
    return JSON.parse(value) as Record<string, unknown>;
  }

  return {};
};

const useTransformValueCallback = (rawValue: unknown) => {
  if (typeof rawValue === 'string') {
    return {
      transform: (value: object) => (value ? JSON.stringify(value) : null),
    };
  }

  return { transform: (value: string | object) => value };
};

export const JsonEditor = <T extends FieldValues>({
  control,
  name,
}: JsonEditorProps<T>) => {
  const {
    field: { onChange, value },
  } = useController({ name, control });

  const { colorMode } = useColorMode();

  const json = useJsonFromValue(value);

  const { transform } = useTransformValueCallback(value);

  const isDisabled = useIsDisabled();

  const editMethods = {
    onEdit: (v: InteractionProps) => {
      const value = transform(v.updated_src);
      onChange(value);
    },
    onAdd: (v: InteractionProps) => {
      const value = transform(v.updated_src);
      onChange(value);
    },
    onDelete: (v: InteractionProps) => {
      const value = transform(v.updated_src);
      onChange(value);
    },
  };

  return (
    <ReactJson
      src={json}
      theme={colorMode === 'light' ? 'bright:inverted' : 'bright'}
      onEdit={isDisabled ? undefined : editMethods.onEdit}
      onAdd={isDisabled ? undefined : editMethods.onAdd}
      onDelete={isDisabled ? undefined : editMethods.onDelete}
      enableClipboard={false}
      displayDataTypes={true}
    />
  );
};
