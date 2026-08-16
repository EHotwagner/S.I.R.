import React from "react";

export default function DocumentationFeature() {
  return React.createElement(
    "section",
    { "aria-label": "Documentation", className: "panel client-documentation-feature", "data-feature-id": "docs" },
    React.createElement("h2", null, "Documentation"),
    React.createElement("p", null, "Rules, architecture, controls, and evidence remain available from the generated documentation site."),
    React.createElement("a", { href: "./docs/index.html" }, "Open generated documentation"),
  );
}
