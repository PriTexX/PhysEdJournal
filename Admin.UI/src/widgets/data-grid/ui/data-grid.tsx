import {
  Box,
  Button,
  Card,
  CardBody,
  Flex,
  Heading,
  HStack,
  Spinner,
  Table,
  TableContainer,
  Tbody,
  Td,
  Th,
  Thead,
  Tr,
} from '@chakra-ui/react';
import { List } from '@refinedev/chakra-ui';
import { CanAccess, useModal, useResource } from '@refinedev/core';
import { useTable } from '@refinedev/react-table';
import { RefinePageHeaderClassNames } from '@refinedev/ui-types';
import { flexRender } from '@tanstack/react-table';
import * as React from 'react';
import { Navigate } from 'react-router-dom';

import { ExportCsvModal } from '@/features/export-csv';
import Filter from '@/features/filters/ui/filter';
import { QuickFilter } from '@/features/filters/ui/quick-filter';
import { ModifyTableButton } from '@/features/modify-table';
import { Pagination } from '@/features/pagination/ui/pagination';

import { handleToggleSort } from '../utils/handle-toggle-sort';
import { useFixCoreFilters } from '../utils/use-fix-core-filters';
import { useTableColumns } from '../utils/use-table-columns';
import { useTableRowLink } from '../utils/use-table-row-link';
import ColumnSorter from './column-sorter';
import { CopyButton } from './copy-button';

import type { DataGridFilter } from '@/features/filters/utils/types';
import type { BaseRecord } from '@refinedev/core';
import type { UseTableProps } from '@refinedev/react-table';
import type { ColumnDef } from '@tanstack/react-table';

export type DataGridProps<D extends BaseRecord> = {
  // https://github.com/TanStack/table/issues/4382
  allColumns: ColumnDef<D, any>[];
  defaultColumnsKeys: (keyof D)[];
  recordId: keyof D;

  filters?: DataGridFilter<D>[];
  quickFilters?: Array<{ field: keyof D; label: string }>;
  canExportCsv?: boolean;
  customCsvMapper?: (items: D, teachersMap: Map<string, string>) => object;
  refineCoreProps?: UseTableProps<D>['refineCoreProps'];
  additionalButtons?: React.ReactElement[];
};

