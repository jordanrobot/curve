# GitHub Pages Setup Instructions

This document provides instructions for enabling and configuring GitHub Pages for the MotorDefinition repository.

## Overview

The repository is configured with:
- **Static Site Generator**: Jekyll (GitHub's default and most reliable option)
- **Source Directory**: `/docs` folder
- **Deployment Method**: GitHub Actions (CI/CD)
- **Automatic Deployment**: Triggers on push to `main` branch or when PRs are merged

## Enabling GitHub Pages

After merging this PR, follow these steps to enable GitHub Pages:

### 1. Navigate to Repository Settings

1. Go to the repository: https://github.com/jordanrobot/MotorDefinition
2. Click on **Settings** tab
3. In the left sidebar, click on **Pages**

### 2. Configure GitHub Pages

In the GitHub Pages settings:

1. **Source**: Select **GitHub Actions** from the dropdown
   - This enables the workflow-based deployment we've configured
   - The workflow file is located at `.github/workflows/deploy-pages.yml`

2. Click **Save** (if required)

### 3. Verify Deployment

After enabling GitHub Pages:

1. The workflow will automatically run on the next push to `main`
2. You can monitor the deployment:
   - Go to the **Actions** tab
   - Look for the "Deploy Documentation to GitHub Pages" workflow
3. Once complete, your site will be available at:
   - **URL**: https://jordanrobot.github.io/MotorDefinition/

## Workflow Configuration

### Trigger Events

The workflow is configured to run when:
- Changes are pushed directly to the `main` branch
- A pull request is merged into the `main` branch

### Workflow Steps

1. **Checkout**: Gets the repository code
2. **Setup Pages**: Configures GitHub Pages settings
3. **Build with Jekyll**: Builds the site from the `/docs` folder
4. **Upload Artifact**: Packages the built site
5. **Deploy**: Deploys the site to GitHub Pages

### Permissions

The workflow requires the following permissions (already configured):
- `contents: read` - To read repository files
- `pages: write` - To deploy to GitHub Pages
- `id-token: write` - For OIDC authentication

## Local Development

To test the documentation locally before pushing:

### Prerequisites

- Ruby 2.7 or higher
- Bundler gem

### Setup and Run

```bash
cd docs
bundle install
bundle exec jekyll serve
```

Visit http://localhost:4000/MotorDefinition/ to preview the site.

## Jekyll Configuration

The site uses:
- **Theme**: Minima (GitHub Pages default)
- **Markdown Processor**: Kramdown with GitHub Flavored Markdown (GFM)
- **Plugins**: 
  - jekyll-feed (RSS feed)
  - jekyll-seo-tag (SEO optimization)
  - jekyll-sitemap (sitemap generation)

Configuration file: `docs/_config.yml`

## Documentation Structure

```
docs/
├── _config.yml           # Jekyll configuration
├── Gemfile              # Ruby dependencies
├── README.md            # Documentation readme
├── index.md             # Home page
├── QuickStart.md        # Quick start guide
├── UserGuide.md         # User guide
├── TermsAndDefinitions.md  # Terminology
├── api/                 # API documentation
│   ├── index.md
│   └── ...
└── adr/                 # Architecture Decision Records
    └── ...
```

## Customization

### Changing the Theme

To change the Jekyll theme, edit `docs/_config.yml`:

```yaml
theme: minima  # Change to another GitHub Pages supported theme
```

Supported themes:
- minima (default)
- jekyll-theme-cayman
- jekyll-theme-slate
- jekyll-theme-minimal
- just-the-docs (requires additional setup)

### Modifying Navigation

Edit the `header_pages` section in `docs/_config.yml` to control which pages appear in the navigation.

### Custom Domain

To use a custom domain:

1. Add a `CNAME` file to the `docs/` directory with your domain
2. Configure DNS settings with your domain provider
3. Enable HTTPS in GitHub Pages settings

## Troubleshooting

### Build Failures

If the build fails:

1. Check the workflow logs in the **Actions** tab
2. Common issues:
   - Invalid YAML front matter in markdown files
   - Missing or incorrect links
   - Jekyll syntax errors

### Page Not Found (404)

If you get 404 errors:

1. Verify GitHub Pages is enabled in repository settings
2. Check that the workflow has completed successfully
3. Ensure the `baseurl` in `_config.yml` matches your repository name
4. Wait a few minutes for the deployment to propagate

### Links Not Working

If internal links are broken:

1. Check that relative links in markdown files are correct
2. Ensure the `baseurl` setting in `_config.yml` is correct
3. Use relative links (e.g., `QuickStart.md`) rather than absolute links

## Additional Resources

- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [Jekyll Documentation](https://jekyllrb.com/docs/)
- [GitHub Actions for Pages](https://github.com/actions/deploy-pages)
