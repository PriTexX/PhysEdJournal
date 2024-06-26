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

  const methods = useForm<BaseRecord, HttpError, LoginFormTypes>({
    ...useFormProps,
  });
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = methods;

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
        </form>
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
