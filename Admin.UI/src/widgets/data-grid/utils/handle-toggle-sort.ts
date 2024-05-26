import type { BaseRecord } from '@/widgets/form';
import type { Header, SortingState } from '@tanstack/react-table';

const willNotChangeSortDirection = <D extends BaseRecord>(
  initialSorting: SortingState,
  header: Header<D, unknown>,
) => {
  const defaultSort = initialSorting[0];
  const currentColumnIsDefaultSorted = defaultSort.id === header.column.id;
  const nextSortOrder = header.column.getNextSortingOrder();

  return (
    currentColumnIsDefaultSorted && defaultSort.desc && nextSortOrder === false
  );
};

const forceChangeSortDirection = <D extends BaseRecord>(
  header: Header<D, unknown>,
) => {
  return header.column.toggleSorting();
};

export const handleToggleSort = <D extends BaseRecord>(
  header: Header<D, unknown>,
  initialSorting: SortingState,
  resetSorting: (v: boolean) => void,
) => {
  if (willNotChangeSortDirection(initialSorting, header)) {
    return forceChangeSortDirection(header);
  }

  const nextSortOrder = header.column.getNextSortingOrder();

  if (nextSortOrder === false) {
    resetSorting(true);
    return;
  }

  header.column.toggleSorting();
};
