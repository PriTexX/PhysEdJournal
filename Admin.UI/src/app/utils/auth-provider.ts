import ky, { HTTPError } from 'ky';

import type { AuthProvider } from '@refinedev/core';

export type Session = {
  name: string;
  pictureUrl?: string;
  role: 'admin';
};

export function getAuthProvider(apiUrl: string) {
  const client = ky.create({
    prefixUrl: apiUrl,
    credentials: 'include',
    retry: 0,
  });

  return {
    async login({ email, password, remember }) {
      try {
        await client.post('login', {
          json: { username: email, password, remember },
        });

        return { success: true, redirectTo: '/' };
      } catch (err) {
        if (err instanceof HTTPError) {
          switch (err.response.status) {
            case 401:
              return {
                success: false,
                error: {
                  name: 'Ошибка в данных',
                  message: 'Неверный логин или пароль',
                },
              };
            case 403:
              return {
                success: false,
                error: {
                  name: 'Ошибка доступа',
                  message: 'Для доступа необходимы права администратора',
                },
              };
          }
        }

        return {
          success: false,
          error: {
            name: 'Неизвестно',
            message: 'Неизвестная ошибка',
          },
        };
      }
    },

    async logout() {
      try {
        await client.post('logout');

        return { success: true, redirectTo: '/login' };
      } catch (error) {
        return {
          success: false,
          error: { name: 'LogoutError', message: 'Failed to logout' },
        };
      }
    },

    async check() {
      try {
        await client.get('session');

        return { authenticated: true };
      } catch (error) {
        return {
          authenticated: false,
          logout: true,
          redirectTo: '/login',
          error: { message: 'Logout', name: 'Unauthorized' },
        };
      }
    },

    async getIdentity() {
      try {
        const {
          name,
          avatar: pictureUrl,
          role,
        } = await client
          .get('session')
          .json<{ name: string; avatar: string; role: 'admin' }>();

        return { session: { name, pictureUrl, role } };
      } catch (error) {
        return null;
      }
    },

    async onError(error) {
      console.error(error);
      return { error };
    },
  } satisfies AuthProvider;
}
