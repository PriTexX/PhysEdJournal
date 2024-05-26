import ky from 'ky';

import type { AuthProvider } from '@refinedev/core';

export type Session = {
  name: string;
  pictureUrl?: string;
};

export function getAuthProvider(apiUrl: string) {
  const client = ky.create({
    prefixUrl: apiUrl,
    credentials: 'include',
    retry: 0,
  });

  return {
    async login({ email, password }) {
      try {
        await client.post('login', { json: { username: email, password } });

        return { success: true, redirectTo: '/' };
      } catch (error) {
        return {
          success: false,
          error: {
            name: 'LoginError',
            message: 'Invalid username or password',
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
        const { name } = await client.get('session').json<{ name: string }>();

        return { session: { name } };
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
