export const areArraysEqual = <T>(arr1: T[], arr2: T[]) => {
  if (arr1 === arr2) {
    return true;
  }

  if (arr1.length !== arr2.length) {
    return false;
  }

  return arr1.every((element, index) => element === arr2[index]);
};
