import {
  Button,
  Modal,
  ModalBody,
  ModalCloseButton,
  ModalContent,
  ModalFooter,
  ModalHeader,
  ModalOverlay,
  VStack,
} from '@chakra-ui/react';
import { useMemo } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';

import { useAvailableOptions } from '../utils/use-available-options';
import { useCheckValuesDuplicates as useCheckDuplicates } from '../utils/use-check-duplicates';
import { useTableSchema } from '../utils/use-table-schema';
import { AddItem } from './add-item';
import { ColumnItem } from './column-item';

import type { FC } from 'react';

export interface Column {
  value: string;
}

export interface ModifyTableForm {
  columns: Column[];
}

export interface ModifyTableModalProps {
  isOpen: boolean;
  onClose: VoidFunction;
  allColumns: string[];
  defaultColumnsNames: string[];
  resourceName: string;
}

export const ModifyTableModal: FC<ModifyTableModalProps> = ({
  isOpen,
  onClose,
  allColumns,
  resourceName,
  defaultColumnsNames,
}) => {
  const { schema, setSchema } = useTableSchema(resourceName);

  const {
    control,
    handleSubmit,
    setError,
    clearErrors,
    setValue,
    formState: { errors },
  } = useForm<ModifyTableForm>({
    defaultValues: {
      columns:
        (schema ?? defaultColumnsNames)?.map((value) => ({ value })) ?? [],
    },
  });

  const { remove, append, fields, update } = useFieldArray({
    control,
    name: 'columns',
  });

  const fieldsValues = useMemo(() => fields.map((f) => f.value), [fields]);

  const availableOptions = useAvailableOptions(allColumns, fieldsValues);

  const formId = 'edit-columns-form';

  useCheckDuplicates(fieldsValues, {
    onError: (indexes) => {
      clearErrors('columns');

      for (const index of indexes) {
        setError(`columns.${index}`, { message: 'Duplicating columns' });
      }
    },

    onNoErrors: () => clearErrors('columns'),
  });

  return (
    <Modal isOpen={isOpen} onClose={onClose}>
      <ModalOverlay />
      <ModalContent>
        <ModalHeader>Edit table structure</ModalHeader>
        <ModalCloseButton />
        <ModalBody>
          <form
            onSubmit={handleSubmit((data) => {
              setSchema(data.columns.map((c) => c.value));
              onClose();
            })}
            onReset={() => {
              setValue(
                'columns',
                defaultColumnsNames.map((v) => ({ value: v })),
              );
              setSchema(defaultColumnsNames);
              onClose();
            }}
            id={formId}
          >
            <VStack>
              {fields.map((field, index, fields) => (
                <ColumnItem
                  key={field.id}
                  control={control}
                  controllerName={`columns.${index}.value`}
                  canRemove={fields.length !== 1}
                  onRemove={() => remove(index)}
                  options={allColumns}
                  onUpdate={(value) => update(index, { value })}
                  isInvalid={Boolean(errors.columns?.[index])}
                />
              ))}

              {availableOptions && (
                <AddItem
                  onAdd={() =>
                    append({
                      value: availableOptions[0],
                    })
                  }
                />
              )}
            </VStack>
          </form>
        </ModalBody>

        <ModalFooter>
          <Button type="reset" form={formId} mr={3}>
            Reset
          </Button>
          <Button
            isDisabled={Boolean(errors.columns)}
            type="submit"
            form={formId}
            colorScheme="blue"
          >
            Apply
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
};
