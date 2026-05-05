import type { SidebarsConfig } from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  docs: [
    { type: 'doc', id: 'introduction/index', label: 'Introduction' },
    { type: 'doc', id: 'introduction/architecture-overview', label: 'Architecture Overview' },
    {
      type: 'category',
      label: 'Authentication',
      items: [
        'authentication/keycloak-overview',
        'authentication/jwt-validation-flow',
        'authentication/token-structure',
      ],
    },
    {
      type: 'category',
      label: 'Authorization',
      items: [
        'authorization/role-based-model',
        'authorization/keycloak-roles-mapping',
        'authorization/protect-endpoints',
      ],
    },
    {
      type: 'category',
      label: 'Setup Guide',
      items: [
        'setup-guide/run-keycloak',
        'setup-guide/configure-backend',
      ],
    },
    {
      type: 'category',
      label: 'Development Guide',
      items: [
        'development-guide/add-new-module',
        'development-guide/clean-architecture-rules',
      ],
    },
    {
      type: 'category',
      label: 'Product Module',
      items: [
        'product-module/domain-design',
        'product-module/api-reference',
        'product-module/example-requests',
      ],
    },
    {
      type: 'category',
      label: 'Deployment',
      items: [
        'deployment/docker-setup',
        'deployment/environment-variables',
      ],
    },
  ],
};

export default sidebars;
