/**
 * md-to-pdf configuration for process-rfp-win
 * Professional PDF generation settings
 */

module.exports = {
  // PDF output options
  pdf_options: {
    format: "Letter",
    margin: {
      top: "1in",
      bottom: "1in",
      left: "1in",
      right: "1in"
    },
    printBackground: true,
    preferCSSPageSize: false
  },

  // Custom stylesheet
  stylesheet: [
    "pdf-theme.css"
  ],

  // CSS class for body
  body_class: "markdown-body",

  // Code highlighting style
  highlight_style: "github",

  // Document metadata
  document: {
    title: "RFP Response",
    author: "[Company Name]"
  },

  // Launch options for Puppeteer
  launch_options: {
    args: [
      "--no-sandbox",
      "--disable-setuid-sandbox"
    ]
  },

  // Markdown-it plugins
  md_file_encoding: "utf-8",

  // Image handling
  basedir: process.cwd(),

  // CSS to inject
  css: `
    @page {
      size: Letter;
      margin: 1in;
    }

    @page :first {
      margin-top: 0.5in;
    }

    /* Ensure corporate blue tables print correctly */
    thead tr,
    table tr:first-child {
      background-color: #003366 !important;
      color: #ffffff !important;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }

    tbody tr:nth-child(even) {
      background-color: #f3f6f9 !important;
      -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
    }
  `
};