export function DataGrid<D extends BaseRecord>({
  allColumns,
  refineCoreProps,
  filters,
  recordId,
  canExportCsv,
  quickFilters,
  defaultColumnsKeys,
  customCsvMapper,
  additionalButtons,
}: DataGridProps<D>) {
  const { resource } = useResource();

  const { columns, defaultColumnsNames } = useTableColumns(
    resource?.name ?? '',
    allColumns,
    defaultColumnsKeys,
  );

  const {
    getHeaderGroups,
    getRowModel,
    refineCore: {
      setCurrent,
      pageCount,
      current,
      tableQueryResult: { isLoading, isFetching, data },
      setFilters,
      filters: coreFilters,
    },
    initialState: { sorting: initialSorting },
    resetSorting,
  } = useTable({
    columns,
    refineCoreProps: {
      pagination: {
        pageSize: 100,
      },
      ...refineCoreProps,
    },
  });

  useFixCoreFilters(coreFilters, filters);

  const { Link, getRecordUrl } = useTableRowLink();

  const {
    close: closeConfirmLoadCsvModal,
    show: openConfirmLoadCsvModal,
    visible: isConfirmLoadCsvModalOpen,
  } = useModal();

  const allColumnsNames = React.useMemo(() => {
    return allColumns.map((c) => String(c.header));
  }, [allColumns]);

  return (
    <CanAccess action="list" fallback={<Navigate to="/" />}>
      <List
        title={
          <Flex alignItems="center" gap="3">
            <Heading
              as="h3"
              size="lg"
              className={RefinePageHeaderClassNames.Title}
            >
              {resource?.meta?.label}
            </Heading>

            <ModifyTableButton
              allColumns={allColumnsNames}
              resourceName={resource?.name ?? ''}
              defaultColumns={defaultColumnsNames}
            />

            {filters && (
              <Filter
                filtersApplied={coreFilters}
                filtersList={filters}
                onApplyFilters={(filters) => {
                  setCurrent(1);
                  setFilters(filters, 'replace');
                }}
              />
            )}
            {data?.total && canExportCsv ? (
              <Button
                isDisabled={isFetching}
                onClick={openConfirmLoadCsvModal}
                variant="outline"
              >
                Экспорт
              </Button>
            ) : null}

            {additionalButtons &&
              additionalButtons.map((b, i) => (
                <Box display="inline-block" key={i}>
                  {b}
                </Box>
              ))}

            {isFetching && !isLoading && <Spinner />}
          </Flex>
        }
      >
        {isLoading && (
          <Flex alignItems="center" justifyContent="center" py="8">
            <Spinner />
          </Flex>
        )}
        {!isLoading && (
          <>
            <ExportCsvModal
              isOpen={isConfirmLoadCsvModalOpen}
              onClose={closeConfirmLoadCsvModal}
              allColumns={allColumns}
              defaultColumnsKeys={defaultColumnsKeys}
              rowsAmount={data?.total ?? 0}
              filters={coreFilters}
              sorters={refineCoreProps?.sorters?.initial}
              customCsvMapper={customCsvMapper}
            />

            <Card variant="filled" size="sm" mt={4}>
              <CardBody>
                Найдено <b>{data?.total ?? 0}</b> записей
              </CardBody>
            </Card>

            {quickFilters && (
              <Box mt={3}>
                <QuickFilter
                  searchBy={quickFilters}
                  filtersApplied={coreFilters}
                  onApplyFilters={(filters) => {
                    setCurrent(1);
                    setFilters(filters, 'replace');
                  }}
                />
              </Box>
            )}

            {data?.total ? (
              <TableContainer mt={2} whiteSpace="pre-line">
                <Table variant="simple" as="div" display="table" role="table">
                  <Thead as="div" display="table-header-group">
                    {getHeaderGroups().map((headerGroup) => (
                      <Tr as="div" display="table-row" key={headerGroup.id}>
                        {headerGroup.headers.map((header) => (
                          <Th as="div" display="table-cell" key={header.id}>
                            {!header.isPlaceholder && (
                              <HStack spacing="2">
                                <Box>
                                  {flexRender(
                                    header.column.columnDef.header,
                                    header.getContext(),
                                  )}
                                </Box>
                                <HStack spacing="2">
                                  {header.column.getCanSort() && (
                                    <ColumnSorter
                                      isSorted={header.column.getIsSorted()}
                                      onToggleSort={() =>
                                        handleToggleSort(
                                          header,
                                          initialSorting,
                                          resetSorting,
                                        )
                                      }
                                    />
                                  )}
                                </HStack>
                              </HStack>
                            )}
                          </Th>
                        ))}
                      </Tr>
                    ))}
                  </Thead>

                  <Tbody as="div" display="table-row-group">
                    {getRowModel().rows.map((row) => (
                      <Tr
                        as={Link}
                        to={getRecordUrl(row.original[recordId])}
                        display="table-row"
                        _hover={{
                          backgroundColor: 'tableHover',
                        }}
                        sx={{
                          transitionProperty:
                            'var(--chakra-transition-property-common)',
                          transitionDuration:
                            'var(--chakra-transition-duration-faster)',
                        }}
                        key={row.id}
                      >
                        {row.getVisibleCells().map((cell) => (
                          <Td
                            as="div"
                            display="table-cell"
                            verticalAlign="middle"
                            key={cell.id}
                            maxWidth={cell.column.getSize()}
                          >
                            {flexRender(
                              cell.column.columnDef.cell,
                              cell.getContext(),
                            )}

                            {cell.column.columnDef.meta?.canCopy && (
                              <Box display="inline-block" ml={2}>
                                <CopyButton value={String(cell.getValue())} />
                              </Box>
                            )}
                          </Td>
                        ))}
                      </Tr>
                    ))}
                  </Tbody>
                </Table>
              </TableContainer>
            ) : null}

            {pageCount > 1 && (
              <Pagination
                current={current}
                pageCount={pageCount}
                setCurrent={setCurrent}
              />
            )}
          </>
        )}
      </List>
    </CanAccess>
  );
}
