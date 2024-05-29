import { useEffect } from 'react';

export const useInitialScrollToTop = () => {
  useEffect(() => {
    window.scrollTo({ top: 0, left: 0 });
  }, []);
};
