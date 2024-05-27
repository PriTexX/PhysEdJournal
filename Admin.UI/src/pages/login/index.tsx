import {
  Box,
  Button,
  Link as ChakraLink,
  Checkbox,
  Divider,
  FormControl,
  FormErrorMessage,
  FormLabel,
  Heading,
  HStack,
  Input,
  useColorModeValue,
  VStack,
} from '@chakra-ui/react';
import { ThemedTitleV2 } from '@refinedev/chakra-ui';
import {
  useActiveAuthProvider,
  useLink,
  useLogin,
  useRouterContext,
  useRouterType,
  useTranslate,
} from '@refinedev/core';
import { useForm } from '@refinedev/react-hook-form';
import React from 'react';
import { FormProvider } from 'react-hook-form';

import type { BoxProps } from '@chakra-ui/react';
import type {
  BaseRecord,
  HttpError,
  LoginFormTypes,
  LoginPageProps,
} from '@refinedev/core';
import type { UseFormProps } from '@refinedev/react-hook-form';

export interface FormPropsType<TFormType> extends UseFormProps {
  onSubmit?: (values: TFormType) => void;
}

export type LoginProps = LoginPageProps<
  BoxProps,
  BoxProps,
  FormPropsType<LoginFormTypes>
>;

const layoutProps: BoxProps = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  justifyContent: 'center',
  backgroundSize: 'cover',
  minHeight: '100dvh',
  padding: '16px',
};

const cardProps: BoxProps = {
  width: '100%',
  maxWidth: '400px',
  borderRadius: '12px',
  padding: '32px',
};

export const LoginPage: React.FC<LoginProps> = ({
  providers,
  registerLink,
  forgotPasswordLink,
  rememberMe,
  contentProps,
  wrapperProps,
  renderContent,
  formProps,
  title,
  hideForm,
}) => {
  const { onSubmit, ...useFormProps } = formProps || {};

  const authProvider = useActiveAuthProvider();
  const { mutate: login } = useLogin<LoginFormTypes>({
    v3LegacyAuthProviderCompatible: Boolean(authProvider?.isLegacy),
  });
  const translate = useTranslate();
  const routerType = useRouterType();
  const NewLink = useLink();
  const { Link: LegacyLink } = useRouterContext();
  const Link = routerType === 'legacy' ? LegacyLink : NewLink;
  const methods = useForm<BaseRecord, HttpError, LoginFormTypes>({
    ...useFormProps,
  });
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = methods;

  const renderProviders = () => {
    if (providers && providers.length > 0) {
      return (
        <>
          <VStack>
            {providers.map((provider) => (
              <Button
                key={provider.name}
                variant="outline"
                width="full"
                leftIcon={<>{provider?.icon}</>}
                fontSize="sm"
                onClick={() =>
                  login({
                    providerName: provider.name,
                  })
                }
              >
                {provider.label ?? <label>{provider.label}</label>}
              </Button>
            ))}
          </VStack>
          {!hideForm && <Divider my="6" />}
        </>
      );
    }
    return null;
  };

  const importantTextColor = useColorModeValue('brand.500', 'brand.200');

  const PageTitle =
    title === false ? null : (
      <div
        style={{
          display: 'flex',
          justifyContent: 'center',
          marginBottom: '32px',
          fontSize: '20px',
        }}
      >
        {title ?? <ThemedTitleV2 collapsed={false} />}
      </div>
    );

  const allContentProps = { ...cardProps, ...contentProps };
  const content = (
    <Box
      bg="chakra-body-bg"
      borderWidth="1px"
      borderColor={useColorModeValue('gray.200', 'gray.700')}
      backgroundColor={useColorModeValue('white', 'gray.800')}
      {...allContentProps}
    >
      <Heading
        mb="8"
        textAlign="center"
        fontSize="2xl"
        color={importantTextColor}
      >
        Войти в аккаунт
      </Heading>
      {renderProviders()}
      {!hideForm && (
        <form
          onSubmit={handleSubmit((data) => {
            if (onSubmit) {
              return onSubmit(data);
            }

            return login(data);
          })}
        >
          <FormControl mt="6" isInvalid={!!errors?.email}>
            <FormLabel htmlFor="email">Логин</FormLabel>
            <Input
              id="email"
              autoComplete="current-password"
              placeholder="Логин"
              type="text"
              {...register('email', {
                required: 'Логин не должен быть пустым',
              })}
            />
            <FormErrorMessage>{`${errors.email?.message}`}</FormErrorMessage>
          </FormControl>

          <FormControl mt="6" isInvalid={!!errors?.password}>
            <FormLabel htmlFor="password">Пароль</FormLabel>
            <Input
              id="password"
              type="password"
              placeholder="Пароль"
              {...register('password', {
                required: 'Пароль не должен быть пустым',
              })}
            />
            <FormErrorMessage>{`${errors.password?.message}`}</FormErrorMessage>
          </FormControl>

          {rememberMe ?? (
            <Checkbox {...register('remember')} mt="6">
              Запомнить меня
            </Checkbox>
          )}

          <Button mt="6" type="submit" width="full" colorScheme="brand">
            Вход
          </Button>

          <Box mt="6">
            <HStack justifyContent="space-between" fontSize="12px">
              {forgotPasswordLink ?? (
                <ChakraLink
                  as={Link}
                  color={importantTextColor}
                  to="/forgot-password"
                >
                  {translate(
                    'pages.login.buttons.forgotPassword',
                    'Forgot password?',
                  )}
                </ChakraLink>
              )}
              {registerLink ?? (
                <Box>
                  <span>
                    {translate(
                      'pages.login.buttons.noAccount',
                      'Don’t have an account?',
                    )}
                  </span>
                  <ChakraLink
                    color={importantTextColor}
                    ml="1"
                    as={Link}
                    fontWeight="bold"
                    to="/register"
                  >
                    {translate('pages.login.register', 'Sign up')}
                  </ChakraLink>
                </Box>
              )}
            </HStack>
          </Box>
        </form>
      )}

      {hideForm && registerLink !== false && (
        <Box mt={6} textAlign="center">
          <span>
            {translate(
              'pages.login.buttons.noAccount',
              'Don’t have an account?',
            )}
          </span>
          <ChakraLink
            color={importantTextColor}
            ml="1"
            as={Link}
            fontWeight="bold"
            to="/register"
          >
            {translate('pages.login.register', 'Sign up')}
          </ChakraLink>
        </Box>
      )}
    </Box>
  );

  return (
    <FormProvider {...methods}>
      <Box
        style={{
          ...layoutProps,
          justifyContent: hideForm ? 'flex-start' : 'center',
          paddingTop: hideForm ? '15dvh' : '16px',
        }}
        {...wrapperProps}
      >
        {renderContent ? (
          renderContent(content, PageTitle)
        ) : (
          <>
            {PageTitle}
            {content}
          </>
        )}
      </Box>
    </FormProvider>
  );
};
