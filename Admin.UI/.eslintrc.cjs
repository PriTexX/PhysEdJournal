/* eslint-env node */

module.exports = {
  env: { browser: true, es2020: true },
  extends: [
    "eslint:recommended",
    "plugin:@typescript-eslint/recommended",
    "plugin:security/recommended-legacy",
    "plugin:react-hooks/recommended",
    "plugin:prettier/recommended",
  ],
  parser: "@typescript-eslint/parser",
  parserOptions: { ecmaVersion: "latest", sourceType: "module" },
  plugins: ["react-refresh", "react", "security"],
  rules: {
    "@typescript-eslint/no-import-type-side-effects": "error",
    "@typescript-eslint/no-floating-promises": "error",
    "@typescript-eslint/no-misused-promises": [
      "error",
      {
        checksVoidReturn: false,
      },
    ],
    "react/jsx-curly-brace-presence": "error",
    "react-refresh/only-export-components": "warn",
    "prettier/prettier": "warn",
    "@typescript-eslint/no-explicit-any": "off",
    "no-console": "off",
    "no-constant-condition": ["error", { checkLoops: false }],
    "security/detect-object-injection": "off",
    "security/detect-unsafe-regex": "off",
    "security/detect-non-literal-regexp": "off",
    "security/detect-non-literal-fs-filename": "off",
    "@typescript-eslint/no-namespace": "off",
    "@typescript-eslint/consistent-type-imports": [
      "error",
      {
        disallowTypeAnnotations: false,
      },
    ],
    "@typescript-eslint/no-non-null-assertion": "error",
    "@typescript-eslint/no-unused-vars": [
      "warn",
      {
        varsIgnorePattern: "^_",
        argsIgnorePattern: "^_",
      },
    ],
    "@typescript-eslint/naming-convention": [
      "warn",
      {
        selector: "variableLike",
        format: ["camelCase", "UPPER_CASE", "PascalCase"],
        leadingUnderscore: "allow",
      },
      {
        selector: "typeLike",
        format: ["PascalCase"],
      },
    ],
  },
};
