export const useAvailableOptions = (
  allOptions: string[],
  selectedOptions: string[],
) => {
  const selectedOptionsSet = new Set(selectedOptions);
  const availableOptions = allOptions.filter((o) => !selectedOptionsSet.has(o));
  return availableOptions.length > 0 ? availableOptions : null;
};
