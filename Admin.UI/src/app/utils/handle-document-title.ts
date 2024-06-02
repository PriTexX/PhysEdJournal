import type { Action, IResourceItem } from '@refinedev/core';

type Options = {
  resource?: IResourceItem | undefined;
  action?: Action | undefined;
  params?: Record<string, string | undefined> | undefined;
  pathname?: string | undefined;
  autoGeneratedTitle: string;
};

const capitalize = (str: string): string => {
  return str.charAt(0).toUpperCase() + str.slice(1);
};

const POSTFIX = 'StealthEX Admin';

const getTitle = (options: Options) => {
  const action = options.action ? capitalize(options.action) : '';
  const name = capitalize(options.resource?.meta?.label ?? '');
  const id = options.params?.id;

  if (options.action === 'edit' || options.action === 'show') {
    return `${name} #${id}`;
  }

  if (options.action === 'create') {
    return `${action} ${name}`;
  }

  if (options.action === 'list') {
    return `${name}`;
  }

  return null;
};

export const handleDocumentTitle = (options: Options): string => {
  const title = getTitle(options);

  if (title) {
    return `${title} | ${POSTFIX}`;
  }

  return POSTFIX;
};