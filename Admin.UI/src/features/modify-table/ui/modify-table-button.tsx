import { IconButton } from '@chakra-ui/react';
import { useModal } from '@refinedev/core';
import { IconSettings } from '@tabler/icons-react';

import { areArraysEqual } from '@/shared/utils/are-arrays-equal';

import { useTableSchema } from '../utils/use-table-schema';
import { ModifyTableModal } from './modify-table-modal';

import type { FC } from 'react';

interface ModifyTableButtonProps {
  allColumns: string[];
  defaultColumns: string[];
  resourceName: string;
}

export const ModifyTableButton: FC<ModifyTableButtonProps> = ({
  allColumns,
  resourceName,
  defaultColumns,
}) => {
  const { close, show, visible } = useModal();
  const { schema } = useTableSchema(resourceName);

  const isSchemaAltered = schema && !areArraysEqual(schema, defaultColumns);

  return (
    <>
      <IconButton
        onClick={show}
        aria-label="Настроить стуктуру таблицы"
        variant={isSchemaAltered ? 'solid' : 'outline'}
      >
        <IconSettings size={20} />
      </IconButton>

      <ModifyTableModal
        resourceName={resourceName}
        allColumns={allColumns}
        defaultColumnsNames={defaultColumns}
        isOpen={visible}
        onClose={close}
      />
    </>
  );
};
