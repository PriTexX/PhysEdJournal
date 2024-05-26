import {
  Avatar,
  Box,
  HStack,
  Icon,
  IconButton,
  Tooltip,
  useColorMode,
  useColorModeValue,
} from '@chakra-ui/react';
import { useThemedLayoutContext } from '@refinedev/chakra-ui';
import { useGetIdentity } from '@refinedev/core';
import {
  IconLayoutSidebarLeftCollapse,
  IconLayoutSidebarLeftExpand,
  IconMenu2,
  IconMoon,
  IconSun,
} from '@tabler/icons-react';
import React from 'react';

import { TimezoneButton } from '../timezone-button';
import { useCollapseMobileSider } from '../use-collapse-mobile-sider';

import type { Session } from '@/app/utils/auth-provider';
import type { BoxProps, IconButtonProps } from '@chakra-ui/react';
import type { RefineThemedLayoutV2HeaderProps } from '@refinedev/chakra-ui';

export const Header: React.FC<RefineThemedLayoutV2HeaderProps> = ({
  sticky,
}) => {
  useCollapseMobileSider();

  const { data: session } = useGetIdentity<Session>();

  const { colorMode, toggleColorMode } = useColorMode();

  const bgColor = useColorModeValue(
    'refine.header.bg.light',
    'refine.header.bg.dark',
  );

  let stickyProps: BoxProps = {};
  if (sticky) {
    stickyProps = {
      position: 'sticky',
      top: 0,
      zIndex: 4,
    };
  }

  return (
    <Box
      py="2"
      pr="4"
      pl="2"
      display="flex"
      alignItems="center"
      justifyContent="space-between"
      w="full"
      height="64px"
      bg={bgColor}
      borderBottom="1px"
      borderBottomColor={useColorModeValue('gray.200', 'gray.700')}
      {...stickyProps}
    >
      <HamburgerMenu />

      <HStack>
        <TimezoneButton />

        <Tooltip label="Toggle theme">
          <IconButton
            variant="ghost"
            aria-label="Toggle theme"
            onClick={toggleColorMode}
          >
            <Icon
              as={colorMode === 'light' ? IconMoon : IconSun}
              w="24px"
              h="24px"
            />
          </IconButton>
        </Tooltip>

        <Avatar size="sm" name={session?.name} src={session?.pictureUrl} />
      </HStack>
    </Box>
  );
};

const HamburgerIcon = ({
  tooltip,
  ...props
}: IconButtonProps & { tooltip?: boolean }) => {
  const button = <IconButton variant="ghost" size="sm" {...props} />;

  if (tooltip) {
    return <Tooltip label={props['aria-label']}>{button}</Tooltip>;
  }

  return button;
};

const HamburgerMenu: React.FC = () => {
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
        aria-label="Toggle sidebar"
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
        aria-label="Toggle sidebar"
        tooltip={false}
        icon={<IconMenu2 />}
        onClick={() => setMobileSiderOpen(!mobileSiderOpen)}
      />
    </>
  );
};
