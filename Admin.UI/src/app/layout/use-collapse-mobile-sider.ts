import { useMediaQuery, useTheme } from '@chakra-ui/react';
import { useThemedLayoutContext } from '@refinedev/chakra-ui';
import { useCallback, useEffect } from 'react';
import { useLocation } from 'react-router-dom';

const useCollapseOnChangedLocation = (collapse: VoidFunction) => {
  const { pathname } = useLocation();

  const rootPath = pathname.split('/').filter(Boolean).at(0);

  useEffect(() => {
    collapse();
  }, [rootPath, collapse]);
};

const useCollapseOnChangedScreenSize = (collapse: VoidFunction) => {
  const theme = useTheme();
  const [isMdMin] = useMediaQuery(`(min-width: ${theme.breakpoints.md})`);

  useEffect(() => {
    if (isMdMin) {
      collapse();
    }
  }, [isMdMin, collapse]);
};

export const useCollapseMobileSider = () => {
  const { setMobileSiderOpen } = useThemedLayoutContext();

  const collapseMobileSlider = useCallback(
    () => setMobileSiderOpen(false),
    [setMobileSiderOpen],
  );

  useCollapseOnChangedScreenSize(collapseMobileSlider);
  useCollapseOnChangedLocation(collapseMobileSlider);
};
