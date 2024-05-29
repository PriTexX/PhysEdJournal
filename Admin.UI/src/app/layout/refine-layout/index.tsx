import { Box } from '@chakra-ui/react';
import { ThemedLayoutContextProvider } from '@refinedev/chakra-ui';
import React from 'react';

import { ThemedHeaderV2 } from './default-header';
import { ThemedSiderV2 as DefaultSider } from './default-sider';

import type { RefineThemedLayoutV2Props } from '@refinedev/chakra-ui';

export const ThemedLayoutV2: React.FC<RefineThemedLayoutV2Props> = ({
  Sider,
  Header,
  Title,
  Footer,
  OffLayoutArea,
  children,
  initialSiderCollapsed,
}) => {
  const SiderToRender = Sider ?? DefaultSider;
  const HeaderToRender = Header ?? ThemedHeaderV2;

  return (
    <ThemedLayoutContextProvider initialSiderCollapsed={initialSiderCollapsed}>
      <Box display="flex">
        <SiderToRender Title={Title} />
        <Box
          display="flex"
          flexDirection="column"
          flex={1}
          minH="100vh"
          overflow="clip"
          /**
           * DO NOT REMOVE WIDTH.
           * There is a strange behavior in Firefox:
           * in this case overflow: clip does not work without width property.
           * Width itself can be any value, it does not affect layout.
           */
          width={0}
        >
          <HeaderToRender />
          <Box p={[2, 4]}>{children}</Box>
          {Footer && <Footer />}
        </Box>
        {OffLayoutArea && <OffLayoutArea />}
      </Box>
    </ThemedLayoutContextProvider>
  );
};
