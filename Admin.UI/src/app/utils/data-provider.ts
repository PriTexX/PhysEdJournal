import ky from 'ky';

import type { CrudOperators, DataProvider } from '@refinedev/core';

export const allowedFilterOperations = [
  'eq',
  'contains',
  'ne',
  'lt',
  'lte',
  'gt',
  'gte',
  'nnull',
  'null',
  'between',
  'nbetween',
  'in',
] satisfies CrudOperators[];

export type AllowedFilterOperation = (typeof allowedFilterOperations)[number];

export function getDataProvider(apiUrl: string) {
  const client = ky.create({
    prefixUrl: apiUrl,
    credentials: 'include',
    retry: 0,
    hooks: {
      beforeError: [
        (error) => ({
          ...error,
          statusCode: error.response.status,
          message: error.message,
        }),
      ],
    },
  });

  return {
    async getList({ resource, pagination = {}, filters = [], sorters = [] }) {
      const { current = 1, pageSize = 50 } = pagination;

      const response = await client(`${resource}/many`, {
        json: {
          offset: (current - 1) * pageSize,
          limit: pageSize,
          sorters: sorters.map(({ field, order }) => ({
            field,
            order: order.toUpperCase(),
          })),
          filters: filters.map((filter) => {
            if (!allowedFilterOperations.includes(filter.operator as never)) {
              throw new Error(
                `[data-provider] operator '${filter.operator}' is not supported.`,
              );
            }

            if (!('field' in filter)) {
              throw new Error(`[data-provider] no 'field' in filter.`);
            }

            return filter;
          }),
        },
        method: 'post',
      });

      const data = await response.json<any>();

      return data;
    },

    async create({ resource, variables }) {
      const data = await client.post(resource, { json: variables }).json<any>();

      return { data };
    },

    async update({ resource, id, variables }) {
      const data = await client
        .patch(`${resource}/${id}`, { json: variables })
        .json<any>();

      return { data };
    },

    async deleteOne({ resource, id, variables }) {
      const data = await client
        .delete(`${resource}/${id}`, { json: variables })
        .json<any>();

      return { data };
    },

    async getOne({ resource, id }) {
      const data = await client.get(`${resource}/${id}`).json<any>();

      return { data };
    },

    getApiUrl() {
      return apiUrl;
    },
  } satisfies Required<
    Pick<
      DataProvider,
      'create' | 'getList' | 'getOne' | 'update' | 'deleteOne' | 'getApiUrl'
    >
  >;
}
