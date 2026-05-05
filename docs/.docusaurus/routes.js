import React from 'react';
import ComponentCreator from '@docusaurus/ComponentCreator';

export default [
  {
    path: '/__docusaurus/debug',
    component: ComponentCreator('/__docusaurus/debug', '5ff'),
    exact: true
  },
  {
    path: '/__docusaurus/debug/config',
    component: ComponentCreator('/__docusaurus/debug/config', '5ba'),
    exact: true
  },
  {
    path: '/__docusaurus/debug/content',
    component: ComponentCreator('/__docusaurus/debug/content', 'a2b'),
    exact: true
  },
  {
    path: '/__docusaurus/debug/globalData',
    component: ComponentCreator('/__docusaurus/debug/globalData', 'c3c'),
    exact: true
  },
  {
    path: '/__docusaurus/debug/metadata',
    component: ComponentCreator('/__docusaurus/debug/metadata', '156'),
    exact: true
  },
  {
    path: '/__docusaurus/debug/registry',
    component: ComponentCreator('/__docusaurus/debug/registry', '88c'),
    exact: true
  },
  {
    path: '/__docusaurus/debug/routes',
    component: ComponentCreator('/__docusaurus/debug/routes', '000'),
    exact: true
  },
  {
    path: '/',
    component: ComponentCreator('/', 'f3b'),
    routes: [
      {
        path: '/',
        component: ComponentCreator('/', '163'),
        routes: [
          {
            path: '/',
            component: ComponentCreator('/', '7f9'),
            routes: [
              {
                path: '/authentication/jwt-validation-flow',
                component: ComponentCreator('/authentication/jwt-validation-flow', 'dc7'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/authentication/keycloak-overview',
                component: ComponentCreator('/authentication/keycloak-overview', '048'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/authentication/token-structure',
                component: ComponentCreator('/authentication/token-structure', '5be'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/authorization/keycloak-roles-mapping',
                component: ComponentCreator('/authorization/keycloak-roles-mapping', 'dfc'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/authorization/protect-endpoints',
                component: ComponentCreator('/authorization/protect-endpoints', '873'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/authorization/role-based-model',
                component: ComponentCreator('/authorization/role-based-model', '133'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/deployment/docker-setup',
                component: ComponentCreator('/deployment/docker-setup', 'fc7'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/deployment/environment-variables',
                component: ComponentCreator('/deployment/environment-variables', '316'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/development-guide/add-new-module',
                component: ComponentCreator('/development-guide/add-new-module', 'efb'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/development-guide/clean-architecture-rules',
                component: ComponentCreator('/development-guide/clean-architecture-rules', '429'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/introduction/architecture-overview',
                component: ComponentCreator('/introduction/architecture-overview', '341'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/product-module/api-reference',
                component: ComponentCreator('/product-module/api-reference', '3f3'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/product-module/domain-design',
                component: ComponentCreator('/product-module/domain-design', 'f4f'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/product-module/example-requests',
                component: ComponentCreator('/product-module/example-requests', '6b6'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/setup-guide/configure-backend',
                component: ComponentCreator('/setup-guide/configure-backend', '873'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/setup-guide/run-keycloak',
                component: ComponentCreator('/setup-guide/run-keycloak', '659'),
                exact: true,
                sidebar: "docs"
              },
              {
                path: '/',
                component: ComponentCreator('/', '4b0'),
                exact: true,
                sidebar: "docs"
              }
            ]
          }
        ]
      }
    ]
  },
  {
    path: '*',
    component: ComponentCreator('*'),
  },
];
