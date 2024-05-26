import { AuthPage } from '@refinedev/chakra-ui';

export const Login = () => {
  return (
    <AuthPage
      type="login"
      formProps={{
        defaultValues: { email: 'test@mail.ru', password: '123' },
      }}
    />
  );
};
