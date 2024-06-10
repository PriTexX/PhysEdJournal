import { useDataProvider, useResource } from '@refinedev/core';
import { useQueries } from '@tanstack/react-query';
import { useMemo } from 'react';

import { useTeachersMap } from '@/features/use-teachers-map';

import type {
  BaseRecord,
  CrudFilters,
  CrudSorting,
  GetListResponse,
} from '@refinedev/core';
import type { UseQueryOptions } from '@tanstack/react-query';

const useFetchListFn = (props: {
  pageSize: number;
  filters: CrudFilters;
  sorters?: CrudSorting;
}) => {
  const getDataProvider = useDataProvider();
  const dataProvider = getDataProvider();
  const { resource } = useResource();

  const fetchList = (page: number): Promise<GetListResponse<BaseRecord>> =>
    resource
      ? dataProvider.getList({
          resource: resource.name,
          pagination: { pageSize: props.pageSize, current: page },
          filters: props.filters,
          sorters: props.sorters,
        })
      : Promise.resolve({ data: [], total: 0 });

  return { fetchList };
};

export interface UseFetchListProps {
  enabled: boolean;
  amountToLoad: number;
  filters: CrudFilters;
  sorters?: CrudSorting;
}

export const useFetchList = (props: UseFetchListProps) => {
  const CHUNK_SIZE = 100;

  const pagesAmount = Math.ceil(props.amountToLoad / CHUNK_SIZE);

  const { fetchList } = useFetchListFn({
    pageSize: CHUNK_SIZE,
    filters: props.filters,
    sorters: props.sorters,
  });

  // It's a very stupid hack to be able to map teachers guids
  // to their names in csv export. Every time we download any csv
  // we load teachers and then pass it to custom csvMap function (if provided)
  const teachersMap = useTeachersMap();

  const results = useQueries({
    queries: Array.from({ length: pagesAmount }).map<
      UseQueryOptions<GetListResponse<BaseRecord>>
    >((_, zeroBasedPage) => ({
      queryKey: [
        'LOAD PAGINATED CSV',
        props.filters,
        props.sorters,
        zeroBasedPage + 1,
      ],
      queryFn: () => fetchList(zeroBasedPage + 1),
      enabled: props.enabled,
    })),
  });

  const isFetching = results.some((q) => q.isFetching);

  const isFetched = results.every((r) => r.data && r.isSuccess);

  const isFetchError = results.some((r) => r.isError);

  const extractedData: unknown[] | null = useMemo(() => {
    if (!isFetched) {
      return null;
    }

    return results.reduce<unknown[]>((acc, value) => {
      if (value.data?.data) {
        acc.push(...value.data.data);
      }
      return acc;
    }, []);
  }, [isFetched, results]);

  return { data: extractedData, isFetching, isFetchError, teachersMap };
};
