// @ts-check
const config = {
  title: 'autoUI MCP Bridge',
  tagline: 'Unity Editor-only MCP Bridge',
  url: 'https://yangjiuk09-sudo.github.io',
  baseUrl: '/autoUI/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'yangjiuk09-sudo',
  projectName: 'autoUI',
  i18n: { defaultLocale: 'en', locales: ['en','ko'] },
  presets: [[ 'classic', ({ docs: { sidebarPath: require.resolve('./sidebars.js') }, theme: { customCss: require.resolve('./src/css/custom.css') } }) ]],
  themeConfig: {
    navbar: {
      title: 'autoUI MCP Bridge',
      items: [
        { type: 'doc', docId: 'quick-start', position: 'left', label: 'Docs' },
        { href: 'https://github.com/yangjiuk09-sudo/autoUI', label: 'GitHub', position: 'right' },
      ],
    },
    footer: { style: 'dark', copyright: `MIT Licensed. Built with Docusaurus.` },
  },
};
module.exports = config;

