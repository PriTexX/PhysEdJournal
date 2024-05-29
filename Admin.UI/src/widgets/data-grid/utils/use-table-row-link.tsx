import { useCan, useLink, useNavigation, useResource } from '@refinedev/core';

/** Provides link component and function to get url of record */
export const useTableRowLink = () => {
  const Link = useLink();
  const { editUrl } = useNavigation();
  const { resource } = useResource();

  const { data } = useCan({
    resource: resource?.name,
    action: 'show',
    params: { resource },
  });

  const getRecordUrl = (recordItemId: string | number) =>
    resource && data?.can ? editUrl(resource, recordItemId) : '';

  return {
    Link,
    getRecordUrl,
  };
};
