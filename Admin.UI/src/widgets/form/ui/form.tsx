import { Heading } from '@chakra-ui/react';
import { zodResolver } from '@hookform/resolvers/zod';
import { Create as BaseCreate, Edit as BaseEdit } from '@refinedev/chakra-ui';
import { CanAccess, useCan, useNavigation, useResource } from '@refinedev/core';
import { useForm } from '@refinedev/react-hook-form';
import * as React from 'react';
import { get } from 'react-hook-form';

import { useInitialScrollToTop } from '@/app/utils/use-initial-scroll-to-top';
import { getDirtyValues } from '@/shared/utils/form';

import { isFormGroup } from '../utils/form-group';
import { FormControl } from './form-control';
import { FormGroupWrapper } from './form-group-wrapper';

import type { FormGroup } from '../utils/form-group';
import type { HttpError } from '@refinedev/core';
import type {
  Control,
  Path,
  RegisterOptions,
  UseFormRegisterReturn,
} from 'react-hook-form';
import type { z } from 'zod';

export type BaseRecord = Record<string, unknown>;

export type RenderProps<
  D extends BaseRecord = BaseRecord,
  P extends Path<D> = Path<D>,
> = {
  register(options?: RegisterOptions<D, P>): UseFormRegisterReturn<P>;
  control: Control<D>;
  name: P;
};

export type Field<
  D extends BaseRecord = BaseRecord,
  P extends Path<D> = Path<D>,
> = {
  type: 'field';
  path: P;
  name: string;
  render(props: RenderProps<D, P>): React.ReactNode;
};

export type FieldOrGroup<D extends BaseRecord> = FormGroup<D> | Field<D, any>;

type EditProps<D extends BaseRecord> = {
  fields: FieldOrGroup<D>[];
  schema?: z.ZodSchema;
  type: 'edit' | 'create';
};

export function Form<D extends BaseRecord>({
  fields,
  schema,
  type,
}: EditProps<D>) {
  const {
    refineCore: { formLoading, queryResult, onFinish },
    saveButtonProps,
    register,
    formState: { errors, dirtyFields, isDirty },
    handleSubmit,
    control,
  } = useForm<D, HttpError, D>({
    refineCoreProps: { redirect: type === 'edit' ? false : undefined },
    resolver: schema && zodResolver(schema),
  });

  useInitialScrollToTop();

  const { list } = useNavigation();
  const { identifier, resource } = useResource();
  const { data } = useCan({ action: 'edit', resource: identifier });

  const Container = type == 'create' ? BaseCreate : BaseEdit;

  const hasData =
    queryResult?.status == 'success' && queryResult?.data?.data != undefined;

  React.useEffect(() => {
    if (formLoading || hasData || !resource || type == 'create') {
      return;
    }

    list(resource);
  }, [hasData, formLoading, resource, list, type]);

  const isLoading = formLoading || data == undefined;

  return (
    <CanAccess action={type == 'edit' ? 'show' : type}>
      <Container
        isLoading={isLoading}
        saveButtonProps={{
          form: 'form',
          type: 'submit',
          isDisabled: saveButtonProps.disabled || !isDirty,
          onClick: (event) =>
            handleSubmit((data) => {
              void onFinish(
                type == 'create'
                  ? data
                  : (getDirtyValues(dirtyFields, data) as D),
              );
            })(event),
        }}
      >
        <form id="form">
          {fields.map((fieldOrGroup) => {
            if (isFormGroup(fieldOrGroup)) {
              const fields = fieldOrGroup.fields;
              const title = fieldOrGroup.name;

              return (
                <React.Fragment key={`wrapper-for-${title}`}>
                  {title && (
                    <Heading mt="5" mb="4" size="md" as="h6" key={title}>
                      {title}
                    </Heading>
                  )}
                  <FormGroupWrapper key={`${title}-group`}>
                    {fields.map((field) => (
                      <FormControl
                        fieldName={field.name}
                        error={get(errors, field.path)?.message}
                        key={`${field.path}-${field.name}`}
                        isInvalid={get(errors, field.path) != undefined}
                        isDisabled={data?.can == false}
                      >
                        {field.render({
                          register: (options) => register(field.path, options),
                          control,
                          name: field.path,
                        })}
                      </FormControl>
                    ))}
                  </FormGroupWrapper>
                </React.Fragment>
              );
            }

            const field = fieldOrGroup;

            return (
              <FormControl
                fieldName={field.name}
                error={get(errors, field.path)?.message}
                key={`${field.path}-${field.name}`}
                isInvalid={get(errors, field.path) != undefined}
                isDisabled={data?.can == false}
              >
                {field.render({
                  register: (options) => register(field.path, options),
                  control,
                  name: field.path,
                })}
              </FormControl>
            );
          })}
        </form>
      </Container>
    </CanAccess>
  );
}
