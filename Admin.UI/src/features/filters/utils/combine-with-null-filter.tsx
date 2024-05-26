import { NullFilter } from '../ui/filters/null-filter';

import type { XFilterProps } from '@/features/filters/utils/types';

export interface WithNullFilterProps extends XFilterProps {}

export const combineFilterWithNullFilter =
  (Filter: React.JSXElementConstructor<XFilterProps>) =>
  ({ logicalFilter, onChange, additionalInfo }: XFilterProps) => {
    return (
      <>
        <NullFilter logicalFilter={logicalFilter} onChange={onChange} />

        <Filter
          logicalFilter={logicalFilter}
          onChange={onChange}
          additionalInfo={additionalInfo}
        />
      </>
    );
  };
