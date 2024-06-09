import { useLink, useNavigation } from '@refinedev/core';

export const useTeacherLink = (guid: string) => {
  const Link = useLink();
  const { editUrl } = useNavigation();

  return {
    Link,
    teacherUrl: editUrl('teachers', guid),
  };
};
