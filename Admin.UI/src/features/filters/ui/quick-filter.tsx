import { Box, IconButton, Input } from '@chakra-ui/react';
import { IconCheck, IconTrash } from '@tabler/icons-react';
import React from 'react';

import { useQuickFiltersForm } from '../utils/use-quick-filters-form';

import type { XFilterProps } from '@/features/filters/utils/types';
import type { BaseRecord, CrudFilters } from '@refinedev/core';
import type { FC } from 'react';

export interface QuickFilterProps<D extends BaseRecord> {
  searchBy: Array<{ field: keyof D; label: string }>;
  filtersApplied: CrudFilters;
  onApplyFilters: (filters: CrudFilters) => void;
}

const QuickTextFilter: FC<XFilterProps & { label: string }> = ({
  logicalFilter,
  onChange,
  label,
}) => {
  return (
    <Input
      variant="filled"
      placeholder={label}
      value={logicalFilter.value}
      maxWidth={['auto', 350]}
      minWidth={['auto', 200]}
      width={['100%', 'auto']}
      onChange={(e) =>
        onChange({
          ...logicalFilter,
          value: e.target.value,
          operator: 'contains',
        })
      }
    />
  );
};

export const QuickFilter = <D extends BaseRecord>({
  searchBy,
  filtersApplied,
  onApplyFilters,
}: QuickFilterProps<D>) => {
  const { setFormValue, getFormValue, handleSubmitForm, resetForm } =
    useQuickFiltersForm<D>(
      filtersApplied,
      searchBy.map((v) => v.field),
    );

  return (
    <Box
      display="flex"
      gap={2}
      as="form"
      flexWrap="wrap"
      onSubmit={(e: React.FormEvent) => {
        e.preventDefault();
        handleSubmitForm((filters) => onApplyFilters(filters));
      }}
      onReset={() => {
        resetForm((filters) => onApplyFilters(filters));
      }}
    >
      {searchBy.map(({ field, label }) => (
        <QuickTextFilter
          label={label}
          key={field as string}
          logicalFilter={getFormValue(field)}
          onChange={(newValue) => setFormValue(field, newValue)}
        />
      ))}

      <IconButton type="submit" aria-label="submit quick filters">
        <IconCheck size={20} />
      </IconButton>

      <IconButton type="reset" aria-label="reset quick filters">
        <IconTrash size={20} />
      </IconButton>
    </Box>
  );
};
