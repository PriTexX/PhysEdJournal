import { IconButton } from '@chakra-ui/react';
import { useThemedLayoutContext } from '@refinedev/chakra-ui';
import {
  IconLayoutSidebarLeftCollapse,
  IconLayoutSidebarLeftExpand,
  IconMenu2,
} from '@tabler/icons-react';
import React from 'react';

import type { IconButtonProps } from '@chakra-ui/react';

const HamburgerIcon = (props: IconButtonProps) => (
  <IconButton variant="ghost" size="sm" {...props} />
);

export const HamburgerMenu: React.FC = () => {
  const {
    siderCollapsed,
    setSiderCollapsed,
    mobileSiderOpen,
    setMobileSiderOpen,
  } = useThemedLayoutContext();

  return (
    <>
      <HamburgerIcon
        display={{ base: 'none', md: 'flex' }}
        aria-label="drawer-sidebar-toggle"
        icon={
          siderCollapsed ? (
            <IconLayoutSidebarLeftExpand />
          ) : (
            <IconLayoutSidebarLeftCollapse />
          )
        }
        onClick={() => setSiderCollapsed(!siderCollapsed)}
      />
      <HamburgerIcon
        display={{ base: 'flex', md: 'none' }}
        aria-label="sidebar-toggle"
        icon={<IconMenu2 />}
        onClick={() => setMobileSiderOpen(!mobileSiderOpen)}
      />
    </>
  );
};
