import { useDataProvider } from '@refinedev/core';
import { useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';

import type { Teacher } from '@/pages/teachers/types';

export const useTeachersMap = (): {
  teacherNamesMap: Map<string, string>;
  teacherGuidsMap: Map<string, string>;
} => {
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
      return { teacherNamesMap: new Map(), teacherGuidsMap: new Map() };
    }

    const teacherNamesMap = new Map<string, string>();
    const teacherGuidsMap = new Map<string, string>();

    data.data.forEach((t) => {
      teacherNamesMap.set(t.teacherGuid, t.fullName);
      teacherGuidsMap.set(t.fullName, t.teacherGuid);
    });

    return { teacherNamesMap, teacherGuidsMap };
  }, [data?.data]);

  return value;
};
