import { useDataProvider } from '@refinedev/core';
import { useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';

import type { Teacher } from '@/pages/teachers/types';

export const useTeachersMap = (): Map<string, string> => {
  const getDataProvider = useDataProvider();
  const dataProvider = getDataProvider();

  const { data, status } = useQuery({
    queryKey: ['teachers-map'],
    queryFn: () =>
      dataProvider.getList<Teacher>({
        resource: 'teachers',
        pagination: { pageSize: 200 },
      }),
  });

  const value = useMemo(() => {
    if (status != 'success') {
      return new Map();
    }

    const teachersMap = new Map<string, string>();

    data.data.forEach((t) => teachersMap.set(t.teacherGuid, t.fullName));

    return teachersMap;
  }, [data?.data]);

  return value;
};
