import React from 'react';
import { Navigate, useSearchParams } from 'react-router-dom';

export const NavigateHandler = () => {
  const [urlSearchParams] = useSearchParams();
  const to = urlSearchParams.get('to');

  if (!to) {
    return <Navigate to="/" />;
  }

  return null;
};
